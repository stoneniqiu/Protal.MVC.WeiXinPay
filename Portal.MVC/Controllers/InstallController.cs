using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Niqiu.Core.Domain;
using Niqiu.Core.Domain.Config;
using Niqiu.Core.Domain.Messages;
using Niqiu.Core.Domain.Payments;
using Niqiu.Core.Domain.Questions;
using Niqiu.Core.Domain.Reports;
using Niqiu.Core.Domain.User;
using Niqiu.Core.Helpers;
using Niqiu.Core.Services;
using Niqiu.Core.Services.Firends;
using Niqiu.Core.Services.Payments;
using Niqiu.Core.Services.Questiones;
using Portal.MVC.Models;
using Portal.MVC.Models.Services;

namespace Portal.MVC.Controllers
{
    /// <summary>
    /// 用于初始化
    /// </summary>
    public class InstallController : Controller
    {
        private readonly IQuestionService _questionService;
        private readonly IPaymentService _paymentService;
        private readonly IUserService _userService;
        private readonly IAccountService _accountService;
        private readonly IFirendService _firendService;
        private readonly IPermissionService _permissionService;

        public InstallController(IQuestionService questionService,
            IAccountService accountService,
            IFirendService firendService,
            IPermissionService permissionService,
            IPaymentService paymentService, IUserService userService)
        {
            _questionService = questionService;
            _paymentService = paymentService;
            _userService = userService;
            _accountService = accountService;
            _firendService = firendService;
            _permissionService = permissionService;
        }

        public ActionResult Install()
        {
            InstallQuestionStrategies();
            InstallSystemWallet();
            InstallPermission();
            InstallAdmin();
            return Content("success");
        }

        public void InstallQuestionStrategies()
        {
            var strages = _questionService.AllStrategies();
            if (!strages.Any())
            {
                //price表示倍数
                _questionService.InsertStrategy(new QuestionStrategy()
                {
                    Name = "字数提示 ",
                    SystemName = SystemQuestionStrategyName.WordNum,
                    MinRate = (decimal)0.2,
                    MaxRate = (decimal)0.2,
                    StartRate = (decimal)0.2
                });
                _questionService.InsertStrategy(new QuestionStrategy()
                {
                    Name = "关键字提示",
                    SystemName = SystemQuestionStrategyName.KeyWord,
                    MinRate = (decimal)0.1,
                    MaxRate = (decimal)0.5,
                    StartRate = (decimal)0.1,

                });
                _questionService.InsertStrategy(new QuestionStrategy()
                {
                    Name = "完整答案",
                    SystemName = SystemQuestionStrategyName.Answer,
                    MaxRate = 1,
                    MinRate = 1,
                    StartRate = 1
                });
            }
        }

        public void InstallSystemWallet()
        {
            var total = _paymentService.GetBySystemName(WalletSystemNames.SystemTotalWallet);
            var systemUser = _userService.GetUserByUsername(SystemUserNames.SystemWallet);
            if (systemUser == null)
            {
                systemUser = new User()
                {
                    Email = PortalConfig.SystemEmail,
                    Username = SystemUserNames.SystemWallet,
                    SystemName = SystemUserNames.SystemWallet,
                };
                var registerRequest = new UserRegistrationRequest(systemUser, systemUser.Email, systemUser.Email,
                    SystemUserNames.SystemWallet, "SystemWallet", PasswordFormat.Encrypted);
                var registrationResult = _accountService.RegisterUser(registerRequest, false);
                if (!registrationResult.Success)
                {
                    throw new PortalException(registrationResult.Errors.First());
                }
                systemUser = _userService.GetUserByUsername(SystemUserNames.SystemWallet);
            }

            if (total == null)
            {
                _paymentService.InsertWallet(new Wallet()
                {
                    SystemName = WalletSystemNames.SystemTotalWallet,
                    UserId = systemUser.Id
                });
            }
            var consume = _paymentService.GetBySystemName(WalletSystemNames.SystemConsumeWallet);
            if (consume == null)
            {
                _paymentService.InsertWallet(new Wallet()
                {
                    SystemName = WalletSystemNames.SystemConsumeWallet,
                    UserId = systemUser.Id
                });
            }
        }

