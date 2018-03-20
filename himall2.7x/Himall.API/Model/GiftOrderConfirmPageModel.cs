using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Model;

namespace Himall.API.Model
{
    /// <summary>
    /// 礼品订单确认页面模型
    /// </summary>
    public class GiftOrderConfirmPageModel
    {
        /// <summary>
        /// 兑换礼品
        /// </summary>
        public List<GiftOrderItemDtoModel> GiftList { get; set; }
        /// <summary>
        /// 订单总价
        /// <para>应付积分总数</para>
        /// </summary>
        public int TotalAmount { get; set; }
        /// <summary>
        /// 订单礼品价值总额
        /// </summary>
        public decimal GiftValueTotal { get; set; }
        /// <summary>
        /// 收货地址
        /// </summary>
        public ShippingAddressDtoModel ShipAddress { get; set; }
    }
}
