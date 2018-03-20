using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.SmallProgAPI.Model
{
    public class ProductBrowsedHistoryModel
    {
        public long ProductId { set; get; }

        public decimal ProductPrice { set; get; }

        public string ProductName { set; get; }

        public string ImagePath { set; get; }

        public DateTime BrowseTime { set; get; }

        public long UserId { set; get; }
    }
}