        public void InstallTestUser()
        {
            //InstallUser("stoneniqiu", "stonzrj@163.com", "15250198031", "111111");
            InstallUser("tester01", "stonzrj1@163.com", "15250198032", "111111");
            //InstallUser("揭迷达人", "stonzrj2@163.com", "15250198033", "111111");
        }

        private void InstallUser(string username, string email, string mobile, string password)
        {
            var systemUser = new User()
             {
                 Email = email,
                 Username = username,
             };
            var registerRequest = new UserRegistrationRequest(systemUser, email, mobile,
                username, password, PasswordFormat.Encrypted);
            var registrationResult = _accountService.RegisterUser(registerRequest, false);
            if (!registrationResult.Success)
            {
                throw new PortalException(registrationResult.Errors.First());
            }
        }
        private void InstallUserOlder(string username, string email, string mobile, string password,DateTime time)
        {
            var systemUser = new User()
            {
                Email = email,
                Username = username,
            };
            systemUser.CreateTime = time;
            systemUser.ModifyTime = time;
            var registerRequest = new UserRegistrationRequest(systemUser, email, mobile,
                username, password, PasswordFormat.Encrypted);
            var registrationResult = _accountService.RegisterUser(registerRequest, false);
            if (!registrationResult.Success)
            {
                throw new PortalException(registrationResult.Errors.First());
            }
        }
        public ActionResult CreatOlderUser()
        {
            //InstallUserOlder("10月老人1", "stonzrj12@163.com", "15251198033", "111111", DateTime.Now.AddMonths(-2));
            //InstallUserOlder("9月老人1", "stonzrj13@163.com", "15251198034", "111111", DateTime.Now.AddMonths(-3));
            InstallUserOlder("9月老人2", "stonzrj14@163.com", "15251198035", "111111", DateTime.Now.AddMonths(-3));
            InstallUserOlder("未来", "stonzrj15@163.com", "15251198036", "111111", DateTime.Now.AddMonths(1));
            return Content("success");
        }

        private void ClearPaymentData()
        {
            if (!PortalConfig.IsDevelopEnvironment) return;
            using (var db = new PortalDb())
            {
                //清除日志
                var paylogs = db.PaymentLogs;
                db.PaymentLogs.RemoveRange(paylogs);
                //清除作答
                var anwsers = db.Answers;
                db.Answers.RemoveRange(anwsers);
                //清除订单
                var oders = db.Orders;
                db.Orders.RemoveRange(oders);
                //清除用户
                var users = db.Users;
                db.Users.RemoveRange(users);

                //清除钱包
                var ws = db.Wallets;
                db.Wallets.RemoveRange(ws);
                db.Questions.RemoveRange(db.Questions);

                db.SaveChanges();
            }
        }

        private void InstallQuestions(int userid)
        {
            //还要扣钱
            install_question("今天晚上吃啥", (decimal)4.00, 4, RewardType.Average, "米粉", userid, "不是米饭");
            install_question("你啥时候回家", (decimal)3.00, 1, RewardType.Only, "8点", userid, "5点之后");
            install_question("iPhone7卖多少钱", (decimal)5.00, 4, RewardType.Decline, "5288", userid, "32G");
        }

        private void install_question(string title, decimal money, int people, RewardType type, string anwer, int userid, string tip = "")
        {
            var question = new Question()
            {
                Title = title,
                Reward = money,
                RewardPeopleNum = people,
                Answer = anwer,
                RewardType = type,
                UserId = userid,
                Tip = tip,
                ImageUrl = "/images/jgimg.png",
            };
            _questionService.InsertQuestion(question);
            _paymentService.LockMoney(userid, money);
        }
        private PayMeentDbService _payMeentDbService = new PayMeentDbService();

