using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Niqiu.Core.Domain.Messages;
using Niqiu.Core.Domain.User;

namespace Portal.MVC.ViewModel
{
    public class ChatModel
    {
        public User Me { get; set; }
        public User ToUser { get; set; }

        public IEnumerable<Message> Messages { get; set; }
    }

    public class Msg
    {
        public int id { get; set; }
        public int fromid { get; set; }
        public int toid { get; set; }
        public string content { get; set; }

        public DateTime time { get; set; }

    }

    public class ChatJsonModel
    {
        public ChatJsonModel(User me, User to, IEnumerable<Message> ms)
        {
            meId = me.Id;
            toId = to.Id;
            Msgs=new List<Msg>();
            lastId = 0;
            foreach (var message in ms)
            {
                Msgs.Add(new Msg()
                {
                    id = message.Id,
                    fromid = message.FromUserId,
                    toid = message.ToUserId,
                    content = message.Content,
                    time=message.CreateTime
                });
                lastId = message.Id;
            }

        }
        public int meId { get; set; }
        public int toId { get; set; }
        public int lastId { get; set; }

        public List<Msg> Msgs { get; set; }  
    }
}