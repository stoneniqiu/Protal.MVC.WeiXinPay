using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Niqiu.Core.Domain.Common;
using Niqiu.Core.Domain.Payments;
using Niqiu.Core.Domain.Questions;
using Niqiu.Core.Domain.User;
using Niqiu.Core.Helpers;
using Niqiu.Core.Services;
using Niqiu.Core.Services.Firends;
using Niqiu.Core.Services.Questiones;
using Portal.MVC.Attributes;
using Portal.MVC.Models.Services;

namespace Portal.MVC.Controllers
{
    [LoginValid]
    [MenuStatistics]
    public class UserCenterController : Controller
    {
        private readonly IWorkContext _workContext;
        private readonly IPhoneCodeSerice _phoneCodeSerice;
        public UserCenterController(IWorkContext workContext,IPhoneCodeSerice phoneCodeSerice)
        {
            _workContext = workContext;
            _phoneCodeSerice = phoneCodeSerice;
        }
        //
        // GET: /UserCenter/

        private PayMeentDbService _payMeentDbService = new PayMeentDbService();
        private QuestionDbService _questionService=new QuestionDbService();
        private UserDbService _userService=new UserDbService();
        public ActionResult Index()
        {
            var user = CurrentUser;
            var wallet = _payMeentDbService.CheckAndCreate(user.Id);
            //粉丝数，好友数，id
            ViewBag.Wallet = wallet;
            return View(user);
        }

        #region 我的资料

        [LoginValid]
        public ActionResult Detail(string returnUrl = "")
        {
            if (string.IsNullOrWhiteSpace(CurrentUser.ImgUrl)||CurrentUser.ImgUrl == "/Content/user_img.jpg")
            {
                CurrentUser.ImgUrl = "/images/navimg3.png";
            }
            ViewBag.Url = returnUrl;
            return View(CurrentUser);
        }

        public ActionResult ConnectService()
        {
            return View();
        }

        public ActionResult Portrait()
        {
            return View(CurrentUser);
        }
        [HttpPost]
        public ActionResult ChangeAvatar(string url)
        {
            try
            {
                var user = _userService.GetUserById(CurrentUser.Id);
                user.ImgUrl = url;
                _userService.UpdateUser(user);
                return Json(1);
            }
            catch (Exception)
            {
                return Json(0);
            }
        }
        public ActionResult NikeName()
        {
            return View(CurrentUser);
        }

        [HttpPost]
        public ActionResult ChangeName(string name)
        {
            //check
            if (string.IsNullOrWhiteSpace(name)) return Json(new PortalResult("用户名不能为空"));
            if (name.Length < 4) return Json(new PortalResult("用户名不能少于4个字"));

            //不能和现在重复
            if (name == CurrentUser.Username) return Json(new PortalResult(true));

            var exist = _userService.GetUserByUsername(name);
            if (exist != null) return Json(new PortalResult("用户名已经存在!"));

            var user = _userService.GetUserById(CurrentUser.Id);
            user.Username = name;
            _userService.UpdateUser(user);
            return Json(new PortalResult(true) { Message = "修改成功" });
        }


        public ActionResult Wdzl(string img, string name, string mobile, string realname,string idcard)
        {
            if (string.IsNullOrEmpty(img))
            {
                return Json(new PortalResult("请上传图片"));
            }
            //check
            if (string.IsNullOrWhiteSpace(name)) return Json(new PortalResult("用户名不能为空"));
            if (name.Length < 2) return Json(new PortalResult("用户名不能少于2个字"));

            //不能和现在重复
            if (name != CurrentUser.Username && _userService.GetUserByUsername(name) != null)
            {
                return Json(new PortalResult("用户名已经存在!"));
            }

            var result = CommonHelper.ValidateString(mobile, ValidataType.Mobile);
            if (!result) return Json(new PortalResult("请输入正确的手机号码!"));

            //var result1 = CommonHelper.CheckIDCard(idcard);
            //if (!result1) return Json(new PortalResult("请输入正确的身份证号码!"));

            //if (!string.IsNullOrWhiteSpace(realname)&&realname.Length<2)
            //{
            //    return Json(new PortalResult("请输入姓名!"));
            //}

            var user = _userService.GetUserById(CurrentUser.Id);
            user.Mobile = mobile;
            user.ImgUrl = img;
            user.Username = name;
           // user.RealName = realname;
            //user.IdCared = idcard;
            _userService.UpdateUser(user);

            return Json(new PortalResult(true) { Message = "修改成功" });

        }


        public ActionResult Mobile()
        {
            return View(CurrentUser);
        }

        [HttpPost]
        public ActionResult ChangeMobile(string mobile, string code)
        {
            var result = CommonHelper.ValidateString(mobile, ValidataType.Mobile);
            if (!result) return Json(new PortalResult("请输入正确的手机号码!"));

            if (string.IsNullOrWhiteSpace(code)) return Json(new PortalResult("验证码不能为空!"));

            //校验验证码
            if (!checkcode(code))
            {
                return Json(new PortalResult("验证码错误!"));
            }

            //不能和现在重复
            if (mobile == CurrentUser.Mobile) return Json(new PortalResult(true));

            var exist = _userService.GetUserByMobile(mobile);
            if (exist != null) return Json(new PortalResult("该手机号码已经存在!"));

            var user = _userService.GetUserById(CurrentUser.Id);
            user.Mobile = mobile;
            _userService.UpdateUser(user);

            return Json(new PortalResult(true) { Message = "修改成功" });
        }

