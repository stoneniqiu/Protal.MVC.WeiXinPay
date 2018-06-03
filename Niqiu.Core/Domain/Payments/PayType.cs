using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Niqiu.Core.Domain.Payments
{
    public enum PayType
    {
        [Display(Name = "余额支付")]
        Wallet,
        [Display(Name = "微信支付")]
        WeiXin,
        [Display(Name = "银行卡支付")]
        Bank,
        [Display(Name = "支付宝支付")]
        AliPay,
    }
}
