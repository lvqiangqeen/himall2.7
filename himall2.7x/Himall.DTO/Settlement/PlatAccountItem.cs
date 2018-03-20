using Himall.CommonModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Himall.Core;
using System.Threading.Tasks;

namespace Himall.DTO
{
    /// <summary>
    /// 店铺收支详情
    /// </summary>
    public class PlatAccountItem
    {
        /// <summary>
        /// 详情ID
        /// </summary>
        public long Id { set; get; }
        /// <summary>
        /// 时间
        /// </summary>
        public string CreateTime { set; get; }

        /// <summary>
        /// 资金流水号
        /// </summary>
        public string AccountNo { set; get; }

        /// <summary>
        /// 类型字符串
        /// </summary>
        public string AccountType { get { return PlatAccountType.ToDescription(); } }

        /// <summary>
        /// 收支类型枚举
        /// </summary>

        public PlatAccountType PlatAccountType { set; get; }

        /// <summary>
        /// 收入
        /// </summary>
        public string Income { set; get; }

        /// <summary>
        /// 支出
        /// </summary>
        public string Expenditure { set; get; }

        /// <summary>
        /// 余额
        /// </summary>
        public string Balance { set; get; }

        /// <summary>
        /// 详情ID
        /// </summary>
        public string DetailId { set; get; }

        /// <summary>
        /// 详情链接
        /// </summary>
        public string DetailLink
        {
            get
            {
                var detailLink = string.Empty;
                switch (this.PlatAccountType)
                {
                    case PlatAccountType.MarketingServices:
                        detailLink = "/Admin/billing/MarketServiceRecordInfo/" + this.DetailId;
                        break;

                    case PlatAccountType.SettledPayment:
                        detailLink = "/Admin/billing/SettledPaymentDetail/" + this.DetailId;
                        break;

                    case PlatAccountType.PlatCommissionRefund:
                        detailLink = "/Admin/OrderRefund/management?orderid="+this.DetailId;
                        break;
                    case PlatAccountType.SettlementIncome:
                        detailLink = "/Admin/Billing/SettlementOrders?detailId="+this.DetailId;
                        break;

                }
                return detailLink;
            }
        }
    }

}
