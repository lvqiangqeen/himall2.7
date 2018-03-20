using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.API.Model
{
    public class GiftOrderItemDtoModel
    {
        public long Id { get; set; }
        public Nullable<long> OrderId { get; set; }
        public long GiftId { get; set; }
        public int Quantity { get; set; }
        public Nullable<int> SaleIntegral { get; set; }
        public string GiftName { get; set; }
        public Nullable<decimal> GiftValue { get; set; }
        public string ImagePath { get; set; }
        public string DefaultImage { get; set; }
    }
}
