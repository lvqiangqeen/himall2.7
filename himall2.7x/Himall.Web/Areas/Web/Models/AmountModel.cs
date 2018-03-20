using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.Web.Models
{
    public class AmountModel
    {
        public decimal totalProductAmount { get; set; }

        public decimal totalFreightAmount { get; set; }

        public decimal totalAmount { get; set; }

        public List<decimal> freightAmounts { get; set; }
    }
}