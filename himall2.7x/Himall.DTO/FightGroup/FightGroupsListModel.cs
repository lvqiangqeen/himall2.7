using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Core;
using Himall.Model;
using Himall.CommonModel;

namespace Himall.DTO
{
    /// <summary>
    /// 拼团列表
    /// </summary>
    public class FightGroupsListModel
    {
        /// <summary>
        /// 编号
        ///</summary>
        public long Id { get; set; }
        /// <summary>
        /// 团长用户编号
        ///</summary>
        public long HeadUserId { get; set; }
        /// <summary>
        /// 团长用户名
        /// </summary>
        public string HeadUserName { get; set; }
        /// <summary>
        /// 团长头像
        /// </summary>
        public string HeadUserIcon { get; set; }
        /// <summary>
        /// 对应活动
        ///</summary>
        public long ActiveId { get; set; }
        /// <summary>
        /// 开团时间
        ///</summary>
        public DateTime AddGroupTime { get; set; }
        /// <summary>
        /// 人数限制
        ///</summary>
        public int LimitedNumber { get; set; }
        /// <summary>
        /// 时间限制
        /// </summary>
        public decimal LimitedHour { get; set; }
        /// <summary>
        /// 已参团人数
        ///</summary>
        public int JoinedNumber { get; set; }
        /// <summary>
        /// 是否异常
        ///</summary>
        public bool IsException { get; set; }
        /// <summary>
        /// 组团状态 成团中  成功   失败
        ///</summary>
        public FightGroupBuildStatus BuildStatus { get; set; }
        /// <summary>
        /// 组团状态
        /// </summary>
        public string ShowBuildStatus
        {
            get
            {
                return this.BuildStatus.ToDescription();
            }
        }
        /// <summary>
        /// 结束时间 成功或失败的时间
        ///</summary>
        public DateTime OverTime { get; set; }
        /// <summary>
        /// 参团订单
        /// <para>已付款的订单</para>
        /// </summary>
       public List<long> OrderIds { get; set; }
        /// <summary>
        /// 距结束小时
        /// </summary>
        public decimal GetEndHour
        {
            get
            {
                decimal result = 0;
                result = (decimal)(this.LimitedHour - (decimal)((DateTime.Now - this.AddGroupTime).TotalHours));
                return result;
            }
        }
        public string EndHourOrMinute { get; set; }
        /// <summary>
        /// 显示时间(小于1小时显示分钟)
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public string ShowHourOrMinute(decimal num)
        {
            string result = num.ToString("#.##") + "小时";
            if (num < 1)
            {
                result = ((int)(num * 60)).ToString() + "分钟";
            }
            return result;
        }
        /// <summary>
        /// 团组时限（秒）
        /// </summary>
        public int Seconds { get; set; }
    }
}
