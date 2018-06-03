using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Niqiu.Core.Domain;
using Niqiu.Core.Domain.Common;
using Niqiu.Core.Domain.Config;
using Niqiu.Core.Domain.Payments;
using Niqiu.Core.Domain.Questions;
using Niqiu.Core.Domain.Reports;
using Niqiu.Core.Domain.User;
using NPOI.HSSF.Record.Formula.Functions;

namespace Niqiu.Core.Services.Questiones
{
    public class QuestionService : IQuestionService
    {
        private readonly IRepository<Question> _questionRepository;
        private readonly IRepository<QuestionStrategy> _strageRepository;
        private readonly IRepository<Answer> _answerRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Wallet> _walletRepository;
        private readonly IRepository<RewardUser> _rewarduseRepository;
        private readonly IRepository<Comment> _commentRepository;
        private readonly IRepository<PraiseLog> _praiseRepository;
        private readonly IRepository<Report> _reportRepository; 

        public QuestionService(
            IRepository<Question> questionRepository,
            IRepository<Answer> answerRepository,
           IRepository<Wallet> walletRepository,
           IRepository<RewardUser> rrRepository,
           IRepository<Comment> cRepository,
            IRepository<Report> reportRepository,
           IRepository<PraiseLog> qpRepository,
          IRepository<User> userRepository,
            IRepository<QuestionStrategy> strageRepository)
        {
            _questionRepository = questionRepository;
            _strageRepository = strageRepository;
            _answerRepository = answerRepository;
            _userRepository = userRepository;
            _walletRepository = walletRepository;
            _rewarduseRepository = rrRepository;
            _commentRepository = cRepository;
            _praiseRepository = qpRepository;
            _reportRepository = reportRepository;
        }

        public void InsertQuestion(Question model)
        {
            if (model == null) throw new ArgumentNullException("model");
            model.RemanidReward = model.Reward;
            _questionRepository.Insert(model);
        }

        public void UpdateQuestion(Question question)
        {
            if (question == null) throw new ArgumentNullException("question");

            _questionRepository.Update(question);
        }

        public void DeletQuestion(int questionId)
        {
            var question = _questionRepository.GetById(questionId);
            if (question != null)
            {
                _questionRepository.Delete(question);
            }
        }

        public void AddRewardUser(int questionId, int userId)
        {
            if (!_rewarduseRepository.Table.Any(n => n.QuestionId == questionId && userId == n.UserId))
            {
                var ru = new RewardUser()
                {
                    QuestionId = questionId,
                    UserId = userId,
                };
                _rewarduseRepository.Insert(ru);
            }
        }

        public IEnumerable<User> GetAllRewardUsers(int questionId)
        {
            var usersid =
                _rewarduseRepository.Table.Where(n => n.QuestionId == questionId);
                     

            var users = new List<User>();
            foreach (var i in usersid)
            {
                var model = _userRepository.GetById(i.UserId);
                model.ModifyTime = i.CreateTime;
                users.Add(model);
            }
            return users;
        }

