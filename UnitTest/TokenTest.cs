using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Niqiu.Core.Domain.User;
using Niqiu.Core.Services;

namespace UnitTest
{
    [TestClass]
    public class TokenTest
    {
        private UserDbService _userDb=new UserDbService();
        /// <summary>
        /// 测试token加密解密
        /// </summary>
        [TestMethod]
        public void CreateToken()
        {
            var token = _userDb.CreateToken(new User() {Username = "stone"});
            var result = _userDb.validToken(token);
            //随意创建的user，guid对不上，所以是非法
            Assert.IsTrue(result.Code=="006");
        }


        [TestMethod]
        public void ValidToken()
        {
            var user = _userDb.GetUserByUsername("stone");
            var token = _userDb.CreateToken(user);
            var result = _userDb.validToken(token);
            Assert.IsTrue(result.Code == "001");
        }
    }
}
