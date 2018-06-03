using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Niqiu.Core.Domain.Messages;
using Niqiu.Core.Domain.User;
using Portal.MVC.Models;

namespace Portal.MVC.Controllers
{
    public class CommonEFHelper
    {
        private static object o = new object();

        public static void SendToAttentioned(int userid, string content, Guid? guid = null)
        {
            lock (o)
            {
                using (var db = new PortalDb())
                {
                    var sys = db.Users.FirstOrDefault(n => n.SystemName == SystemUserNames.SystemWallet) ?? new User();
                    var attendeds = db.Firends.Where(n => n.FirendId == userid);
                    IList<Message> mess = new List<Message>();
                    foreach (var f in attendeds)
                    {
                        var mes = createMessage(sys.Id, f.UserId, content, MessageType.Weibo);
                        var user = db.Users.Find(f.UserId);
                        if (user != null)
                        {
                            mes.RelateImg = user.ImgUrl;
                        }
                        if (guid != null)
                            mes.RelateGuid = guid.Value;

                        mess.Add(mes);
                    }
                    db.Messages.AddRange(mess);
                    db.SaveChanges();
                }
            }
        }

        public static void SaveRewardInfo(int userid, string content, MessageType type, Guid guid)
        {
            lock (o)
            {
                using (var db = new PortalDb())
                {
                    var sys = db.Users.FirstOrDefault(n => n.SystemName == SystemUserNames.SystemWallet) ?? new User();
                    var mesg = createMessage(sys.Id, userid, content, type);
                    mesg.RelateGuid = guid;
                    db.Messages.Add(mesg);
                    db.SaveChanges();
                }
            }
        }

        public static void SaveRewardInfo(Message message)
        {
            if (message != null)
            {
                lock (o)
                {
                    using (var db = new PortalDb())
                    {
                        db.Messages.Add(message);
                        db.SaveChanges();
                    }
                }
            }
        }


        private static Message createMessage(int from, int to, string content, MessageType type)
        {
            return new Message()
            {
                Content = content,
                FromUserId = from,
                ToUserId = to,
                MessageType = type
            };
        }
    }
}