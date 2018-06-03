using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.MVC.WXDevice
{
    public class tempe_humidity
    {
        public double humidity { get; set; }
        public double temperature { get; set; }
        public tempe_humidity_value_range value_range { get; set; }
    }

    public class tempe_humidity_value_range
    {
      public  string humidity { get; set; }
    }
}