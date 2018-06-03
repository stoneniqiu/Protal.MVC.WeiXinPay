using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Niqiu.Core.Domain;
using Niqiu.Core.Domain.Common;
using Niqiu.Core.Domain.Payments;
using Niqiu.Core.Domain.Questions;
using Niqiu.Core.Domain.User;
using Niqiu.Core.Services;
using Portal.MVC.ViewModel;

namespace Portal.MVC.Areas.Admin.Controllers
{
    public class ChartsController : AdminBaseController
    {
        private readonly IRepository<User> _useRepository;
        private readonly IRepository<MenuStatistic> _menuRepository;
        private readonly IRepository<Question> _questionRepository;
        private readonly IRepository<PaymentLog> _payRepository;
        public ChartsController(IRepository<PaymentLog> payRepository, IRepository<Question> questionRepository, IRepository<User> useRepository, IRepository<MenuStatistic> menuRepository)
        {
            _useRepository = useRepository;
            _menuRepository = menuRepository;
            _questionRepository = questionRepository;
            _payRepository = payRepository;
        }


        public ActionResult UserChart()
        {
            return View();
        }

        public ActionResult MonthUser()
        {
            //每个月新增用户统计
            //先找到第一个用户
            //再找到最后一个用户
            //var fuser = _useRepository.Table.First();
            //var lastuser = _useRepository.Table.Last();
            var alluser = _useRepository.Table.ToList().OrderBy(n => n.CreateTime).ToList();
            var baesMenu = alluser.Select(n => n as BaseEntity).ToList();
            var list = getMonthJsonList(baesMenu);
            return Json(list);
        }

        public ActionResult DailyUser()
        {
            var alluser = _useRepository.Table.ToList().OrderBy(n => n.CreateTime).ToList();
            var baesMenu = alluser.Select(n => n as BaseEntity).ToList();
            var list = getDayJsonList(baesMenu);
            return Json(list);
        }

        //日均统计
        public ActionResult MenuChartJson(DateTime? start, DateTime? end)
        {
            var menus = _menuRepository.Table.OrderBy(n => n.CreateTime).ToList();
            var baesMenu = menus.Select(menuStatistic => menuStatistic as BaseEntity).ToList();
            var list = getDayJsonList(baesMenu);
            return Json(list);
        }


        private LineList getDayJsonList(List<BaseEntity> menus)
        {
            var list = new LineList();
            if (menus == null || !menus.Any()) return list;
            var dics = new Dictionary<string, int>();
            var first = menus[0];
            var last = menus.Last();
            var sp = last.CreateTime - first.CreateTime;
            var totalDays = Math.Ceiling(sp.TotalDays);
            for (int i = 0; i < totalDays; i++)
            {
                var date = first.CreateTime.AddDays(i);
                if(date>last.CreateTime) break;
                var datekey = date.ToShortDateString();
                if (!dics.ContainsKey(datekey))
                {
                    var cout = menus.Count(n => n.CreateTime.ToShortDateString() == datekey);
                    dics.Add(datekey, cout);
                    var model = new UnitModel() { Date = datekey, key = i, value = cout };
                    list.data.Add(model);
                }
            }
            return list;
        }

        private LineList getMonthJsonList(List<BaseEntity> alluser)
        {
            var first = alluser[0];
            var last = alluser.Last();
            //todo 还未考虑年份
            var firstMonth = first.CreateTime.Month;
            var firstYear = first.CreateTime.Year;
            var lastMonth = last.CreateTime.Month;
            var lastYear = last.CreateTime.Year;

            var diffMonth = (lastYear - firstYear) * 12 + (lastMonth - firstMonth);
            var list = new LineList();
            for (int i = firstMonth, y = firstYear; i <= diffMonth + firstMonth && y <= lastYear; i++)
            {
                var cm = i % 12;
                if (cm == 0) cm = 12;
                var current = alluser.Count(n => n.CreateTime.Month == cm && n.CreateTime.Year == y);
                var model = new UnitModel();
                model.key = i;
                model.value = current;
                model.Date = MonthStr(new DateTime(y, cm, 1));
                list.data.Add(model);
                if (i % 12 == 0) y++;
            }
            list.firstYear = firstYear;
            return list;
        }

        private string MonthStr(DateTime date)
        {
            return string.Format("{0}/{1}", date.Year, date.Month);
        }

