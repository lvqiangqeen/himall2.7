using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
   
    public class BrokerageModel
    {
        public long Id { set; get; }
        public string TypeName { get { return "分销佣金"; } }

        public long OrderId { set; get; }

        public string ProductName { set; get; }

        public long ProductId { set; get; }

        public decimal RealTotal { set; get; }

        public decimal Brokerage { set; get; }

        public long UserId { set; get; }

        public string UserName { set; get; }

        public DateTime SettlementTime { set; get; }

        public string SettlementTimeString { get { return SettlementTime.ToString("yyyy-MM-dd"); } }
    }
    
}
