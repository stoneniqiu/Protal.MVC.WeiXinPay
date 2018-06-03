using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Niqiu.Core.Domain.Security
{
    public class FoundLost : VerifyEntity
    {
       public FoundLost()
       {
           LostTime = DateTime.Now;
       }

       [Required(ErrorMessage = "请输入类型")]
       [Display(Name = "类型")]
       public FoundType Type { get; set; }

       [Display(Name = "物品名称")]
       [Required(ErrorMessage = "请输入物品名称")]
       public string Name { get; set; }

       [Required(ErrorMessage = "请输入描述")]
       [Display(Name = "描述")]
       public string Description { get; set; }

       [Required(ErrorMessage = "请输入丢失时间")]
       [Display(Name = "丢失时间")]
       [DataType(DataType.DateTime)]
       public DateTime LostTime { get; set; }

       [Required(ErrorMessage = "请上传图片")]
       [Display(Name = "图片")]
       public string Image { get; set; }

       [Required(ErrorMessage = "请输入学校")]
       [Display(Name = "学校")]
       public string School { get; set; }

       [Required(ErrorMessage = "请输入校区")]
       [Display(Name = "校区")]
       public string Campus { get; set; }

       [Required(ErrorMessage = "请输入地点")]
       [Display(Name = "地点")]
       public string Place { get; set; }

       [Display(Name = "联系方式")]
       [Required(ErrorMessage = "请输入手机")]
       [RegularExpression(@"^1[3458][0-9]{9}$", ErrorMessage = "手机号格式不正确")]
       [DataType(DataType.PhoneNumber)]
       public string Mobile { get; set; }

       public FoundState FoundState { get; set; }
    }

    [Flags]
    public enum FoundType
    {
        //寻找
       [Display(Name = "寻物")]
        Found,
        //招领
       [Display(Name = "招领")]
       Receive,
    }

    [Flags]
    public enum FoundState
    {
        //未找到
        [Display(Name = "未找到")]
        UnFound,
        [Display(Name = "已找到")]
        Founded,
        //已领取
        [Display(Name = "已领取")]
        Received,
        //未领取
        [Display(Name = "未领取")]
        UnReceived,
    }
}
