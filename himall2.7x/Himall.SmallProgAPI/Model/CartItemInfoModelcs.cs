using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.SmallProgAPI.Model
{
    public class CartItemInfoModelcs
    {
        public string SkuID { get; set; }
        public string Quantity { get; set; }
        public string ShippQuantity { get; set; }
        public string IsfreeShipping { get; set; }
        public string IsSendGift { get; set; }
        public string MemberPrice { get; set; }
        public string Name { get; set; }
        public string ProductId { get; set; }
        public string PromoteType { get; set; }
        public string PromotionId { get; set; }
        public string PromotionName { get; set; }
        public string SKU { get; set; }
        public string SkuContent { get; set; }
        public string SubTotal { get; set; }
        public string ThumbnailUrl100 { get; set; }
        public string ThumbnailUrl40 { get; set; }
        public string ThumbnailUrl60 { get; set; }
        public string Weight { get; set; }
        public int Stock { get; set; }
        public string HasStore { get; set; }
        public bool IsMobileExclusive { get; set; }

        /// <summary>
        /// 是否有效（用于在购物车显示失效）
        /// </summary>
        public bool IsValid
        {
            get;
            set;
        }
        public bool HasEnoughStock { get; set; }

        /// <summary>
        /// 供应商ID
        /// </summary>
        public int SupplierId { get; set; }

        /// <summary>
        /// 供应商名称
        /// </summary>
        public string SupplierName { get; set; }

        /// <summary>
        /// 成本价
        /// </summary>
        public decimal CostPrice { get; set; }

        /// <summary>
        /// 门店ID
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// 门店名称
        /// </summary>
        public string StoreName { get; set; }

        /// <summary>
        /// 门店状态
        /// </summary>
        //public DetailException StoreStatus { get; set; }
        public List<ShoppingCartSendGift> SendGift { get; set; }
    }
    /// <summary>
    /// 赠送礼品谢谢
    /// </summary>
    public class ShoppingCartSendGift
    {
        /// <summary>
        /// 礼品ID
        /// </summary>
        public int GiftId { get; set; }

        /// <summary>
        /// 礼品名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 购买数量
        /// </summary>
        public int Quantity { get; set; }

        public string ThumbnailUrl40 { get; set; }

        public string ThumbnailUrl60 { get; set; }

        public string ThumbnailUrl100 { get; set; }

        public string ThumbnailUrl180 { get; set; }
    }
    public class GiftInfo
    {
        public string GiftId { get; set; }
        public string Name { get; set; }
        public string NeedPoint { get; set; }
        public string PromoType { get; set; }
        public string Quantity { get; set; }
        public string SubPointTotal { get; set; }
        public string ThumbnailUrl100 { get; set; }
        public string ThumbnailUrl40 { get; set; }
        public string ThumbnailUrl60 { get; set; }
    }
}
