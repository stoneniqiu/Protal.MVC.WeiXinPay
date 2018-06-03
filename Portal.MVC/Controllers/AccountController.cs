using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Ninject;
using Niqiu.Core.Domain.Config;
using Niqiu.Core.Domain.User;
using Niqiu.Core.Helpers;
using Niqiu.Core.Services;
using Portal.MVC.Attributes;
using Portal.MVC.ViewModel;

namespace Portal.MVC.Controllers
{
    [MenuStatistics]
    public class AccountController : BaseController
    {
        private readonly IAccountService _accountService;
        private readonly IUserService _service;
        private readonly IWorkContext _workContext;
        private readonly IPhoneCodeSerice _phoneCodeSerice;
        public AccountController(IUserService repository,IPhoneCodeSerice phoneCodeSerice, IAccountService accountService, IWorkContext workContext)
        {
            _service = repository;
            _workContext = workContext;
            _accountService = accountService;
            _phoneCodeSerice = phoneCodeSerice;
        }

        #region 修改密码

        public ActionResult MChangePassword()
        {
            return View();
        }


        [HttpPost]
        [LoginValid]
        public ActionResult MChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _workContext.CurrentUser;
                if (_accountService.ChangePassword(user.Id, model.NewPassword))
                {
                    Success();
                    return RedirectToAction("InfoMsg", "Home");
                }
                Error();
                return View();
            }
            Error();
            return View();
        }


        public ActionResult ChangePassword()
        {
            return View();
        }


        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _workContext.CurrentUser;
                if (_accountService.ChangePassword(user.Id, model.Password))
                {
                    Success();
                }
                else
                {
                    Error();
                }
                return View();
            }
            Error();
            return View();
        }

        public JsonResult CheckPassword(string password)
        {
            var user = _workContext.CurrentUser;
            if (user != null)
            {
                var res = _accountService.ValidateUser(user.Username, password);
                return Json(res == UserLoginResults.Successful, JsonRequestBehavior.AllowGet);
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 登陆退出

        [HttpGet]
        public ActionResult Logon(string returnUrl = "", bool msg = true)
        {
            var model = new LogOnModel();
            ViewBag.ReturnUrl = returnUrl;
            if (!msg)
            {
                Error("请用管理员账户登录！");
            }
            //DataBaseInit();
            //if (!_service.GetAllUsers().Any())
            //{
            //    var user = new User
            //    {
            //        UserGuid = Guid.NewGuid(),
            //        Username = "stoneniqiu",
            //        RealName = "stoneniqiu",
            //        Mobile = "15250198031",
            //        Active = true,
            //        //加密存储
            //        Password = Encrypt.GetMd5Code("admin"),
            //    };
            //    var role = _service.GetUserRoleBySystemName(SystemUserRoleNames.Administrators);
            //    user.UserRoles.Add(role);
            //    //默认增加注册角色
            //    // 先插入
            //    _service.InsertUser(user);

            //}
            return View(model);
        }

        [HttpPost]
        public ActionResult Logon(LogOnModel model, string returnUrl)
        {


            if (ModelState.IsValid)
            {
                if (model.UserName != null)
                {
                    model.UserName = model.UserName.Trim();
                }
                UserLoginResults loginResult = _accountService.ValidateUser(model.UserName, model.Password);
                if (loginResult == UserLoginResults.UserNotExist)
                {
                    loginResult = _accountService.ValidateUserByMobile(model.UserName, model.Password);
                }

                switch (loginResult)
                {
                    case UserLoginResults.Successful:
                        {
                            User user = _service.GetUserByUsername(model.UserName) ?? _service.GetUserByMobile(model.UserName);
                            //sign in new customer
                            AuthenticationService.SignIn(user, model.RememberMe);
                            user.LastActivityDateUtc = DateTime.Now;
                            _service.UpdateUser(user);

                            return RedirectToAction("Index", "Home", new { Area = "Admin" });
                        }
                    case UserLoginResults.UserNotExist:
                        ModelState.AddModelError("", "用户不存在");
                        break;
                    case UserLoginResults.Deleted:
                        ModelState.AddModelError("", "用户已删除");
                        break;
                    case UserLoginResults.NotActive:
                        ModelState.AddModelError("", "用户没有激活");
                        break;
                    case UserLoginResults.NotRegistered:
                        ModelState.AddModelError("", "用户未注册");
                        break;
                    case UserLoginResults.WrongPassword:
                    default:
                        ModelState.AddModelError("", "密码错误");
                        break;
                }
            }
            return View(model);
        }


        public ActionResult MLogon(string returnUrl = "")
        {
            var model = new MLogonModel();
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpPost]
        public ActionResult MLogon(MLogonModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (model.Mobile != null)
                {
                    model.Mobile = model.Mobile.Trim();
                }
                UserLoginResults loginResult = _accountService.ValidateUserByMobile(model.Mobile, model.Password);
                if (loginResult == UserLoginResults.UserNotExist)
                {
                    loginResult = _accountService.ValidateUser(model.Mobile, model.Password);
                }
                switch (loginResult)
                {
                    case UserLoginResults.Successful:
                        {
                            User user = _service.GetUserByMobile(model.Mobile) ?? _service.GetUserByUsername(model.Mobile);
                            //sign in new customer
                            AuthenticationService.SignIn(user, true);

                            if (String.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                                return RedirectToAction("Index", "Home");
                            return Redirect(returnUrl);
                        }
                    case UserLoginResults.UserNotExist:
                        ModelState.AddModelError("", "用户不存在");
                        break;
                    case UserLoginResults.IsIllegal:
                        ModelState.AddModelError("", "您的账户已被禁用");
                        break;
                    case UserLoginResults.Deleted:
                        ModelState.AddModelError("", "用户已删除");
                        break;
                    case UserLoginResults.NotActive:
                        ModelState.AddModelError("", "用户没有激活");
                        break;
                    case UserLoginResults.NotRegistered:
                        ModelState.AddModelError("", "用户未注册");
                        break;
                    case UserLoginResults.WrongPassword:
                    default:
                        ModelState.AddModelError("", "密码错误");
                        break;
                }
            }
            return View(model);
        }


        public ActionResult LogonJson(string mobile, string password,bool isreme)
        {

            UserLoginResults loginResult = _accountService.ValidateUserByMobile(mobile, password);
            if (loginResult == UserLoginResults.UserNotExist)
            {
                loginResult = _accountService.ValidateUser(mobile, password);
            }
            switch (loginResult)
            {
                case UserLoginResults.Successful:
                    {
                        User user = _service.GetUserByMobile(mobile) ?? _service.GetUserByUsername(mobile);
                        //sign in new customer
                        AuthenticationService.SignIn(user, isreme);
                        return Json(1);
                    }
                case UserLoginResults.UserNotExist:
                    return Json("用户不存在");
                case UserLoginResults.IsIllegal:
                    return Json("您的账户已被禁用");
                case UserLoginResults.Deleted:
                    return Json("用户已删除");
                case UserLoginResults.NotActive:
                    return Json("用户没有激活");
                case UserLoginResults.NotRegistered:
                    return Json("用户未注册");
                default:
                    return Json("密码错误");
            }
            
        }

        public ActionResult RegisterJson(string mobile, string name, string password,string compassword, string code)
        {
            var model = new MRegisterModel()
            {
                ConfirmPassword = compassword,
                Mobile = mobile,
                Password = password,
                UserName = name
            };

            var codevalid = _phoneCodeSerice.Valid(code, mobile);
            if (!codevalid)
            {
                return Json("验证码错误!");
            }

            var m=CommonHelper.ValidateString(mobile, ValidataType.Mobile);
            if (!m)
            {
                return Json("请输入正确的手机号码");
            }

            var user = _service.InsertGuestUser();
            if (_service.GetUserByMobile(model.Mobile) != null)
            {
                return Json("电话号码已注册过");
            }

            if (ModelState.IsValid)
            {
                if (model.UserName != null)
                {
                    model.UserName = model.UserName.Trim();
                }

                var registerRequest = new UserRegistrationRequest(user, "", model.Mobile, model.UserName, model.Password, PasswordFormat.Encrypted);
                var registrationResult = _accountService.RegisterUser(registerRequest, true);
                if (registrationResult.Success)
                {
                    AuthenticationService.SignIn(user, true);
                    Success("注册成功!初始密码为学号，请修改密码");
                    return Json(1);
                }
                var str = "";
                foreach (var error in registrationResult.Errors)
                {
                    str += error+"\n";
                }

                return Json(str);
            }
            return Json("注册失败");
        }

        public ActionResult ForgetPwd()
        {
            if (string.IsNullOrEmpty(_workContext.CurrentUser.Mobile))
            {
                return RedirectToAction("BindMobile","UserCenter");
            }
          
            return View();
        }
        public ActionResult ForgetPwdJson(string mobile,string code,string password)
        {
            var codevalid = _phoneCodeSerice.Valid(code, mobile);
            if (!codevalid) return Json("验证码错误", JsonRequestBehavior.AllowGet);

            if (string.IsNullOrEmpty(password) || password.Length < 6)
            {
                return Json("密码不能少于6位", JsonRequestBehavior.AllowGet);
            }

            var user = _service.GetUserByMobile(mobile);
            _accountService.ChangePassword(user.Id, password);
            AuthenticationService.SignIn(user, true);

            return Json(1);
        }
        public ActionResult Loadding()
        {
            return View();
        }

        public ActionResult Shenming()
        {
            return View();
        }



        [HttpGet]
        public ActionResult MRegister()
        {
            var model = new MRegisterModel();
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult MRegister(MRegisterModel model, string returnUrl)
        {
            //如果当前用户再注册别的用户，就让他先退出，加入一个Guest角色用户进来准备。
            var user = _service.InsertGuestUser();
            if (_service.GetUserByMobile(model.Mobile) != null)
            {
                ModelState.AddModelError("", "电话号码已注册过!");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                if (model.UserName != null)
                {
                    model.UserName = model.UserName.Trim();
                }

                var registerRequest = new UserRegistrationRequest(user, "", model.Mobile, model.UserName, model.Password, PasswordFormat.Encrypted);
                var registrationResult = _accountService.RegisterUser(registerRequest, true);
                if (registrationResult.Success)
                {
                    AuthenticationService.SignIn(user, true);
                    Success("注册成功!初始密码为学号，请修改密码");
                    if (String.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                        return RedirectToAction("Index", "Home");
                    return Redirect(returnUrl);
                }
                foreach (var error in registrationResult.Errors)
                {
                    ModelState.AddModelError("", error);
                }

            }
            return View(model);
        }




        [AllowAnonymous]
        public ActionResult Register()
        {
            var model = new RegisterModel();
            return View(model);
        }


        /// <summary>
        /// 注册【加密类型】
        /// </summary>
        /// <param name="model"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model, string returnUrl)
        {
            //如果当前用户再注册别的用户，就让他先退出，加入一个Guest角色用户进来准备。
            var user = _service.InsertGuestUser();

            if (ModelState.IsValid)
            {
                if (model.UserName != null)
                {
                    model.UserName = model.UserName.Trim();
                }

                var isApprove = true;
                var registerRequest = new UserRegistrationRequest(user, model.Email, model.Mobile, model.UserName, model.Password, PasswordFormat.Encrypted, isApprove);
                var registrationResult = _accountService.RegisterUser(registerRequest);
                if (registrationResult.Success)
                {
                    if (isApprove)
                    {
                        AuthenticationService.SignIn(user, true);
                    }
                    if (String.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                        return RedirectToAction("Index", "Home", new { Area = "Admin" });
                    return Redirect(returnUrl);
                }
                foreach (var error in registrationResult.Errors)
                {
                    ModelState.AddModelError("", error);
                }

            }
            return View(model);
        }


        /// <summary>
        ///     退出函数 还需要处理，退出时统计退出时间,然后关闭网页。
        /// </summary>
        /// <returns></returns>
        public ActionResult LogOff(bool isadmin = false)
        {
            AuthenticationService.SignOut();

            Session["User"] = null;

            if (isadmin)
                return RedirectToAction("Logon", "Account");

            return RedirectToAction("Index", "Home");
        }
        public ActionResult MLogOff()
        {
            AuthenticationService.SignOut();
            return RedirectToAction("MLogon", "Account");
        }


        public ActionResult Unauthorized(string name, string returnUrl)
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("Logon", new { returnUrl });
            }
            ViewBag.P = name;
            return View();
        }

        public ActionResult ValidComplete(string name, string active = "")
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(active))
            {
                TempData["error"] = "链接有误";

                return View();
            }

            string username = Encrypt.DecryptString(name);
            User user = _service.GetUserByUsername(username);
            //已经激活
            if (user.Active)
            {
                return View();
            }
            try
            {
                ViewBag.Email = user.Email;
                string activetime = Encrypt.DecryptString(active);
                var time = Convert.ToDateTime(activetime);
                if (DateTime.Now > time.AddHours(1))
                {
                    TempData["error"] = "链接已经失效";
                }
                else
                {
                    user.Active = true;
                    _service.UpdateUser(user);
                }
            }
            catch
            {
                TempData["error"] = "验证失败";
            }

            return View();
        }



        public ActionResult ValidMail(string name)
        {
            User user = _service.GetUserByUsername(name);

            if (user == null) return View("NoData");


            if (!user.Active)
            {
                ViewBag.Email = user.Email;
                //发送邮件
                string relative = Url.Action("ValidComplete", "Account",
                    new
                    {
                        name = Encrypt.EncryptString(user.Username),
                        active = Encrypt.EncryptString(DateTime.Now.ToString(CultureInfo.InvariantCulture))
                    });
                var timenow = DateTime.Now;

                if (Request.Url != null)
                {
                    string url = Request.Url.OriginalString.Replace(Request.Url.PathAndQuery, "") + relative;

                    string alink = string.Format("<a href='{0}'>{1}</a>", url, "点击这里确认您的账号");
                    string content =
                        string.Format("亲爱的用户 {0}: 您好，您已成功注册{4}在线账号，您可以下载{4}相关资料并获得相关资讯和技术支持！<br /> <br />" +
                                      "{1}" +
                                      " 如果上面的链接点击无效，请将下面的地址复制到浏览器中<br />" +
                                      "{2}<br /><br />注意:请您在收到邮件1个小时内({3}前)使用，否则该链接将会失效。<br /><br />",
                            user.Username, alink, url, timenow.AddHours(1), PortalConfig.WebSiteName);

                    _workContext.AsyncSendMail(user.Email, content, "邮箱激活");
                }

                //获得服务器地址  是outlook 就打开邮箱
                var str = user.Email.Split('@')[1];
                ViewBag.Server = "http://mail." + str;
            }
            else
            {
                return RedirectToAction("ValidComplete",
                    new { name = Encrypt.EncryptString(user.Username), active = "valided" });
                //返回到已经激活页面
            }

            // _portalContext.AsyncSendMail(user.Email, "注册成功！谢谢你的支持", "注册成功");
            //用户邮箱
            //用户邮箱网站 比如163.com
            return View();
        }


        #endregion


        [Inject]
        public IAuthenticationService AuthenticationService { get; set; }

    }
}