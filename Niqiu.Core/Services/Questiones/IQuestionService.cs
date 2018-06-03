using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Niqiu.Core.Domain;
using Niqiu.Core.Domain.Common;
using Niqiu.Core.Domain.Payments;
using Niqiu.Core.Domain.Questions;
using Niqiu.Core.Domain.Reports;
using Niqiu.Core.Domain.User;

namespace Niqiu.Core.Services.Questiones
{
   public interface IQuestionService
   {
       void InsertQuestion(Question model);
       void UpdateQuestion(Question question);
       void DeletQuestion(int questionId);
      void AddRewardUser(int questionId, int userId);
       Question GetById(int id);
       Question GetByGuid(Guid id);

       IPagedList<Question> GetAllQuestiones(string title = "", int? userid = null, bool? isright = null,
           OrderbyType orderby = OrderbyType.Id, int pageIndex = 0, int pageSize = 2147483647);

       IQueryable<Question> QuestionsTable(); 

       void InsertStrategy(QuestionStrategy strategy);

       QuestionStrategy GetStrategyById(int id);

       QuestionStrategy GetStrategyBySystemName(string name);

       IEnumerable<QuestionStrategy> AllStrategies();

       IEnumerable<User> GetAllRewardUsers(int questionId);
       Answer GetRightAnswerByUserIdAndQuestionId(int userId, int questionId);
       Answer GetAnswerById(int id);

       IEnumerable<User> GetAnswerRightUsers(int questionId);

       IEnumerable<Answer> GetRightAnswers(int questionId);
       void InsertAnswer(Answer answer);
       void UpdateAnswer(Answer answer);

       decimal GetReward(int userid, int questionId);
       IEnumerable<Answer> GetAllAnswers();

       PortalResult CanAnswerQuestion(int userId, int questionId);
       void FinishQuestion(int id);

       #region 点赞

       PraiseLog GetPraiseLog(int userid, int rid, PraiseType type = PraiseType.Question);

       PortalResult AddQuestionPraise(int userid, int questionid);

       PortalResult AddCommentPraise(int userid, int commentid);

       #endregion

       PortalResult CommentQuestion(int userid, int questionid, string content);
       PortalResult InsertReport(Report report);

       void UpdateReport(Report report);
       void DeleteReport(int id);
       Report GetByReportId(int id);
       IEnumerable<Report> GetAllReports();


   }
}
