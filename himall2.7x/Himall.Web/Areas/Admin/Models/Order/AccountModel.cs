
namespace Himall.Web.Areas.Admin.Models
{
    public class AccountModel
    {

        public long Id { get; set; }
        public long ShopId { get; set; }
        public string ShopName { get; set; }

        public string TimeSlot { get; set; }
        public string AccountDate { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public int Status { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal RefundAmount { get; set; }

        /// <summary>
        /// 分销佣金
        /// </summary>
        public decimal BrokerageAmount { set; get; }

        public decimal ReturnBrokerageAmount { set; get; }

        public string Remark { get; set; }
        public decimal FreightAmount { get; set; }
        public decimal RefundCommissionAmount { get; set; }
        public decimal AdvancePaymentAmount { get; set; }
        public decimal PeriodSettlement { get; set; }
        public decimal ProductActualPaidAmount { get; set; }
    }
}