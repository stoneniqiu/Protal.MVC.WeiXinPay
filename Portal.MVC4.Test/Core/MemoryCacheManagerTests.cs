using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Niqiu.Core.Domain.Common;
using Portal.MVC4.Test.Tests;

namespace Portal.MVC4.Test.Core
{
    [TestClass]
    public class MemoryCacheManagerTests
    {
        [TestMethod]
        public void Can_set_and_get_object_from_cache()
        {
            var cacheManager = new MemoryCacheManager();
            cacheManager.Set("key",3,int.MaxValue);
            cacheManager.Get<int>("key").ShouldEqual(3);
        }
    }
}
