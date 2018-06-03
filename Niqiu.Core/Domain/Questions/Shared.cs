using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Niqiu.Core.Domain.Questions
{
   public class Shared:BaseEntity
    {
       public int QuestionId { get; set; }

       [ForeignKey("QuestionId")]
       public Question Question { get; set; }

       public string Type { get; set; }

       [Display(Name = "分享着Id")]
       public int ShardUserId { get; set; }

    }
}
