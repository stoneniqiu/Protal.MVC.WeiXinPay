using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.MVC.WXDevice
{
    public class UserWxData
    {
        private WxResponseData _responseData;

        public UserWxData()
        {
            CreateTime = DateTime.Now;
        }
        public DateTime CreateTime { get; set; }
        public TokenResult AccessToken { get; set; }

        public WxResponseData ResponseData
        {
            get { return _responseData??(_responseData=new WxResponseData()); }
            set { _responseData = value; }
        }

        public string OpenId { get; set; }
    }
}