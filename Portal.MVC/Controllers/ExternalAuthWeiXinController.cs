using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.ApplicationServices;
using System.Web.Mvc;
using Ninject;
using Niqiu.Core.Domain.Common;
using Niqiu.Core.Domain.User;
using Niqiu.Core.Services;
using Portal.MVC.Models.Services;
using Portal.MVC.WXDevice;
using Portal.MVC.WxPayAPI.lib;
using Senparc.Weixin;
using Senparc.Weixin.Open.CommonAPIs;
using Senparc.Weixin.Open.OAuthAPIs;
using WxPayAPI;

namespace Portal.MVC.Controllers
{
    public class ExternalAuthWeiXinController : Controller
    {
        private readonly UserService _userService;
        //这是在开发者平台添加的网站应用的appid和appsecret
        //private string appId = "wx0caae1edac6498a2";
        private readonly ICacheManager _cacheManager;
        //private string appSecret = "a4ebe497840f5006dff0a183033f5b5f";
        public ExternalAuthWeiXinController(UserService userService, ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
            _userService = userService;
        }
        private UserDbService _userDbService=new UserDbService();
        private PayMeentDbService _payMeentDbService=new PayMeentDbService();
        public ActionResult Index()
        {
            Logger.Info("进入到微信");
            var code = Request.QueryString["code"];
            var state = Request.QueryString["state"];
            //若用户禁止授权，则重定向后不会带上code参数，仅会带上state参数
            if (code != null)
            {
                //redirect_uri?code=CODE&state=STATE
                Logger.Info("code为：" + code);
                Logger.Info("state为：" + state);
                //2. 通过code参数加上AppID和AppSecret等，通过API换取access_token；
                var url =
                    string.Format(
                        "https://api.weixin.qq.com/sns/oauth2/access_token?appid={0}&secret={1}&code={2}&grant_type=authorization_code",
                        WxPayConfig.APPID, WxPayConfig.APPSECRET, code);
                var result = CommonJsonSend.Send<OAuthAccessTokenResult>(null, url, null, CommonJsonSendType.GET);
                OAuthUserInfo userInfo = OAuthApi.GetUserInfo(result.access_token, result.openid);
                // Logger.Info("url为：" + url);
                Logger.Info("nickname为：" + userInfo.nickname);
                // Logger.Info("headimgurl为：" + userInfo.headimgurl);
                // Logger.Info("city为：" + userInfo.city);
                // Logger.Info("openid为：" + userInfo.openid);
                var user = CreateUser(userInfo);

                AuthenticationService.SignIn(user, true);

               
            }

            return RedirectToAction("Index", "Home");
        }

        private string ticketKey = "jsapi_ticket";
        private WxDeviceService wxDeviceService = new WxDeviceService();
        public ActionResult H5Login()
        {
            var model = new WXShareModel();
            model.appId = WxPayConfig.APPID;
            model.nonceStr = WxPayApi.GenerateNonceStr();
            model.timestamp = Util.CreateTimestamp();
            model.ticket = GetTicket();
            model.url = "http://www.haoyoucai888.com/ExternalAuthWeiXin/H5Login";// domain + Request.Url.PathAndQuery;
            model.MakeSign();
            Logger.Debug("获取到ticket:" + model.ticket);
            Logger.Debug("获取到签名:" + model.signature);
            return View(model);
        }
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
        private string getToken()
        {
            var token = getcacheToken();
            if (token == null || string.IsNullOrEmpty(token.access_token))//|| checkExpire(token.expires_in)
            {
                token = wxDeviceService.GetAccessToken();
                Logger.Debug("获取到token：" + token.access_token + ",超时:" + token.expires_in);
                setToken(token);
            }
            return token.access_token;
        }

        private string tokenKey = "access_token";
        private void setToken(TokenResult token)
        {
            _cacheManager.Set(tokenKey, token, 7200);
        }

        private TokenResult getcacheToken()
        {
            return _cacheManager.Get<TokenResult>(tokenKey);
        }
        private string getCacheTicket()
        {
            return _cacheManager.Get<string>(ticketKey);
        }
        private void setCacheTicket(string cache)
        {
            _cacheManager.Set(tokenKey, cache, 7200);
        }
        private User CreateUser(OAuthUserInfo userInfo)
        {
            var user = _userService.GetUserByOpenId(userInfo.openid);
            if (user == null)
            {
                Logger.Info("用户不存在");
                user = new User()
                {
                    Username = userInfo.nickname,
                    OpenId = userInfo.openid,
                    AuthType = AuthType.WeiXin,
                    Gender = userInfo.sex.ToString(),
                    Province = userInfo.province,
                    Country = userInfo.country,
                    City = userInfo.city,
                    ImgUrl = userInfo.headimgurl,
                    Active = true

                };
                _userDbService.InsertUser(user);
                var checkuser = _userDbService.GetUserByOpenId(user.OpenId);
                _payMeentDbService.CheckAndCreate(checkuser.Id);
            }
            else
            {
                Logger.Info("更新微信登录信息：" + userInfo.nickname);

                _userDbService.UpdateUserForWx(user.OpenId, userInfo);
            }
            //if (!user.IsRegistered())
            //{
            //    var role = _userService.GetUserRoleBySystemName(SystemUserRoleNames.Registered);
            //    user.UserRoles.Add(role);
            //    _userService.UpdateUser(user);
            //}


            return user;
        }

        [Inject]
        public IAuthenticationService AuthenticationService { get; set; }

    }
}
