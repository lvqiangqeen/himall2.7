using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.API.Model
{
    /// <summary>
    /// 商品营销活动显示
    /// </summary>
    public class VShopShowPromotionModel
    {
        /// <summary>
        /// 店铺编号
        /// </summary>
        public long ShopId { get; set; }
        /// <summary>
        /// 优惠券
        /// <para>0会不显示</para>
        /// </summary>
        public int CouponCount { get; set; }
        /// <summary>
        /// 满额免运费
        /// <para>0会不显示</para>
        /// </summary>
        public decimal FreeFreight { get; set; }
        /// <summary>
        /// 满就送
        /// <para>0会不显示</para>
        /// </summary>
        public int BonusCount { get; set; }
        /// <summary>
        /// 满就送的金额条件
        /// </summary>
        public decimal BonusGrantPrice { get; set; }
        /// <summary>
        /// 随机红包金额下限
        /// <para>我是猜的 by dzy</para>
        /// </summary>
        public decimal BonusRandomAmountStart { get; set; }
        /// <summary>
        /// 随机红包金额上限
        /// <para>我是猜的 by dzy</para>
        /// </summary>
        public decimal BonusRandomAmountEnd { get; set; }
    }
}
