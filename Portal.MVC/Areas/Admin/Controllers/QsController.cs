using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Niqiu.Core.Domain.Questions;
using Niqiu.Core.Services;
using Niqiu.Core.Services.Questiones;
using Portal.MVC.Models;

namespace Portal.MVC.Areas.Admin.Controllers
{
    public class QsController : AdminBaseController
    {
        private readonly IQuestionService _questionService;
        private readonly IRepository<Comment> _commentRepository;
        public QsController(IQuestionService questionService, IRepository<Comment> commentRepository)
        {
            _questionService = questionService;
            _commentRepository = commentRepository;
        }
        public ActionResult Index()
        {
            var qs = _questionService.QuestionsTable().Where(n=>!n.Deleted).OrderByDescending(n=>n.Id);

            foreach (var question in qs)
            {
                question.RewardUsers = _questionService.GetAllRewardUsers(question.Id).ToList();
            }
            return View(qs);
        }

        public ActionResult Detail(int id)
        {
            var q = _questionService.GetById(id);
            return View(q);
        }

        public ActionResult Delete(int id)
        {
            using (var db=new PortalDb())
            {
                var q = db.Questions.Find(id);
                q.Deleted = true;
                db.SaveChanges();
                return Json(1);
            }
        }

        public ActionResult SetClassicalQuestion(int id)
        {
            using (var db = new PortalDb())
            {
                var q = db.Questions.Find(id);
                if (q != null)
                {
                    q.IsClassical = !q.IsClassical;
                    db.SaveChanges();
                }
                return Json(q.IsClassical);
            }

        }

        public ActionResult Comments()
        {
            var coms = _commentRepository.Table;
            return View(coms);
        }

        public ActionResult DeleteComment(int id)
        {
            using (var db=new PortalDb())
            {
                var q = db.Comments.Find(id);
                db.Comments.Remove(q);
                db.SaveChanges();
                return Json(1);
            }
        }
    }
}
