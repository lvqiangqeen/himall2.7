using Himall.Model;
using System;

namespace Himall.IServices.QueryModel
{
    public partial class OrderCommentQuery : QueryBase
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public long? OrderId { get; set; }

        public long? ShopId { get; set; }

        public string ShopName { get; set; }

        public long? UserId { get; set; }

        public string UserName { get; set; }
    }
}
