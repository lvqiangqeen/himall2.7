using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Core.Plugins.Message
{
    /// <summary>
    /// 发送优惠券消息需要的参数实体
    /// </summary>
  public  class MessageCouponInfo
    {

      /// <summary>
      /// 用户名
      /// </summary>
      public string UserName { set; get; }

      /// <summary>
      /// 优惠券金额
      /// </summary>
      public decimal Money { set; get; }

      /// <summary>
      /// Url
      /// </summary>
      public string Url { set; get; }

      /// <summary>
      /// 商城名称
      /// </summary>
      public string SiteName { set; get; }
    }
}
