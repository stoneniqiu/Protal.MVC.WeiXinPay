using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.MVC.WXDevice
{
    public class WxResponseData
    {
        public int asy_error_code { get; set; }

        public string asy_error_msg { get; set; }

        public string create_time { get; set; }

        public string msg_id { get; set; }

        /// <summary>
        /// notify 说明是设备变更
        /// set_resp 说明是设置设备
        /// get_resp 说明获取设备信息
        /// </summary>
        public string msg_type { get; set; }

        public string device_type { get; set; }
        public string device_id { get; set; }
        public object data { get; set; }

        public Service services { get; set; }

        public string user { get; set; }

        public string rawStr { get; set; }
    }
}