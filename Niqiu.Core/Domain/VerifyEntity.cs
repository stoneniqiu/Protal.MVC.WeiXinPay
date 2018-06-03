using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Niqiu.Core.Domain
{
   public class VerifyEntity:BaseEntity
    {
       public VerifyEntity()
       {
           VerifyTime = DateTime.Today;
       }

       [Display(Name = "审核时间")]
       [Required(ErrorMessage = "请选择现场审核时间")]
       [DataType(DataType.DateTime)]
       public DateTime VerifyTime { get; set; }

       [Display(Name = "备注")]
       [DataType(DataType.MultilineText)]
       public string Remarks { get; set; }

       [Display(Name = "申请状态")]
       public VerifyState State { get; set; }

       public int UserId { get; set; }

       [Display(Name = "审核人")]
       public string UserName { get; set; }

       [Display(Name = "申请人")]
       [ForeignKey("ApplyUserId")]
       public virtual User.User ApplyUser { get; set; }

       [Display(Name = "申请人id")]
       public virtual int ApplyUserId { get; set; }

       [Display(Name = "申请人")]
       public virtual string ApplyUserName { get; set; }
    }

   public enum VerifyState
   {
       //在申请
       [Display(Name = "申请中")]
       Padding,
       //通过
       [Display(Name = "已通过")]
       Passed,
       //拒绝
       [Display(Name = "已拒绝")]
       Refuse
   }
}
