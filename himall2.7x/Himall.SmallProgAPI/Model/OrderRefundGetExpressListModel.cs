using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.SmallProgAPI.Model
{
    public class OrderRefundGetExpressListModel : BaseResultModel
    {
        public OrderRefundGetExpressListModel(bool status) : base(status)
        {
        }

        public List<ExpressItem> Data { get; set; }
    }

    public class ExpressItem
    {
        public string ExpressName { get; set; }
        public string Kuaidi100Code { get; set; }
        public string TaobaoCode { get; set; }
    }

}
