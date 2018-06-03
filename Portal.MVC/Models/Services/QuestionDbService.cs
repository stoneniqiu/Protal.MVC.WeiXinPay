using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Niqiu.Core.Domain;
using Niqiu.Core.Domain.Common;
using Niqiu.Core.Domain.Config;
using Niqiu.Core.Domain.Questions;
using Niqiu.Core.Domain.Reports;
using Niqiu.Core.Domain.User;
using WxPayAPI;

namespace Portal.MVC.Models.Services
{
    public class QuestionDbService
    {
        //private static object o = new object();
        private PortalDb _db = new PortalDb();

        public Question GetById(int id)
        {
            return _db.Questions.Find(id);
        }
        public Question GetByGuid(Guid id)
        {
            return _db.Questions.FirstOrDefault(n => n.Guid == id);
        }
        public IEnumerable<QuestionStrategy> AllStrategies()
        {
            return _db.QuestionStrategies;
        }
        public QuestionStrategy GetStrategyBySystemName(string name)
        {
            return _db.QuestionStrategies.FirstOrDefault(n => n.SystemName == name);
        }
        public QuestionStrategy GetStrategyById(int id)
        {
            return _db.QuestionStrategies.Find(id);
        }

        public Comment GetComentById(int id)
        {
            return _db.Comments.Find(id);
        }

        public PortalResult CanAnswerQuestion(int userId, int questionId)
        {
            var qq = GetById(questionId);
            if (qq == null) return new PortalResult("问题不存在");

            //获奖人数已经大于悬赏人数
            if (qq.RewardUsers.Count >= qq.RewardPeopleNum)
                return new PortalResult("悬赏已经领取完了!");

            if (qq.IsFinished || qq.RemanidReward == 0) return new PortalResult("悬赏已经领取完了!");

            if (!PortalConfig.CanAnswerOwnQuestion)
                if (qq.UserId == userId) return new PortalResult("不能回答自己的问题");

            var hasRight = GetRightAnswerByUserIdAndQuestionId(userId, questionId);
            if (hasRight != null && hasRight.IsPay) return new PortalResult("你已经回答正确");

            // var times = _answerRepository.Table.Count(n => n.UserId == userId && n.QuestioId == questionId);
            //if (times >= PortalConfig.AnswserTimes)
            //{
            //    return new PortalResult(string.Format("你已经回答过{0}次", times));
            //}

            return new PortalResult(true) { Message = "Ok" };
        }

        public Answer GetRightAnswerByUserIdAndQuestionId(int userId, int questionId)
        {
            return _db.Answers.FirstOrDefault(n => n.IsRight && n.UserId == userId && n.QuestioId == questionId);
        }

        public PortalResult AddCommentPraise(int userid, int commentid)
        {
            using (var db = new PortalDb())
            {
                var exist = db.Comments.Find(commentid);
                if (exist != null)
                {
                    if (userid == exist.CommentUserId && !PortalConfig.CanPraiseSelf)
                    {
                        return getaPortalResult(false, "不能赞自己啦", exist.PraiseNum);
                    }

                    var old = GetPraiseLog(userid, commentid, PraiseType.Comment);
                    //如果可以取消
                    if (old == null)
                    {
                        exist.PraiseNum = exist.PraiseNum + 1;
                        UpdateComment(exist);
                        return CreatePraise(userid, commentid, exist.PraiseNum, PraiseType.Comment);
                    }
                    if (!PortalConfig.CanCancelPraise)
                        return getaPortalResult(false, "你已经赞过啦", exist.PraiseNum);

                    db.Praises.Remove(old);
                    exist.PraiseNum = exist.PraiseNum - 1;
                    UpdateComment(exist);
                    return getaPortalResult(true, "已取消赞", exist.PraiseNum);
                }
            }
            return new PortalResult("评论不存在");
        }

        public PortalResult PraiseQuestion(User user, Question exist)
        {
            var old = GetPraiseLog(user.Id, exist.Id);
            using (var db=new PortalDb())
            {
                //如果可以取消
                if (old == null)
                {
                    exist.PraisesNum = exist.PraisesNum + 1;
                    var p = new PraiseLog() { UserId = user.Id, RelateId = exist.Id, PraiseType = PraiseType.Question };
                    db.Praises.Add(p);
                    db.SaveChanges();
                    return new PortalResult(true) { Num = exist.PraisesNum };
                }
                if (!PortalConfig.CanCancelPraise)
                    return getaPortalResult(false, "你已经赞过啦", exist.PraisesNum);
                db.Praises.Remove(old);
                exist.PraisesNum = exist.PraisesNum - 1;
                db.SaveChanges();
            }
         
            return new PortalResult(true) { Num = exist.PraisesNum };
        }

