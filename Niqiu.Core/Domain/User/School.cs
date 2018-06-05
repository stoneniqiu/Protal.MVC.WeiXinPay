using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Niqiu.Core.Domain.User
{
   public class School:BaseEntity
    {
       [Display(Name = "学院名称")]
       [Required(ErrorMessage = "请输入学院名称")]
       [StringLength(30, MinimumLength = 3, ErrorMessage = "不少于三个字")]
       public string Name { get; set; }
    }
}
