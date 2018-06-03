using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Niqiu.Core.Domain.User
{
    public class Firend : BaseEntity
    {
        [Display(Name = "谁的")]
        public int UserId { get; set; }

        [NotMapped]
        public virtual User Me { get; set; }

        public int FirendId { get; set; }

        [Display(Name = "朋友图像")]
        public string FirendImg { get; set; }

        [Display(Name = "朋友姓名")]
        public string FirendName { get; set; }

        [Display(Name = "姓名")]
        [ForeignKey("FirendId")]
        public virtual User FirendUser { get; set; }

        //如果加好友需要确认的话
        [Display(Name = "是否同意")]
        public bool IsArgee { get; set; }

        [Display(Name = "是否拉黑")]
        public bool Blacklisted { get; set; }

        //被加的人是否已经读了
        public bool Readed { get; set; }

    }
}
