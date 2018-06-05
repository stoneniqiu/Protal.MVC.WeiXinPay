using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Niqiu.Core.Services;

namespace UnitTest
{
    [TestClass]
    public class GroupTest
    {
        private UserDbService _userDb = new UserDbService();
        private string groupId = "20180320161532153241";

        [TestMethod]
        public void CreateGroupTest()
        {
            var res=_userDb.AddUserAndCreateGroup(1, new List<int>() {2, 3});
            Assert.IsTrue(res.Code=="001");
        }
        [TestMethod]
        public void CreateGroupTest1()
        {
            var res = _userDb.CreateGroup(1, "测试群");
            Assert.IsTrue(res.Code == "001");
        }
        [TestMethod]
        public void RemoveUsersFromGroup_Cannot_RomveOwner_Test()
        {
            var res = _userDb.RemoveUsersFromGroup(new List<int>() { 1 }, groupId);
            Assert.IsTrue(res.Code == "002");
        }

        [TestMethod]
        public void RemoveUsersFromGroup_Test()
        {
            var res = _userDb.RemoveUsersFromGroup(new List<int>() { 2 }, groupId);
            Assert.IsTrue(res.Code == "001");
        }

        [TestMethod]
        public void AddUserToGroup_Test()
        {
            var res = _userDb.AddUsersToGroup(new List<int>() { 2,3 }, groupId);
            Assert.IsTrue(res.Code == "001");
        }

        [TestMethod]
        public void GroupUserTest()
        {
            var group = _userDb.GetGroup(groupId);
            Assert.IsTrue(group.Users.Count>0);
        }

        [TestMethod]
        public void GetFriendsTest()
        {
            var fs = _userDb.GetFriends("be61f8b7-2b59-4a9b-a917-f82cc2b621fd");
            Assert.IsTrue(fs.Any(n=>n.IsGroup));
        }
    }
}
