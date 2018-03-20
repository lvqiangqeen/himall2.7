using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Model;

namespace Himall.IServices.QueryModel
{
    public partial class LimitTimeQuery : QueryBase
    {
        public Himall.Model.LimitTimeMarketInfo.LimitTimeMarketAuditStatus? AuditStatus { get; set; }
        public string ShopName { get; set; }
        public long? ShopId { get; set; }
        public string ItemName { get; set; }
        public string CategoryName { get; set; }
        public int OrderType { get; set; }
        public int OrderKey { get; set; }
        public bool CheckProductStatus { get; set; }
    }

    public partial class FlashSaleQuery : QueryBase 
    {
        public Himall.Model.FlashSaleInfo.FlashSaleStatus? AuditStatus { get; set; }
        public string ShopName { get; set; }
        public long? ShopId { get; set; }
        public string ItemName { get; set; }
        public string CategoryName { get; set; }
        public int OrderType { get; set; }
        public int OrderKey { get; set; }
        /// <summary>
        /// 是否查询预热条件
        /// </summary>
        public bool IsPreheat { set; get; }
        public int IsStart { get; set; }
        public bool CheckProductStatus { get; set; }
    }
}
