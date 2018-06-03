using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Niqiu.Core.Domain;
using Niqiu.Core.Domain.Config;
using Niqiu.Core.Domain.Messages;
using Niqiu.Core.Domain.Payments;
using Niqiu.Core.Domain.Questions;

namespace Portal.MVC.Models.Services
{
    public class PayMeentDbService
    {
        // private static object o = new object();
        private PortalDb _db = new PortalDb();
        public Wallet CheckAndCreate(int userid)
        {
            var wallet = _db.Wallets.FirstOrDefault(n => n.UserId == userid);
            if (wallet == null)
            {
                using (var db = new PortalDb())
                {
                    wallet = new Wallet() { UserId = userid };
                    db.Wallets.Add(wallet);
                    db.SaveChanges();
                }
            }
            return _db.Wallets.FirstOrDefault(n => n.UserId == userid);

        }
        public PagedList<PaymentLog> GetUserPaymentLogs(int userwalletId, OrderType type = OrderType.All, DateTime? start = null, DateTime? end = null, int pageIndex = 0,
       int pageSize = 2147483647)
        {
            var query = _db.PaymentLogs.Where(n => !n.Deleted).Where(n => n.FromWalletId == userwalletId || n.ToWalletId == userwalletId);
            if (type != OrderType.All)
            {
                query = query.Where(n => n.OrderType == type);
            }
            if (start != null)
            {
                query = query.Where(n => n.CreateTime >= start);
            }
            if (end != null)
            {
                query = query.Where(n => n.CreateTime <= end);
            }
            query = query.OrderByDescending(n => n.Id);
            return new PagedList<PaymentLog>(query, pageIndex, pageSize);
        }

        public PagedList<PaymentLog> GetPaymentLogs(int fromwalletId = 0, int towalletId = 0, OrderType type = OrderType.All, DateTime? start = null, DateTime? end = null, int pageIndex = 0,
          int pageSize = 2147483647)
        {
            var query = _db.PaymentLogs.Where(n => !n.Deleted);
            if (fromwalletId != 0)
            {
                query = query.Where(n => n.FromWalletId == fromwalletId);
            }
            if (towalletId != 0)
            {
                query = query.Where(n => n.ToWalletId == towalletId);
            }
            if (type != OrderType.All)
            {
                query = query.Where(n => n.OrderType == type);
            }
            if (start != null)
            {
                query = query.Where(n => n.CreateTime >= start);
            }
            if (end != null)
            {
                query = query.Where(n => n.CreateTime <= end);
            }
            query = query.OrderBy(n => n.Id);
            return new PagedList<PaymentLog>(query, pageIndex, pageSize);
        }
        public IEnumerable<Order> GetQuestionStrategyOrder(int userId, int questionId, int strategyId)
        {

            return
                _db.Orders.Where(
                    n => n.QuestionId == questionId && n.UserId == userId && n.RelationId == strategyId);
        }
        public Order GetOrderByOrderNumber(string orderNumber)
        {

            return _db.Orders.FirstOrDefault(n => n.OrderNumber == orderNumber);
        }

        public Order CreateRecharegOrder(int userid, decimal money)
        {
            var order = CreateOrder(userid, OrderType.Recharge, money, PayType.WeiXin);
            return order;

        }


        public Order CreateRewardOrder(int userid, int fromuserId, decimal money,int questionId)
        {
            var order = CreateOrder(userid, OrderType.PayReward, money, PayType.Wallet, fromuserId,money,null,questionId);
            return order;
        }
        public void InsertMessage(Message message)
        {

            if (message != null && !string.IsNullOrEmpty(message.Content))
            {
                using (var db = new PortalDb())
                {
                    db.Messages.Add(message);
                    db.SaveChanges();
                }

            }
        }


