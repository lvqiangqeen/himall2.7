using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.Web.Models
{
    public class ProductBonusLableModel
    {
        public decimal GrantPrice { get; set; }

        public decimal RandomAmountStart { get; set; }

        public decimal RandomAmountEnd { get; set; }

        public int Count { get; set; }
    }
}