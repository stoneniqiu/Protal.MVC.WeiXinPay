using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Niqiu.Core.Services;

namespace UnitTest
{
    [TestClass]
   public class ConnectStringTest
    {
        [TestMethod]
        public void  ReadXml()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("Entity.config");
            XmlNode node = doc.SelectSingleNode("/Root/connectionString");
            Assert.IsNotNull(node.InnerText);
        }

        [TestMethod]
        public void DbTest()
        {
            var db = new PortalDb();
            
        }
    }
}
