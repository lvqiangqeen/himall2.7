using Himall.Model;
using System;

namespace Himall.IServices.QueryModel
{
    public partial class ComplaintQuery : QueryBase
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public OrderComplaintInfo.ComplaintStatus? Status { get; set; }

        public long? OrderId { get; set; }

        public long? ShopId { get; set; }

        public string ShopName { get; set; }

        public long? UserId { get; set; }

        public string UserName { get; set; }

    }
}
