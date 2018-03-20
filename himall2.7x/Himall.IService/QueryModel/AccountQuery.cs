using Himall.Model;
using System;

namespace Himall.IServices.QueryModel
{
    public partial class AccountQuery : QueryBase
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public AccountInfo.AccountStatus? Status { get; set; }

        public Himall.Model.AccountDetailInfo.EnumOrderType EnumOrderType { get; set; }

        public long? ShopId { get; set; }

        public string ShopName { get; set; }

        /// <summary>
        /// 结算编号
        /// </summary>
        public long AccountId { get; set; }
    }
}
