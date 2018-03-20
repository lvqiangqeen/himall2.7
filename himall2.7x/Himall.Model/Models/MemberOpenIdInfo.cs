using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public partial class MemberOpenIdInfo
    {
        public enum AppIdTypeEnum
        {
            /// <summary>
            /// 支付的Appid
            /// </summary>
            Payment,
            /// <summary>
            /// 一般的Appid
            /// </summary>
            Normal
        }
    }
}
