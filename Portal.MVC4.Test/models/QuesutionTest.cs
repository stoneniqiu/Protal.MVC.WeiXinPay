using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Niqiu.Core.Domain.Questions;
using Niqiu.Core.Domain.User;

namespace Portal.MVC4.Test.models
{
    [TestClass]
    public class QuesutionTest
    {
        private Question quesiton;
        private List<Question> questions;

        [TestInitialize]
        public void init()
        {
            quesiton = new Question()
            {
                RewardType = RewardType.Average,
                Reward = 10,
                RewardPeopleNum = 20
            };
            var question2 = new Question();
            var question3 = new Question();
            var user = new User { Username = "stoneniqiu", Id = 1 };
            var testuser = new User() { Username = "test", Id = 2 };
            var testuser1 = new User() { Username = "test1", Id = 3 };
            var testuser2 = new User() { Username = "test2", Id = 4 };
            var answer1 = new Answer() { Content = "key", UserId = 1 };
            var answer2 = new Answer() { Content = "key", UserId = 2 };
            var anws3 = new Answer() { Content = "Xx", UserId = 3 };
            quesiton.Answers.Add(answer1);
            quesiton.Answers.Add(answer2);
            quesiton.RewardUsers.Add(user);
            quesiton.RewardUsers.Add(testuser);

            question2.Answers.Add(answer1);

            question3.Answers.Add(answer2);
            question3.Answers.Add(anws3);
            question3.RewardUsers.Add(testuser1);
            question3.RewardUsers.Add(testuser2);

            var q = new Question();
            q.Answers.Add(answer1);

            questions = new List<Question>();
            questions.Add(quesiton);
            questions.Add(question2);
            questions.Add(question3);
            questions.Add(q);

        }

        [TestMethod]
        public void SearchJoinQuestion_All_Test()
        {
            var taget = questions.Where(question => question.Answers.Any(n => n.UserId == 1)).ToList();
            Assert.AreEqual(3, taget.Count);
        }

        [TestMethod]
        public void SearchJoinQuestion_Reward_Test()
        {
            var taget = questions.Where(question => question.Answers.Any(n => n.UserId == 1)).ToList();
            taget = taget.Where(n => n.RewardUsers.Any(m => m.Id == 1)).ToList();
            Assert.AreEqual(1, taget.Count);
        }
        [TestMethod]
        public void SearchJoinQuestion_NOReward_Test()
        {
            var taget = questions.Where(question => question.Answers.Any(n => n.UserId == 1)).ToList();
            taget = taget.Where(n => n.RewardUsers.All(m => m.Id != 1)).ToList();
            Assert.AreEqual(2, taget.Count);
        }
        [TestMethod]
        public void ValidReward_Can_Average_Test()
        {
            var result = quesiton.ValidReward();
            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod]
        public void ValidReward_MinLimit_Test()
        {
            quesiton.Reward = (decimal)0.07;
            quesiton.RewardPeopleNum = 8;
            var result = quesiton.ValidReward();
            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void ValidReward_CanNot_Average_Test()
        {
            quesiton.Reward = (decimal)0.09;
            quesiton.RewardPeopleNum = 8;
            var result = quesiton.ValidReward();
            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void GetReward_Can_Average_Test()
        {
            quesiton.Reward = 22;
            quesiton.RewardPeopleNum = 11;
            var result = quesiton.GetReward();
            Assert.AreEqual(result[0], 2);
        }
        [TestMethod]
        public void GetReward_CanNot_Average_Test()
        {
            quesiton.Reward = 21;
            quesiton.RewardPeopleNum = 11;
            var result = quesiton.GetReward();
            Assert.AreEqual(result[0], 0);
        }

        [TestMethod]
        public void Decline_Can_Test()
        {
            quesiton.Reward = (decimal)0.55;
            quesiton.RewardPeopleNum = 5;
            quesiton.RewardType = RewardType.Decline;
            quesiton.MinReward = (decimal)0.11;
            var result = quesiton.GetReward();
            Assert.AreEqual(result[0], (decimal)0.11);
        }

        [TestMethod]
        //顺利递减
        public void Decline_Can_Test1()
        {
            quesiton.Reward = 15;
            quesiton.RewardPeopleNum = 5;
            quesiton.RewardType = RewardType.Decline;
            quesiton.MinReward = 1;
            var result = quesiton.GetReward();
            Assert.AreEqual(result[0], 5);
        }

        [TestMethod]
        //带有小数了 超过
        public void Decline_Can_Test2()
        {
            quesiton.Reward = (decimal)15.8;
            quesiton.RewardPeopleNum = 5;
            quesiton.RewardType = RewardType.Decline;
            quesiton.MinReward = 1;
            var result = quesiton.GetReward();
            Assert.AreEqual(quesiton.Reward, result.Sum());
        }


        [TestMethod]
        //带有小数了 低于
        public void Decline_Can_Test3()
        {
            quesiton.Reward = (decimal)13.95;
            quesiton.RewardPeopleNum = 5;
            quesiton.RewardType = RewardType.Decline;
            quesiton.MinReward = 1;
            var result = quesiton.GetReward();
            Assert.AreEqual(quesiton.Reward, result.Sum());
        }

        [TestMethod]
        public void ReplaceEmpty()
        {
            var anwser = " 1 2 3  4";
            var key = anwser.Replace(" ", "");
            Assert.AreEqual(key,"1234");
        }
    }
}
