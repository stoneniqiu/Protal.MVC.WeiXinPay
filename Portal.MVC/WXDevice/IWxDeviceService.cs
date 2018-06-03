using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.MVC.WXDevice
{
    public interface IWxDeviceService
    {
        TokenResult GetAccessToken();
        T GetWxResponse<T>(HttpRequestBase request);
        WxResponseData GetDeviceStatus(HttpRequestBase request);
        string GetOpenId(string accessToken, string deviceType, string deviceId);
    }
}