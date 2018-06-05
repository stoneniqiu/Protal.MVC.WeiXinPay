using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Niqiu.Core.Domain.IM
{
    //单聊消息
   public class Message
    {
       private string _receiverId;

       public Message()
       {
           SendTime = DateTime.Now;
           MsgId = Guid.NewGuid().ToString().Replace("-", "");
           MessageType = "002";
           //002正常聊天
           //003图片消息
       }

       [Key]
       public string MsgId { get; set; }
       public string SenderId { get; set; }
       public string Content { get; set; }
       public DateTime SendTime { get; set; }
       public bool IsRead { get; set; }

       public string MessageType { get; set; }

       public string ReceiverId
       {
           get
           {
               return _receiverId;
           }
           set
           {
               _receiverId = value;
               IsGroup=isGroup(_receiverId);
           }
       }

       [NotMapped]
       public Int32 MsgIndex { get; set; }
       
       [NotMapped]
       public bool IsGroup { get; set; }

       public static bool isGroup(string key)
       {
           return !string.IsNullOrEmpty(key) && key.Length == 20;
       }
    }
 
    public class MessageLog:BaseEntity
    {
        public string UserId { get; set; }
        public string SenderId { get; set; }
       // [ForeignKey("UserId")]
        [NotMapped]
        public User.User User { get; set; }
        public string MsgId { get; set; }
        public bool IsDelete { get; set; }
        public bool IsRead { get; set; }
    }
}
