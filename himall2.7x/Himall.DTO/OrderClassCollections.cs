using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.ViewModel
{
    /// <summary>
    /// 订单提交页面ViewModel 
    /// </summary>
    public class OrderSubmitModel
    {
        /// <summary>
        /// 多少积分可以换一元钱
        /// </summary>
        public int IntegralPerMoney { get; set; }
        /// <summary>
        /// 订单使用的积分
        /// </summary>
        public int Integral { get; set; }
        /// <summary>
        /// 用户信息
        /// </summary>
        public UserMemberInfo Member { get; set; }

        public string cartItemIds { get; set; }
        /// <summary>
        /// 完成订单将可以获得的积分
        /// </summary>
        public decimal TotalIntegral { get; set; }
        /// <summary>
        /// 消费多少钱可以获处一积分
        /// </summary>
        public int MoneyPerIntegral { get; set; }

        public List<InvoiceTitleInfo> InvoiceTitle { get; set; }

        public List<InvoiceContextInfo> InvoiceContext { get; set; }

        public List<ShopCartItemModel> products { get; set; }

        public List<OrderSubmitItemModel> freightProductGroup { get; set; }

        /// <summary>
        /// 订单总金额
        /// </summary>
        public decimal totalAmount { get; set; }

        public decimal Freight { get; set; }

        public decimal orderAmount
        {
            get { return Freight + totalAmount; }
        }

        public ShippingAddressInfo address { get; set; }

        public string collIds { get; set; }
        public string skuIds { get; set; }

        public string counts { get; set; }

        public bool IsCashOnDelivery { get; set; }

        public bool IsLimitBuy { get; set; }
    }

    public class OrderSubmitItemModel
    {
        public long id { get; set; }
        public long ProductId { get; set; }

        public long FreightTemplateId { get; set; }

        public decimal price { get; set; }

        public int count { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string skuId { get; set; }

        public string skuColor { get; set; }

        public string skuSize { get; set; }

        public string skuVersion { get; set; }

        public string name { get; set; }

        public string productCode { get; set; }

        public string imgUrl { get; set; }
        /// <summary>
        /// 七天退货标记
        /// </summary>
        public bool sevenDayNoReasonReturn { get; set; }
        /// <summary>
        /// 急速发货
        /// </summary>
        public bool timelyShip { get; set; }

        /// <summary>
        /// 消费者保障
        /// </summary>
        public bool customerSecurity { get; set; }

    }

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

        public List<BaseCoupon> BaseCoupons { get; set; }

        public List<OrderSubmitItemModel> freightProductGroup { get; set; }
    }


    public class ShipAddressInfo
    {
        public long id { get; set; }
        public string fullRegionName { get; set; }
        public string address { get; set; }
        public string phone { get; set; }
       public string shipTo { get; set; }
       public string fullRegionIdPath { get; set; }
    }
}
