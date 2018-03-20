using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Himall.Model;
using Himall.Web.Areas.SellerAdmin.Models;

namespace Himall.Web.Areas.Web.Models
{
    public class CollocationSkusModel
    {
        public CollocationSkusModel()
        {
            Color = new CollectionSKU();
            Size = new CollectionSKU();
            Version = new CollectionSKU();
        }
        /// 商品图片地址
        /// </summary>
        public string ImagePath { get; set; }
        /// <summary>
        /// 商品名
        /// </summary>
        public string ProductName { get; set; }

        public string MeasureUnit { set; get; }

        //总库存
        public long Stock { set; get; }

        //最小价格

        public decimal MinPrice { set; get; }
        /// <summary>
        /// 商品ID
        /// </summary>
        public long ProductId { get; set; }

        //组合购商品ID
        public long ColloProductId { set; get; }
        public CollectionSKU Color { get; set; }
        public CollectionSKU Size { get; set; }
        public CollectionSKU Version { get; set; }

        public string ColorAlias { get; set; }
        public string SizeAlias { get; set; }
        public string VersionAlias { get; set; }

    }
}