        public ActionResult Recover()
        {
            //清除数据
            ClearPaymentData();
            //添加用户
            InstallTestUser();
            InstallSystemWallet();
            //充值
            //创建订单
            var user = _userService.GetUserByUsername("stoneniqiu");
            var order = _paymentService.CreateRechargeOrder(user.Id, 50);
            //支付订单
            var res = _payMeentDbService.Payment(order);
            //设置题目
            if (res.IsSuccess)
            {
                InstallQuestions(user.Id);
                return Content("success Recover");
            }
            return Content(res.Error);
        }

        public ActionResult InstallQuestions()
        {
            var user = _userService.GetUserByUsername("stoneniqiu");
            InstallQuestions(user.Id);
            return Content("success InstallQuestions");
        }

        public ActionResult InsertTestUsers()
        {
            InstallUser("test1", "stonzrj5@163.com", "15250198011", "111111");
            InstallUser("test2", "stonzrj6@163.com", "15250198022", "111111");
            InstallUser("test3", "stonzrj7@163.com", "15250198063", "111111");
            InstallUser("test4", "stonzrj8@163.com", "15250198053", "111111");
            InstallUser("test5", "stonzrj9@163.com", "15250198043", "111111");
            return Content("success installer 5 test users");
        }

        public ActionResult AttentionTest()
        {
            var user = _userService.GetAllUsers();
            var me = _userService.GetUserByUsername("stoneniqiu");
            foreach (var u in user)
            {
                _firendService.AddOrCanelFirend(me.Id, u);
            }
            return Content("success Attention");
        }

        public ActionResult SendToAttentioned()
        {
            CommonEFHelper.SendToAttentioned(21, string.Format("你的好友{0}发布了谜题", "stoneniqiu"), new Guid("8f11f2e2-6464-4640-a7c2-e24bcf92ff8d"));
            return Content("success");
        }


        //todo 目前数据恢复到这儿

        public ActionResult InfoMessages()
        {
            using (var db = new PortalDb())
            {
                var infos = db.Messages.Where(n => n.MessageType == MessageType.Weibo);
                db.Messages.RemoveRange(infos);

                //通知消息
                //找到所有的谜题
                var questions = db.Questions;
                //通知每个人的朋友
                foreach (var question in questions)
                {
                    var attends = db.Firends.Where(n => n.FirendId == question.UserId);
                    var mess = new List<Message>();
                    foreach (var attend in attends)
                    {
                        var mes = new Message()
                        {
                            RelateGuid = question.Guid,
                            Content = string.Format("你的好友{0}发布了谜题", question.User.Username),
                            FromUserId = question.UserId,
                            ToUserId = attend.UserId,
                            MessageType = MessageType.Weibo
                        };
                        mess.Add(mes);
                    }
                    db.Messages.AddRange(mess);
                }
                db.SaveChanges();
            }
            return Content("InfoMessages success");
        }

        public ActionResult RecharegMessage()
        {
            using (var db = new PortalDb())
            {
                var sys = db.Users.FirstOrDefault(n => n.SystemName == SystemUserNames.SystemWallet) ?? new User();
                var logs = db.PaymentLogs.Where(n => n.OrderType == OrderType.Recharge);
                var models = new List<Message>();
                foreach (var paymentLog in logs)
                {
                    var userid = db.Wallets.Find(paymentLog.ToWalletId).UserId;
                    models.Add(new Message()
                    {
                        MessageType = MessageType.ReChange,
                        Content = "您成功充值"+paymentLog.Amount+"元",
                        ToUserId = userid,
                        FromUserId = sys.Id
                    });
                }
                db.Messages.AddRange(models);
                db.SaveChanges();

            }

            return Content("RecharegMessage success");

        }


