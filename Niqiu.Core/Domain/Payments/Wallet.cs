using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Niqiu.Core.Domain.Payments
{
    public class Wallet:BaseEntity
    {
        [Display(Name = "余额")]
        public decimal Balance { get; set; }

        [Display(Name = "冻结的钱")]
        public decimal LockMoney { get; set; }

        [Display(Name = "系统名称")]
        public string SystemName { get; set; }

        [Display(Name = "所属用户")]
        [ForeignKey("UserId")]
        public virtual User.User User { get; set; }

        [Display(Name = "所属用户")]
        public virtual int UserId { get; set; }

        [Display(Name = "我的银行卡")]
        public virtual ICollection<BankCard> BankCards { get; set; }

        public bool Reduce(decimal money)
        {
            if (Balance < money) return false;
            Balance = Balance - money;
            return true;
        }

        public bool ReduceLockMoney(decimal money)
        {
            if (LockMoney < money) return false;
            LockMoney = LockMoney - money;
            return true;
        }

        public void Increase(decimal money)
        {
            Balance += money;
        }
    }
}
