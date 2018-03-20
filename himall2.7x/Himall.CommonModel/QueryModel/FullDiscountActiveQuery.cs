using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    public class FullDiscountActiveQuery : BaseQuery
    {
        public long? ShopId { set; get; }
        public FullDiscountStatus? Status { set; get; }
        public DateTime? StartTime { set; get; }
        public DateTime? EndTime { set; get; }
        public string ActiveName { set; get; }
    }
}