        private PortalResult CreatePraise(int userid, int rid, int num, PraiseType type = PraiseType.Question)
        {
            using (var db = new PortalDb())
            {
                var p = new PraiseLog() {UserId = userid, RelateId = rid, PraiseType = type};
                db.Praises.Add(p);
                db.SaveChanges();
                return new PortalResult(true) {Num = num};
            }
        }
        public void UpdateComment(Comment model)
        {
            using (var db = new PortalDb())
            {
                var raw = db.Comments.Find(model.Id);
                raw.PraiseNum = model.PraiseNum;
                raw.ModifyTime = DateTime.Now;
                raw.Content = model.Content;
                db.SaveChanges();
            }
        }

        private PortalResult getaPortalResult(bool res, string meg, int mum)
        {
            return new PortalResult(res, meg) { Num = mum };

        }
        public IEnumerable<Answer> GetAllAnswers()
        {
            return _db.Answers;
        }
        public IPagedList<Question> GetAllQuestiones(string title = "", int? userid = null, bool? isright = null, OrderbyType orderby = OrderbyType.Id, int pageIndex = 0, int pageSize = 2147483647)
        {
                //没有被删除的 没有结束的 合法的
                var query =
                    _db.Questions.Where(n => !n.Deleted && !n.IsIllegal && !n.IsFinished && n.ExpireTime > DateTime.Now);

                if (!string.IsNullOrEmpty(title))
                {
                    query = query.Where(n => n.Title.Contains(title));
                }
                if (userid != null)
                {
                    query = query.Where(n => n.UserId == userid.Value);
                }
                if (isright != null)
                {
                    query = query.Where(n => n.IsAnsweredRight == isright.Value);
                }
                query = query.Include(n => n.Commnets).Include(n => n.RewardUsers);
                switch (orderby)
                {
                    case OrderbyType.Id:
                        query = query.OrderByDescending(c => c.Id);
                        break;
                    case OrderbyType.Comments:
                        query = query.OrderByDescending(c => c.CommentNum);
                        break;
                    case OrderbyType.Reward:
                        query = query.OrderByDescending(c => c.Reward);
                        break;
                    default:
                        query = query.OrderByDescending(c => c.Id);
                        break;
                }

                return new PagedList<Question>(query, pageIndex, pageSize);
        }

        public IPagedList<Question> GetClassicalQuestions(int pageIndex = 0, int pageSize = 2147483647)
        {
            var query =
                   _db.Questions.Where(n => !n.Deleted && !n.IsIllegal && n.IsFinished && n.IsClassical);
            query = query.OrderByDescending(c => c.Id);
            return new PagedList<Question>(query, pageIndex, pageSize);
        }
        
        public void AddShardCount(int questionid, Shared model)
        {

            using (var db = new PortalDb())
            {
                if (model != null)
                {
                    var question = db.Questions.Find(questionid);
                    if (question != null)
                    {
                        question.Shareds.Add(model);
                        db.SaveChanges();
                    }
                }
            }
        }

        public int GetQuestionPraiseNum(int questionId)
        {
            return _db.Praises.Count(n => n.RelateId == questionId && n.PraiseType == PraiseType.Question);
        }
        public IEnumerable<User> GetAllRewardUsers(int questionId)
        {
            var usersid = _db.RewardUsers.Where(n => n.QuestionId == questionId);
            var users = new List<User>();
            foreach (var i in usersid)
            {
                var model = _db.Users.Find(i.UserId);
                model.ModifyTime = i.CreateTime;
                users.Add(model);
            }
            return users;
        }
        public PortalResult InsertReport(Report report)
        {
            using (var db = new PortalDb())
            { if (report == null) throw new ArgumentException("report");

            if (report.RelateId == 0)
            {
                return new PortalResult("请设置举报对象!");
            }

            if (report.RelateUserId == 0)
            {
                return new PortalResult("请完善被举报人信息!");
            }

            //先查询是否存在相似的
            var exit =
                db.Reports.Any(
                    n => n.RelateId == report.RelateId && n.UserId == report.UserId && n.RelateType == report.RelateType);
            if (exit)
            {
                return new PortalResult("你已经举报过了！");
            }
            db.Reports.Add(report);
            db.SaveChanges();
            return new PortalResult(true, "举报成功，谢谢你的监督，工作人员会尽快处理!");
            //然后插入
            }
        }


