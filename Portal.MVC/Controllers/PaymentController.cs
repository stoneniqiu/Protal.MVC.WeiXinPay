using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Niqiu.Core.Domain.Common;
using Niqiu.Core.Domain.Messages;
using Niqiu.Core.Domain.Payments;
using Niqiu.Core.Domain.Questions;
using Niqiu.Core.Domain.User;
using Niqiu.Core.Services;
using Portal.MVC.Attributes;
using Portal.MVC.Models;
using Portal.MVC.Models.Services;

namespace Portal.MVC.Controllers
{
    [MenuStatistics]
    public class PaymentController : Controller
    {
        private readonly IWorkContext _workContext;
        public PaymentController(IWorkContext workContext)
        {
            _workContext = workContext;
        }

        private PayMeentDbService _payMeentDbService = new PayMeentDbService();
        private QuestionDbService _questionDbService=new QuestionDbService();
        private UserDbService _userDbService=new UserDbService();
        private MessageDbService _messageDbService =new MessageDbService();



        [LoginValid]
        public ActionResult Recharge()
        {

            return View();
        }

        /// <summary>
        /// 购买策略
        /// </summary>
        /// <param name="strategyId"></param>
        /// <param name="questionId"></param>
        /// <param name="money"></param>
        /// <returns></returns>
        [LoginValid]
        public ActionResult BuyQuestionStrategy(int strategyId, int questionId, decimal money)
        {
            var user = _workContext.CurrentUser;
            var strategy = _questionDbService.GetStrategyById(strategyId);
            if (strategy == null)
            {
                return Json(new { isSuccess = false, message = "购买对象不存在" }, JsonRequestBehavior.AllowGet);
            }
            strategy.Price = money;
            var question = _questionDbService.GetById(questionId);
            if (question == null)
            {
                return Json(new { isSuccess = false, message = "谜题不存在" }, JsonRequestBehavior.AllowGet);
            }
            //是自己的问题就无需购买
            if (question.UserId == user.Id)
            {
                return Json(new { isSuccess = false, message = "自己的问题，无需购买" }, JsonRequestBehavior.AllowGet);
            }

            #region 订单检查
            Order order = _payMeentDbService.GetOrders(user.Id, questionId).FirstOrDefault(n => n.RelationId == strategyId);
            if (order == null)
            {
                order = _payMeentDbService.CreateStrategyOrder(user.Id, strategy, questionId);

            }
            //已经存在订单
            else
            {
                //检查类型
                if (order.OrderState == OrderState.Success)
                {
                    //需要检查策略类型
                    if (strategy.SystemName == SystemQuestionStrategyName.Answer)
                    {
                        //答案买一次就行了
                        return Json(new { isSuccess = true, message = "已支付成功", order = order.OrderNumber }, JsonRequestBehavior.AllowGet);
                    }
                    //如果是其他类型,钱多你就买吧。
                    order = _payMeentDbService.CreateStrategyOrder(user.Id, strategy, questionId);
                }
                else
                {
                    //取消了就重新开启
                    order.OrderState = OrderState.Padding;
                    order.ModifyTime = DateTime.Now;
                    _payMeentDbService.UpdateOrder(order);
                }
            }
            //就直接跳转
            if (order == null)
            {
                return Json(new { isSuccess = false, message = "订单创建失败" }, JsonRequestBehavior.AllowGet);
            }
            #endregion
            //支付
            var res = _payMeentDbService.Payment(order, string.Format("谜题：{0}，类型：{1}", question.Title, strategy.Name));
            if (res.IsSuccess)
            {
                //让用户看到答案？
                Logger.DebugAsync(string.Format("用户{0}成功支付了订单{1}", user.Id, order.OrderNumber));

                //插入一个提示消息
                var sys = _userDbService.GetUserBySystemName(SystemUserNames.SystemWallet);

                var content = "";
                if (strategy.SystemName == SystemQuestionStrategyName.Answer)
                {
                    content = "“" + question.Title + "”完整答案:" + question.Answer;
                }
                if (strategy.SystemName == SystemQuestionStrategyName.WordNum)
                {
                    content = "“" + question.Title + "”字数提示:" + question.Answer.Length + "个字";
                }
                if (strategy.SystemName == SystemQuestionStrategyName.KeyWord)
                {
                    var key = "";
                    if (question.Answer.Length <= 1) key = "无";
                    else
                    {
                        var keyStrategy = _questionDbService.GetStrategyBySystemName(SystemQuestionStrategyName.KeyWord);
                        var keywordOrder = _payMeentDbService.GetQuestionStrategyOrder(user.Id, questionId, keyStrategy.Id).Count() - 1;
                        key = question.GetIndexChar(keywordOrder);

                    }
                    content = "“" + question.Title + "”关键字提示:" + key;
                }
                var mes = new Message()
                {
                    MessageType = MessageType.BuyInfo,
                    Content = content,
                    ToUserId = user.Id,
                    RelateGuid = question.Guid,
                    FromUserId = sys.Id,
                };

                var t1 = new Task(() => insertMeg(mes));
                t1.Start();

                return Json(new { isSuccess = true, message = "支付成功", order = order.OrderNumber }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { isSuccess = false, message = res.Error }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult CanBuyStrategy(int strategyId, int questionId )
        {
            var user = _workContext.CurrentUser;
            var strategy = _questionDbService.GetStrategyById(strategyId);
            if (strategy == null)
            {
                return Json(new { isSuccess = false, message = "购买对象不存在" }, JsonRequestBehavior.AllowGet);
            }
            var question = _questionDbService.GetById(questionId);
            if (question == null)
            {
                return Json(new { isSuccess = false, message = "谜题不存在" }, JsonRequestBehavior.AllowGet);
            }
            //是自己的问题就无需购买
            if (question.UserId == user.Id)
            {
                return Json(new { isSuccess = false, message = "自己的问题，无需购买" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new {isSuccess = true});
        }


        private static object o = new object();
        private int insertMeg(Message msg)
        {
            int n = -1;
            try
            {
                lock (o)
                {
                    using (var db = new PortalDb())
                    {
                        db.Messages.Add(msg);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("更新访问次数错误{0}", e.Message));

            }
            return n;
        }

        [LoginValid]
        public JsonResult RechargeAction(decimal money)
        {
            if (money < (decimal)0.01) return Json(new PaymentResult("充值金额非法！"));
            var user = _workContext.CurrentUser;
            var order = _payMeentDbService.CreateRechargeOrder(user.Id, money);
            var res = _payMeentDbService.Payment(order);
            res.Money = _payMeentDbService.GetBalance(user.Id);
            Logger.DebugAsync(string.Format("用户{0}id{1},充值{2},结果{3}", user.Username, user.Id, money, res.IsSuccess));

            var sys = _userDbService.GetUserBySystemName(SystemUserNames.SystemWallet);
            var mes = new Message()
            {
                MessageType = MessageType.ReChange,
                Content = "您成功充值" + money + "元",
                ToUserId = user.Id,
                FromUserId = sys.Id
            };
            _messageDbService.InsertMessage(mes);

            return Json(res);
        }
        //充值 应该是先创建订单，然后返回到相应的支付页面
        public JsonResult CreateRecharegOrder(decimal money)
        {
            if (money < (decimal)0.01) return Json(new PaymentResult("充值金额非法！"));
            var user = _workContext.CurrentUser;
            var order = _payMeentDbService.CreateRecharegOrder(user.Id, money);
            return Json(new PaymentResult(true) { OrderId = order.OrderNumber });
        }

        [ExceptionLog]
        public JsonResult WeiXinPaySuccess(string ordernumber)
        {
            var user = _workContext.CurrentUser;
            var order = _payMeentDbService.GetOrderByOrderNumber(ordernumber);
            if (order != null)
            {
                var res = _payMeentDbService.Payment(order);
                res.Money = _payMeentDbService.GetBalance(user.Id);
                Logger.DebugAsync(string.Format("用户{0}id{1},充值{2},结果{3}", user.Username, user.Id, order.Amount, res.IsSuccess));

                var sys = _userDbService.GetUserBySystemName(SystemUserNames.SystemWallet);
                var mes = new Message()
                {
                    MessageType = MessageType.ReChange,
                    Content = "您成功充值" + order.Amount + "元",
                    ToUserId = user.Id,
                    FromUserId = sys.Id
                };
                _payMeentDbService.InsertMessage(mes);

                return Json(res);
            }
            return Json(new PaymentResult(false));
        }


        //支付成功 然后显示在用户的账户上面

        [LoginValid]
        public JsonResult PayReward(int questionId)
        {
            var question = _questionDbService.GetById(questionId);
            if (question == null) return PaymentResult("迷题不存在!");
            var fromuserId = question.UserId;
            //验证用户是否作答
            var user = _workContext.CurrentUser;
            var rightAnswer = _questionDbService.GetRightAnswerByUserIdAndQuestionId(user.Id, questionId);
            if (rightAnswer == null) return PaymentResult("用户没有回答正确!");
            //using (var sc = new TransactionScope())
            //{
            try
            {
                //根据策略计算金额
                var money = _questionDbService.GetReward(user.Id, questionId);
                //最后才是支付
                var rewarded = string.Format("你获得来自{0}的谜题悬赏金{1}", question.User.Username, money.ToString("c"));

                var res = PayRewardAction(fromuserId, money, questionId, rewarded);

                if (!res.IsSuccess) return Json(res);
                //问题需要减少金额 
                question.ReduceRemanidReward(money);
                //也就是修改剩余奖金
                // _questionDbService.UpdateQuestion(question);
                //更新问题
                rightAnswer.IsPay = true;
                // _questionDbService.UpdateAnswer(rightAnswer);

                using (var db = new PortalDb())
                {
                    var qu = db.Questions.Find(questionId);
                    qu.IsFinished = question.IsFinished;
                    qu.RemanidReward = question.RemanidReward;
                    var anw = db.Answers.Find(rightAnswer.Id);
                    anw.IsPay = true;
                    db.SaveChanges();
                }




                CommonEFHelper.SaveRewardInfo(user.Id, rewarded, MessageType.Reward, question.Guid);
                if (question.IsFinished)
                {
                    var finished = string.Format("你{0}{1}发布的谜题已经被全部领取", question.CreateTime.ToShortDateString(), question.CreateTime.ToShortTimeString());
                    CommonEFHelper.SaveRewardInfo(question.UserId, finished, MessageType.SystemInfo, question.Guid);
                }
                // sc.Complete();
                res.Money = money;
                return Json(res);
            }
            catch (Exception e)
            {
                return PaymentResult(e.Message);
            }
            // }
        }


        private PaymentResult PayRewardAction(int fromuserId, decimal money, int qustionid, string remark = "")
        {
            var fromuserWallet = _payMeentDbService.GetByUserId(fromuserId);
            if (fromuserWallet == null) return new PaymentResult("悬赏用户钱包不存在");
            if (fromuserWallet.LockMoney < money) return new PaymentResult("悬赏用户钱冻结金额不够支付");

            var user = _workContext.CurrentUser;
            var order = _payMeentDbService.CreateRewardOrder(user.Id, fromuserId, money, qustionid);
            var res = _payMeentDbService.Payment(order, remark);
            Logger.DebugAsync(string.Format("用户{0}id{1},获得用户{4}的悬赏{2},结果{3}", user.Username, user.Id, money.ToString("c"), res.IsSuccess, fromuserId));
            return res;
        }

        /// <summary>
        /// 充值没冲过的可以再冲
        /// </summary>
        /// <param name="orderid"></param>
        /// <returns></returns>
        [LoginValid]
        public JsonResult RechargeActionByOrderId(int orderid)
        {
            var order = _payMeentDbService.GetOrderById(orderid);
            if (order == null) return Json(new PaymentResult("订单不存在"));
            var res = _payMeentDbService.Payment(order);
            return Json(res);
        }

        private JsonResult PaymentResult(string error = "")
        {
            return Json(new PaymentResult(error));
        }


        private readonly decimal tocashMin = 1;
        private readonly double feePercent = 0.06;
        [LoginValid]
        public JsonResult CheckWalletAndFee(decimal money)
        {
            if (money < tocashMin)
            {
                return Json(new PortalResult() { Message = string.Format("提现金额不能小于{0}元", tocashMin) });
            }
            var total = money * (decimal)(1 + feePercent);

            var user = _workContext.CurrentUser;
            var wallert = _payMeentDbService.GetByUserId(user.Id);
            if (wallert.Balance < total)
            {
                return Json(new PortalResult() { Message = string.Format("余额不足以提现!包含服务费{0}元", money * (decimal)feePercent) });
            }

            return Json(new PortalResult(true));
        }

        public ActionResult CreateToCashOrder(decimal money)
        {
            if (money < tocashMin) return Json(new PaymentResult(string.Format("提现金额不能小于{0}元", tocashMin)));
            var user = _workContext.CurrentUser;
            var order = _payMeentDbService.CreateToCashOrder(user.Id, money);
            return Json(new PaymentResult(true) { OrderId = order.OrderNumber });
        }

        public ActionResult DealCashFee(string orderNumber)
        {
            //处理订单状态，并扣除费用
            var order = _payMeentDbService.GetOrderByOrderNumber(orderNumber);
            if (order.OrderState == OrderState.Padding)
            {
                _payMeentDbService.Payment(order);
                //扣钱
                var cashfee = _payMeentDbService.CreateToCashFeeOrder(order.UserId, order.Amount * (decimal)feePercent);
                _payMeentDbService.Payment(cashfee);

                return Json(new PortalResult(true, "提现成功!"));
            }
            if (order.OrderState == OrderState.Success)
            {
                return Json(new PortalResult(false, "订单已处理过!"));
            }
            return Json(new PortalResult(false, "订单已取消"));
        }
    }
}
