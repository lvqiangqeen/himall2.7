using System;
using System.Linq;

namespace Himall.Model
{
    public class AccountMetaModel
    {

        public long Id { get; set; }
        public long AccountId { get; set; }
        /// <summary>
        /// 类型（限时购、优惠券）
        /// </summary>
        public string MetaKey { get; set; }
        /// <summary>
        /// 费用值
        /// </summary>
        public string MetaValue { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        /// <summary>
        /// 日期范围
        /// </summary>
        public string DateRange { set; get; }

    }
}