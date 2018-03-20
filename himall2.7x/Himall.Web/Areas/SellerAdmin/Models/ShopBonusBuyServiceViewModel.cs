using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Himall.Model;

namespace Himall.Web.Areas.SellerAdmin.Models
{
    public class ShopBonusBuyServiceViewModel
    {
        public ActiveMarketServiceInfo Market { get; set; }
        public bool IsNo { get; set; }
        public string EndDate { get; set; }
        public decimal Price { get; set; }

        [Range(1, 12, ErrorMessage = "只能为数字，且只能是1到12之间的整数!")]
        public int Month { get; set; }
    }
}