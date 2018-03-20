using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
  public  class ProformanceModel
    {
      public string UserName { set; get; }

      /// <summary>
      /// 未结佣金
      /// </summary>
      public decimal UnPaid { set; get; }

      /// <summary>
      /// 已结佣金
      /// </summary>
      public decimal Paid { set; get; }

      /// <summary>
      /// 成交总金额
      /// </summary>
      public decimal TotalTurnover { set; get; }

      /// <summary>
      /// 成交总数
      /// </summary>
      public decimal TotalNumber { set; get; }

      /// <summary>
      /// ID用户ID
      /// </summary>
      public long Id { set; get; }
    }
}
