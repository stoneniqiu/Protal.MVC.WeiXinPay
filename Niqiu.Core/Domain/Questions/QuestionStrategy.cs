using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Niqiu.Core.Domain.Questions
{
    public class QuestionStrategy : BaseEntity
    {
        private decimal _startRate;
        public string Name { get; set; }
        public string SystemName { get; set; }
        //price 现在表示倍数
        public decimal Price { get; set; }

        public decimal MinRate { get; set; }

        public decimal MaxRate { get; set; }

        public decimal StartRate
        {
            get { return _startRate; }
            set
            {
                _startRate = value;
                if (_startRate < MinRate) _startRate = MinRate;
                if (_startRate > MaxRate) _startRate = MaxRate;
            }
        }

        public void Seed(Question model, int keywrodNum)
        {
            if (string.IsNullOrWhiteSpace(SystemName)) return;
            if(model.RewardPeopleNum<1) throw new Exception("悬赏人数不能小于1");
            decimal seed = model.Reward;//一人获得的时候
            //平均的时候
            if (model.RewardType == RewardType.Average)
            {
                seed = model.GetReward()[0];
            }
            if (model.RewardType == RewardType.Decline)
            {
                seed = model.GetReward().Max();
            }

            switch (SystemName)
            {
                case "KeyWord":
                    StartRate = MinRate + keywrodNum * MinRate;
                    Price = seed * StartRate;
                    break;
                case "WordNum":
                    Price = seed * StartRate;
                    break;
                case "Answer":
                    Price = seed * StartRate;
                    break;
            }
            Price=decimal.Round(Price,2);
        }

    }

    public class SystemQuestionStrategyName
    {
        public static string KeyWord = "KeyWord";
        public static string WordNum = "WordNum";
        public static string Answer = "Answer";
    }
}
