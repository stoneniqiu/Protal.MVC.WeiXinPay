using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.MVC.WXDevice
{
    public class air_conditioner
    {
        public double tempe_indor { get; set; }
        public double tempe_outdoor { get; set; }
        public double tempe_target { get; set; }
        public int ac_mode { get; set; }
        public int fan_speed { get; set; }
        public int is_horiz_fan_on { get; set; }
        public int is_verti_fan_on { get; set; }
        public air_conditioner_value_range value_range { get; set; }
    }

    public class air_conditioner_value_range
    {
        //空调
        public string ac_mode { get; set; }
        public string fan_speed { get; set; }
        public string is_horiz_fan_on { get; set; }
        public string is_verti_fan_on { get; set; }
    }

   
}