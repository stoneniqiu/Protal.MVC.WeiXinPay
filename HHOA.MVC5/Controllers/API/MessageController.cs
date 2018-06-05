using System.Web.Http;
using Niqiu.Core.Services;
using Niqiu.Core.Domain.Common;
using Niqiu.Core.Domain.IM;
using Portal.MVC5.Attributes;
using Portal.MVC5.Models;

namespace Portal.MVC5.Controllers.API
{
    public class MessageController : ApiController
    {
        private MessageDbService _messageDbService = new MessageDbService();
        /// <summary>
        /// toId 包含了个人和群，通过长度来区别
        /// 后期加入屏蔽和拉黑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        /// 20长度为groupid，36为用户id -- guid
        /// <param name="toId"></param>
        /// <returns></returns>
        [ValidToken]
        [HttpPost]
        public Result SendMessage(MessageDto dto)
        {
            if (string.IsNullOrEmpty(dto.sender)) return new Result("000", "发送人为空");
            if (string.IsNullOrEmpty(dto.msg)) return new Result("000", "消息不能为空");
            if (string.IsNullOrEmpty(dto.toId)) return new Result("000", "接收人不能为空");
            if (dto.toId.Length != 20 && dto.toId.Length != 36) return new Result("002", "接收人id错误");
            if (dto.sender.Length != 36) return new Result("002", "发送人id错误");

            var model = new Message()
            {
                Content = dto.msg,
                SenderId = dto.sender,
                ReceiverId = dto.toId,
            };
            _messageDbService.Insert(model);

            return new Result();
        }


        [ValidToken]
        public Result DeleteMessage(MsgDto dto)
        {
            var msgId = dto.msgId;
            var userId = dto.msgId;
            if (string.IsNullOrEmpty(msgId)) return new Result("000");
            if (msgId.Length != 36 || msgId.Length != 20) return new Result("002");

            //执行权限
            //都能删除，因为只是消息的不可见

            //执行逻辑  创建一个msglog
            _messageDbService.DeleteMessage(userId, msgId);
            return new Result();
        }
        [ValidToken]
        public Result ReadMessage(MsgDto dto)
        {
            var msgId = dto.msgId;
            var userId = dto.userId;
            if (string.IsNullOrEmpty(msgId)) return new Result("000");
            if (msgId.Length != 32) return new Result("002");

            // var msg = _messageDbService.GetById(msgId);
            // if (msg == null) return new Result("002","消息不存在");

            //自己不用已读自己的消息
            // if(msg.SenderId==userId) return new Result("002");

            //执行逻辑  创建一个msglog
            _messageDbService.ReadMessage(userId, msgId, dto.senderId);
            return new Result();
        }

        [ValidToken]
        public Result GetMessage(string sender, string reciver, int index = 0, int size = 20)
        {
            if (string.IsNullOrEmpty(sender) || string.IsNullOrEmpty(reciver)) return new Result("000");
            var res = _messageDbService.GetChatMessages(sender, reciver, index, size);
            return new Result()
            {
                Data = res
            };
        }


        /// <summary>
        /// 获取消息
        /// 需要再获取历史消息之后再去调用这个方法
        /// 不然房间消息太多，刚进去的人就迟迟拿不到最新的消息
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [ValidToken]
        [HttpPost]
        public Result GetMessageByIndex(GetMsgDto dto)
        {
            //还需要排除调用者已经删除的消息
            var res = _messageDbService.GetMessagesByIndex(dto.sender, dto.reciver, dto.index);
            return new Result()
            {
                Data = res,
                Count = res.Count
            };
        }


        /// <summary>
        /// 获取消息记录，用来判断消息是否已读和删除
        /// 但存在的问题是 历史消息太多这么处理就不太和适合
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [ValidToken]
        [HttpPost]
        public Result GetMsgLogs(GetMsgDto dto)
        {
            var res = _messageDbService.GetLogList(dto.sender, dto.reciver, dto.index);
            return new Result()
            {
                Data = res,
                Count = res.Count
            };
        }



    }
}
