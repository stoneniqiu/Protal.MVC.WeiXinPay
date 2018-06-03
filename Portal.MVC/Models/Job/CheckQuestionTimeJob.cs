using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentScheduler;
using Niqiu.Core.Domain.Config;
using Niqiu.Core.Domain.Messages;
using Niqiu.Core.Domain.Payments;
using Niqiu.Core.Domain.User;

namespace Portal.MVC.Models.Job
{
    public class CheckQuestionTimeJob:IJob
    {
        public void Execute()
        {
            Logger.Debug("开始执行CheckQuestionTimeJob...");
            var i = 0;
            decimal all = 0;
            using (var db = new PortalDb())
            {
                var sys = db.Users.FirstOrDefault(n => n.SystemName == SystemUserNames.SystemWallet);
                var allquestion = db.Questions.Where(n=>!n.IsFinished).ToList();
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
                            wallet.ReduceLockMoney(question.RemanidReward);
                            all += question.RemanidReward;
                            var remaind = question.RemanidReward;
                            question.RemanidReward = 0;
                            question.IsFinished = true;

                            var rawquestion = db.Questions.Find(question.Id);
                            rawquestion.RemanidReward = 0;
                            rawquestion.IsFinished = true;

                            //插入消息
                            var mes = new Message()
                            {
                                MessageType = MessageType.BuyInfo,
                                Content = string.Format("你的谜题{0},因为时间截止，系统将剩余金额{1}元已退回你的钱包,请及时查看哦", question.Title, remaind),
                                ToUserId = wallet.UserId,
                                RelateGuid = question.Guid,
                                FromUserId = sys.Id
                            };
                            db.Messages.Add(mes);
                            //需要创建一个回退的日志
                            var syswallet =
                                db.Wallets.FirstOrDefault(n => n.SystemName == WalletSystemNames.SystemTotalWallet);
                            var paylog = new PaymentLog
                            {
                                Amount = remaind,
                                FromWalletId = syswallet.Id,
                                ToWalletId = wallet.Id,
                                ToBeforeAmount = wallet.Balance-remaind,
                                ToAfterAmount = wallet.Balance,
                                OrderId =0,
                                OrderType = OrderType.RewardBack,
                                PayType = PayType.Wallet,
                                Remarks = string.Format("谜题{0}余额退回",question.Title),
                                IsSuccess = true,
                                //bug 还未配置appid
                                FromWeiXinId = PortalConfig.SystemWeiXinAppId,
                            };
                            Logger.Debug(string.Format("谜题{0}余额{1}退回,钱包{2},用户{3}", question.Title,remaind,wallet.Id,wallet.UserId));

                            db.PaymentLogs.Add(paylog);
                        }

                    }
                }
                db.SaveChanges();
            }
            Logger.Debug("CheckQuestionTimeJob执行完毕");
        }
    }
}