using System;
using System.ComponentModel.DataAnnotations;

namespace Niqiu.Core.Domain
{
    public  class BaseEntity
    {

        [Key]
        public int Id { get; set; }
        [Display(Name = "创建时间")]
        public DateTime CreateTime { get; set; }
        [Display(Name = "更新时间")]
        public DateTime ModifyTime { get; set; }

        public BaseEntity()
        {
            CreateTime = DateTime.Now;
            ModifyTime = DateTime.Now;
        }


        public string SimpleTime(DateTime? time = null)
        {
            if (time == null) time = CreateTime;

            var now = DateTime.Now;

            TimeSpan sp = now - time.Value;
            if (sp.TotalMinutes < 2)
            {
                return "刚刚";
            }
            if (sp.TotalMinutes < 60)
            {
                return GetInt(sp.TotalMinutes) + "分钟前";
            }

            if (sp.TotalHours < 24)
            {
                return GetInt(sp.TotalHours)+"小时前";
            }
            if (sp.TotalDays < 2)
            {
                return "昨天";
            }
            if (sp.TotalDays < 7)
            {
                return GetInt(sp.TotalDays)+"天前";
            }
            if (sp.TotalDays < 8)
            {
                return "一周前";
            }
            return time.Value.ToString();
        }

        public int  GetInt(double data)
        {
            return (int) Math.Floor(data);
        }
    }
}
