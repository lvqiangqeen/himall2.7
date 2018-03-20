using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    /// <summary>
    /// 管理员认证发送验证码返回枚举
    /// </summary>
    public enum SendMemberCodeReturn
    {
        /// <summary>
        /// 成功
        /// </summary>
        [Description("发送成功")]
        success = 1,

        /// <summary>
        /// 120秒时间限制
        /// </summary>
        [Description("120秒内只允许请求一次，请稍后重试！")]
        limit = 2,

        /// <summary>
        /// 联系方式已绑定
        /// </summary>
        [Description("此号码已绑定")]
        repeat = 3
    }
}
