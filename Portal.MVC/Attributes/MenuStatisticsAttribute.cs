using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Ninject;
using Niqiu.Core.Domain;
using Niqiu.Core.Domain.Common;
using Niqiu.Core.Domain.User;
using Niqiu.Core.Services;
using Portal.MVC.Models;

namespace Portal.MVC.Attributes
{
    public class MenuStatisticsAttribute : ActionFilterAttribute
    {
        [Inject]
        public IWorkContext WorkContext { get; set; }

        [Inject]
        public IUserService UserService { get; set; }


        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext == null || filterContext.HttpContext == null || filterContext.HttpContext.Request == null)
                return;
             

            //don't apply filter to child methods
            if (filterContext.IsChildAction)
                return;

            //only GET requests
            if (!String.Equals(filterContext.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                return;

            string contr = filterContext.RouteData.Values["controller"].ToString();
            string action = filterContext.RouteData.Values["action"].ToString();

            var user = WorkContext.CurrentUser??new User();
            using (var  db=new PortalDb())
            {
                var ms = new MenuStatistic();
                ms.ControllerName = contr;
                ms.ActionName = action;
                ms.MenuName = contr + "/" + action;
                ms.Url = filterContext.RequestContext.HttpContext.Request.Path;
                ms.UserId = user.Id;
                ms.UserName = user.Username;
                db.MenuStatistics.Add(ms);
                db.SaveChanges();
            }
        }
    }
}