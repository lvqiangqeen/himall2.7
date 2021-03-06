//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Himall.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class GiftOrderInfo:BaseModel
    {
        public GiftOrderInfo()
        {
            this.Himall_GiftOrderItem = new HashSet<GiftOrderItemInfo>();
        }
    
        long _id;
        public new long Id { get{ return _id; } set{ _id=value; base.Id = value; } }
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
    
        public virtual ICollection<GiftOrderItemInfo> Himall_GiftOrderItem { get; set; }
    }
}