        private bool checkcode(string code)
        {
            return true;
        }

        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [OutputCache(Duration = 0)]
        public ActionResult ChangePassword(string oldpwd, string pwd)
        {
            if (string.IsNullOrWhiteSpace(pwd) || string.IsNullOrWhiteSpace(oldpwd)) return Json(new PortalResult("密码不能为空"));

            if (Encrypt.GetMd5Code(oldpwd) != CurrentUser.Password) return Json(new PortalResult("原密码输入错误"));

            if (pwd.Length < 6) return Json(new PortalResult("密码长度不能少于6位"));

            var user = _userService.GetUserById(CurrentUser.Id);
            if (user != null)
            {
                user.Password = Encrypt.GetMd5Code(pwd);
                _userService.UpdateUser(user);
                return Json(new PortalResult(true), JsonRequestBehavior.AllowGet);
            }
            return Json(new PortalResult("用户不存在!"), JsonRequestBehavior.AllowGet);
        }

        private User _currentUser;
        public User CurrentUser
        {
            get { return _currentUser ?? (_currentUser = _workContext.CurrentUser); }
            set { _currentUser = value; }
        }
        #endregion
        #region 我的钱包

        public ActionResult MyWallet()
        {
            var wallet = _payMeentDbService.CheckAndCreate(CurrentUser.Id);
            return View(wallet);
        }

        public ActionResult ToCash()
        {
            var raw = _workContext.CurrentUser;
            var user = _userService.GetUserById(raw.Id);
            if (string.IsNullOrEmpty(user.PaymentPassword))
            {
                return RedirectToAction("SetPayPassword", "UserCenter",
                    new { returnUrl = Url.Action("ToCash")});
            }

            if (string.IsNullOrEmpty(user.Mobile))
            {
                //添加资料
                return RedirectToAction("Detail", "UserCenter",  new { returnUrl = Url.Action("ToCash")});
            }
            var wallet = _payMeentDbService.CheckAndCreate(CurrentUser.Id);
            ViewBag.Cash = wallet.Balance;
            return View(user);
        }

        public ActionResult Recharge()
        {
            var wallet = _payMeentDbService.CheckAndCreate(CurrentUser.Id);
            return View(wallet);
        }

        public ActionResult List()
        {
            return View();
        }

        public ActionResult BankCards()
        {
            return View();
        }



        #endregion
        #region 我的账单

        public ActionResult RechargeList()
        {
            var wallet = getWallet();
            var list = _payMeentDbService.GetUserPaymentLogs(wallet.Id, OrderType.Recharge);
            return View(list);
        }

        public ActionResult AllList()
        {
            var wallet = getWallet();
            var list = _payMeentDbService.GetUserPaymentLogs(wallet.Id);
            ViewBag.MyWalletId = wallet.Id;
            return PartialView(list);
        }

        private Wallet getWallet()
        {
            return _payMeentDbService.CheckAndCreate(CurrentUser.Id);
        }
        public ActionResult GetRewardList()
        {
            var wallet = getWallet();
            var list = _payMeentDbService.GetPaymentLogs(0, wallet.Id, OrderType.Reward);
            return View(list);
        }

        public ActionResult PayRewardList()
        {
            var wallet = getWallet();
            var list = _payMeentDbService.GetPaymentLogs(wallet.Id, 0, OrderType.Reward);
            return View(list);
        }

        public ActionResult ToCashList()
        {
            var wallet = getWallet();
            var list = _payMeentDbService.GetPaymentLogs(wallet.Id, 0, OrderType.ToCash);
            return View(list);
        }

        public ActionResult BuyStrategyList()
        {
            var wallet = getWallet();
            var list = _payMeentDbService.GetPaymentLogs(wallet.Id, 0, OrderType.QuestionStrategy);
            return View(list);
        }

        #endregion
        #region 我的谜题

        public ActionResult MyQuestions()
        {
            return View();
        }

        public ActionResult MyQuestionsPage(bool? hasrewared=null)
        {
            //未被领取，//已被领取
            var questions = _questionService.QuestionsTable().Where(n=>n.UserId==CurrentUser.Id);
            if (hasrewared != null)
            {
                if (hasrewared.Value)
                {
                    questions = questions.Where(n => n.RewardUsers.Any());
                }
                else
                {
                    questions = questions.Where(n => !n.RewardUsers.Any());
                }
            }
            return PartialView(questions);
        }

        public ActionResult AllQuestions()
        {
            var list = new List<Question>();
            list.AddRange(_questionService.QuestionsTable().Where(n => n.UserId == CurrentUser.Id));
            list.AddRange(_questionService.QuestionsTable().Include(n => n.Answers).Where(m => m.Answers.Any(n => n.UserId == CurrentUser.Id)).Include(n => n.RewardUsers));
            return PartialView(list);
        }


