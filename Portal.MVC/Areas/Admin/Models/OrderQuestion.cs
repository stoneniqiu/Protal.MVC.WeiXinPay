using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Niqiu.Core.Domain.Payments;
using Niqiu.Core.Domain.Questions;
using Niqiu.Core.Domain.User;

namespace Portal.MVC.Areas.Admin.Models
{
    public class OrderQuestion
    {

        public Order Order { get; set; }
        public Question Question { get; set; }

        public QuestionStrategy QuestionStrategy { get; set; }

        public User User { get; set; }
    }
}