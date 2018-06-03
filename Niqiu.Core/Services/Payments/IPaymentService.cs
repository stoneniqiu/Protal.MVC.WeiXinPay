using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Niqiu.Core.Domain;
using Niqiu.Core.Domain.Payments;
using Niqiu.Core.Domain.Questions;
using Niqiu.Core.Domain.User;

namespace Niqiu.Core.Services.Payments
{
   public interface IPaymentService
   {
       Wallet GetByUserId(int userid);
       Wallet GetBySystemName(string systemName);
       Wallet CheckAndCreate(int userid);


       decimal GetBalance(int userid);
       void InsertWallet(Wallet wallet);

       PagedList<PaymentLog> GetUserPaymentLogs(int userwalletId, OrderType type = OrderType.All, DateTime? start = null,
           DateTime? end = null, int pageIndex = 0,
           int pageSize = 2147483647);

       PagedList<PaymentLog> GetPaymentLogs(int fromwalletId = 0, int towalletId = 0, OrderType type = OrderType.All,
           DateTime? start = null, DateTime? end = null, int pageIndex = 0,
           int pageSize = 2147483647);

       /// <summary>
       /// 用户悬赏的钱，暂时没用被扣掉
       /// 这里应该有个明细的，锁住哪些钱
       /// 不然不好核算
       /// </summary>
       /// <param name="userid"></param>
       /// <param name="money"></param>
       /// <returns></returns>
       bool LockMoney(int userid, decimal money);

       bool UnLockMoney(int userid, decimal money);
       void UpdateWallet(Wallet wallet);
       User GetWalletUser(int walletId);
       PaymentResult Payment(Order order, string remark = "");

       void InsertOrder(Order order);
       void UpdateOrder(Order order);

       Order CreateOrder(int userid, OrderType orderType, decimal money, PayType payType, int fromuserId = 0,
           decimal? rawprice = 0, QuestionStrategy strategy = null, int questionId = 0);

       Order CreateStrategyOrder(int userid, QuestionStrategy strategy, int questionId, PayType payType = PayType.Wallet);

       IEnumerable<Order> GetQuestionStrategyOrder(int userId, int questionId, int strategyId);

       Order CreateRechargeOrder(int userid, decimal money, PayType payType = PayType.WeiXin);
       Order CreateRewardOrder(int userid, int fromuserId, decimal money);

       Order GetOrderById(int id);
       IEnumerable<Order> GetOrders(int userid, int questionid);
       IEnumerable<Order> GetOrders(int userid);
       Order GetOrderByOrderNumber(string orderNumber);
       Order CreateToCashFeeOrder(int userid, decimal money, PayType payType = PayType.WeiXin);
       Order CreateToCashOrder(int userid, decimal money, PayType payType = PayType.WeiXin);
       void DeletePaymentLog(int id);
       void DeleteOrder(int id);
      void InsertPaymentLog(PaymentLog log);
   }
}
