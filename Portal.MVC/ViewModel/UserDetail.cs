using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using Niqiu.Core.Domain.Questions;
using Niqiu.Core.Domain.User;

namespace Portal.MVC.ViewModel
{
    public class UserDetail
    {
       public UserDetail(IEnumerable<Question> items,User user)
       {
           User = user;
           Questions = items;
       }

        public UserDetail()
        {
            
        }
        private IEnumerable<Question> _questions;
        public User User { get; set; }

        public IEnumerable<Question> Questions
        {
            get { return _questions??(_questions=new List<Question>()); }
            set { _questions = value; }
        }
    }
}