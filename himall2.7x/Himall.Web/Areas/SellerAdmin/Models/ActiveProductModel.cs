using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.SellerAdmin.Models
{
    public class ActiveProductModel
    {
        public long Id { get; set; }
        public decimal Price { get; set; }
        public string Image { get; set; }
        public string Name { get; set; }
        public string BrandName { get; set; }
        public long BrandId { get; set; }
        public long CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string ProductCode { get; set; }
        public string Unit { get; set; }
        /// <summary>
        /// 是否异常商品
        /// <para>己参与其他冲突活动，或商品状态异常</para>
        /// </summary>
        public bool IsException { get; set; }
    }
}