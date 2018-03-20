using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public class SalesExportModel
    {
        public string ProductName { get; set; }

        public int SaleCount { get; set; }

        public decimal SaleAmount { get; set; }
    }
}
