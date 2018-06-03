using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Niqiu.Core.Domain.Common;
using Niqiu.Core.Helpers;
using Niqiu.Core.Services;
using Portal.MVC.Models;
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;

namespace Portal.MVC.Controllers
{
    public class AliMessageController : Controller
    {
        private readonly IPhoneCodeSerice _phoneCodeSerice;
        public AliMessageController(IPhoneCodeSerice phoneCodeSerice)
        {
            _phoneCodeSerice = phoneCodeSerice;
        }

        //
        // GET: /AliMessage/

        public static string url = "http://gw.api.taobao.com/router/rest";
        public static string appkey = "23583689";
        public static string secret = "074b6861cb74da5ac98c02c1172e0750";
        public ActionResult Index()
        {
            var res = SendRandomCodeToMobile("18807445880", "stoneniqiu");
            return res;
        }


        public JsonResult SendRandomCodeToMobile(string phone,string username="",int type=0)
        {
            ITopClient client = new DefaultTopClient(url, appkey, secret);
            AlibabaAliqinFcSmsNumSendRequest req = new AlibabaAliqinFcSmsNumSendRequest();
            req.Extend = "";
            req.SmsType = "normal";
            req.SmsFreeSignName = "好油菜";
            var randomCode = GetID();
            //req.SmsParam = "{name:'stone',number:'3345'}";
            req.SmsParam = "{name:'" + username + "',number:'" + randomCode + "'}";
            req.RecNum = phone;
            req.SmsTemplateCode = "SMS_36290127";
            AlibabaAliqinFcSmsNumSendResponse rsp = client.Execute(req);
            //存储结果，发送时间，随机数和手机号
            var code = new PhoneCode
            {
                Mobile = phone,
                Code = randomCode.ToString(),
                Successed = !rsp.IsError,
                ErrorCode = rsp.ErrCode,
                ErrorMsg = rsp.ErrMsg,
                PhoneCodeType = (PhoneCodeType)type
            };
            _phoneCodeSerice.Insert(code);
            if (rsp.IsError)
            {
                Logger.Debug(rsp.ErrCode + " " + rsp.ErrMsg);
            }
            return Json(new { success = !rsp.IsError, message = rsp.ErrMsg, code = rsp.ErrCode },JsonRequestBehavior.AllowGet);
        }

        private int GetID()
        {
            Random rd = new Random();
            int num = rd.Next(1000, 9999);
            return num;

        }

    }
}