        public ActionResult JoinQuestion()
        {

            return View();
        }

        public ActionResult JoinQuestionPage(bool?isreward=null)
        {
            var question = _questionService.QuestionsTable().Include(n => n.Answers).Where(m => m.Answers.Any(n => n.UserId == CurrentUser.Id)).Include(n => n.RewardUsers);
            if (isreward != null)
            {
                if (isreward.Value)
                {
                    question = question.Where(n => n.RewardUsers.Any(m => m.Id == CurrentUser.Id));
                }
                else
                {
                    question = question.Where(n => n.RewardUsers.All(m => m.Id != CurrentUser.Id));
                }
            }
            return PartialView(question);
        }

        #endregion
        #region  绑定银行卡

        public ActionResult BindCard1()
        {
            return View();
        }

        public ActionResult BindCard2()
        {

            return View();
        }

         
        #endregion
        #region 设置

        public ActionResult Set()
        {
            return View();
        }
        public ActionResult About()
        {
            return View();
        }

        public ActionResult SetPayPassword(string returnUrl="")
        {
            if (!string.IsNullOrWhiteSpace(CurrentUser.PaymentPassword)) 
                return RedirectToAction("ForgetPayPassword");
            ViewBag.Url = returnUrl;
            return View();
        }

        public JsonResult SetPayPwdJson(string pwd)
        {
            var user = _workContext.CurrentUser;
            var res = _userService.SetPaymentPassword(user.Id, pwd);
            if (res)
            {
                //需要更新
               var nuser = _userService.GetUserById(user.Id);
                Session["User"] = nuser;
                _workContext.CurrentUser = user;
            }

            return Json(res);
        }

        public ActionResult ChangePayPassword()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CheckPaymentPassword(string pwd)
        {
            //todo验证密码是否正确
            return Json(_userService.ValidPaymentPassword(CurrentUser.Id,pwd));
        }
        [HttpPost]
        [OutputCache(Duration = 0)]
        public ActionResult ChangePayPassword(string oldpwd, string pwd)
        {
            if (string.IsNullOrWhiteSpace(pwd) || string.IsNullOrWhiteSpace(oldpwd)) return Json(new PortalResult("密码不能为空"));

            if (!_userService.ValidPaymentPassword(CurrentUser.Id,oldpwd)) return Json(new PortalResult("原密码输入错误"));

            if (pwd.Length < 6) return Json(new PortalResult("密码长度不能少于6位"));

            var user = _userService.GetUserById(CurrentUser.Id);
            if (user != null)
            {
                _userService.SetPaymentPassword(CurrentUser.Id, pwd);
                return Json(new PortalResult(true), JsonRequestBehavior.AllowGet);
            }
            return Json(new PortalResult("用户不存在!"), JsonRequestBehavior.AllowGet);
        }
        public ActionResult ForgetPayPassword()
        {
            if (string.IsNullOrEmpty(CurrentUser.PaymentPassword))
            {
                return RedirectToAction("SetPayPassword", new { returnUrl =Url.Action("ForgetPayPassword")});
            }

            if (string.IsNullOrEmpty(CurrentUser.Mobile))
            {
                return RedirectToAction("BindMobile");
            }

            ViewBag.Mobile = CurrentUser.Mobile;
            return View();
        }

        [LoginValid]
        public ActionResult BindMobile(string returnUrl="")
        {
            if(string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = Url.Action("Set");
            }
            ViewBag.Back = returnUrl;
            return View();
        }

        [HttpPost]
        [LoginValid]
        public ActionResult BindMobile(string mobile, string code)
        {
            if (!CommonHelper.ValidateString(mobile, ValidataType.Mobile))
            {
                return Json("请输入正确的手机号码", JsonRequestBehavior.AllowGet);
            }

            var codevalid = _phoneCodeSerice.Valid(code, mobile);
            if (!codevalid) return Json("验证码错误", JsonRequestBehavior.AllowGet);

            var user = _userService.GetUserById(CurrentUser.Id);
            user.Mobile = mobile;

            _userService.UpdateUser(user);

            return Json(true, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public ActionResult ForgetPayPassword(string code, string password)
        {
            var mobile = CurrentUser.Mobile;
            var codevalid = _phoneCodeSerice.Valid(code, mobile);
            if (!codevalid) return Json("验证码错误", JsonRequestBehavior.AllowGet);

            if (string.IsNullOrEmpty(password) || password.Length < 6)
            {
                return Json("密码不能少于6位", JsonRequestBehavior.AllowGet);
            }
            return Json(_userService.SetPaymentPassword(CurrentUser.Id,password));
        }

        public ActionResult CommonQA()
        {

            return View();
        }

        public ActionResult Feeback(string content,string phone="")
        {
            if (string.IsNullOrWhiteSpace(content)) return Json(new PortalResult("内容不能为空"));
            if (content.Length < 10) return Json(new PortalResult("内容不能少于10个字"));
            _userService.Feeback(CurrentUser, content);
            return Json(new PortalResult(true) { Message = "反馈成功!谢谢你的支持" });
        }

        #endregion
    }
}
