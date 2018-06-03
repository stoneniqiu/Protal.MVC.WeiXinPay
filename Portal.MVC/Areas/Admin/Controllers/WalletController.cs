using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Niqiu.Core.Domain.Common;
using Niqiu.Core.Domain.Payments;
using Niqiu.Core.Domain.Questions;
using Niqiu.Core.Domain.User;
using Niqiu.Core.Services;
using Niqiu.Core.Services.Payments;
using Niqiu.Core.Services.Questiones;
using Portal.MVC.Areas.Admin.Models;
using Portal.MVC.Models;
using Portal.MVC.Models.Services;
using Portal.MVC.ViewModel;

namespace Portal.MVC.Areas.Admin.Controllers
{
    public class WalletController : AdminBaseController
    {
        //
        // GET: /Admin/Wallet/
        private readonly IPaymentService _paymentService;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<Wallet> _walletRepository;
        private readonly IQuestionService _questionService;

        public WalletController(IRepository<Wallet> walletRepository, IQuestionService userService, IPaymentService paymentService, IRepository<Order> paylogRepository)
        {
            _paymentService = paymentService;
            _orderRepository = paylogRepository;
            _walletRepository = walletRepository;
            _questionService = userService;
        }

        public ActionResult List()
        {
            var lists = _walletRepository.Table.Where(n => n.Id > 2);
            return View(lists);
        }
        private PayMeentDbService _payMeentDbService=new PayMeentDbService();

        public ActionResult ToCashList()
        {
           // var list = _paymentService.GetPaymentLogs(0, 0, OrderType.ToCash);
            var orders = _orderRepository.Table.Where(n => n.OrderType == OrderType.ToCash&&n.OrderState==OrderState.Success);
            return View(orders);
        }

        public ActionResult RechargeList()
        {
            var list = _paymentService.GetPaymentLogs(0, 0, OrderType.Recharge);
            foreach (var paymentLog in list)
            {
                paymentLog.ToUser =_paymentService.GetWalletUser(paymentLog.ToWalletId);
            }
            return View(list);
        }

        //购买提示返现处理
        public ActionResult RewardBack()
        {
            var list = new List<OrderQuestion>();
            using (var db=new PortalDb())
            {
                var strategyOrders =
                    db.Orders.Where(n => !n.Deleted && n.OrderType == OrderType.QuestionStrategy).ToList();
                foreach (var order in strategyOrders)
                {
                    var str = db.QuestionStrategies.Find(order.RelationId);
                    var qus = db.Questions.Find(order.QuestionId);
                    var user = db.Users.Find(order.UserId);
                    if (str != null && qus != null)
                    {
                        list.Add(new OrderQuestion()
                        {
                            Order = order,
                            Question = qus,
                            QuestionStrategy = str,
                            User=user
                        });
                    }

                }
                return View(list);
            }
        }

        private Order CreateRewardBackOrder(decimal money,int orderId,int toUserId)
        {
            using (var db = new PortalDb())
            {
                var consumeWallet =
                    db.Wallets.FirstOrDefault(n => n.SystemName == WalletSystemNames.SystemConsumeWallet);
                var rawOrder = db.Orders.Find(orderId);
                var order = new Order()
                {
                    UserId = toUserId,
                    OrderType = OrderType.QuestionStrategyBack,
                    Amount = money,
                    PayType = PayType.Wallet,
                    RawPrice = money,
                    FromUserId = consumeWallet.UserId,
                    QuestionStrategyOrderId = orderId,
                    RelationId = rawOrder.RelationId,
                    QuestionId = rawOrder.QuestionId
                   
                };
                db.Orders.Add(order);
                db.SaveChanges();
                return db.Orders.FirstOrDefault(n => n.OrderNumber == order.OrderNumber);
            }
        }

        public ActionResult PayRewardBackAction(int questionId,int orderId)
        {
            //谜题检测
            var question = _questionService.GetById(questionId);
            if (question == null) return Json(new PortalResult("谜题不存在"));

            var raworder = _paymentService.GetOrderById(orderId);
            if (raworder == null) return Json(new PortalResult("订单不存在!"));

            if (raworder.IsPayBack) return Json(new PortalResult("订单已经返现了!"));

            //谜题是否已经结束
            if(!question.IsFinished)
                 return Json(new PortalResult("谜题还未结束!"));

            if (question.IsIllegal)
                return Json(new PortalResult("举报谜题不能返现!"));

            if (raworder.Amount < (decimal)0.02) return Json(new PortalResult("订单金额太小,不能参与返现!"));
            var money = decimal.Round(raworder.Amount/2, 2);
            var toUserId = question.UserId;


            //是否没有举报
            var order= CreateRewardBackOrder(money, orderId, toUserId);
            var remark = string.Format("用户购买你的谜题{0}提示，系统返现{1}元到您的账户", question.Title,money);
            var res = _payMeentDbService.Payment(order, remark);
            return Json(res);
        }



        public ActionResult RewardList()
        {

            var list = new List<RewardLog>();
            var db = new PortalDb();
                var rewardOrders = db.Orders.Where(n => !n.Deleted && n.OrderType == OrderType.PayReward&& n.OrderState == OrderState.Success).ToList();
                foreach (var o in rewardOrders)
                {
                    var question = db.Questions.FirstOrDefault(n=>n.Id==o.QuestionId);
                    if (question != null&&list.All(n => n.Question.Id != question.Id))
                    {
                        var res = db.RewardUsers.Where(n => n.QuestionId == question.Id).ToList();
                        var users = res.Select(rewardUser => db.Users.Find(rewardUser.UserId)).ToList();
                        question.RewardUsers = users;

                        var raw = new RewardLog()
                        {
                            Question = question,
                            List = question.GetReward()
                        };
                        list.Add(raw);
                    }
            }
          
            return View(list);
        }

        public ActionResult OrderList()
        {
            var orders = _orderRepository.Table.Where(n => n.OrderState == OrderState.Success);
            return View(orders);
        }

        public ActionResult Delete(int id)
        {
            _paymentService.DeletePaymentLog(id);
            return Json(1);
        }
        public ActionResult DeleteOrder(int id)
        {
            _paymentService.DeleteOrder(id);
            return Json(1);
        }
    }
}
