using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public class UserCouponInfo : IBaseCoupon
    {
        public long UserId { get; set; }
        public long CouponId { get; set; }
        public long ShopId { get; set; }
        public Nullable<long> VShopId { get; set; }
        public string ShopName { get; set; }
        public string ShopLogo { get; set; }
        public decimal Price { get; set; }
        public int PerMax { get; set; }
        public decimal OrderAmount { get; set; }
        public int Num { get; set; }
        public System.DateTime StartTime { get; set; }
        public System.DateTime EndTime { get; set; }
        public string CouponName { get; set; }
        public System.DateTime CreateTime { get; set; }

        public Nullable<DateTime> UseTime { get; set; }
        public CouponRecordInfo.CounponStatuses UseStatus { get; set; }
        public Nullable<long> OrderId { get; set; }

        public VShopInfo VShop { get; set; }

        public long BaseId
        {
            get { return this.CouponId; }
        }

        public decimal BasePrice
        {
            get { return this.Price; }
        }

        public string BaseName
        {
            get { return this.CouponName; }
        }

        public CouponType BaseType
        {
            get { return CouponType.Coupon; }
        }

        public string BaseShopName
        {
            get { return this.ShopName; }
        }

        public DateTime BaseEndTime
        {
            get { return this.EndTime; }
        }


        public long BaseShopId
        {
            get { return this.ShopId; }
        }
        /// <summary>
        /// 优惠券领取状态
        /// </summary>
        public int ReceiveStatus
        {
            get;set;
        }
    }
}