        public Order CreateOrder(int userid, OrderType orderType, decimal money, PayType payType, int fromuserId = 0,
            decimal? rawprice = 0, QuestionStrategy strategy = null, int questionId = 0)
        {
            var order = new Order()
            {
                UserId = userid,
                OrderType = orderType,
                Amount = money,
                PayType = payType,
                RawPrice = rawprice != null ? rawprice.Value : money,
                FromUserId = fromuserId,
                QuestionId = questionId
            };
            if (orderType == OrderType.Reward)
            {
                order.FromUserId = fromuserId;
            }
            if (orderType == OrderType.QuestionStrategy)
            {
                if (strategy == null) return null;
                order.RelationId = strategy.Id;
                order.QuestionId = questionId;
            }
            InsertOrder(order);
            return GetOrderByOrderNumber(order.OrderNumber);
        }

        public void InsertOrder(Order order)
        {
            using (var db = new PortalDb())
            {
                db.Orders.Add(order);
                db.SaveChanges();
            }
        }

        public void UpdateWallet(Wallet wallet)
        {
            using (var db = new PortalDb())
            {
                var model = db.Wallets.Find(wallet.Id);
                model.Balance = wallet.Balance;
                model.LockMoney = wallet.LockMoney;
                model.ModifyTime = DateTime.Now;
                db.SaveChanges();
            }
        }
        public Wallet GetByUserId(int userid)
        {

            return _db.Wallets.FirstOrDefault(n => n.UserId == userid);
        }
        public bool LockMoney(int userid, decimal money)
        {

            var model = GetByUserId(userid);
            if (model == null) throw new PortalException("用户钱包不存在");

            if (model.Balance < money) return false;

            model.Balance = model.Balance - money;
            model.LockMoney += money;
            UpdateWallet(model);
            return true;


        }
        public bool UnLockMoney(int userid, decimal money)
        {
            var model = GetByUserId(userid);
            if (model == null) throw new PortalException("用户钱包不存在");

            if (model.LockMoney < money) return false;
            model.LockMoney -= money;
            model.Balance = model.Balance + money;
            UpdateWallet(model);
            return true;
        }
        private void UpdateOrderAfterPay(Order order)
        {

            using (var db = new PortalDb())
            {
                var model = db.Orders.Find(order.Id);
                model.OrderState = OrderState.Success;
                model.PayTime = DateTime.Now;
                if (model.OrderType == OrderType.QuestionStrategyBack)
                {
                    //找到父订单
                    var parent = db.Orders.Find(model.QuestionStrategyOrderId);
                    if (parent != null)
                    {
                        parent.IsPayBack = true;
                        parent.QuestionStrategyOrderId = model.Id;
                    }

                }
                db.SaveChanges();
            }

        }
        private void InsertPayLog(Wallet from, decimal frombefore, Wallet to, decimal tobefore, Order order, bool istrue = true, string msg = "")
        {

            using (var db = new PortalDb())
            {
                var paylog = new PaymentLog
                {
                    Amount = order.Amount,
                    FromWalletId = from.Id,
                    FromBeforeAmount = frombefore,
                    FromAfterAmount = from.Balance,
                    ToWalletId = to.Id,
                    ToBeforeAmount = tobefore,
                    ToAfterAmount = to.Balance,

                    OrderId = order.Id,
                    OrderType = order.OrderType,
                    PayType = order.PayType,

                    Remarks = msg,
                    IsSuccess = istrue

                };
                db.PaymentLogs.Add(paylog);
                db.SaveChanges();
            }


        }

        private void InsertRechargeLog(Wallet to, decimal tobefore, Order order, bool istrue = true, string msg = "")
        {

            using (var db = new PortalDb())
            {
                var paylog = new PaymentLog
                {
                    Amount = order.Amount,
                    ToWalletId = to.Id,
                    ToBeforeAmount = tobefore,
                    ToAfterAmount = to.Balance,
                    OrderId = order.Id,
                    OrderType = order.OrderType,
                    PayType = order.PayType,
                    Remarks = msg,
                    IsSuccess = istrue,
                    //bug 还未配置appid
                    FromWeiXinId = PortalConfig.SystemWeiXinAppId,
                };
                db.PaymentLogs.Add(paylog);
                db.SaveChanges();
            }

        }

