using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Niqiu.Core.Domain.Questions;
using Niqiu.Core.Domain.User;

namespace Portal.MVC.ViewModel
{
    public class RewardLog
    {
        public Question Question { get; set; }
        public decimal[] List { get; set; }

    }
}