using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Himall.Model;

namespace Himall.ViewModel
{
    public class CartItemModel
    {
        public string skuId { get; set; }

        public string size { get; set; }

        public string color { get; set; }

        public string version { get; set; }
        public long id { get; set; }
        public string imgUrl { get; set; }

        public string shopName { get; set; }
        public string name { get; set; }

        public decimal price { get; set; }

        public int count { get; set; }

        public long shopId { set; get; }
        public long vshopId { set; get; }
        /// <summary>
        /// 是否自营店
        /// </summary>
        public bool IsSelf { set; get; }
        public string productCode { get; set; }
        /// <summary>
        /// 计量单位
        /// </summary>
        public string unit { get; set; }
        public List<CouponRecordInfo> UserCoupons { set; get; }
    }
}