        public ActionResult RewardInfo()
        {

            using (var db=new PortalDb())
            {
                var payeds = db.Questions.Where(n => n.IsFinished);
                var sys = db.Users.FirstOrDefault(n => n.SystemName == SystemUserNames.SystemWallet) ?? new User();
                var msg = new List<Message>();
                foreach (var question in payeds)
                {
                   var finished = string.Format("你{0}日{1}发布的谜题已经被全部领取", question.CreateTime.ToShortDateString(),question.CreateTime.ToShortTimeString());

                    var mesg = new Message()
                    {
                        FromUserId = sys.Id,
                        Content = finished,
                        ToUserId = question.UserId,
                        Guid = question.Guid,
                        MessageType = MessageType.SystemInfo,
                    };
                    msg.Add(mesg);
                }
                db.Messages.AddRange(msg);
                db.SaveChanges();
            }

            return Content("success");

        }


        public ActionResult GetRewardInfo()
        {
            using (var db = new PortalDb())
            {
                var logs = db.PaymentLogs.Where(n => n.OrderType == OrderType.Reward).ToList() ;
                var msg = new List<Message>();
                var sys = db.Users.FirstOrDefault(n => n.SystemName == SystemUserNames.SystemWallet) ?? new User();
                foreach (var paymentLog in logs)
                {
                    var order = db.Orders.Find(paymentLog.OrderId);
                    var question = db.Questions.Find(order.QuestionId);
                    var wallet = db.Wallets.Find(paymentLog.ToWalletId);
                    if (question != null)
                    {
                        var rewarded = string.Format("你获得来自{0}的谜题悬赏金{1}", question.User.Username,paymentLog.Amount.ToString("c"));
                        var mesg = new Message()
                        {
                            FromUserId = sys.Id,
                            Content = rewarded,
                            ToUserId = wallet.UserId,
                            Guid = question.Guid,
                            MessageType = MessageType.Reward,
                        };
                        msg.Add(mesg);
                        
                    }
                    
                }
                db.Messages.AddRange(msg);
                db.SaveChanges();
            }
            return Content("success");
        }

        public ActionResult AddRewardUser()
        {
            using (var db = new PortalDb())
            {
                var logs = db.PaymentLogs.Where(n => n.OrderType == OrderType.Reward).ToList();
                var msg = new List<RewardUser>();
                foreach (var paymentLog in logs)
                {
                    var order = db.Orders.Find(paymentLog.OrderId);
                    var question = db.Questions.Find(order.QuestionId);
                    var wallet = db.Wallets.Find(paymentLog.ToWalletId);
                    if (question != null)
                    {
                        var ru = new RewardUser()
                        {
                            QuestionId = order.QuestionId,
                            UserId = wallet.UserId,
                        };
                        msg.Add(ru);
                    }

                }
                db.RewardUsers.AddRange(msg);
                db.SaveChanges();
            }
            return Content("success AddRewardUser");
        }

        public ActionResult InstallPaymentPassword()
        {
            using (var db = new PortalDb())
            {
                var users = db.Users;
                foreach (var user in users)
                {
                    if (string.IsNullOrEmpty(user.PaymentPassword))
                    {
                        _userService.SetPaymentPassword(user.Id, "654321");
                    }
                }
                db.SaveChanges();
            }

            return Content("Password success");
        }

        public ActionResult InstallPermission()
        {
            _permissionService.InstallPermissions(new StandardPermissionProvider());
            return Content("install compelted");
        }

        public ActionResult InstallAdmin()
        {
            using (var db = new PortalDb())
            {
                var user = db.Users.FirstOrDefault(n => n.Username == "Admin");
                if (user == null)
                {
                    InstallUser("Admin", "HYCAdmin@163.com", "16250198031", "admin");
                    user = db.Users.FirstOrDefault(n => n.Username == "Admin");
                }
                var role = db.Roles.FirstOrDefault(n => n.SystemName == SystemUserRoleNames.Administrators);
                user.UserRoles.Add(role);
                db.SaveChanges();

            }


            return Content("Admin as Administrators");
        }

