using System;
using System.IO;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;
using Niqiu.Core.Domain.Common;
using Niqiu.Core.Domain.IM;
using Niqiu.Core.Services;
using WebSocketSharp.Net;


namespace Websoket.Server
{
    public class Chat : WebSocketBehavior
    {
        //字典没用 只有本身 每个behavior都是一个新的对象

        private UserDbService _userService = new UserDbService();
        private MessageDbService _msgService = new MessageDbService();
        private bool IsValid { get; set; }
        private string UserGuid { get; set; }
        private string UserName { get; set; }

        private MemoryCacheManager cacheManager = new MemoryCacheManager();
        
        protected override async Task OnMessage(MessageEventArgs e)
        {

            StreamReader reader = new StreamReader(e.Data);
            string text = reader.ReadToEnd();
            try
            {
                var obj = Json.JsonParser.Deserialize<JsonDto>(text);
                var toid = obj.toId;
                var key = "";
                switch (obj.type)
                {
                    case "001": //验证token
                        var token = obj.content;
                        log("验证token：" + token);
                        var res = _userService.validToken(token);
                        if (res.Code != "001")
                        {
                            //token失败，关闭连接
                            IsValid = false;
                            //验证错误先不马上关闭
                            //Close(CloseStatusCode.IncorrectData, "token错误");
                            SendToSelf("token错误","0011");
                        }
                        else
                        {
                            //验证成功
                            IsValid = true;
                            log(Id + "验证成功 ");
                            //应该获取用户的信息。
                            var infos = _userService.DecryptToken(token);

                            UserGuid = infos[0];
                            UserName = infos[1];
                            if (!cacheManager.IsSet(infos[0]))
                            {
                                cacheManager.Set(infos[0], Id, 60);
                            }
                            //告之client验证结果，并把guid发过去
                            SendToSelf("token验证成功");
                            //当前用户已经满足继续聊天的条件
                        }
                        break;
                    case "002"://正常聊天
                    case "004"://正常聊天
                        //先检查是否合法
                        if (!IsValid)
                        {
                            SendToSelf("请先验证!","002");
                            break;
                        }
                        //在这里创建消息 避免群消息的时候多次创建
                        var msg = new Message()
                        {
                            SenderId = UserGuid,
                            Content = obj.content,
                            IsRead = false,
                            ReceiverId = toid,
                            MessageType = obj.type
                        };
                        //if (msg.MessageType == "004")
                        //{
                        //    msg.Content = Json.JsonParser.Serialize(obj);
                        //}
                        //先发送给自己 两个作用 1告知对方服务端已经收到消息 2 用于对方通过msgid查询已读未读
                        SendToSelf(msg);

                        //判断toid是user还是 group
                        if (msg.IsGroup)
                        {
                            log("群消息:"+obj.content+",发送者："+UserGuid);
                            //那么要找出这个group的所有用户
                            var group = _userService.GetGroup(toid);
                            foreach (var user in group.Users)
                            {
                                //除了发消息的本人
                                //群里的其他人都要收到消息
                                if (user.UserGuid.ToString() != UserGuid)
                                {
                                    SendToUser(user.UserGuid.ToString(), msg);
                                }
                            }
                        }
                        else
                        {
                            log("单消息:" + obj.content + ",发送者：" + UserGuid);
                            SendToUser(toid, msg);
                        }
                        //save message
                        //_msgService.Insert(msg);
                        break;
                    //处理回执
                    //对于群消息，回执只用告诉发消息的人即可
                    //所以在客户端 回执的toid只能是user的guid
                    case "003":
                        key = cacheManager.Get<string>(toid);
                        var recepit = new Receipt()
                        {
                            MsgId = obj.content,
                            UserId = UserGuid,
                        };
                        //发送给 发回执的人，告知服务端已经收到他的回执
                        SendToSelf(recepit);
                        if (key != null)
                        {
                            //发送给对方
                           await Sessions.SendTo(key, Json.JsonParser.Serialize(recepit));
                        }

                        //这条回执 要发送给谁呢？消息的拥有者
                        //如何避免去查询一次数据库 因为知道userid就可以了
                        // save recepit
                        break;
                    default:
                        break;
                }
            }
            catch (Exception exception)
            {
                SendToSelf("格式有误！" + exception);
                Console.WriteLine(exception);
            }
        }

        private void SendToUser(string toId, Message msg)
        {
            var userKey = cacheManager.Get<string>(toId);
            //这个判断可以拿掉 不存在的用户肯定不在线
            //var touser = _userService.GetUserByGuid(obj.toId);
            if (userKey != null)
            {
                //发送给对方
                Sessions.SendTo(userKey, Json.JsonParser.Serialize(msg));
            }
            else
            {
                //不需要通知对方
                //SendToSelf(toId + "还未上线!");
            }
        }
        //1000
        public void Close(CloseStatusCode code, string msg)
        {
            Sessions.CloseSession(Id, code, msg);

        }
        private void log(string txt)
        {
            Console.WriteLine(txt + " " + DateTime.Now);
        }

        protected override async Task OnClose(CloseEventArgs e)
        {
            Console.WriteLine("连接关闭" + Id);
            Broadcast(string.Format("{0}下线，共有{1}人在线", Id, Sessions.Count), "3");
            cacheManager.Remove(UserGuid);

        }

        protected override async Task OnError(WebSocketSharp.ErrorEventArgs e)
        {
            var el = e;
        }

        protected override async Task OnOpen()
        {
            Console.WriteLine("建立连接" + Id);
            SendToSelf(string.Format("{0}上线了，共有{1}人在线", Id, Sessions.Count), "3");

        }

        private void Broadcast(string msg, string type = "1")
        {
            var data = new JsonDto() { content = msg, type = type, sender = Id };
            Sessions.Broadcast(Json.JsonParser.Serialize(data));
        }
        private void SendToSelf(string msg, string type = "001")
        {
            var data = new JsonDto() { content = msg, type = type, sender = UserGuid??Id };
            SendToSelf(data);
        }
        private void SendToSelf(JsonDto dto)
        {
            Send(Json.JsonParser.Serialize(dto));
        }
        private void SendToSelf(Receipt dto)
        {
            Send(Json.JsonParser.Serialize(dto));
        }
        private void SendToSelf(Message dto)
        {
            Send(Json.JsonParser.Serialize(dto));
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var wssv = new WebSocketServer(null, 8080);
            wssv.AddWebSocketService<Chat>("/Chat",()=>new Chat()
            {
                Protocol = "socketcrutch",
                CookiesValidator = (req, res) =>
                {
                    // Check the Cookies in 'req', and set the Cookies to send to the client with 'res'
                    // if necessary.
                    foreach (Cookie cookie in req)
                    {
                        cookie.Expired = true;
                        res.Add(cookie);
                    }
                    return true; // If valid.
                }
            });
            wssv.Start();
            Console.WriteLine("启动成功!");
            Console.ReadKey(true);
            wssv.Stop();
        }
    }
}
