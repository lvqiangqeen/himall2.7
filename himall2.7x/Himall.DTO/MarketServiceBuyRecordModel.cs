using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    /// <summary>
    /// 服务购买记录
    /// </summary>
    public class MarketServiceBuyRecordModel
    {
        public long Id { get; set; }
        public string ShopName { get; set; }
        public long MarketServiceId { get; set; }
        public System.DateTime StartTime { get; set; }
        public System.DateTime EndTime { get; set; }
        public long SettlementFlag { get; set; }
    }
}
