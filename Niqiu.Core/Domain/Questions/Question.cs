using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Niqiu.Core.Domain.Questions
{
    public class Question : BaseEntity
    {
        public Question()
        {
            MinReward = (decimal)0.1;
            RewardPeopleNum = 1;
            Guid = Guid.NewGuid();
            ExpireTime = DateTime.Now.AddDays(7);
        }
        public Guid Guid { get; set; }
        private ICollection<User.User> _rewardUsers;
        private ICollection<Comment> _commnets;
        private ICollection<PraiseLog> _praises;
        private int _commentNum;
        private int _praisesNum;
        private ICollection<Answer> _answers;
        private ICollection<Shared> _shareds;

        [Display(Name = "谜题")]
        [Required(ErrorMessage = "请输入你的问题")]
        public string Title { get; set; }
        [Display(Name = "悬赏金额")]
        [Range(typeof(decimal), "0.01", "10000.00")]
        public decimal Reward { get; set; }

        [Display(Name = "剩余悬赏")]
        [Range(typeof(decimal), "0.00", "10000.00")]
        public decimal RemanidReward { get; set; }

        [Display(Name = "悬赏人数")]
        [Range(1,10000)]
        public int RewardPeopleNum { get; set; }

        [Display(Name = "获奖方式")]
        public RewardType RewardType { get; set; }

        [Display(Name = "提示")]
        public string Tip { get; set; }

        [Display(Name = "图片")]
        public string ImageUrl { get; set; }

        [Display(Name = "答案")]
        public string Answer { get; set; }
        [Display(Name = "是否答对")]
        public bool IsAnsweredRight { get; set; }

        [Display(Name = "是否结束")]
        public bool IsFinished { get; set; }

        [Display(Name = "是否删除")]
        public bool Deleted { get; set; }

        [Display(Name = "浏览次数")]
        [ConcurrencyCheck]
        public int VisitCount { get; set; }

        [Display(Name = "设计者")]
        [ForeignKey("UserId")]
        public virtual User.User User { get; set; }

        [Display(Name = "设计者")]
        public int UserId { get; set; }

        [Display(Name = "最小悬赏")]
        //用于多人递减规则
        public decimal MinReward
        {
            get { return _minReward1; }
            set
            {
                if (value < _minReward) value = _minReward;
                _minReward1 = value;
            }
        }

        //默认七天
        public DateTime  ExpireTime { get; set; }

        public int ExpireHours { get; set; }

        //是否公布答案
        public bool IsShowAnwser { get; set; }

        public bool IsIllegal { get; set; }

        public bool HasShowShare { get; set; }

        public bool IsClassical { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [NotMapped]
        //是否在重发中
        public bool IsResend { get; set; }

        //是否结束
        public bool IsEnd()
        {
            return ExpireTime <= DateTime.Now;
        }


        public Question Clone()
        {
            return new Question()
            {
                Tip = Tip,
                Title = Title,
                UserId = UserId,
                Answer = Answer,
                ExpireTime = DateTime.Now.AddHours(ExpireHours),
                Reward = Reward,
                RemanidReward = Reward,
                RewardPeopleNum = RewardPeopleNum,
                RewardType = RewardType,
                CreateTime = DateTime.Now,
                ModifyTime = DateTime.Now,
                Id = 0,
                ImageUrl = ImageUrl,
                ExpireHours = ExpireHours
            };
        }

        public string ClearAnwser(string answer="")
        {
            if (string.IsNullOrWhiteSpace(answer)) answer = Answer;
            if (string.IsNullOrWhiteSpace(answer)) return "";
            return answer.Replace(" ", "").ToLower();
        }
    
        [Display(Name = "获奖用户")]
        public virtual ICollection<User.User> RewardUsers
        {
            get { return _rewardUsers ?? (_rewardUsers = new Collection<User.User>()); }
            set { _rewardUsers = value; }
        }

        [Display(Name = "评论")]
        public virtual ICollection<Comment> Commnets
        {
            get { return _commnets ?? (_commnets = new Collection<Comment>()); }
            set { _commnets = value; }
        }

        public virtual ICollection<Shared> Shareds
        {
            get { return _shareds??(_shareds=new Collection<Shared>()); }
            set { _shareds = value; }
        }

        public int CommentNum
        {
            get
            {
                if (_commentNum != Commnets.Count) return Commnets.Count;
                return _commentNum;
            }
            set { _commentNum = value; }
        }


        public int PraisesNum { get; set; }
      

        public virtual ICollection<Answer> Answers
        {
            get { return _answers ?? (_answers = new Collection<Answer>()); }
            set { _answers = value; }
        }

        public string GetIndexChar(int index)
        {
            if (index < 0) index = 0;
             if (index >= Answer.Length)
                        {
                            index = Answer.Length-1;
                        }
            return Answer.ToCharArray()[index].ToString();
        }

        private decimal _minReward = new decimal(0.1);
        private decimal _minReward1;

        public SetRewardResult ValidReward(RewardType type, decimal reward, int people)
        {
            var res = new SetRewardResult() { IsSuccess = false };
            if (people < 1)
            {
                res.Message = "悬赏人数非法";
                return res;
            }
            //if (people*_minReward > reward)
            //{
            //    res.Message = string.Format("最小红包不能少于{0}元",_minReward);
            //    return res;
            //}

            if (reward < _minReward)
            {
                res.Message = "金额非法";
                return res;
            }
            var min = people * _minReward;
            if (reward < min)
            {
                res.Message = string.Format("{0}人至少设置{1}元奖励", people, min);
                return res;
            }
            var key = (reward * 100) % people;
            if (type == RewardType.Average && key != 0)
            {
                res.Message = "金额无法平均";
                return res;
            }


            if (type == RewardType.Decline)
            {
                //需要验证min 和 gap
                if (MinReward < _minReward) res.Message = string.Format("最小值不能小于{0}元", _minReward);
                //min不能大于平均值。gap可以是0，gap的最大值如何计算 计算一个等差数列
                var pre = reward / people;
                if (MinReward > pre)
                {
                    res.Message = string.Format("多人递减的最小值不能大于平均值({0})", pre);
                    return res;
                }
                //等差求和 reward=min+ n*gap/2
                //允许末尾相等呢
            }
            res.IsSuccess = true;
            return res;
        }

        public bool ReduceRemanidReward(decimal money)
        {
            if (money <= RemanidReward)
            {
                RemanidReward = RemanidReward - money;

                //悬赏是否结束呢？
                if (RemanidReward == 0)
                {
                    IsFinished = true;
                }

                return true;
            }
            return false;
        }

        public SetRewardResult ValidReward()
        {
            return ValidReward(RewardType, Reward, RewardPeopleNum);
        }

        public decimal[] GetReward()
        {
            var error = new decimal[RewardPeopleNum];
            var res = ValidReward();
            if (!res.IsSuccess) return error;

            var total = Reward;

            switch (RewardType)
            {
                case RewardType.Only:
                    return new[] { total };
                case RewardType.Average:
                    return GetAverages(Reward, RewardPeopleNum);
                case RewardType.Decline:
                    return GetDeclines(Reward, RewardPeopleNum, MinReward);
                default:
                    return error;
            }
        }

        private decimal[] GetAverages(decimal reward, int people)
        {
            var per =  reward / people;
            var ds = new decimal[people];
            for (int i = 0; i < people; i++)
            {
                ds[i] = per;
            }
            return ds;
        }

        public TimeSpan DurationTime()
        {
            return ExpireTime - DateTime.Now;
        }
        public string DurationTimeToStr()
        {
            var sp = ExpireTime - DateTime.Now;
            if (sp.TotalSeconds < 0) return "悬赏已结束";
            var h = GetInt(sp.TotalHours);
            var m = (GetInt(sp.TotalMinutes) - h*60);
            var s = GetInt(sp.TotalSeconds - h*3600 - m*60);
            return "悬赏剩余时间:"+h+":"+m.ToString("00")+":"+s.ToString("00");
        }

        public bool ValidAnswer(string answer)
        {
            if (string.IsNullOrEmpty(answer)) return false;
            return Answer.Trim().ToLower() == ClearAnwser(answer);
        }

        private decimal[] GetDeclines(decimal reward, int people, decimal min)
        {
            //需要验证min 和 gap
            //min不能大于平均值。gap可以是0，gap的最大值如何计算 计算一个等差数列
            //求和公式 2*reward=(a0+ak)*n       k=n-1
            //people只有一人
            if (people == 1)
                return new[] { reward };
            //min等于平均值
            if (min == (reward / people) && ((int)reward * 100 % people == 0)) return GetAverages(reward, people);

            var a_0a_n = 2 * reward / people;
            var gap_n_1 = a_0a_n - 2 * min;
            var gap100 = (int)(gap_n_1 / (people - 1) * 100);
            var gap = (decimal)gap100 / 100;
            var results = new decimal[people];

            for (int i = 0; i < people; i++)
            {
                results[i] = min + gap * (people - i - 1);
            }

            var sum = people * (results[0] + results[people - 1]) / 2;
            var more = reward - sum;
            //if (more*100 > people)
            //{
            //    var pin = (int)more*100/people;
            //    for (int i = 0; i < people; i++)
            //    {
            //        results[i] += (decimal)pin / 100;
            //    }
            //    results[0] += (decimal)((int)more*100%people)/100;
            //    return results;
            //}
            results[0] = results[0] + more;

            return results;
        }
    }
}