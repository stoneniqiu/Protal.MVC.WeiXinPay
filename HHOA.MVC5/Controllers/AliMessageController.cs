using System;
using System.Web.Mvc;
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;

namespace Portal.MVC5.Controllers
{
    public class AliMessageController : Controller
    {
        //
        // GET: /AliMessage/

        public static string url = "http://v.juhe.cn/sms/send";
        public static string appkey = "23583689";
        public static string secret = "074b6861cb74da5ac98c02c1172e0750";
        public ActionResult Index()
        {
            ITopClient client = new DefaultTopClient(url, appkey, secret);
            AlibabaAliqinFcSmsNumSendRequest req = new AlibabaAliqinFcSmsNumSendRequest();
            req.Extend = "123456";
            req.SmsType = "normal";
            req.SmsFreeSignName = "阿里大于";
            req.SmsParam = "{\"code\":\"1234\",\"product\":\"alidayu\"}";
            req.RecNum = "13000000000";
            req.SmsTemplateCode = "SMS_585014";
            AlibabaAliqinFcSmsNumSendResponse rsp = client.Execute(req);
            Console.WriteLine(rsp.Body);
            return Content(rsp.Body);
        }

    }
}
