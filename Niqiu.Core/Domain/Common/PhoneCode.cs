using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Niqiu.Core.Domain.Common
{
    public class PhoneCode:BaseEntity
    {
        [Required]
        public string Mobile { get; set; }
        [Required]
        public string Code { get; set; }

        public string Name { get; set; }
        public PhoneCodeType PhoneCodeType { get; set; }

        public bool Successed { get; set; }

        public string ErrorCode { get; set; }

        public string ErrorMsg { get; set; }

    }

    /// <summary>
    /// 发送短信的用途
    /// </summary>
    public enum PhoneCodeType
    {
        [Display(Name = "注册")]
        Register,
        [Display(Name = "找回密码")]
        FindPassword
    }

}
