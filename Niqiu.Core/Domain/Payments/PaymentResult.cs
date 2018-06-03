using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Niqiu.Core.Domain.Payments
{
    public class PaymentResult
    {
        public PaymentResult()
        {
            IsSuccess = false;
        }
        public PaymentResult(bool isSuccess)
        {
            IsSuccess = isSuccess;
            Error = "支付成功";
        }
        public PaymentResult(string msg)
        {
            IsSuccess = false;
            Error = msg;
        }
        public decimal Money { get; set; }
        public bool IsSuccess { get; set; }
        public string Error { get; set; }
        public string OrderId { get; set; }
    }
}
