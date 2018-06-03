using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Niqiu.Core.Domain.Questions;
using Niqiu.Core.Helpers;

namespace Niqiu.Core.Domain.Payments
{
    /// <summary>
    /// 用于充值和购买服务
    /// </summary>
    public class Order : BaseEntity
    {
        public Order()
        {
            Discount = 1;
            OrderNumber = Encrypt.GenerateOrderNumber();
        }

        public Order(string mchid)
        {
            OrderNumber = mchid + DateTime.Now.ToString("yyyyMMdd") + GetUniqueId().PadLeft(9, '0');// Encrypt.GenerateOrderNumber();
            
        }
        private string GetUniqueId()
        {
            //可以通过数据库来生成唯一的ID，这个唯一ID不要求全局唯一，只要求在一个自然日内唯一。
            //这里用随机数模拟。
            var r = new Random();
            return r.Next(1, 1000000).ToString();
        }

        [Display(Name = "订单号")]
        public string OrderNumber { get; set; }

        [Display(Name = "金额")]
        public decimal Amount { get; set; }
        [Display(Name = "类型")]
        public OrderType OrderType { get; set; }

        [Display(Name = "相关状态")]
        public int RelationId { get; set; }

        [ForeignKey("UserId")]
        public virtual User.User User { get; set; }

        [Required]
        [Display(Name = "用户")]
        public int UserId { get; set; }

        /// <summary>
        /// 用于支付悬赏
        /// </summary>
        [Display(Name = "悬赏用户")]
        public int FromUserId { get; set; }

        [Display(Name = "状态")]
        public OrderState OrderState { get; set; }

        [Display(Name = "折扣")]
        public double Discount { get; set; }

        [Display(Name = "原价")]
        public decimal RawPrice { get; set; }

        [Display(Name = "支付方式")]
        public PayType PayType { get; set; }

        //如果是购买策略或者是活的悬赏，需要提供问题id
        [Display(Name = "问题")]
        public int QuestionId { get; set; }

        public DateTime? PayTime { get; set; }

        [Display(Name = "时间戳")]
        [Timestamp]
        public byte[] RowVersion { get; set; }

        public string TradeNumber { get; set; }

        public bool Deleted { get; set; }

        [Display(Name = "是否已经返现")]
        //用于用户购买提示后给题主返现
        public bool IsPayBack { get; set; }

        [Display(Name = "购买策略的订单")]
        public int QuestionStrategyOrderId { get; set; }

        [NotMapped]
        public Order QuestionStrategyOrder { get; set; }

    }

    public enum OrderState
    {
        [Display(Name = "处理中")]
        Padding,
        [Display(Name = "已完成")]
        Success,
        [Display(Name = "已取消")]
        Canceled
    }
}
