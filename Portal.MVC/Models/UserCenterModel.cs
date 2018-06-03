using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Niqiu.Core.Domain.Payments;
using Niqiu.Core.Domain.User;

namespace Portal.MVC.Models
{
    public class UserCenterModel
    {
        public User User { get; set; }

        public Wallet Wallet { get; set; }
    }
}