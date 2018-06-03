using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.MVC.WXDevice
{
    public class Service
    {
        public lightbulb lightbulb { get; set; }

        public air_conditioner air_conditioner { get; set; }

        public power_switch power_switch { get; set; }

        public operation_status operation_status { get; set; }

        public tempe_humidity tempe_humidity { get; set; }
    }
}