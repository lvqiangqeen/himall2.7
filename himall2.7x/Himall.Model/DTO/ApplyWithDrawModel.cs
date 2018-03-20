
namespace Himall.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class ApplyWithDrawModel
    {
        public long Id { get; set; }
        public long MemId { get; set; }

        public string MemberName { get; set; }
        public string NickName { get; set; }
        public string OpenId { get; set; }
        public Model.ApplyWithDrawInfo.ApplyWithDrawStatus ApplyStatus { get; set; }
        /// <summary>
        /// 提现申请的处理状态
        /// </summary>
        public string ApplyStatusDesc { get; set; }
        public decimal ApplyAmount { get; set; }
        public string ApplyTime { get; set; }
        public string ConfirmTime { get; set; }
        public string PayTime { get; set; }
        public string PayNo { get; set; }
        public string OpUser { get; set; }
        public string Remark { get; set; }
    }
}