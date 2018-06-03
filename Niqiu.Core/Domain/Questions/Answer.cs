using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Niqiu.Core.Domain.Questions
{
    public class Answer:BaseEntity
    {
        [Display(Name = "答案")]
        public string Content { get; set; }

        [Display(Name = "回答是否正确")]
        public bool IsRight { get; set; }

        [Display(Name = "相关问题")]
        [Required]
        [ForeignKey("QuestioId")]
        public virtual Question Question { get; set; }

        [Display(Name = "相关问题")]
        public int QuestioId { get; set; }

        [Display(Name = "回答者")]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        [Display(Name = "回答者")]
        public virtual User.User User { get; set; }

        [Display(Name = "是否已经支付")]
        public bool IsPay { get; set; }
    }
}
