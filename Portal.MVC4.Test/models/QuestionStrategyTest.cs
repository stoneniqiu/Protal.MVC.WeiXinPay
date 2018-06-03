using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Niqiu.Core.Domain.Questions;

namespace Portal.MVC4.Test.models
{
    [TestClass]
    public class QuestionStrategyTest
    {
        private Question onlymodel;
        private Question averageModel;
        private Question declineModel;

        private QuestionStrategy KeyWordStrategy;
        private QuestionStrategy WordNumStrategy;
        private QuestionStrategy AnswerStrategy;

        [TestInitialize]
        public void Init()
        {
            onlymodel=new Question()
            {
                Title = "strategyTest",
                Answer = "1234",
                Reward = 10,
                RewardType = RewardType.Only,
            };
            averageModel = new Question()
            {
                Title = "strategyTest",
                Answer = "1234",
                Reward = 10,
                RewardType = RewardType.Average,
                RewardPeopleNum = 10
            };
            declineModel = new Question()
            {
                Title = "strategyTest",
                Answer = "1234",
                Reward = 10,
                RewardType = RewardType.Decline,
                RewardPeopleNum = 10
            };
            KeyWordStrategy = new QuestionStrategy() { SystemName = SystemQuestionStrategyName.KeyWord, MinRate = (decimal)0.1, MaxRate = (decimal)0.5, StartRate = (decimal)0.1 };
            WordNumStrategy = new QuestionStrategy() { SystemName = SystemQuestionStrategyName.WordNum, MinRate = (decimal)0.2, MaxRate = (decimal)0.2, StartRate = (decimal)0.2 };
            AnswerStrategy = new QuestionStrategy() { SystemName = SystemQuestionStrategyName.Answer, MaxRate = 1, MinRate = 1, StartRate = 1 };
        }


        [TestMethod]
        public void Only_keyTest()
        {
            KeyWordStrategy.Seed(onlymodel,1);
            Assert.AreEqual(KeyWordStrategy.Price,2);
        }
        [TestMethod]
        public void Only_NumTest()
        {
            WordNumStrategy.Seed(onlymodel, 0);
            Assert.AreEqual(WordNumStrategy.Price, 2);
        }
        [TestMethod]
        public void Only_AnswerTest()
        {
            AnswerStrategy.Seed(onlymodel, 0);
            Assert.AreEqual(AnswerStrategy.Price, 10);
        }

        [TestMethod]
        public void Average_NumTest()
        {
            WordNumStrategy.Seed(averageModel, 0);
            Assert.AreEqual(WordNumStrategy.Price, (decimal)0.2);
        }
        [TestMethod]
        public void Average_KeyTest()
        {
            KeyWordStrategy.Seed(averageModel, 1);
            Assert.AreEqual(KeyWordStrategy.Price, (decimal)0.2);
        }
        public void Average_Key_MaxTest()
        {
            KeyWordStrategy.Seed(averageModel, 10);
            Assert.AreEqual(KeyWordStrategy.Price, (decimal)0.5);
        }

        [TestMethod]
        public void Average_AnswerTest()
        {
            AnswerStrategy.Seed(averageModel, 0);
            Assert.AreEqual(AnswerStrategy.Price, 1);
        }
        [TestMethod]
        public void Decline_KeyTest()
        {
            KeyWordStrategy.Seed(declineModel, 1);
            Assert.AreEqual(KeyWordStrategy.Price, (decimal)0.38);
        }
        [TestMethod]
        public void Decline_Key_MaxTest()
        {
            KeyWordStrategy.Seed(declineModel, 5);
            Assert.AreEqual(KeyWordStrategy.Price, (decimal)0.95);
        }
        [TestMethod]
        public void Decline_NumTest()
        {
            WordNumStrategy.Seed(declineModel, 1);
            Assert.AreEqual(WordNumStrategy.Price, (decimal)0.38);
        }

        [TestMethod]
        public void Decline_AnswerTest()
        {
            AnswerStrategy.Seed(declineModel, 0);
            Assert.AreEqual(AnswerStrategy.Price, (decimal)1.9);
        }
    }
}
