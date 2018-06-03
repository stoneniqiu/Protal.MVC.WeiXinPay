using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Niqiu.Core.Domain.User;
using Niqiu.Core.Services;
using Niqiu.Core.Services.Firends;
using Niqiu.Core.Services.Questiones;
using Portal.MVC.Attributes;
using Portal.MVC.ViewModel;

namespace Portal.MVC.Controllers
{
    [LoginValid]
    [MenuStatistics]
    public class FirendsController : Controller
    {
        private readonly IWorkContext _workContext;
        private readonly IFirendService _firendService;
        private readonly IQuestionService _questionService;
        private readonly IUserService _userService;

        public FirendsController(IWorkContext workContext,IUserService userService, IFirendService firendService, IQuestionService questionService)
        {
            _workContext = workContext;
            _firendService = firendService;
            _questionService = questionService;
            _userService = userService;
        }
        //
        // GET: /Firends/

        public ActionResult Index()
        {
            var fs = _firendService.GetAllFirends(CurrentUser.Id);
            var group = new FirendsListGroup(fs);
            ViewBag.New = _firendService.GetNewFirendCount(CurrentUser.Id);
            ViewBag.HasNew = _firendService.HasNewFirends(CurrentUser.Id);
            return View(group);
        }

        public JsonResult GetNewFirends()
        {
            var n = _firendService.GetNewFirendCount(CurrentUser.Id);
            return Json(n);
        }

        public ActionResult Detail(int userid)
        {
            //查出用户 还要更新资料
            var user = _userService.GetUserById(userid);
            if (user == null) return View("NoData");

            var questions=_questionService.GetAllQuestiones("",user.Id);
            var userdetail = new UserDetail(questions, user);
            //谜题列表
            return PartialView(userdetail);
        }

        public ActionResult FirendsNew()
        {
            //关注我的
            var myats = _firendService.GetAttentioned(CurrentUser.Id).ToList();
            var myfs = _firendService.GetAllFirends(CurrentUser.Id).ToList();
            var models = new List<FirendsNew>();
            foreach (var firend in myats)
            {
                if (!firend.Readed)
                {
                    firend.Readed = true;
                    _firendService.UpdateFirend(firend);
                }

                models.Add(new FirendsNew()
                {
                    Firend = myfs.FirstOrDefault(n => n.FirendId == firend.UserId) ?? getNewFirend(firend.UserId),
                    //关注我的人 是否在我的好友列表里面
                    Attentioned = myfs.Any(n=>n.FirendId==firend.UserId)
                });
                
            }
            return View(models);
        }

        private Firend getNewFirend(int userid)
        {
            var user = _userService.GetUserById(userid);
            var f = new Firend()
            {
                UserId = CurrentUser.Id,
                FirendId = user.Id,
                FirendName = user.Username,
                FirendImg = user.ImgUrl,
                FirendUser = user
            };

            return f;

        }


        public JsonResult Attention(int userid)
        {
            var user = _workContext.CurrentUser;
            var res = _firendService.AddOrCanelFirend(user.Id, userid);
            return Json(res);
        }


        private User _currentUser;
        public User CurrentUser
        {
            get { return _currentUser ?? (_currentUser = _workContext.CurrentUser); }
            set { _currentUser = value; }
        }
    }
}
