using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Niqiu.Core.Domain.IM
{
    /// <summary>
    /// client收到消息后发送一个回执 确定消息是否收到
    /// </summary>
   public class Receipt
    {
       public Receipt()
       {
           CreateTime = DateTime.Now;
           ReceiptId = Guid.NewGuid().ToString().Replace("-", "");
       }
       [Key]
       public string ReceiptId { get; set; }
       public string MsgId { get; set; }
       /// <summary>
       /// user的guid
       /// </summary>
       public string UserId { get; set; }
       public DateTime CreateTime { get; set; }
    }
}
