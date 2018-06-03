using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Niqiu.Core.Domain.Common;

namespace Niqiu.Core.Services
{
   public class StatisticService:IStatisticService
   {
       private IRepository<MenuStatistic> _menuRepository;
       public StatisticService(IRepository<MenuStatistic> menuRepository)
       {
           _menuRepository = menuRepository;
       }
       public void InsertMenuStatistic(MenuStatistic model)
       {

       }
    }
}
