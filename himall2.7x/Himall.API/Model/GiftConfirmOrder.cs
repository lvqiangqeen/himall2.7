using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.API.Model
{
    public class GiftConfirmOrder
    {
        public long ID { set; get; }
        public long? RegionId { set; get; }
        public int Count { set; get; }
    }

    public class GiftConfirmOrderOver
    {
        public long OrderId { set; get; }
    }
}
