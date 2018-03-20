using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Core;

namespace Himall.Model
{
    public class UserProformanceModel:BaseModel
    {

      public new long Id { set; get; }


      public string UserName { set; get;}

        /// <summary>
        /// 用户ID
        /// </summary>
      public long UserId { set; get; }

      /// <summary>
      /// 商品名称
      /// </summary>
      public string ProductName { set; get; }

      /// <summary>
      /// 商品实付金额
      /// </summary>
      public decimal RealTotalPrice { set; get; }

      /// <summary>
      /// 成交总金额
      /// </summary>
      public OrderInfo.OrderOperateStatus OrderStatus { set; get; }

      /// <summary>
      /// 是否过维权期
      /// </summary>
      public bool Expired { set; get; }

        /// <summary>
        /// 下单时间
        /// </summary>
      public DateTime OrderTime { set; get; }

        //订单完成时间
      public DateTime? FinshedTime { set; get; }


      public string ExpriedStatus { get { return Expired ? "是" : "否"; } }

      public string OrderStatusDesc { get { return OrderStatus.ToDescription(); } }

      public string OrderTimeString { get { return OrderTime.ToString("yyyy-MM-dd HH:mm:ss"); } }

      public string OrderIdString { get { return Id.ToString(); } }

      /// <summary>
      /// 佣金
      /// </summary>
      public decimal Brokerage { set; get; }
    }
}
