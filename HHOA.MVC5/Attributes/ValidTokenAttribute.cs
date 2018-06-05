using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Filters;
using Niqiu.Core.Services;


namespace Portal.MVC5.Attributes
{
    public class ValidTokenAttribute : ActionFilterAttribute
    {
        private UserDbService _userService = new UserDbService();
        public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext filterContext)
        {
            var token = _userService.GetToken();
            //进行验证
            var res = _userService.validToken(token);
            if (res.Code != "001")
            {
                filterContext.Response = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK);
                filterContext.Response.Content = new ObjectContent(typeof(object), res, new JsonMediaTypeFormatter());
            }
        }
    }
}