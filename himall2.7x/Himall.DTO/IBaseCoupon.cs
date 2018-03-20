using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.ViewModel
{
    // 1.因历史遗留问题无法采用继承方式，只能选择接口
    // 2.为了不影响数据库字段采用冗余方式抽象出优惠类
    /// <summary>
    /// 优惠模型
    /// </summary>
    public class BaseCoupon
    {
        public long BaseId { get; set; }

        public decimal BasePrice { get; set; }

        public string BaseName { get; set; }

        public CouponType BaseType { get; set; }

        public string BaseShopName { get; set; }

        public DateTime BaseEndTime { get; set; }

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
