using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.MVC.WXDevice
{
    public class lightbulb
    {
        public int alpha { get; set; }
        public lightbulb_value_range value_range { get; set; }
    }

    public class lightbulb_value_range
    {
        public string alpha { get; set; }
    }



    public class operation_status
    {
        /// <summary>
        /// 0为开，1为关
        /// </summary>
        public int status { get; set; }

        public operation_status_value_range value_range { get; set; }
    }

    public class operation_status_value_range
    {
        public string status { get; set; }
    }
}