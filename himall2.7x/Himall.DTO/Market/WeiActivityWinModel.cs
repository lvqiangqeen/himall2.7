using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.CommonModel;
namespace Himall.DTO
{
    public  class WeiActivityWinModel
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public long userId { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string userName { get; set; }

        /// <summary>
        /// 用户积分
        /// </summary>
        public int integrals { get; set; }
        /// <summary>
        /// 微信活动Id
        /// </summary>
        public long activityId { get; set; }
        /// <summary>
        /// 微信活动奖项Id
        /// </summary>
        public long awardId { get; set; }
        /// <summary>
        /// 是否中奖
        /// </summary>
        public bool isWin { get; set; }
        /// <summary>
        /// 红包Id
        /// </summary>
        public long bonusId { get; set; }

        /// <summary>
        /// 优惠券Id
        /// </summary>
        public long couponId { get; set; }

        /// <summary>
        /// 奖品名称
        /// </summary>
        public string awardName { get; set; }

        /// <summary>
        /// 领取限制
        /// </summary>
        public string amount { get; set; }
        /// <summary>
        /// 添加时间
        /// </summary>
        public DateTime addDate { get; set; }
        /// <summary>
        /// 添加时间
        /// </summary>
        public string strAddDate
        {
            get
            {
                return addDate.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
        /// <summary>
        /// 奖品类型
        /// </summary>
        public WeiActivityAwardType awardType { get; set; }
    }
}
