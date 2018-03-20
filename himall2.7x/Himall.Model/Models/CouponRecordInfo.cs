using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace Himall.Model
{
    public partial class CouponRecordInfo : IBaseCoupon
    {
        public enum CounponStatuses
        {
            [Description("未使用")]
             Unuse,
            [Description("已使用")]
             Used
        }
        /// <summary>
        /// 卡券关联信息
        /// </summary>
        [NotMapped]
        public WXCardCodeLogInfo WXCardCodeInfo { get; set; }

        [NotMapped]
        public long BaseId
        {
            get { return this.Id;  }
        }

        [NotMapped]
        public decimal BasePrice
        {
            get { return this.Himall_Coupon.Price; }
        }
        [NotMapped]
        public decimal BaseOrderAmount
        {
            get { return this.Himall_Coupon.OrderAmount; }
        }

        [NotMapped]
        public string BaseName
        {
            get { return this.Himall_Coupon.CouponName; }
        }

        [NotMapped]
        public CouponType BaseType
        {
            get { return CouponType.Coupon; }
        }

        [NotMapped]
        public string BaseShopName
        {
            get { return this.ShopName; }
        }

        [NotMapped]
        public DateTime BaseEndTime
        {
            get { return this.Himall_Coupon.EndTime; }
        }


        public long BaseShopId
        {
            get { return this.Himall_Coupon.ShopId; }
        }
    }
}
