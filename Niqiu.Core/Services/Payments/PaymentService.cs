using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;
using Niqiu.Core.Domain;
using Niqiu.Core.Domain.Config;
using Niqiu.Core.Domain.Payments;
using Niqiu.Core.Domain.Questions;
using Niqiu.Core.Domain.User;
using NPOI.HSSF.Record.Formula.Functions;

namespace Niqiu.Core.Services.Payments
{
    public class PaymentService : IPaymentService
    {
        private readonly IRepository<Wallet> _walletRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<PaymentLog> _paylogRepository;
        private readonly IRepository<User> _userRepository; 
        public PaymentService(IRepository<User> userRepository,IRepository<Wallet> walletRepository, IRepository<Order> orderRepository,
            IRepository<PaymentLog> paylogRepository
             )
        {
            _walletRepository = walletRepository;
            _orderRepository = orderRepository;
            _userRepository = userRepository;
            _paylogRepository = paylogRepository;
        }

        public Wallet GetByUserId(int userid)
        {
            return _walletRepository.Table.FirstOrDefault(n => n.UserId == userid);
        }

        public decimal GetBalance(int userid)
        {
            var wall = CheckAndCreate(userid);
            return wall.Balance;
        }
        public Wallet GetBySystemName(string systemName)
        {
            return _walletRepository.Table.FirstOrDefault(n => n.SystemName == systemName);
        }
        public void InsertWallet(Wallet wallet)
        {
            if (wallet == null) throw new ArgumentNullException("wallet");
            _walletRepository.Insert(wallet);
        }

        public void UpdateWallet(Wallet wallet)
        {
            if (wallet == null)
                throw new ArgumentNullException("wallet");
            wallet.ModifyTime = DateTime.Now;
            _walletRepository.Update(wallet);
        }

        public User GetWalletUser(int walletId)
        {
            var wallet = _walletRepository.GetById(walletId);
            return _userRepository.GetById(wallet.UserId);
        }


        public Wallet CheckAndCreate(int userid)
        {
            var model = GetByUserId(userid);
            if (model == null)
            {
                model = new Wallet() { UserId = userid };
                InsertWallet(model);
            }
            return GetByUserId(userid);
        }

      

        /// <summary>
        /// 用户悬赏的钱，暂时没用被扣掉
        /// 这里应该有个明细的，锁住哪些钱
        /// 不然不好核算
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="money"></param>
        /// <returns></returns>
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


        public void InsertOrder(Order order)
        {
            if (order == null) throw new ArgumentNullException("order");
            _orderRepository.Insert(order);
        }

        public Order GetOrderById(int id)
        {
            return _orderRepository.GetById(id);
        }

        public IEnumerable<Order> GetOrders(int userid, int questionid)
        {
            return _orderRepository.Table.Where(n => n.UserId == userid && n.QuestionId == questionid);
        }



