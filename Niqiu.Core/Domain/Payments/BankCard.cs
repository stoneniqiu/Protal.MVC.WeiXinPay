using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Niqiu.Core.Domain.Payments
{
   public class BankCard:BaseEntity
    {
       [Display(Name = "银行名称")]
       public string BankName { get; set; }
       [Display(Name = "银行卡号")]
       public string CardNumber { get; set; }

       [Display(Name = "图片地址")]
       public string ImageUrl { get; set; }
    }
}
