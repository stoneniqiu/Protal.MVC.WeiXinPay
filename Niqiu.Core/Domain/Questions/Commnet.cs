using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Niqiu.Core.Domain.Questions
{
    public class Comment:BaseEntity
    {
        private ICollection<CommentPraise> _praises;

        [Display(Name = "评论内容")]
        public string Content { get; set; }

        [Display(Name = "评论人")]
        [ForeignKey("CommentUserId")]
        public  virtual User.User User { get; set; }

        public int CommentUserId { get; set; }

        public int QuestionId { get; set; }

        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; }

        public int PraiseNum { get; set; }
    }
}