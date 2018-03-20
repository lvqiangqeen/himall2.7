using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.IServices.QueryModel
{
  public  class ProformanceQuery : QueryBase
    {
      //用户名
      public string UserName { set; get; }

      //用户ID
      public long ?UserId { set; get; }

      //开始时间
      public DateTime? startTime { get; set; }

      //结束时间
      public DateTime? endTime { get; set; }
    }

  public class UserProformanceQuery : QueryBase
  {
      //用户ID
      public long? UserId { set; get; }

      /// <summary>
      /// 订单编号
      /// </summary>
      public long? OrderId { set; get; }

      //开始时间
      public DateTime? startTime { get; set; }

      //结束时间
      public DateTime? endTime { get; set; }
  }
}
