using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading;
using Niqiu.Core.Domain;
using Niqiu.Core.Domain.Messages;
using Niqiu.Core.Domain.User;
using Niqiu.Core.Services.Firends;
using NPOI.HSSF.Record.Formula.Functions;

namespace Niqiu.Core.Services.Messages
{
    public class MessageService : IMessageService
    {
        private readonly IRepository<Message> _messageRepository;
        private readonly IRepository<User> _useRepository;
        private readonly IRepository<Firend> _fRepository;
        public MessageService(IRepository<Message> messageRepository, IRepository<User> uRepository, IRepository<Firend> fRepository)
        {
            _messageRepository = messageRepository;
            _useRepository = uRepository;
            _fRepository = fRepository;
        }
        public void InsertMessage(Message message)
        {
            if (message == null) throw new ArgumentNullException("message");
            _messageRepository.Insert(message);
        }

        public IEnumerable<Message> AllChatMessages()
        {
            return _messageRepository.Table.Include(n => n.FromUser).Where(n => n.MessageType == MessageType.Chat);
        }

        public void UpdateMessage(Message message)
        {
            if (message == null) throw new ArgumentNullException("message");
            _messageRepository.Update(message);
        }

        public void DeleteMessage(int id)
        {
            var model = _messageRepository.GetById(id);
            if (model != null)
            {
                _messageRepository.Delete(model);
            }
        }

        public Message GetById(int id)
        {
            return _messageRepository.GetById(id);
        }

        public Message GetByGuid(Guid guid)
        {
            return _messageRepository.Table.FirstOrDefault(n => n.Guid == guid);
        }

        public void ReadedMessage(int id)
        {
            var msg = _messageRepository.GetById(id);
            if (msg != null)
            {
                msg.IsRead = true;
              UpdateMessage(msg);
            }
        }
        public IPagedList<Message> GetAllMessages(int fromuserid = 0, int touserid = 0, MessageType type = MessageType.All, string content = "", bool? read = null, int pageIndex = 0, int pageSize = 2147483647)
        {
            var query = _messageRepository.Table.Where(n => !n.Deleted);
            if (fromuserid != 0)
            {
                query = query.Where(n => n.FromUserId == fromuserid);
            }
            if (touserid != 0)
            {
                query = query.Where(n => n.ToUserId == touserid);
            }
            if (!string.IsNullOrWhiteSpace(content))
            {
                query = query.Where(n => n.Content.Contains(content));
            }
            if (read != null)
            {
                query = query.Where(n => n.IsRead == read.Value);
            }
            if (type != MessageType.All)
            {
                query = query.Where(n => n.MessageType == type);
            }
            query = query.OrderByDescending(n => n.Id);
            return new PagedList<Message>(query, pageIndex, pageSize);
        }

        public IPagedList<Message> GetUserMessages(int userid, int touserid, MessageType type, int lastId = 0, int pageIndex = 0,
            int pageSize = 2147483647)
        {
            var query = _messageRepository.Table.Where(n => !n.Deleted);
            if (touserid != 0)
            //包含两者的消息
            {
                query =
                      query.Where(
                          n =>
                              (n.FromUserId == userid && n.ToUserId == touserid&&!n.FromUserHide) ||
                              (n.FromUserId == touserid && n.ToUserId == userid&&!n.ToUserHide));
            }
            else
            {
                //只要有我的消息
                query =
             query.Where(
                 n =>
                     ((n.ToUserId == userid && !n.ToUserHide) || (n.FromUserId == userid && !n.FromUserHide)));

            }
            if (type != MessageType.All)
            {
                query = query.Where(n => n.MessageType == type);
            }

            if (lastId != 0)
            {
                query = query.Where(n => n.Id > lastId);
            }
            query = query.OrderBy(n => n.Id);
            return new PagedList<Message>(query, pageIndex, pageSize);
        }

        public Message GetLastMessage(int userid, int touserid)
        {
            var query = _messageRepository.Table.Where(n => !n.Deleted&&n.MessageType==MessageType.Chat);
            query =
             query.Where(
                          n =>
                              (n.FromUserId == userid && n.ToUserId == touserid && !n.FromUserHide) ||
                              (n.FromUserId == touserid && n.ToUserId == userid && !n.ToUserHide)).OrderByDescending(n=>n.Id);

            var msg= query.FirstOrDefault();
            if (msg != null)
            {
                //获取我未读的消息条数
                msg.UnReadCount = query.Count(n => !n.IsRead&&n.ToUserId==userid);
            }
            return msg;
        }

        public Message SendTo(int userid, int touserid, string message)
        {
            if (string.IsNullOrWhiteSpace(message) || userid == 0)
            {
                throw new ArgumentException("参数有误");
            }
            var model = createMessage(userid, touserid, message, MessageType.Chat);
            _messageRepository.Insert(model);
            return GetByGuid(model.Guid);
        }





        public bool SendToAttentioned(int userid, string content, Guid? guid = null)
        {
            throw new NotImplementedException();
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
