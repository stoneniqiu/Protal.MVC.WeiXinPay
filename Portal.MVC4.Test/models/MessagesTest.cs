using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Niqiu.Core.Domain.Messages;

namespace Portal.MVC4.Test.models
{
    [TestClass]
    public class MessagesTest
    {
        private List<Message> messsages;

        [TestInitialize]
        public void init()
        {
            messsages = new List<Message>();
            messsages.Add(new Message() { FromUserId = 1, ToUserId = 2, Content = "test",Id = 1,FromUserHide = true});
            messsages.Add(new Message() { FromUserId = 2, ToUserId = 1, Content = "test", Id = 2,ToUserHide = true});
            messsages.Add(new Message() { FromUserId = 2, ToUserId = 1, Content = "test", Id = 3 });
            messsages.Add(new Message() { FromUserId = 2, ToUserId = 3, Content = "test", Id = 4 });
            messsages.Add(new Message() { FromUserId = 2, ToUserId = 4, Content = "test", Id = 5 });
        }

        [TestMethod]
        public void  AllTest()
        {
            var userid = 1;
            var touserid = 2;
            var query = messsages.Where(n => (n.ToUserId == userid || n.FromUserId == userid) &&
                                             ((n.ToUserId == touserid || n.FromUserId == touserid))).ToList();
            var res = query.Count();
            Assert.AreEqual(res, 3);
        }

        [TestMethod]
        public void FromUserHideTest()
        {
            var userid = 1;
            var touserid = 2;
            var query = messsages.Where(n => ((n.ToUserId == userid&&!n.ToUserHide) || (n.FromUserId == userid&&!n.FromUserHide)) &&
                                             ((n.ToUserId == touserid || n.FromUserId == touserid))).ToList();
            var res = query.Count();
            Assert.AreEqual(res, 1);
        }

        [TestMethod]
        public void ToUserHideTest()
        {
            var userid = 2;
            var touserid = 1;
            var query = messsages.Where(n => ((n.ToUserId == userid && !n.ToUserHide) || (n.FromUserId == userid && !n.FromUserHide)) &&
                                            ((n.ToUserId == touserid || n.FromUserId == touserid))).ToList();
            var res = query.Count();
            Assert.AreEqual(res, 3);
        }
    }
}
