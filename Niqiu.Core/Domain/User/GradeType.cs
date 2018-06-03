using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Niqiu.Core.Domain.User
{
    public enum GradeType
    {
        [Display(Name = "普通")]
        Ordinary,
        [Display(Name = "vip")]
        Vip,
        [Display(Name = "白银vip")]
        Silver,
        [Display(Name = "铂金vip")]
        Gold,
    }
}
