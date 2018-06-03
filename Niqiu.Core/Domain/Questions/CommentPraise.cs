using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Niqiu.Core.Domain.Questions
{
   public class CommentPraise:BaseEntity
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int CommentId { get; set; }
    }
}
