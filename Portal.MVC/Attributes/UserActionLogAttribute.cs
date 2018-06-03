using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ninject;
using Niqiu.Core.Services;

namespace Portal.MVC.Attributes
{
    public class UserActionLogAttribute : ActionFilterAttribute
    {
        private bool _isAdmin;

        public UserActionLogAttribute(bool isadmin = false)
        {
            _isAdmin = isadmin;
        }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string contr = filterContext.RouteData.Values["controller"].ToString();
            string action = filterContext.RouteData.Values["action"].ToString();
            var isAuthenticated = filterContext.HttpContext.Request.IsAuthenticated;
            var user = WorkContext.CurrentUser;
            if (user != null)
            {
                Logger.Info(string.Format("用户{0}访问了{1}/{2},是否认证{3},是否是后台{4}", user.Username, contr, action, isAuthenticated, _isAdmin));
            }
            else
            {
                Logger.Info(string.Format("未登录用户访问了{0}/{1},是否认证{2},是否是后台{3}", contr, action, isAuthenticated, _isAdmin));
                
            }

        }

        [Inject]
        public IWorkContext WorkContext { get; set; }
    }
}