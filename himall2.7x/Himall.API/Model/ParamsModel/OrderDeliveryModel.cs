using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.API.Model.ParamsModel
{
    /// <summary>
    /// 添加商品到购物车-参数模型
    /// </summary>
    public class OrderDeliveryModel
    {
        public long orderId { get; set; }
        public int deliveryType { get; set; }
        public string companyName { get; set; }
        public string shipOrderNumber { get; set; }
    }
}
