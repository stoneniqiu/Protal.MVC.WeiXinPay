using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.MVC.WXDevice
{
    public class OpenIdResult
    {
        private List<string> _openId;

        public List<string> open_id
        {
            get { return _openId??(_openId=new List<string>()); }
            set { _openId = value; }
        }

        public resp_msg resp_msg { get; set; }

        public string GetOpenId()
        {
            if (open_id!=null&&open_id.Count==3)
            {
                return open_id[2];
            }
            return "";
        }
    }

    public class resp_msg
    {
        public int ret_code { get; set; }
        public string error_info { get; set; }
    }
}