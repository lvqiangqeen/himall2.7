using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public partial class PromoterInfo
    {
      public enum PromoterStatus
      {
          /// <summary>
          /// 未审核
          /// </summary>
          [Description("未审核")]
          UnAudit = 0,
          /// <summary>
          /// 已审核
          /// </summary>
          [Description("已审核")]
          Audited=1,
          /// <summary>
          /// 已拒绝
          /// </summary>
          [Description("已拒绝")]
          Refused = 2,
          /// <summary>
          /// 已清退
          /// </summary>
          [Description("已清退")]
          NotAvailable = 3,
      }
    }
}