        public PaymentResult Payment(Order order, string remark = "")
        {

            using (var db = new PortalDb())
            {
                //do it
                var result = paymentResult();
                order = db.Orders.Find(order.Id);
                if (order == null) return result;
                if (order.OrderState == OrderState.Success) return result;

                var totalWallet = db.Wallets.FirstOrDefault(n => n.SystemName == WalletSystemNames.SystemTotalWallet);
                if (totalWallet == null) return paymentResult("系统总账户不存在");
                var consumeWallet =
                    db.Wallets.FirstOrDefault(n => n.SystemName == WalletSystemNames.SystemConsumeWallet);
                if (consumeWallet == null) return paymentResult("系统消费账户不存在");
                var userWallert = CheckAndCreate(order.UserId);
                if (userWallert == null) return paymentResult("用户钱包不存在");

                var money = order.Amount;
                if (money < 0) return paymentResult("金额不能小于0");
                //bug 如果出现服务器异常，如何回滚呢？
                var userbefore = userWallert.Balance;
                var consumebefore = consumeWallet.Balance;

                switch (order.OrderType)
                {
                    case OrderType.QuestionStrategy:
                        //购买策略，将金额划到系统消费账户，并创建日志
                        //扣除用户钱包的钱
                        if (userWallert.Balance < money) return paymentResult("用户钱包余额不足！");

                        userWallert.Reduce(money);
                        consumeWallet.Increase(money);
                        try
                        {
                            UpdateWallet(userWallert);
                            UpdateWallet(consumeWallet);
                            UpdateOrderAfterPay(order);
                            InsertPayLog(userWallert, userbefore, consumeWallet, consumebefore, order, true, remark);

                            return new PaymentResult(true);
                        }
                        catch (Exception e)
                        {
                            InsertPayLog(userWallert, userbefore, consumeWallet, consumebefore, order, false,
                                "支付失败：" + e.Message);
                            return paymentResult(e.Message);
                        }

                    case OrderType.Recharge:
                        //用户充值，将金额划到系统总账户，并创建日志
                        //执行充值，比如调用微信接口.....
                        userWallert.Increase(money);
                        totalWallet.Increase(money);
                        try
                        {
                            UpdateWallet(userWallert);
                            UpdateWallet(totalWallet);
                            UpdateOrderAfterPay(order);
                            InsertRechargeLog(userWallert, userbefore, order);
                            return new PaymentResult(true);
                        }
                        catch (Exception e)
                        {
                            InsertRechargeLog(userWallert, userbefore, order, false, e.Message);
                            return paymentResult(e.Message);
                        }
                    //用户提现，将总账户金额划到用户的微信零钱，并创建日志
                    case OrderType.ToCash:
                        if (userWallert.Balance < money) return paymentResult("用户钱包余额不足！");
                        userWallert.Reduce(money);
                        totalWallet.Reduce(money);
                        //用户账户减少// 系统总账户也减少
                        try
                        {
                            UpdateWallet(userWallert);
                            UpdateWallet(totalWallet);
                            UpdateOrderAfterPay(order);
                            InsertPayLog(userWallert, userbefore, consumeWallet, consumebefore, order);
                            return new PaymentResult(true);
                        }
                        catch (Exception e)
                        {
                            InsertPayLog(userWallert, userbefore, consumeWallet, consumebefore, order, false,
                                "支付失败：" + e.Message);
                            return paymentResult(e.Message);
                        }
                        break;
                    //提现服务费
                    case OrderType.Fee:
                        if (userWallert.Balance < money) return paymentResult("用户钱包余额不足！");
                        userWallert.Reduce(money);
                        consumeWallet.Increase(money);
                        //用户账户减少// 系统账户不变
                        try
                        {
                            UpdateWallet(userWallert);
                            UpdateWallet(consumeWallet);
                            UpdateOrderAfterPay(order);
                            InsertPayLog(userWallert, userbefore, consumeWallet, consumebefore, order);
                            return new PaymentResult(true);
                        }
                        catch (Exception e)
                        {
                            InsertPayLog(userWallert, userbefore, consumeWallet, consumebefore, order, false,
                                "支付失败：" + e.Message);
                            return paymentResult(e.Message);
                        }
                        break;
                    case OrderType.QuestionStrategyBack:
                        if (consumeWallet.Balance < money) paymentResult("系统钱包余额不足！");
                        consumeWallet.Reduce(money);
                        userWallert.Increase(money);
                          try
                        {
                            UpdateWallet(userWallert);
                            UpdateWallet(consumeWallet);
                            UpdateOrderAfterPay(order);//QuestionStrategyOrderId
                            //还需要update上个订单。

                            
                            InsertPayLog(userWallert, userbefore, consumeWallet, consumebefore, order,true,remark);
                            return new PaymentResult(true);
                        }
                        catch (Exception e)
                        {
                            InsertPayLog(userWallert, userbefore, consumeWallet, consumebefore, order, false,
                                "支付失败：" + e.Message);
                            return paymentResult(e.Message);
                        }
                        break;
                        break;

                    //case OrderType.Reward:
                    //case OrderType.GetReward:
                    case OrderType.PayReward:
                        //获得悬赏，将钱从提问用户的钱包划到获得悬赏的用户，并创建日志
                        var fromuserWallet = db.Wallets.FirstOrDefault(n => n.UserId == order.FromUserId);
                        if (fromuserWallet == null) return paymentResult("悬赏用户钱包不存在");
                        if (fromuserWallet.LockMoney < money) return paymentResult("悬赏用户钱冻结金额不够支付");
                        //开始支付 悬赏方扣款
                        var fuserbefore = fromuserWallet.LockMoney;
                        fromuserWallet.ReduceLockMoney(money);
                        // 获得悬赏方 加钱
                        userWallert.Increase(money);
                        try
                        {
                            UpdateWallet(userWallert);
                            UpdateWallet(fromuserWallet);
                            UpdateOrderAfterPay(order);
                            InsertPayLog(fromuserWallet, fuserbefore, userWallert, userbefore, order, true, remark);
                            return new PaymentResult(true);
                        }
                        catch (Exception e)
                        {
                            InsertPayLog(fromuserWallet, fuserbefore, userWallert, userbefore, order, false,
                                e.Message);
                            return paymentResult(e.Message);
                        }
                    default:
                        //其他的还没实现
                        return paymentResult("其他订单类型还未实现");
                }
            }


        }
        public IEnumerable<Order> GetOrders(int userid, int questionid)
        {
            return _db.Orders.Where(n => n.UserId == userid && n.QuestionId == questionid);
        }

