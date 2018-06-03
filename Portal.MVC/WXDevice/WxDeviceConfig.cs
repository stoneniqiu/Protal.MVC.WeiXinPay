using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.MVC.WXDevice
{
    public class WxDeviceConfig
    {
        public const string AppId = "wxf6dd794bce9261b5";
        public const string APPSECRET = "f179077517f6ac9d38505f58b2e21d71";
        public const string GetDeviceStatusUrl="https://api.weixin.qq.com/hardware/mydevice/platform/get_device_status?access_token={0}";
        public const string SetDeviceUrl =
            "https://api.weixin.qq.com/hardware/mydevice/platform/ctrl_device?access_token={0}";
        public const string AccessTokenUrl = "https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}";

        public const string GetOpenid =
            "https://api.weixin.qq.com/device/get_openid?access_token={0}&device_type={1}&device_id={2}";

    }

}