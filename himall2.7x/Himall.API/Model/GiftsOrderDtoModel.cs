using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Model;

namespace Himall.API.Model
{
    public class GiftsOrderDtoModel
    {
        public long Id { get; set; }
        public GiftOrderInfo.GiftOrderStatus OrderStatus { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string UserRemark { get; set; }
        public string ShipTo { get; set; }
        public string CellPhone { get; set; }
        public Nullable<int> TopRegionId { get; set; }
        public Nullable<int> RegionId { get; set; }
        public string RegionFullName { get; set; }
        public string Address { get; set; }
        public string ExpressCompanyName { get; set; }
        public string ShipOrderNumber { get; set; }
        public Nullable<System.DateTime> ShippingDate { get; set; }
        public System.DateTime OrderDate { get; set; }
        public Nullable<System.DateTime> FinishDate { get; set; }
        public Nullable<int> TotalIntegral { get; set; }
        public string CloseReason { get; set; }
        /// <summary>
        /// 显示订单状态
        /// </summary>
        public string ShowOrderStatus { get; set; }
        /// <summary>
        /// 物流公司名称显示
        /// </summary>
        public string ShowExpressCompanyName { get; set; }

        public List<GiftsOrderItemDtoModel> Items { get; set; }
    }
}
