using System.Collections.Generic;
using System.Linq;
using Niqiu.Core.Domain;
using Niqiu.Core.Domain.IM;

namespace Niqiu.Core.Services 
{
    public class MessageDbService
    {
        private PortalDb db = new PortalDb();

        public bool Insert(Message model)
        {
            if (model == null || string.IsNullOrEmpty(model.SenderId)) return false;

            using (var _db = new PortalDb())
            {
                _db.Messages.Add(model);
                _db.SaveChanges();
                return true;
            }
        }

        public Message GetById(string msgId)
        {
            return db.Messages.Find(msgId);
        }
        /// <summary>
        /// 单方面删除消息
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="msgId"></param>
        public void DeleteMessage(string userId, string msgId)
        {
            using (var _db = new PortalDb())
            {
                var log = _db.MessageLogs.FirstOrDefault(n => n.MsgId == msgId && n.UserId == userId);
                if (log == null)
                {
                    log = new MessageLog()
                    {
                        UserId = userId,
                        MsgId = msgId,
                        IsDelete = true
                    };
                    _db.MessageLogs.Add(log);
                }
                else
                {
                    log.IsDelete = true;
                }
                _db.SaveChanges();

            };
        }

        /// <summary>
        /// 记录已读消息
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="msgId"></param>
        /// <param name="senderId"></param>
        public void ReadMessage(string userId, string msgId,string senderId)
        {
            using (var _db = new PortalDb())
            {
                //修改原来的IsRead;
                var msg = _db.Messages.Find(msgId);
                if(msg!=null)
                {
                    if (!msg.IsGroup)
                    {
                        msg.IsRead = true;
                    }
                    var log = _db.MessageLogs.FirstOrDefault(n => n.MsgId == msgId && n.UserId == userId);
                    if (log == null)
                    {
                        log = new MessageLog()
                        {
                            UserId = userId,
                            MsgId = msgId,
                            IsRead = true,
                            SenderId = senderId
                        };
                        _db.MessageLogs.Add(log);
                    }
                    else
                    {
                        log.IsRead = true;
                    }

                    _db.SaveChanges();
                }
            };
        }

        /// <summary>
        /// 某条消息的已读未读情况
        /// </summary>
        /// <param name="msgId"></param>
        /// <returns></returns>
        public List<MessageLog> GetLogList(string msgId)
        {
            return db.MessageLogs.Where(n => n.MsgId == msgId).ToList();
        }

  

        public bool IsRead(string userId,string msgId)
        {
            var msg = db.Messages.Find(msgId);
            if (msg != null)
            {
                if (msg.IsGroup)
                {
                    return db.MessageLogs.Any(n => n.MsgId == msgId && n.UserId == userId && n.IsRead);
                }
                return msg.IsRead;
            }
            return false;
        }


        public void Remove(Message model)
        {
            using (var _db = new PortalDb())
            {
                var f = _db.Messages.Find(model.MsgId);
                _db.Messages.Remove(f);
                _db.SaveChanges();
            }
        }
        public void Remove(string msgId)
        {
            using (var _db = new PortalDb())
            {
                var f = _db.Messages.Find(msgId);
                _db.Messages.Remove(f);
                _db.SaveChanges();
            }
        }


        //分页
        public IPagedList<Message> GetChatMessages(string senderId, string reciverId, int pageIndex = 0, int pageSize = 2147483647)
        {
            var query = db.Messages.Where(n => n.SenderId == senderId && n.ReceiverId == reciverId);
            query = query.OrderByDescending(c => c.SendTime);
            var items = new PagedList<Message>(query, pageIndex, pageSize);
            return items;
        }


        public List<Message> GetMessagesByIndex(string senderId, string reciverId, int index,int size=50)
        {
            var query = db.Messages.Where(n => (n.SenderId == senderId && n.ReceiverId == reciverId) || (n.SenderId == reciverId && n.ReceiverId == senderId));
             query = query.OrderBy(c => c.SendTime);
            return query.Skip(index).Take(size).ToList();
        }
        public List<MessageLog> GetLogList(string userId, string senderId, int index, int size = 50)
        {
            var query = db.MessageLogs.Where(n => (n.SenderId == senderId && n.UserId == userId));
            query = query.OrderBy(c => c.Id);
            return query.Skip(index).Take(size).ToList();
        }

    }
}