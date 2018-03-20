
using Himall.Model;
using System.Collections.Generic;
using Himall.Web.Models;
namespace Himall.Web.Areas.Web.Models
{
    public class ShopCartItemModel
    {
        public ShopCartItemModel()
        {
            CartItemModels = new List<CartItemModel>();
            UserCoupons = new List<CouponRecordInfo>();
        }
        public long shopId { set; get; }

        public string ShopName { set; get; }

        public decimal Freight { set; get; }
        public decimal FreeFreight { set; get; }
        public List<CartItemModel> CartItemModels { set; get; }

        public List<CouponRecordInfo> UserCoupons { set; get; }

        public List<ShopBonusReceiveInfo> UserBonus { set; get; }

        public List<IBaseCoupon> BaseCoupons { get; set; }

        public List<OrderSubmitItemModel> freightProductGroup { get; set; }
    }
}