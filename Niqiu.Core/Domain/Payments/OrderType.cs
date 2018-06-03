using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Niqiu.Core.Domain.Payments
{
    public enum OrderType
    {
        [Display(Name = "充值金额")]
        Recharge,
        [Display(Name = "提现金额")]
        ToCash,
        [Display(Name = "购买提示")]
        QuestionStrategy,
        //发布问题后扣的钱
        [Display(Name = "悬赏金额")]
        Reward,
        [Display(Name = "提现服务费")]
        Fee,
        //谜题没有领取完的，到时间后退回用户账户
        [Display(Name = "悬赏退回")]
        RewardBack,
        [Display(Name = "获得悬赏")]
        GetReward,
        [Display(Name = "支付悬赏")]
        PayReward,
        [Display(Name = "谜题返现")]
        QuestionStrategyBack,
        All
    }
}
