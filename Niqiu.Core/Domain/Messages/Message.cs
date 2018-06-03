using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using Niqiu.Core.Domain.Questions;

namespace Niqiu.Core.Domain.Messages
{
   public class Message:BaseEntity
    {
       public Message()
       {
           Guid = Guid.NewGuid();
       }
       public Guid Guid { get; set; }
       [Required]
       public string Content { get; set; }
       [Required]
       public int FromUserId { get; set; }
       [ForeignKey("FromUserId")]
       public virtual User.User FromUser { get; set; }
       [Required]
       public int ToUserId { get; set; }

       public bool ToUserHide { get; set; }
       public bool FromUserHide { get; set; }

       [ForeignKey("ToUserId")]
       public virtual User.User ToUser { get; set; }
       public bool IsRead { get; set; }

       public bool Deleted { get; set; }

       public MessageType MessageType { get; set; }

       [Display(Name = "回复id")]
       public int ReplyId { get; set; }

       //好跳转
       [Display(Name = "相关Guid")]
       public Guid RelateGuid { get; set; }
       public string RelateImg { get; set; }

       [NotMapped]
       public Question Question { get; set; }

       [NotMapped]
       public virtual Message ReplyMessage { get; set; }

       [NotMapped]
       public int UnReadCount { get; set; }

    }

    public enum MessageType
    {
        [Display(Name = "私信")]
        Chat,
        [Display(Name = "系统提示")]
        SystemInfo,
        [Display(Name = "获得悬赏")]
        Reward,
        [Display(Name = "好友动态")]
        //好友发布了谜题
        Weibo,
        [Display(Name = "充值成功")]
        ReChange,
        [Display(Name = "购买提示")]
        //todo  注意购买的提示格式
        // <p><span class="col">“这是什么星系的星云...”</span>答案提示：邻近星系间的相互作用</p>
        BuyInfo,
        All
    }


    
}
