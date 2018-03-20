using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.CommonModel;

namespace Himall.DTO
{
    public class WeiActivityModel
    {
        private long _id = 0;
        /// <summary>
        /// 微信活动Id
        /// </summary>
        public long Id { get { return _id; } set { _id = value; } }
        /// <summary>
        /// 微信活动标题
        /// </summary>
        public string activityTitle { get; set; }
        /// <summary>
        /// 微信活动类型
        /// </summary>
        public WeiActivityType activityType { get; set; }


        /// <summary>
        /// 微信活动分享详细
        /// </summary>
        public string activityDetails { get; set; }
        /// <summary>
        /// 微信活动分享图标URL
        /// </summary>
        public string activityUrl { get; set; }
        /// <summary>
        /// 微信活动开始时间
        /// </summary>
        public DateTime beginTime { get; set; }
        /// <summary>
        /// 微信活动结束时间
        /// </summary>
        public DateTime endTime { get; set; }
        /// <summary>
        /// 微信活动参与次数类型
        /// </summary>
        public WeiParticipateType participationType { get; set; }
        /// <summary>
        /// 微信活动参与次数
        /// </summary>
        public int participationCount { get; set; }
        
        /// <summary>
        /// 微信活动消耗积分
        /// </summary>
        public int consumePoint { get; set; }

        /// <summary>
        /// 是否消耗积分
        /// </summary>
        public int isPoint {
            get {
                if (consumePoint>0)
                {
                    return 1;//消耗
                }
                else
                {
                    return 0;//不消耗
                }
            }
            
            }
        /// <summary>
        /// 微信活动二维码地址
        /// </summary>
        public string codeUrl { get; set; }
        /// <summary>
        /// 微信活动增加时间
        /// </summary>
        public DateTime addDate { get; set; }

        /// <summary>
        /// 微信活动奖项集合
        /// </summary>
        public List<WeiActivityAwardModel> awards
        {
            get;

            set;
        }

        /// <summary>
        /// 活动奖等
        /// </summary>
        public WeiActivityWinModel winModel
        {
            get;
            set;
        }
        /// <summary>
        /// 用户ID
        /// </summary>
        public long userId { get; set; }

        /// <summary>
        /// 活动有效时间
        /// </summary>
        public string validTime
        {
            get
            {
                return beginTime.ToString("yyyy-MM-dd") +"至"+ endTime.ToString("yyyy-MM-dd");
            }
        }

        /// <summary>
        /// 累计次数
        /// </summary>
        public string WeiParticipate
        {
            get
            {
                if (participationType == WeiParticipateType.CommonCount)
                {
                    return "一共" + participationCount + "次";
                }
                else if (participationType == WeiParticipateType.DayCount)
                {
                    return "一天" + participationCount + "次";
                }
                else
                {
                    return "无限制";
                }
            }
        }
        /// <summary>
        /// 抽奖人数
        /// </summary>
        public string totalNumber
        {
            get;
            set;
        }
        /// <summary>
        /// 中奖人数
        /// </summary>
        public string winNumber
        {
            get;
            set;
        }
    }

    
}