        public PraiseLog GetPraiseLog(int userid, int rid, PraiseType type = PraiseType.Question)
        {
          
            try
            {
                return _db.Praises.FirstOrDefault(n => n.UserId == userid && rid == n.RelateId && n.PraiseType == type);
            }
            catch (Exception e)
            {
                Logger.Debug("GetPraiseLog Error"+e.Message);
                return null;
            }
        }
        public void AnswerQuestion(bool notpay, int questionid, int userid, string answer, bool result)
        {
            using (var db = new PortalDb())
            {
                if (notpay)
                {
                    var model = new Answer
                    {
                        Content = answer,
                        QuestioId = questionid,
                        UserId = userid,
                        IsRight = result,
                    };
                    InsertAnswer(model);
                }
                var dbdate = db.Questions.Find(questionid);
                dbdate.IsAnsweredRight = result;
                var hasAnswered = db.RewardUsers.FirstOrDefault(n => n.UserId == userid && n.QuestionId == questionid);
                if (result && dbdate.RewardPeopleNum > dbdate.RewardUsers.Count() && hasAnswered==null)
                {
                    db.RewardUsers.Add(new RewardUser()
                    {
                        QuestionId = questionid,
                        UserId = userid,
                    });
                }
                db.SaveChanges();
            }
        }


        public void FinishQuestion(int id)
        {
            using (var db = new PortalDb())
            {
                var q = db.Questions.Find(id);
                q.IsFinished = true;
                q.RemanidReward = 0;
                q.ModifyTime = DateTime.Now;
                db.SaveChanges();
            }
        }


        public void InsertAnswer(Answer answer)
        {

            using (var db = new PortalDb())
            {//同一个问题，一个用户只能回答正确一次
            var hasRight =
                db.Answers.FirstOrDefault(
                    n => n.IsRight && n.UserId == answer.UserId && n.QuestioId == answer.QuestioId);
                if (hasRight == null)
                {
                    answer.User = db.Users.Find(answer.UserId);
                    answer.Question = db.Questions.Find(answer.QuestioId);
                    db.Answers.Add(answer);
                    db.SaveChanges();
                }
            }
        }

        public void VisitCountAdd(int id)
        {
            using (var db = new PortalDb())
            {
                var dbdate = db.Questions.Find(id);
                dbdate.VisitCount++;
                db.SaveChanges();
            }
        }

        public void InsertQuestion(User user, Question model)
        {
            using (var db = new PortalDb())
            {
                model.Answer = model.ClearAnwser();
                model.RemanidReward = model.Reward;
                db.Questions.Add(model);
                db.SaveChanges();
                Logger.DebugAsync(string.Format("用户成功创建谜题{0}，金额为{1}", user.Username, model.Title));
            }
        }
        public IQueryable<Question> QuestionsTable()
        {
            return _db.Questions;
        }
        public decimal GetReward(int userid, int questionId)
        {
            var question = GetById(questionId);
            if (question == null) throw new PortalException("谜题不存在！");

            //验证用户是否回答正确 否则抛出异常
            var anwser = GetRightAnswerByUserIdAndQuestionId(userid, questionId);
            //获取分配数组
            if (anwser == null) throw new PortalException("用户没有回答正确,不能领取悬赏");

            //是否已经领取了？
            if (anwser.IsPay) throw new PortalException("该回答已经领取悬赏");

            //已经回答正确的人数
            var num = GetRightAnswers(questionId).Count();

            var rewards = question.GetReward();

            //确定当前用户是第几位领取的
            if (num > rewards.Length) throw new PortalException("超额领取悬赏！");
            if (num == rewards.Length) throw new PortalException("悬赏已经领取完了！");

            //返回金额  不做支付动作，只是返回金额
            return rewards[num];
        }
        public IEnumerable<Answer> GetRightAnswers(int questionId)
        {
            return _db.Answers.Where(n => n.IsRight && n.QuestioId == questionId && n.IsPay);
        }
    }
}