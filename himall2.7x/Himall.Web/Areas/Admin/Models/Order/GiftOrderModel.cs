using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using Himall.Core;

namespace Himall.Web.Areas.Admin.Models
{
    public class GiftOrderPageModel
    {
        public long Id { get; set; }
        public Model.GiftOrderInfo.GiftOrderStatus OrderStatus { get; set; }
        public long UserId { get; set; }
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
        public long FirstGiftId { get; set; }
        public string FirstGiftName { get; set; }
        public int FirstGiftBuyQuantity { get; set; }
        public string UserName { get; set; }
        /// <summary>
        /// 显示订单状态
        /// </summary>
        [NotMapped]
        public string ShowOrderStatus
        {
            get
            {
                return this.OrderStatus.ToDescription();
            }
        }

    }
}