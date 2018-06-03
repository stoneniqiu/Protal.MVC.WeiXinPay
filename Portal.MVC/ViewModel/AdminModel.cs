using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Portal.MVC.ViewModel
{
    public class AdminModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "登录名不能为空")]
        [Display(Name = "姓名")]
        public string UserName { get; set; }
        
        [Required]
        [Display(Name = "角色")]
        public int RoleId { get; set; }

        [Display(Name = "手机号码")]
        [Required(ErrorMessage = "请输入您的手机号")]
        [RegularExpression(@"^1[3458][0-9]{9}$", ErrorMessage = "手机号格式不正确")]
        [Remote("CheckMoble", "User", ErrorMessage = "该手机号已经存在！")]
        [DataType(DataType.PhoneNumber)]
        public string Mobile { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        [Remote("CheckMail", "User", ErrorMessage = "该邮箱已经存在！")]
        [RegularExpression(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}", ErrorMessage = "请输入正确的email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "密码不能为空")]
        [StringLength(20, ErrorMessage = "{0}由6到20个字符或数字组成。", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "确认密码")]
        [Compare("Password", ErrorMessage = "密码和确认密码不匹配。")]
        public string ComfirmPassword { get; set; }

    }


    public class EditAdminModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "登录名不能为空")]
        [Display(Name = "姓名")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "角色")]
        public int RoleId { get; set; }

        [Display(Name = "手机号码")]
        [Required(ErrorMessage = "请输入您的手机号")]
        [RegularExpression(@"^1[3458][0-9]{9}$", ErrorMessage = "手机号格式不正确")]
        [DataType(DataType.PhoneNumber)]
        public string Mobile { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        [RegularExpression(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}", ErrorMessage = "请输入正确的email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "密码不能为空")]
        [StringLength(20, ErrorMessage = "{0}由6到20个字符或数字组成。", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "确认密码")]
        [Compare("Password", ErrorMessage = "密码和确认密码不匹配。")]
        public string ComfirmPassword { get; set; }

    }
}