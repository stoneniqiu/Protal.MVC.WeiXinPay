using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Niqiu.Core.Domain.Reports
{
  public class Report:BaseEntity
    {
      public string Title { get; set; }
      public string Content { get; set; }
      public ReportType ReportType { get; set; }
      public ReportRelateType RelateType { get; set; }

      [Display(Name = "相关id")]
      public int RelateId { get; set; }

      public string RelateUserName { get; set; }

      public int RelateUserId { get; set; }


      [Display(Name = "举报人")]
      public int UserId { get; set; }
      [Display(Name = "举报人")]

      public string UserName { get; set; }
      [Display(Name = "举报人")]
      [ForeignKey("UserId")]
      public virtual User.User User { get; set; }

      [Display(Name = "处理人")]
      public int AdminUserId { get; set; }
      [Display(Name = "处理人")]
      public string AdminUserName { get; set; }

      public bool Deleted { get; set; }

      [Display(Name = "是否处理")]
      public bool IsDeal { get; set; }

      [Display(Name = "备注")]
      public string Remarks { get; set; }


      [Display(Name = "处理时间")]
      public DateTime? DealTime { get; set; }

    }

    public enum ReportRelateType
    {
        [Display(Name = "谜题")]
        Question,
        [Display(Name = "评论")]
        Comment
    }

    public enum ReportType
    {
        [Display(Name = "垃圾营销")]
        JunkMarketing,
        [Display(Name = "不实信息")]
        Unreal,
        [Display(Name = "有害信息")]
        Harmful,
        [Display(Name = "违法信息")]
        Illegal,
        [Display(Name = "污秽色情")]
        Pornographic,
        [Display(Name = "抄袭我的题")]
        Plagiarism,
        [Display(Name = "对我人身攻击")]
        Attack
    }
}
