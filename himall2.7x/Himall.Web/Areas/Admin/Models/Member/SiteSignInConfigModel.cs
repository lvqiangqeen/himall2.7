using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.Admin.Models
{
    public class SiteSignInConfigModel
    {
        public bool IsEnable { get; set; }
        [Required(ErrorMessage = "请填写每日签到可获取积分")]
        [Range(0,int.MaxValue,ErrorMessage="不可为负数,为零不开启功能")]
        public int DayIntegral { get; set; }
        [Required(ErrorMessage = "请填写连续周期")]
        [Range(0, int.MaxValue, ErrorMessage = "连续周期不可为负数")]
        public int DurationCycle { get; set; }
        [Required(ErrorMessage = "请填写奖励积分数")]
        [Range(0, int.MaxValue, ErrorMessage = "奖励积分数不可为负数")]
        public int DurationReward { get; set; }
    }
}