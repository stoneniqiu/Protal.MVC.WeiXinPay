using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Niqiu.Core.Domain.User;

namespace Portal.MVC.ViewModel
{
    public class MeanChartModel
    {
        private Dictionary<string, int> Menus { get; set; }

        private int UserNum { get; set; }
        private int AllDays { get; set; }

        public MeanChartModel(int usernu,int days)
        {
            Menus=new Dictionary<string, int>();
            UserNum = usernu;
            AllDays = days;
        }

        public void Add(string menuName)
        {
            if (Menus.ContainsKey(menuName))
            {
                Menus[menuName]++;
            }
            else
            {
                Menus.Add(menuName,1);
            }
        }

        public IList<MenuUnit> GetOrderList()
        {
            var all = Menus.Values.Sum();
            var list = Menus.Select(menu => new MenuUnit()
            {
                Name = menu.Key, Count = menu.Value,
                Percent = Math.Round((double)menu.Value / all * 100, 2) + "%",
                PerDay = Math.Round((double)menu.Value / AllDays,2),
                PerUser = Math.Round((double)menu.Value / UserNum,2),
            }).ToList();

            return list;
        }

   
    }

    public class MenuUnit
    {
        public string Name { get; set; }

        public int Count { get; set; }

        public string Percent { get; set; }

        public double PerDay { get; set; }

        public double PerUser { get; set; }
    }

}