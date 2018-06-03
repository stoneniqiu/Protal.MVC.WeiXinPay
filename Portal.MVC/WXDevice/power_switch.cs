using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.MVC.WXDevice
{
    public class power_switch
    {
        public bool on_off { get; set; }

        public power_switch_value_range value_range { get; set; }
    }

    public class power_switch_value_range
    {
        //开关
        public string on_off { get; set; }
    }
}