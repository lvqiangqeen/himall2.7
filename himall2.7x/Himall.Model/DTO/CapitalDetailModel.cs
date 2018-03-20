
using System;
namespace Himall.Model
{
    /// <summary>
    /// 
    /// </summary>
    public  class CapitalDetailModel 
    {
        string _createTime;
        public CapitalDetailModel()
        {
            _createTime = DateTime.Now.ToString();
        }
        public long Id { get; set; }
        public long UserId { get; set; }
        public long CapitalID { get; set; }
        public Model.CapitalDetailInfo.CapitalDetailType SourceType { get; set; }

        public decimal Amount { get; set; }
        /// <summary>
        /// 来源数据（）
        /// </summary>
        public string SourceData { get; set; }
        public string CreateTime { get { return _createTime; } set { _createTime = value; } }
        /// <summary>
        /// 备注信息(充值时为：支付方式)
        /// </summary>
        public string Remark { get; set; }

        public string PayWay { get; set; }

    }
}