using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Niqiu.Core.Domain.Questions
{
   public class RewardUser:BaseEntity
    {
       public int QuestionId { get; set; }
       public int UserId { get; set; }

    }
}
