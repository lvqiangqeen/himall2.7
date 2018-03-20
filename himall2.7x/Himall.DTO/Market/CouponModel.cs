using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/**
 * 优惠券实体
 * 2016-05-23
 * **/
namespace Himall.DTO
{
    /// <summary>
    /// 优惠券实体
    /// </summary>
    public class CouponModel
    {
        private long _Id { get; set; }

        /// <summary>
        /// 优惠券ID
        /// </summary>
        public long Id { get { return _Id; } set { _Id = value; } }

        /// <summary>
        /// 优惠券名称
        /// </summary>
        public string CouponName { get; set; }

        /// <summary>
        /// 优惠券面值
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 领取限制，显示字符串
        /// </summary>
        public string OrderAmount { get; set; }

        /// <summary>
        /// 优惠券所在店铺ID
        /// </summary>
        public long ShopId { get; set; }

        /// <summary>
        /// 优惠券所在店铺名称
        /// </summary>
        public string ShopName { get; set; }
        /// <summary>
        /// 优惠券截至日期
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 显示时间
        /// </summary>
        public string strEndTime
        {
            get
            {
                return EndTime.ToString("yyyy-MM-dd");
            }
        }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 开始显示时间
        /// </summary>
        public string strStartTime
        {
            get
            {
                return StartTime.ToString("yyyy-MM-dd");
            }
        }

        /// <summary>
        /// 库存
        /// </summary>
        public int Num { get; set; }

        /// <summary>
        /// 已领取数量
        /// </summary>
        public int useNum { get; set; }

        /// <summary>
        /// 库存数量
        /// </summary>
        public int inventory { get; set; }
        /// <summary>
        /// 限领张数
        /// </summary>
        public int perMax { get; set; }
    }
}
