using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Niqiu.Core.Domain.Common;
using Niqiu.Core.Domain.Questions;
using Niqiu.Core.Domain.Reports;
using Niqiu.Core.Domain.User;
using Niqiu.Core.Helpers;
using Niqiu.Core.Services;
using Niqiu.Core.Services.Firends;
using Niqiu.Core.Services.Messages;
using Niqiu.Core.Services.Payments;
using Niqiu.Core.Services.Questiones;
using Portal.MVC.Attributes;
using Portal.MVC.Models;
using Portal.MVC.Models.Services;
using Portal.MVC.ViewModel;
using Portal.MVC.WXDevice;
using Portal.MVC.WxPayAPI.lib;
using WxPayAPI;

namespace Portal.MVC.Controllers
{
    [UserLastActivityIp]
    [MenuStatistics]
    public class HomeController : BaseController
    {
       // private readonly IQuestionService _questionDbService;
        //private readonly IPaymentService _paymentService;
        private readonly IWorkContext _workContext;
        private readonly ICacheManager _cacheManager;
        private WxDeviceService wxDeviceService = new WxDeviceService();
        public HomeController(ICacheManager cacheManager,IFirendService firendService,IWorkContext workContext)
        {
          //  _questionDbService = questionService;
            _workContext = workContext;
            _cacheManager = cacheManager;
        }

        private QuestionDbService _questionDbService= new QuestionDbService();
        private PayMeentDbService _payMeentDbService=new PayMeentDbService();
        private UserDbService _userDbService=new UserDbService();
        private FriendDbService _friendDbService=new FriendDbService();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult H5Login()
        {
            var model = new WXShareModel();
            model.appId = WxPayConfig.APPID;
            model.nonceStr = WxPayApi.GenerateNonceStr();
            model.timestamp = Util.CreateTimestamp();
            model.ticket = GetTicket();
            model.url = "http://www.haoyoucai888.com/Home/H5Login";// domain + Request.Url.PathAndQuery;
            model.MakeSign();
            return View(model);
        }

        public ActionResult QuestionesList(int page = 0, int take = 20, bool? isright = null, OrderbyType type = OrderbyType.Id)
        {
            var questions = _questionDbService.GetAllQuestiones(orderby: type, isright: isright);
            return PartialView(questions);
        }
        public ActionResult ClassicalQuestionesList()
        {
            var questions = _questionDbService.GetClassicalQuestions();
            return PartialView(questions);
        }



        public ActionResult Classical()
        {
           // return RedirectToAction("Index");
            return View();
        }

        //[LoginValid]
        public ActionResult Detail(int id)
        {
            var question = _questionDbService.GetById(id);
            if (question == null) return View("NoData");
            question.RewardUsers.Clear();
            question.RewardUsers = _questionDbService.GetAllRewardUsers(id).ToList();
            question.PraisesNum = _questionDbService.GetQuestionPraiseNum(id);

            var t1 = new Task(() => Update(id));
            t1.Start();

            var user = _workContext.CurrentUser;
           

            if (user != null)
            {
                ViewBag.isSelf = user.Id == question.UserId;
                var ll = _questionDbService.GetPraiseLog(user.Id, question.Id) != null;
                ViewBag.IsLike = ll;
                var aa = _friendDbService.GetFirendByUserId(user.Id, question.UserId) != null;
                ViewBag.Attentioned = aa;
            }
            else
            {
                ViewBag.Attentioned = false;
                ViewBag.isSelf = false;
                ViewBag.IsLike = false;
            }
            ViewBag.Reward = 0.1;
            ViewBag.IsAnwserRight = HasAnwserRight(question);
            if (ViewBag.IsAnwserRight)
            {
                var my = question.RewardUsers.FirstOrDefault(n => n.Id == user.Id);
                if (my != null)
                {
                    var index = question.RewardUsers.ToList().IndexOf(my);
                    ViewBag.Reward = question.GetReward()[index];
                }
            }

            var model = new WXShareModel();
            model.appId = WxPayConfig.APPID;
            model.nonceStr = WxPayApi.GenerateNonceStr();
            model.timestamp = Util.CreateTimestamp();
            model.ticket = GetTicket();
            model.url = "http://www.haoyoucai888.com" + Request.Url.PathAndQuery;
            model.MakeSign();

            ViewBag.Share = model;
            ViewBag.imgUrl = "http://www.haoyoucai888.com" + question.ImageUrl;
            return View(question);
        }
        private void Update(int id)
        {
            try
            {
                _questionDbService.VisitCountAdd(id);
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("更新访问次数错误{0}", e.Message));

            }
        }


       