        public IEnumerable<Order> GetOrders(int userid)
        {
            return _orderRepository.Table.Where(n => n.UserId == userid);
        }
        public Order CreateOrder(int userid,OrderType orderType, decimal money, PayType payType, int fromuserId = 0, decimal? rawprice = 0, QuestionStrategy strategy = null, int questionId = 0)
        {
            var order = new Order()
            {
                UserId = userid,
                OrderType = orderType,
                Amount = money,
                PayType = payType,
                RawPrice = rawprice != null ? rawprice.Value : money,
                FromUserId = fromuserId
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
        public IEnumerable<Order> GetQuestionStrategyOrder(int userId, int questionId,int strategyId)
        {
            return
                _orderRepository.Table.Where(
                    n => n.QuestionId == questionId && n.UserId == userId && n.RelationId == strategyId);
        }
        public void UpdateOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");
            _orderRepository.Update(order);
        }

        [Obsolete("有并发问题，使用静态方法")]
        public PaymentResult Payment(Order order,string remark="")
        {
            //do it
            var result = paymentResult();
            order = _orderRepository.GetById(order.Id);
            if (order == null) return result;
            if (order.OrderState == OrderState.Success) return result;

            var totalWallet = GetBySystemName(WalletSystemNames.SystemTotalWallet);
            if (totalWallet == null) return paymentResult("系统总账户不存在");
            var consumeWallet = GetBySystemName(WalletSystemNames.SystemConsumeWallet);
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
                        InsertPayLog(userWallert, userbefore, consumeWallet, consumebefore, order,true,remark);

                        return new PaymentResult(true);
                    }
                    catch (Exception e)
                    {
                        InsertPayLog(userWallert, userbefore, consumeWallet, consumebefore, order, false, "支付失败：" + e.Message);
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
                        InsertPayLog(userWallert, userbefore, consumeWallet, consumebefore, order, false, "支付失败：" + e.Message);
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
                        InsertPayLog(userWallert, userbefore, consumeWallet, consumebefore, order, false, "支付失败：" + e.Message);
                        return paymentResult(e.Message);
                    }
                    break;
                //case OrderType.Reward:
                //case OrderType.GetReward:
                case OrderType.PayReward:
                    //获得悬赏，将钱从提问用户的钱包划到获得悬赏的用户，并创建日志
                    var fromuserWallet = GetByUserId(order.FromUserId);
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
                        InsertPayLog(fromuserWallet, fuserbefore, userWallert, userbefore, order,true,remark);
                        return new PaymentResult(true);
                    }
                    catch (Exception e)
                    {
                        InsertPayLog(fromuserWallet, fuserbefore, userWallert, userbefore, order,false,e.Message);
                        return paymentResult(e.Message);
                    }
                default:
                    //其他的还没实现
                    return paymentResult("其他订单类型还未实现");
            }
            return result;
        }

        private PaymentResult paymentResult(string error = "")
        {
            return new PaymentResult(error);
        }
        private void UpdateOrderAfterPay(Order order)
        {
            order.OrderState = OrderState.Success;
            order.PayTime = DateTime.Now;
            _orderRepository.Update(order);
        }

        public void DeletePaymentLog(int id)
        {
            var log = _paylogRepository.GetById(id);
            if (log != null)
            {
                log.Deleted = true;
                _paylogRepository.Update(log);
            }
        }

        public void DeleteOrder(int id)
        {
            var log = _orderRepository.GetById(id);
            if (log != null)
            {
                log.Deleted = true;
                _orderRepository.Update(log);
            }
        }

        public void RemovePaymentLog(int id)
        {
            var log = _paylogRepository.GetById(id);
            if (log != null)
            {
                _paylogRepository.Delete(log);
            }
        }

        private void InsertPayLog(Wallet from, decimal frombefore, Wallet to, decimal tobefore, Order order, bool istrue = true, string msg = "")
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
            InsertPaymentLog(paylog);
        }

        private void InsertRechargeLog(Wallet to, decimal tobefore, Order order, bool istrue = true, string msg = "")
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
            InsertPaymentLog(paylog);
        }
        public void InsertPaymentLog(PaymentLog log)
        {
            if (log == null) throw new ArgumentNullException("log");
            _paylogRepository.Insert(log);
        }

        public PagedList<PaymentLog> GetPaymentLogs(int fromwalletId = 0, int towalletId = 0, OrderType type = OrderType.All, DateTime? start = null, DateTime? end = null, int pageIndex = 0,
            int pageSize = 2147483647)
        {
            var query = _paylogRepository.Table.Where(n=>!n.Deleted);
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
                query = query.Where(n => n.CreateTime >=start);
            }
            if (end != null)
            {
                query = query.Where(n => n.CreateTime <= end);
            }
            query = query.OrderBy(n => n.Id);
            return new PagedList<PaymentLog>(query, pageIndex, pageSize);
        }

        public PagedList<PaymentLog> GetUserPaymentLogs(int userwalletId, OrderType type = OrderType.All, DateTime? start = null, DateTime? end = null, int pageIndex = 0,
         int pageSize = 2147483647)
        {
            var query = _paylogRepository.Table.Where(n => !n.Deleted).Where(n => n.FromWalletId == userwalletId || n.ToWalletId == userwalletId);
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

        public Order GetOrderByOrderNumber(string orderNumber)
        {
            return _orderRepository.Table.FirstOrDefault(n => n.OrderNumber == orderNumber);
        }
        public Order CreateRewardOrder(int userid, int fromuserId, decimal money)
        {
            return CreateOrder(userid, OrderType.PayReward, money, PayType.Wallet, fromuserId);
        }
        public Order CreateRechargeOrder(int userid, decimal money, PayType payType = PayType.WeiXin)
        {
            return  CreateOrder(userid, OrderType.Recharge, money, payType);
        }
        public Order CreateToCashOrder(int userid, decimal money, PayType payType = PayType.WeiXin)
        {
            return CreateOrder(userid, OrderType.ToCash, money, payType);
        }
        public Order CreateToCashFeeOrder(int userid, decimal money, PayType payType = PayType.WeiXin)
        {
            return CreateOrder(userid, OrderType.Fee, money, payType);
        }
        public Order CreateStrategyOrder(int userid, QuestionStrategy strategy, int questionId, PayType payType = PayType.Wallet)
        {
            return  CreateOrder(userid, OrderType.QuestionStrategy, strategy.Price, payType, 0, null, strategy, questionId);
        }

       
    }
}
