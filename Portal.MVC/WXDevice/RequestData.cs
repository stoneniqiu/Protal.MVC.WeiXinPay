using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.MVC.WXDevice
{
    public class RequestData
    {
        public string device_type { get; set; }
        public string device_id { get; set; }
        public string user { get; set; }
        public Service services { get; set; }
        public object data { get; set; }
    }


}