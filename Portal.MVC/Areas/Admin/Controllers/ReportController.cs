using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Niqiu.Core.Domain.User;
using Niqiu.Core.Services;
using Niqiu.Core.Services.Questiones;

namespace Portal.MVC.Areas.Admin.Controllers
{
    public class ReportController : AdminBaseController
    {
        private readonly IQuestionService _questionService;
        private readonly IUserService _userService;
       private readonly IRepository<User> _useRepository;
       private readonly IWorkContext _workContext;
       public ReportController(IWorkContext workContext, IRepository<User> useRepository, IQuestionService questionService, IUserService userService)
        {
            _userService = userService;
            _questionService = questionService;
           _useRepository = useRepository;
           _workContext = workContext;
        }
        //
        // GET: /Admin/Report/

        public ActionResult Index()
        {
            var rs = _questionService.GetAllReports();
            return View(rs);
        }

        public ActionResult FinishReport(int id)
        {
            var admin = _workContext.CurrentUser;
            var user = _questionService.GetByReportId(id);
            user.IsDeal = true;
            user.AdminUserId = admin.Id;
            user.AdminUserName = admin.Username;
            user.DealTime = DateTime.Now;
            _questionService.UpdateReport(user);
            return Json(1);
        }

        public ActionResult Delete(int id)
        {
            _questionService.DeleteReport(id);
            return Json(1);
        }

        //被举报的谜题
        public ActionResult RQs()
        {
            var qs = _questionService.QuestionsTable().Where(n => n.IsIllegal&&!n.Deleted);
            return View(qs);
        }

        //被举报的用户
        public ActionResult RUsers()
        {
            var rusers = _useRepository.Table.Where(n => n.IsIllegal);
            return View(rusers);
        }

        public ActionResult SetUser(int id,bool isillegal)
        {
            var user = _userService.GetUserById(id);
            user.IsIllegal = isillegal;
            _userService.UpdateUser(user);
            return Json(1);
        }

        public ActionResult SetQuestion(int id, bool isillegal)
        {
            var user = _questionService.GetById(id);
            user.IsIllegal = isillegal;

            _questionService.UpdateQuestion(user);
            return Json(1);
        }
    }
}
