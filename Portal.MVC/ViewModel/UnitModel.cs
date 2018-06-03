using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.MVC.ViewModel
{
    public class UnitModel
    {
        public int key { get; set; }
        public decimal value { get; set; }

        public string Date { get; set; }
    }

    public class LineList
    {
        public LineList()
        {
            data=new List<UnitModel>();
        }
        public List<UnitModel> data { get; set; }

        public int firstYear { get; set; }

        public string Name { get; set; }
    }
}