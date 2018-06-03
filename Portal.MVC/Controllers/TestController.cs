using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Niqiu.Core.Services.Questiones;
using Portal.MVC.Models;
using Portal.MVC.Models.Services;

namespace Portal.MVC.Controllers
{
    public class TestController : Controller
    {
        
        private QuestionDbService questionDbService = new QuestionDbService();
        private readonly IQuestionService _questionService;
        public TestController(IQuestionService questionService)
        {
            _questionService = questionService;
        }
        public ActionResult Index()
        {

            return View();
        }

        public ActionResult TestAnswerQuestion(int qid,string answer)
        {

            return Json(1);
        }

        public ActionResult TestFinishQuestion(int qid)
        {
            try
            {
                //var q1 = _questionService.GetById(qid);
                //q1.IsFinished = true;

                questionDbService.FinishQuestion(qid);
                return Json(1);
            }
            catch (Exception e)
            {

                return Json(e.Message);
            }
        
        }

        public ActionResult TestFinishQuestionResp(int qid)
        {
            try
            {

                var q = _questionService.GetById(qid);
                q.IsFinished = true;
                _questionService.UpdateQuestion(q);
                return Json(1);
            }
            catch (Exception e)
            {

                return Json(e.Message);
            }
        }

        public ActionResult UsingFinishQuestion(int qid)
        {
            try
            {
                using (var db=new PortalDb())
                {
                    var q = db.Questions.Find(qid);
                    q.IsFinished = true;
                    q.ModifyTime = DateTime.Now;
                    db.SaveChanges();
                }
                return Json(1);
            }
            catch (Exception e)
            {

                return Json(e.Message);
            }
        }

    }
}
