using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Portal.MVC.WxPayAPI.lib
{
    public class WXShareModel
    {
        public string appId { get; set; }
        public string nonceStr { get; set; }
        public long timestamp { get; set; }

        public string signature { get; set; }

        public string ticket { get; set; }
        public string url { get; set; }

        public void MakeSign()
        {
             var string1Builder = new StringBuilder();
             string1Builder.Append("jsapi_ticket=").Append(ticket).Append("&")
                          .Append("noncestr=").Append(nonceStr).Append("&")
                          .Append("timestamp=").Append(timestamp).Append("&")
                          .Append("url=").Append(url.IndexOf("#") >= 0 ? url.Substring(0, url.IndexOf("#")) : url);
            var string1 = string1Builder.ToString();
            signature = Util.Sha1(string1, Encoding.Default);

        }
    }

    public class jsapiTicketModel
    {
        public string errcode { get; set; }
        public string errmsg { get; set; }

        public string ticket { get; set; }

        public string expires_in { get; set; }
    }
}