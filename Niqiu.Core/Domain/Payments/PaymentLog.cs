using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Niqiu.Core.Domain.Payments
{
    public class PaymentLog : BaseEntity
    {
        [Display(Name = "金额")]
        [Required]
        public decimal Amount { get; set; }

        //从哪儿来
        [Display(Name = "支付方钱包Id")]
        public int FromWalletId { get; set; }

        [Display(Name = "支付方钱包")]
        //[ForeignKey("FromWalletId")]
        [NotMapped]
        public virtual Wallet FromWallet { get; set; }

        [Display(Name = "支付方支付前钱包金额")]
        public decimal FromBeforeAmount { get; set; }

        [Display(Name = "支付方支付后钱包金额")]
        public decimal FromAfterAmount { get; set; }


        //到哪儿去
        [Display(Name = "收款方钱包Id")]
        public int ToWalletId { get; set; }

     

        [Display(Name = "收款方钱包")]
        [NotMapped]
        //[ForeignKey("ToWalletId")]
        public virtual Wallet ToWallet { get; set; }

        [Display(Name = "收款方支付前钱包金额")]
        public decimal ToBeforeAmount { get; set; }

        [Display(Name = "收款方支付后钱包金额")]
        public decimal ToAfterAmount { get; set; }

        //如果不是钱包支付呢？
        public string FromWeiXinId { get; set; }
        public string ToWeiXinId { get; set; }


        [Display(Name = "订单类型")]
        public OrderType OrderType { get; set; }

        [Display(Name = "订单")]
        //[ForeignKey("OrderId")]
        [NotMapped]
        public virtual Order Order { get; set; }


        [Display(Name = "订单")]
        public int OrderId { get; set; }


        [Display(Name = "备注")]
        public string Remarks { get; set; }

        [Display(Name = "支付方式")]
        public PayType PayType { get; set; }

        public bool IsSuccess { get; set; }

        public bool Deleted { get; set; }

        [NotMapped]
        public User.User ToUser { get; set; }

        [NotMapped]
        public Questions.Question RelateQuestion { get; set; }

        public string PayedRemark(int walletId)
        {
            if (string.IsNullOrWhiteSpace(Remarks)) return string.Empty;

            if (OrderType == OrderType.PayReward && FromWalletId == walletId)
            {
                return Remarks.Replace("你获得来自", "支付");
            }
            return Remarks;
        }
    }
}
