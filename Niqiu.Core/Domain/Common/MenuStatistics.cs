using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Niqiu.Core.Domain.Common
{
   public class MenuStatistic:BaseEntity
    {
       [Required]
        public string MenuName { get; set; }
       public string ControllerName { get; set; }
       public string ActionName { get; set; }

       public string Url { get; set; }

       //点击人的Id
       public string UserName { get; set; }
       public int  UserId { get; set; }

    }
}
