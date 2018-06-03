using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Niqiu.Core.Domain.Security
{
    public class Photo : VerifyEntity
    {
        [Required(ErrorMessage = "请输入事物")]
        [Display(Name = "事物")]
        public string Stuffy { get; set; }

        [Required(ErrorMessage = "请输入状况")]
        [Display(Name = "状况")]
        public string Action { get; set; }

        [Required(ErrorMessage = "请输入地点")]
        [Display(Name = "地点")]
        public string Place { get; set; }

        [Required(ErrorMessage = "请上传现场图片")]
        [Display(Name = "现场图片")]
        public string Image { get; set; }
 
    }
}
