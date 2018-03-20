using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Model;

namespace Himall.SmallProgAPI.Model
{
    public class ShopCartItemModel
    {
        public ShopCartItemModel()
        {
            CartItemModels = new List<CartItemModel>();
        }
        public long shopId { set; get; }

        public long vshopId { set; get; }
        public string ShopName { set; get; }

        public decimal Freight { set; get; }
        public bool isFreeFreight { get; set; }
        public decimal shopFreeFreight { get; set; }
        /// <summary>
        /// 店铺订单金额
        /// </summary>
        public decimal OrderAmount { get; set; }
        public IBaseCoupon Coupon { get; set; }
        /// <summary>
        /// 是否自营店
        /// </summary>
        public bool IsSelf { get; set; }

        public List<CartItemModel> CartItemModels { set; get; }

    }
}
