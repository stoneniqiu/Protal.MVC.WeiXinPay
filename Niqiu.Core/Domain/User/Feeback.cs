using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Niqiu.Core.Domain.User
{
   public class Feeback:BaseEntity
    {
       [Display(Name = "反馈内容")]
       public string Content { get; set; }
       [Display(Name = "是否确认")]
       public bool Ensure { get; set; }
       [Display(Name = "回答")]
       public string Answer { get; set; }
       [Display(Name = "反馈用户id")]
       public int UserId { get; set; }
       [Display(Name = "反馈用户姓名")]
       public string UserName { get; set; }

       [Display(Name = "处理用户姓名")]
       public int AnswerUserId { get; set; }
       [Display(Name = "处理用户姓名")]
       public string AnswerUserName { get; set; }

    }
}