        public Question GetById(int id)
        {
            return _questionRepository.GetById(id);
        }
        public Question GetByGuid(Guid id)
        {
            return _questionRepository.Table.FirstOrDefault(n => n.Guid == id);
        }
        public IPagedList<Question> GetAllQuestiones(string title = "", int? userid = null, bool? isright = null, OrderbyType orderby = OrderbyType.Id, int pageIndex = 0, int pageSize = 2147483647)
        {
            //没有被删除的 没有结束的 合法的
            var query = _questionRepository.Table.Where(n => !n.Deleted && !n.IsIllegal && !n.IsFinished && n.ExpireTime > DateTime.Now);

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

          public void FinishQuestion(int id)
          {
              var q = GetById(id);
              q.IsFinished = true;
              q.RemanidReward = 0;
              UpdateQuestion(q);
          }
        public IQueryable<Question> QuestionsTable()
        {
            return _questionRepository.Table;
        }
        public void InsertStrategy(QuestionStrategy strategy)
        {
            if (strategy == null) throw new ArgumentNullException("strategy");
            _strageRepository.Insert(strategy);
        }

        public QuestionStrategy GetStrategyById(int id)
        {
            return _strageRepository.GetById(id);
        }

        public QuestionStrategy GetStrategyBySystemName(string name)
        {
            return _strageRepository.Table.FirstOrDefault(n => n.SystemName == name);
        }
        public IEnumerable<QuestionStrategy> AllStrategies()
        {
            return _strageRepository.Table;
        }

        public void InsertAnswer(Answer answer)
        {
            if (answer == null) throw new ArgumentNullException("answer");

            //同一个问题，一个用户只能回答正确一次
            var same = GetRightAnswerByUserIdAndQuestionId(answer.UserId, answer.QuestioId);
            if (same != null) return;

            _answerRepository.Insert(answer);
        }

        public Answer GetAnswerById(int id)
        {
            return _answerRepository.GetById(id);
        }

        public Answer GetRightAnswerByUserIdAndQuestionId(int userId, int questionId)
        {
            return _answerRepository.Table.FirstOrDefault(n => n.IsRight && n.UserId == userId && n.QuestioId == questionId);
        }

        public IEnumerable<User> GetAnswerRightUsers(int questionId)
        {
            return
                _answerRepository.Table.Where(n => n.IsRight && n.QuestioId == questionId)
                    .Include(n => n.User)
                    .Select(n => n.User);
        }
        public void UpdateAnswer(Answer answer)
        {
            if (answer == null)
                throw new ArgumentNullException("answer");
            _answerRepository.Update(answer);
        }
        public IEnumerable<Answer> GetRightAnswers(int questionId)
        {
            return _answerRepository.Table.Where(n => n.IsRight && n.QuestioId == questionId && n.IsPay);
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

        public IEnumerable<Answer> GetAllAnswers()
        {
            return _answerRepository.Table;
        }

        public PortalResult CanAnswerQuestion(int userId, int questionId)
        {
            var qq = GetById(questionId);
            if (qq == null) return new PortalResult("问题不存在");

            //获奖人数已经大于悬赏人数
            if(qq.RewardUsers.Count>=qq.RewardPeopleNum)
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



        #region 点赞

        public PortalResult AddQuestionPraise(int userid, int questionid)
        {
            var exist = _questionRepository.GetById(questionid);
            if (exist != null)
            {
                if (userid == exist.UserId && !PortalConfig.CanPraiseSelf)
                {
                    return getaPortalResult(false, "不能赞自己啦", exist.PraisesNum);
                }

                var old = GetPraiseLog(userid, questionid);
                //如果可以取消
                if (old == null)
                {
                    exist.PraisesNum = exist.PraisesNum + 1;
                    _questionRepository.Update(exist);
                    return CreatePraise(userid, questionid, exist.PraisesNum);
                }
                if (!PortalConfig.CanCancelPraise)
                    return getaPortalResult(false, "你已经赞过啦", exist.PraisesNum);

                _praiseRepository.Delete(old);
                exist.PraisesNum = exist.PraisesNum - 1;
                _questionRepository.Update(exist);
                return getaPortalResult(true, "已取消赞", exist.PraisesNum);  
            }
            return new PortalResult("问题不存在");
        }

        private PortalResult getaPortalResult(bool res, string meg, int mum)
        {
            return new PortalResult(res,meg){Num = mum};
        }
        private PortalResult CreatePraise(int userid, int rid,int num, PraiseType type = PraiseType.Question)
        {
            var p = new PraiseLog() { UserId = userid, RelateId = rid, PraiseType = type };
            _praiseRepository.Insert(p);
            return new PortalResult(true){Num = num};
        }

        public PraiseLog GetPraiseLog(int userid, int rid, PraiseType type = PraiseType.Question)
        {
            return _praiseRepository.Table.FirstOrDefault(n => n.UserId == userid && rid == n.RelateId && n.PraiseType == type);
        }

        public PortalResult AddCommentPraise(int userid, int commentid)
        {
            var exist = _commentRepository.GetById(commentid);
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
                    _commentRepository.Update(exist);
                    return CreatePraise(userid,commentid,exist.PraiseNum, PraiseType.Comment);
                }
                if (!PortalConfig.CanCancelPraise)
                    return getaPortalResult(false, "你已经赞过啦", exist.PraiseNum); 

                _praiseRepository.Delete(old);
                exist.PraiseNum = exist.PraiseNum - 1;
                _commentRepository.Update(exist);
                return getaPortalResult(true, "已取消赞", exist.PraiseNum);
            }
            return new PortalResult("评论不存在");
        }

        public PortalResult CommentQuestion(int userid, int questionid, string content)
        {
            //check
            if (string.IsNullOrWhiteSpace(content)) return new PortalResult("评论内容不能为空");
            var user = _userRepository.GetById(userid);
            if (user == null) return new PortalResult("用户不存在");
            var question = _questionRepository.GetById(questionid);
            if(question==null) return new PortalResult("谜题不存在");

            var comment = new Comment()
            {
                CommentUserId = userid,
                QuestionId = questionid,
                Content = content,
            };
            question.Commnets.Add(comment);
            _questionRepository.Update(question);

            return new PortalResult(true);
        }

        

        #endregion

        #region 举报

        public PortalResult InsertReport(Report report)
        {
            if(report==null) throw new ArgumentException("report");

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
                _reportRepository.Table.Any(
                    n => n.RelateId == report.RelateId && n.UserId == report.UserId && n.RelateType == report.RelateType);
            if (exit)
            {
                return new PortalResult("你已经举报过了！");
            }
            _reportRepository.Insert(report);
            return new PortalResult(true,"举报成功，谢谢你的监督，工作人员会尽快处理!");
            //然后插入
        }

        public void UpdateReport(Report report)
        {
            if (report == null) throw new ArgumentNullException("report");
            _reportRepository.Update(report);
        }

        public void DeleteReport(int id)
        {
            var model = _reportRepository.GetById(id);
            if (model != null)
            {
                model.Deleted = true;
                _reportRepository.Update(model);
            }
        }

        public Report GetByReportId(int id)
        {
            return _reportRepository.GetById(id);
        }

        public IEnumerable<Report> GetAllReports()
        {
            var query = _reportRepository.Table.Where(n=>!n.Deleted).OrderByDescending(n => n.Id); ;
            return query;
        }



        #endregion
    }
}
