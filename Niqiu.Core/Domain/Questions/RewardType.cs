using System.ComponentModel.DataAnnotations;

namespace Niqiu.Core.Domain.Questions
{
    public enum RewardType
    {
        [Display(Name = "多人平均获得")]
        Average,
        [Display(Name = "一位获得者")]
        Only,
        [Display(Name = "多人递减获得")]
        Decline,
        [Display(Name = "随机获得")]
        Random,
        All
    }
}