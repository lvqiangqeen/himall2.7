using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Model;

namespace Himall.IServices.QueryModel
{
    public partial class CouponRecordQuery : QueryBase
    {
        public string UserName { get; set; }

        public long? UserId { set; get; }
        public long? ShopId { get; set; }

        public int? Status { set; get; }

        public long? CouponId { set; get; }
    }
}
