using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Niqiu.Core.Domain;
using Niqiu.Core.Domain.Messages;
using Niqiu.Core.Domain.User;

namespace Niqiu.Core.Services.Messages
{
    public interface IMessageService
    {
        void InsertMessage(Message message);
        void UpdateMessage(Message message);
        void DeleteMessage(int id);
        Message GetById(int id);
        Message GetByGuid(Guid guid);

        void ReadedMessage(int id);

        IEnumerable<Message> AllChatMessages();
        IPagedList<Message> GetAllMessages(int fromuserid = 0, int touserid = 0,MessageType type=MessageType.All, string content = "", bool? read = null, int pageIndex = 0, int pageSize = 2147483647);

        IPagedList<Message> GetUserMessages(int userid, int touserid, MessageType type, int lastId = 0, int pageIndex = 0,
            int pageSize = 2147483647);

        Message SendTo(int userid, int touserid, string message);

        bool SendToAttentioned(int userid, string content, Guid? guid=null);
        Message GetLastMessage(int userid, int touserid);
    }
}
