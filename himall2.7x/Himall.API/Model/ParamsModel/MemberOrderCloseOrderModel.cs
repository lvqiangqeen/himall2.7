using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.API.Model.ParamsModel
{
    /// <summary>
    /// 取消订单
    /// </summary>
    public class MemberOrderCloseOrderModel
    {
        public long orderId { get; set; }
    }
}