        public ActionResult RecoverReport()
        {
            using (var db = new PortalDb())
            {
                var reports = db.Reports;
                foreach (var report in reports)
                {
                    if (string.IsNullOrEmpty(report.RelateUserName))
                    {
                        if (report.RelateType == ReportRelateType.Question)
                        {
                            var question = db.Questions.Find(report.RelateId);
                            report.RelateUserId = question.UserId;
                            report.RelateUserName = question.User.Username;
                        }
                        if (report.RelateType == ReportRelateType.Comment)
                        {
                            var cc = db.Comments.Find(report.RelateId);
                            report.RelateUserId = cc.CommentUserId;
                            report.RelateUserName = cc.User.Username;
                        }
                    }

                }
                db.SaveChanges();


            }
            return Content("success");
        }

        public ActionResult ChangeMyPassword()
        {
            _accountService.ChangePassword(2, "111111");
            return Content("修改密码成功");
        }

        //超时的谜题，把钱退回
        public ActionResult UnFinishQuestionBackMoney()
        {
            var i = 0;
            decimal all = 0;
            var sys = _userService.GetUserBySystemName(SystemUserNames.SystemWallet);
            using (var db = new PortalDb())
            {
                var allquestion = db.Questions.ToList();
                foreach (var question in allquestion)
                {
                    //到时间了 还未结束
                    if (question.IsEnd() && !question.IsFinished)
                    {
                        i++;
                        question.IsFinished = true;
                        //把钱返回给用户
                        var wallet = db.Wallets.FirstOrDefault(n => n.UserId == question.UserId);
                        if (wallet != null)
                        {
                            wallet.Increase(question.RemanidReward);
                            all += question.RemanidReward;
                            question.RemanidReward = 0;

                            //插入消息
                            var mes = new Message()
                            {
                                MessageType = MessageType.BuyInfo,
                                Content = string.Format("你的谜题{0},因为时间截止，系统将剩余金额{1}元已退回你的钱包,请及时查看哦"),
                                ToUserId = wallet.Id,
                                RelateGuid = question.Guid,
                                FromUserId = sys.Id
                            };
                            db.Messages.Add(mes);
                        }
                     
                    }
                }
                db.SaveChanges();
            }

            return Content(string.Format("共处理了{0}个谜题，回退金额{1}元", i, all));
        }

        // 恢复创建谜题时候的支付日志
        // 一开始钱包创建谜题没有创建支付日志
        private ActionResult RecoverQuestionInsertLog()
        {
            using (var db = new PortalDb())
            {
                var ufqs = db.Questions.Where(n => !n.IsFinished);
                foreach (var item in ufqs)
                {
                    //创建日志
                    var mywallert = _paymentService.GetByUserId(item.UserId);
                    //这个时候并未支付给系统账户，只是钱锁住了，充值的时候就是支付系统账户
                    var paylog = new PaymentLog()
                    {
                        Amount = item.Reward,
                        Remarks = item.Title,
                        OrderType = OrderType.Reward,
                        FromWalletId = mywallert.Id,
                        PayType = PayType.Wallet,
                    };
                    paylog.CreateTime = item.CreateTime;
                    paylog.ModifyTime = item.ModifyTime;
                    db.PaymentLogs.Add(paylog);
                }
                db.SaveChanges();
            }
            return Content("recover ok");
        }

        //支付悬赏日志修正
        public ActionResult PayRewardLogRecover()
        {
            using (var db = new PortalDb())
            {
                var paylogs = db.PaymentLogs.Where(n => n.OrderType == OrderType.Reward&&n.Id<17).ToList();
                foreach (var paymentLog in paylogs)
                {
                    paymentLog.OrderType=OrderType.PayReward;
                }
                db.SaveChanges();
            }

            return Content("recover ok");
        }
    }

}