        public ActionResult MenuChart()
        {
            var menus = _menuRepository.Table.ToList();
            var len = _useRepository.Table.Count();
            var first = menus[0];
            var last = menus.Last();
            var timespan = last.CreateTime - first.CreateTime;
            var totalDays = (int)timespan.TotalDays;
            if (totalDays <= 1) totalDays = 1;
            var model = new MeanChartModel(len, totalDays);
            foreach (var menuStatistic in menus)
            {
                model.Add(menuStatistic.MenuName);
            }
            return View(model);
        }

        public ActionResult QuestionChart()
        {

            return View();
        }

        public ActionResult QuestionJson()
        {
            var menus = _questionRepository.Table.OrderBy(n => n.CreateTime).ToList();
            var baesMenu = menus.Select(n => n as BaseEntity).ToList();
            var list = getDayJsonList(baesMenu);
            return Json(list);
        }

        public ActionResult QuestionTypeJson()
        {
            var menus = _questionRepository.Table.ToList();
            var mutiList = new List<LineList>();

            var type1 = menus.Where(n => n.RewardType == RewardType.Average).OrderBy(n => n.CreateTime).Select(n => n as BaseEntity).ToList();
            var list1 = getDayJsonList(type1);
            list1.Name = "多人平均";
            mutiList.Add(list1);

            var type2 = menus.Where(n => n.RewardType == RewardType.Decline).OrderBy(n => n.CreateTime).Select(n => n as BaseEntity).ToList();
            var list2 = getDayJsonList(type2);
            list2.Name = "多人递减";
            mutiList.Add(list2);

            var type3 = menus.Where(n => n.RewardType == RewardType.Only).OrderBy(n => n.CreateTime).Select(n => n as BaseEntity).ToList();
            var list3 = getDayJsonList(type3);
            list3.Name = "一人获得";
            mutiList.Add(list3);

            return Json(mutiList);
        }



        public ActionResult CapitalChart()
        {
            // 所有人入账资金
            ViewBag.RechargeAll = _payRepository.Table.Where(n => n.OrderType == OrderType.Recharge).Sum(n => n.Amount);
            ViewBag.RewardAll = _payRepository.Table.Where(n => n.OrderType == OrderType.Reward).Sum(n => n.Amount);
            ViewBag.QuestionStrategyAll = _payRepository.Table.Where(n => n.OrderType == OrderType.QuestionStrategy).Sum(n => n.Amount);
            ViewBag.QuestionAll = _questionRepository.Table.Sum(n => n.Reward);
            ViewBag.UnReward = _questionRepository.Table.Sum(n => n.RemanidReward);
            // 悬赏的资金
            // 已经被领取的资金

            return View();
        }

        public ActionResult RecordJson()
        {
            var menus = _payRepository.Table.Where(n => n.OrderType == OrderType.Recharge).ToList();

            var list = new LineList();
            var dics = new Dictionary<string, decimal>();
            var first = menus[0];
            var last = menus.Last();
            var sp = last.CreateTime - first.CreateTime;
            var totalDays = Math.Ceiling(sp.TotalDays);
            for (int i = 0; i < totalDays; i++)
            {
                var date = first.CreateTime.AddDays(i);
                if (date > last.CreateTime) break;
                var datekey = date.ToShortDateString();
                if (!dics.ContainsKey(datekey))
                {
                    var cout = menus.Where(n => n.CreateTime.ToShortDateString() == datekey).Sum(n=>n.Amount);
                    dics.Add(datekey, cout);
                    var model = new UnitModel() { Date = datekey, key = i, value = cout };
                    list.data.Add(model);
                }
            }


         
            return Json(list);
        }

        public ActionResult RewardJson()
        {
            var menus = _questionRepository.Table.ToList();
             var list = new LineList();
            var dics = new Dictionary<string, decimal>();
            var first = menus[0];
            var last = menus.Last();
            var sp = last.CreateTime - first.CreateTime;
            var totalDays = Math.Ceiling(sp.TotalDays);
            for (int i = 0; i < totalDays; i++)
            {
                var date = first.CreateTime.AddDays(i);
                if (date > last.CreateTime) break;
                var datekey = date.ToShortDateString();
                if (!dics.ContainsKey(datekey))
                {
                    var cout = menus.Where(n => n.CreateTime.ToShortDateString() == datekey).Sum(n=>n.Reward);
                    dics.Add(datekey, cout);
                    var model = new UnitModel() { Date = datekey, key = i, value = cout };
                    list.data.Add(model);
                }
            }
            return Json(list);
        }


    }
}
