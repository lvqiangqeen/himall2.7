using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    // 1.因历史遗留问题无法采用继承方式，只能选择接口
    // 2.为了不影响数据库字段采用冗余方式抽象出优惠类
    /// <summary>
    /// 优惠模型
    /// </summary>
    public class BaseCoupon
    {
        /// <summary>
        /// 优惠ID
        /// </summary>
        public long BaseId { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public decimal BasePrice { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string BaseName { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public CouponType BaseType { get; set; }
        /// <summary>
        /// 商铺名称
        /// </summary>
        public string BaseShopName { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime BaseEndTime { get; set; }
        /// <summary>
        /// 商铺ID
        /// </summary>
        public long BaseShopId { get; set; }

        /// <summary>
        /// 买多少减
        /// </summary>
        public decimal OrderAmount { get; set; }
    }


    public enum CouponType
    { 
        /// <summary>
        /// 优惠卷
        /// </summary>
        Coupon = 0 , 

        /// <summary>
        /// 商家红包
        /// </summary>
        ShopBonus = 1
    }
}
