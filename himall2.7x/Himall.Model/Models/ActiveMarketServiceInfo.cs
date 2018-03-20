using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Himall.Model
{
    public partial class ActiveMarketServiceInfo
    {
        /// <summary>
        /// 获取服务的结束时间
        /// </summary>
        

        [NotMapped]
        public DateTime? ServiceEndTime
        {
            get
            {
                DateTime? result = null;
                if (this != null)
                {
                    if (this.MarketServiceRecordInfo != null&&this.MarketServiceRecordInfo.Count>0)
                    {
                        result = this.MarketServiceRecordInfo.Max(d => d.EndTime);
                    }
                }
                return result;
            }
        }

    }
}