        public Order CreateStrategyOrder(int userid, QuestionStrategy strategy, int questionId,
            PayType payType = PayType.Wallet)
        {
            return CreateOrder(userid, OrderType.QuestionStrategy, strategy.Price, payType, 0, null, strategy,
                questionId);

        }

        private static PaymentResult paymentResult(string error = "")
        {
            return new PaymentResult(error);
        }
        public void UpdateOrder(Order order)
        {
            using (var db = new PortalDb())
            {
                if (order == null)
                    throw new ArgumentNullException("order");

                var raw = db.Orders.Find(order.Id);
                raw.OrderNumber = order.OrderNumber;
                raw.Amount = order.Amount;
                raw.RelationId = order.RelationId;
                raw.FromUserId = order.FromUserId;
                raw.OrderState = order.OrderState;
                raw.Discount = order.Discount;
                raw.RawPrice = order.RawPrice;
                raw.PayType = order.PayType;
                raw.QuestionId = order.QuestionId;
                raw.PayTime = order.PayTime;
                raw.RowVersion = order.RowVersion;
                raw.TradeNumber = order.TradeNumber;
                db.SaveChanges();
            }

        }
        public Order CreateRechargeOrder(int userid, decimal money, PayType payType = PayType.WeiXin)
        {
            return CreateOrder(userid, OrderType.Recharge, money, payType);
        }

        public decimal GetBalance(int userid)
        {
            var wall = CheckAndCreate(userid);
            return wall.Balance;
        }
        public Order GetOrderById(int id)
        {
            using (var db = new PortalDb())
            {
                return db.Orders.Find(id);
            }
        }
        public Order CreateToCashOrder(int userid, decimal money, PayType payType = PayType.WeiXin)
        {
            return CreateOrder(userid, OrderType.ToCash, money, payType);
        }
        public Order CreateToCashFeeOrder(int userid, decimal money, PayType payType = PayType.WeiXin)
        {
            return CreateOrder(userid, OrderType.Fee, money, payType);
        }

    }
}