using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Niqiu.Core.Domain.Messages;
using Niqiu.Core.Domain.Payments;
using Niqiu.Core.Domain.User;

namespace Portal.MVC.Models.Services
{
    public class MessageDbService
    {
        private PortalDb db = new PortalDb();

        public void SendToAttentioned(int userid, string content, Guid? guid = null)
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



        public void InsertPaymentLog(PaymentLog log)
        {
            if (log != null)
            {
                db.PaymentLogs.Add(log);
                db.SaveChanges();
            }
        }
        public void InsertMessage(Message message)
        {
            if (message == null) throw new ArgumentNullException("message");
            db.Messages.Add(message);
            db.SaveChanges();
        }
        

        private Message createMessage(int from, int to, string content, MessageType type)
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