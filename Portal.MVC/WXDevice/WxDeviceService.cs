using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;
using Newtonsoft.Json;
using Niqiu.Core.Domain.Common;
using Portal.MVC.Models;
using Portal.MVC.WxPayAPI.lib;
using Senparc.Weixin;
using Senparc.Weixin.Exceptions;
using WxPayAPI;
using SendHelp= Senparc.Weixin.CommonAPIs.CommonJsonSend;

namespace Portal.MVC.WXDevice
{
    public class WxDeviceService:IWxDeviceService
    {
        //private readonly ICacheManager _cacheManager;
        //public WxDeviceService(ICacheManager cacheManager)
        //{
        //    _cacheManager = cacheManager;
        //}

        public TokenResult GetAccessToken()
        {
            var url = string.Format(WxDeviceConfig.AccessTokenUrl, WxPayConfig.APPID, WxPayConfig.APPSECRET);
            var res = SendHelp.Send<TokenResult>(null, url, null, CommonJsonSendType.GET);
            return res;
        }

        public T GetWxResponse<T>(HttpRequestBase request)
        {
            Stream postData = request.InputStream;
            StreamReader sRead = new StreamReader(postData);
            string postContent = sRead.ReadToEnd();
            if (!string.IsNullOrEmpty(postContent))
            {
                Logger.Debug("收到数据:"+postContent);
            }
            try
            {

                return JsonConvert.DeserializeObject<T>(postContent);
            }
            catch (Exception e)
            {
                Logger.Debug(e.Message);
                throw;
            }
        }

        public WxResponseData GetDeviceStatus(HttpRequestBase request)
        {
            Stream postData = request.InputStream;
            StreamReader sRead = new StreamReader(postData);
            string postContent = sRead.ReadToEnd();
            if (!string.IsNullOrEmpty(postContent))
            {
                Logger.Debug("收到数据:" + postContent);
            }
            try
            {
                var data = JsonConvert.DeserializeObject<WxResponseData>(postContent);
                data.rawStr = postContent;
                Logger.Debug("转换消息状态:" + data.asy_error_msg);
                return data;
            }
            catch (Exception e)
            {
                Logger.Debug(e.Message);
                throw;
            }
        }

        public OpenApiResult SendTemplateMessage(string token,TemplateModel model)
        {
            var url = string.Format("https://api.weixin.qq.com/cgi-bin/message/template/send?access_token={0}", token);
            try
            {
                var res = SendHelp.Send<OpenApiResult>(token, url, model);
                return res;
            }
            catch (Exception e)
            {
                
                return new OpenApiResult(){error_code = -1,error_msg = e.Message};
            }
          
        } 

        public OpenApiResult RequestDeviceStatus(string accessToken, RequestData data)
        {
            var url = string.Format(WxDeviceConfig.GetDeviceStatusUrl, accessToken);
            return SendHelp.Send<OpenApiResult>(accessToken, url, data);
        }

        public jsapiTicketModel GetJsApiTicket(string accessToken)
        {
            var url = string.Format(WxPayConfig.Jsapi_ticketUrl, accessToken);
            return SendHelp.Send<jsapiTicketModel>(accessToken, url, "", CommonJsonSendType.GET);
        }


        public OpenApiResult SetDevice(string accessToken, RequestData data)
        {
            var url = string.Format(WxDeviceConfig.GetDeviceStatusUrl, accessToken);
            return SendHelp.Send<OpenApiResult>(accessToken, url, data);
        }

        public string GetOpenId(string accessToken,string deviceType,string deviceId)
        {
            try
            {
                var url = string.Format(WxDeviceConfig.GetOpenid, accessToken, deviceType, deviceId);
                var res = SendHelp.Send<OpenIdResult>(accessToken, url, null, CommonJsonSendType.GET);
                return res.GetOpenId();
            }
            catch (ErrorJsonResultException e)
            {
                Logger.Debug(e.Message);
                throw;
            }
        }
    }
}