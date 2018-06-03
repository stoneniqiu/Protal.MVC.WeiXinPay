using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.MVC.WXDevice
{
    public class OpenApiResult
    {
        public int error_code { get; set; }
        public string error_msg { get; set; }

        public string msg_id { get; set; }
    }

    public class TokenResult
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
    }
}