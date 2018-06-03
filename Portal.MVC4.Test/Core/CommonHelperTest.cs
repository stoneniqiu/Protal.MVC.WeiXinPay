using System;
using System.Collections.Specialized;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Niqiu.Core.Helpers;

namespace Portal.MVC4.Test.Core
{
    [TestClass]
    public class CommonHelperTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var xx = "stone吴";
            var key = CommonHelper.UtilIndexCode(xx);

        }

        [TestMethod]
        public void OrderDic()
        {
          
        }
    }
}
