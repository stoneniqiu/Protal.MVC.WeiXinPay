using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Niqiu.Core.Domain.Payments
{
   public static class WalletSystemNames
   {
       /// <summary>
       /// 系统总钱包，记录充值了多少钱
       /// 即所有用户充值进来的钱
       /// </summary>
       public static string SystemTotalWallet = "SystemTotalWallet";
       /// <summary>
       /// 系统消费钱包
       /// 也就是网站赚到的钱
       /// </summary>
       public static string SystemConsumeWallet = "SystemConsumeWallet";

   }
}
