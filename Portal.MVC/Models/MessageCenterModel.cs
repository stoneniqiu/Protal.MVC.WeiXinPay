using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Niqiu.Core.Domain.Messages;
using Niqiu.Core.Domain.User;

namespace Portal.MVC.Models
{
    public class MessageCenterModel
    {
        public IList<Message> InfoMessages=new List<Message>();
        public IList<Message> PrivateMessage = new List<Message>();
        public List<Message> SystemMessage = new List<Message>();
        public User User { get; set; }

    }
}