        public ActionResult LogoInit(string openid)
        {
            if (!string.IsNullOrEmpty(openid))
            {
                if (Session["User"] == null)
                {
                    var user = _userDbService.GetUserByOpenId(openid);
                    if (user != null)
                    {
                        Session["User"] = user;
                        return Content("初始化成功");
                    }
                }
                return Content("已经登录");

            }
            return Content("openid 为空");
        }

        public ActionResult GetOpenId()
        {
            if (Session["User"] != null)
            {
                var cuser = Session["User"] as User;
                return Json(cuser.OpenId);
            }
            return Json("");
        }

        public ActionResult Shared(int id, string type)
        {
            //统计分享类型？no
            var question = _questionDbService.GetById(id);
            if (question == null) return Json(0);
            var share = new Shared()
            {
                QuestionId = id,
                Type = type
            };
            _questionDbService.AddShardCount(id, share);

            return Json(1);
        }

        public ActionResult SendMessage()
        {
            var token = getToken();
            var toUserId = "oBSBmwQjqwjfzQlKsFNjxFLSiIQM";
           // var toUserId = "oBSBmwY3vy3B17GzRPgzyVOj4oic";
            var data = new TemplateModel("你好，stoneniqiu","审核通过","资料完整","祝你生活幸福！");
            data.touser = toUserId;
            data.template_id = "gXmkeL7Kc-KUy2EQzKqjPKY-kzFyatTztiYFKCaUPO4";
            data.url = "http://www.haoyoucai888.com//Home/saylove";
            data.topcolor = "#FF0000";

            var res=wxDeviceService.SendTemplateMessage(token, data);
            return View(res);
        }

        public ActionResult SayLove()
        {
            return View();
        }

        private string ticketKey = "jsapi_ticket";
        public string GetTicket()
        {
            var cticket = getCacheTicket();
            var token = getToken();
            if (string.IsNullOrEmpty(cticket))
            {
                cticket = wxDeviceService.GetJsApiTicket(token).ticket;
                setCacheTicket(ticketKey);
            }

            return cticket;
        }
        private string getCacheTicket()
        {
            return _cacheManager.Get<string>(ticketKey);
        }
        private void setCacheTicket(string cache)
        {
            _cacheManager.Set(ticketKey, cache, 7000);
        }
        private string getToken()
        {
            var token = getcacheToken();
            if (token == null || string.IsNullOrEmpty(token.access_token))//|| checkExpire(token.expires_in)
            {
                token = wxDeviceService.GetAccessToken();
                setToken(token);
            }
            return token.access_token;
        }

        private string tokenKey = "access_token";
        private void setToken(TokenResult token)
        {
            _cacheManager.Set(tokenKey, token, 7000);
        }

