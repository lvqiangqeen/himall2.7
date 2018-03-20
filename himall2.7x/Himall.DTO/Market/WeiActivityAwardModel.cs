using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.CommonModel;

namespace Himall.DTO
{
    public  class WeiActivityAwardModel
    {

        private long _id = 0;
        /// <summary>
        /// 奖等Id
        /// </summary>
        public long Id { get { return _id; } set { _id = value; } }

        /// <summary>
        /// 微信活动Id
        /// </summary>
        public long activityId { get; set; }
        /// <summary>
        /// 奖项等级
        /// </summary>
        public int awardLevel { get; set; }
        /// <summary>
        /// 奖品类型
        /// </summary>
        public WeiActivityAwardType awardType { get; set; }
        /// <summary>
        /// 奖项积分
        /// </summary>
        public int integral { get; set; }
        /// <summary>
        /// 奖项红包Id
        /// </summary>
        public long bonusId { get; set; }
        /// <summary>
        /// 奖项优惠券Id
        /// </summary>
      
        /// <summary>
        /// 红包名称
        /// </summary>
        public string bonusName { get; set; }
        /// <summary>
        /// 优惠券名称
        /// </summary>
        public string couponName { get; set; }
        public long couponId { get; set; }
        /// <summary>
        /// 奖品数量
        /// </summary>
        public int awardCount { get; set; }
        /// <summary>
        /// 中奖概率
        /// </summary>
        public float proportion { get; set; }
    }
}
