using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Niqiu.Core.Domain.User;

namespace Portal.MVC.ViewModel
{
    public class RewardlistModel
    {
        public RewardlistModel(decimal[] rewardArray,IEnumerable<User> rewardUsers,decimal reward,int num)
        {
            Items=new List<RewardItem>();
            if(rewardArray==null||rewardUsers==null) throw new Exception("参数不能为空");
            var ulist = rewardUsers.ToList();
            if (ulist.Count > rewardArray.Length)
            {
                throw new Exception("获奖用户超额!");
            }
            if (num != rewardArray.Length)
            {
                throw new Exception("金额数组与获奖用户数不匹配!");
            }
            //填充
            Reward = reward;
            RewardNum = num;
            var getCurrent = false;
            for (int i = 0; i < rewardArray.Length; i++)
            {
                var item = new RewardItem()
                {
                    Index = i + 1,
                    Money = rewardArray[i]
                };
                if (ulist[i] != null)
                {
                    item.RewardUser = ulist[i];
                }
                if (!getCurrent && ulist[i] == null)
                {
                    item.IsCurrent = true;
                    getCurrent = true;
                }

            }
        }

        public decimal Reward { get; set; }
        public int  RewardNum { get; set; }

        public List<RewardItem> Items { get; set; } 
    }

    public class RewardItem
    {
        public int Index { get; set; }
        public decimal Money { get; set; }
        public User RewardUser { get; set; }
        public bool IsCurrent { get; set; }
      
    }
}