        private TokenResult getcacheToken()
        {
            return _cacheManager.Get<TokenResult>(tokenKey);
        }
        public bool HasAnwserRight(Question question)
        {
            var user = _workContext.CurrentUser;
            var ans = question.Answers.ToList();
            if (user != null && ans.Any())
            {

                var k = ans.Any(n => n.IsRight && n.UserId == user.Id);
                return k;
            }
            return false;
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult InsertReport(string title, string content, ReportType type, int id, ReportRelateType relateType)
        {
            var user = _workContext.CurrentUser;
            if (user == null) return Json(new PortalResult("请先登录!"));

            if (string.IsNullOrEmpty(content)) return Json(new PortalResult("举报内容不能为空!"));


            var report = new Report()
            {
                Title = title,
                ReportType = type,
                RelateId = id,
                UserId = user.Id,
                UserName = user.Username,
                RelateType = relateType,
                Content = content,
            };

            if (report.RelateType == ReportRelateType.Question)
            {
                var target = _questionDbService.GetById(report.RelateId);
                report.RelateUserId = target.UserId;
                report.RelateUserName = target.User.Username;
            }
            if (report.RelateType == ReportRelateType.Comment)
            {
                var target = _questionDbService.GetComentById(report.RelateId);
                report.RelateUserId = target.CommentUserId;
                report.RelateUserName = target.User.Username;
            }


            var res = _questionDbService.InsertReport(report);
            return Json(res);
        }


        public ActionResult DetailByGuid(Guid guid, int id = 0)
        {
            var question = _questionDbService.GetByGuid(guid);
            if (question == null) return new HttpStatusCodeResult(404);
            return RedirectToAction("Detail", new { id = question.Id });
        }

        public ActionResult HasLogin()
        {
            return Json(_workContext.CurrentUser != null);
        }

        [LoginValid]
        public ActionResult BuyInfo(int questionId)
        {
            //获取之前购买提示的记录
            //关键字提示 第一次购买 题目价格的50%，每次加10%
            //完整答案 题目价格的200%

            var strs = _questionDbService.AllStrategies();
            var question = _questionDbService.GetById(questionId);
            if (question == null) return View("NoData");
            ViewBag.Question = question;
            var user = _workContext.CurrentUser;
            var wallet = _payMeentDbService.CheckAndCreate(user.Id);
            var keyStrategy = _questionDbService.GetStrategyBySystemName(SystemQuestionStrategyName.KeyWord);
            var keywordOrderNum = _payMeentDbService.GetQuestionStrategyOrder(user.Id, questionId, keyStrategy.Id).Count();
            foreach (var qs in strs)
            {
                qs.Seed(question,keywordOrderNum);
                //if (qs.SystemName == SystemQuestionStrategyName.Answer)
                //{
                //    //购买答案是谜题金额的两倍
                //    qs.Price = question.Reward * 2;
                //}
                //if (qs.SystemName == SystemQuestionStrategyName.WordNum)
                //{
                //    //字数是悬赏金额的一半
                //    qs.Price = (decimal)((double)question.Reward * 0.5);
                //}
                //if (qs.SystemName == SystemQuestionStrategyName.KeyWord)
                //{
                //    var max = 0.5 + keywordOrderNum * 0.1;
                //    if (max > 1) max = 1;
                //    qs.Price = (decimal)((double)question.Reward * max);
                //}
            }


            ViewBag.B = wallet.Balance;
            ViewBag.isEmptyPwd = string.IsNullOrEmpty(user.PaymentPassword);
            ViewBag.HasKeyword = question.Answer.Length > 1;
            //  ViewBag.ShowYY = !string.IsNullOrWhiteSpace(question.Tip);
            return View(strs);
        }
        [HttpPost]
        public ActionResult CheckPaymentPassword(string pwd)
        {
            var user = _workContext.CurrentUser;
            //todo验证密码是否正确
            return Json(Encrypt.EncryptString(pwd) == user.PaymentPassword);
        }

        //提示消息
        public ActionResult InfoMsg()
        {
            return View();
        }
        public ActionResult PaymentResult(string orderNumber)
        {
            var order = _payMeentDbService.GetOrderByOrderNumber(orderNumber);
            if (order == null) return View("NoData");

            var question = _questionDbService.GetById(order.QuestionId);
            var strategy = _questionDbService.GetStrategyById(order.RelationId);

            var content = "";
            if (strategy.SystemName == SystemQuestionStrategyName.Answer)
            {
                content = "谜题“" + question.Title + "”完整答案:" + question.Answer;
            }
            if (strategy.SystemName == SystemQuestionStrategyName.WordNum)
            {
                content = "谜题“" + question.Title + "”字数提示:" + question.Answer.Length + "个字";
            }
            if (strategy.SystemName == SystemQuestionStrategyName.KeyWord)
            {
                var key = "";
                if (question.Answer.Length <= 1) key = "无";
                else
                {
                    //var ran = new Random();
                    //var index = ran.Next(0, question.Answer.Length - 1);
                    var keyStrategy = _questionDbService.GetStrategyBySystemName(SystemQuestionStrategyName.KeyWord);
                    var keywordOrder = _payMeentDbService.GetQuestionStrategyOrder(order.UserId, order.QuestionId, keyStrategy.Id).Count() - 1;
                    key = question.GetIndexChar(keywordOrder);

                }
                content = "“" + question.Title + "”关键字提示:" + key;
            }

            ViewBag.PayResult = "购买成功!";
            ViewBag.Info = content;
            ViewBag.Qid = question.Id;
            return View();
        }

        [HttpPost]
        public ActionResult UploadImg(HttpPostedFileBase file, string dir = "UserImg")
        {
            if (CheckImg(file, imgtypes) != "ok") return Json(new { Success = false, Message = "文件格式不对！" }, JsonRequestBehavior.AllowGet);

            if (file != null)
            {
                var path = "~/Content/UploadFiles/" + dir + "/";
                var uploadpath = Server.MapPath(path);
                if (!Directory.Exists(uploadpath))
                {
                    Directory.CreateDirectory(uploadpath);
                }
                string fileName = Path.GetFileName(file.FileName);// 原始文件名称
                string fileExtension = Path.GetExtension(fileName); // 文件扩展名
                //string saveName = Guid.NewGuid() + fileExtension; // 保存文件名称 这是个好方法。
                string saveName = Encrypt.GenerateOrderNumber() + fileExtension; // 保存文件名称 这是个好方法。
                var saveUrl = uploadpath + saveName;
                file.SaveAs(saveUrl);
                if (file.ContentLength / 1024 > 500)//大于0.5M
                {
                    var _saveName = Encrypt.GenerateOrderNumber() + "_bnailUrl" + fileExtension;
                    var thumbnailUrl = uploadpath + _saveName;
                    ImageManageHelper.GetPicThumbnail(saveUrl, thumbnailUrl, 400, 400, 90);
                    return Json(new { Success = true, SaveName = path + _saveName });
                }

                return Json(new { Success = true, SaveName = path + saveName });
            }
            return Json(new { Success = false, Message = "请选择要上传的文件！" }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult MUploadImg(HttpPostedFileBase mfile)
        {
            return UploadImg(mfile, "Mobile");
        }

        private string[] excels = { ".xls", ".xlsx" };
        [HttpPost]
        public ActionResult UploadFile(HttpPostedFileBase file)
        {
            if (CheckImg(file, excels) != "ok") return Json(new { Success = false, Message = "文件格式不对！" }, JsonRequestBehavior.AllowGet);

            if (file != null)
            {
                var uploadpath = Server.MapPath("~/Content/UploadFiles/Excel/");
                if (!Directory.Exists(uploadpath))
                {
                    Directory.CreateDirectory(uploadpath);
                }
                string fileName = Path.GetFileName(file.FileName);// 原始文件名称
                string fileExtension = Path.GetExtension(fileName); // 文件扩展名
                //string saveName = Guid.NewGuid() + fileExtension; // 保存文件名称 这是个好方法。
                string saveName = Encrypt.GenerateOrderNumber() + fileExtension; // 保存文件名称 这是个好方法。
                file.SaveAs(uploadpath + saveName);

                return Json(new { Success = true, SaveName = "/Content/UploadFiles/Excel/" + saveName, Name = fileName });
            }
            return Json(new { Success = false, Message = "请选择要上传的文件！" }, JsonRequestBehavior.AllowGet);

        }

        private string[] imgtypes = { ".bmp", ".png", ".gif", ".jpg", ".jpeg" };

        /// <summary>
        /// 核对图片
        /// </summary>
        /// <param name="file"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        private string CheckImg(HttpPostedFileBase file, string[] types)
        {
            if (file == null) return "图片不能空！";
            var extension = Path.GetExtension(file.FileName);
            if (extension != null)
            {
                var image = extension.ToLower();
                return types.Contains(image) ? "ok" : "文件格式不对";
            }
            return "ok";
        }

        public ActionResult Test()
        {

            var model = new ImgModel();
            model.Img = "http://photocdn.sohu.com/20160629/Img456995877.jpeg";

            return View(model);
        }
    }
}
