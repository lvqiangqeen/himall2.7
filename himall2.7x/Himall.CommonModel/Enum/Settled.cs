using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    /// <summary>
    /// 商家类型
    /// </summary>
    public enum BusinessType : int
    {
        /// <summary>
        /// 仅企业可入驻
        /// </summary>
        [Description("仅企业可入驻")]
        Enterprise = 0,

        /// <summary>
        /// 仅个人可入驻
        /// </summary>
        [Description("仅个人可入驻")]
        Personal = 1,

        /// <summary>
        /// 企业和个人均可
        /// </summary>
        [Description("企业和个人均可")]
        All = 2
    }

    /// <summary>
    /// 验证方式
    /// </summary>
    public enum VerificationType : int
    {
        /// <summary>
        /// 验证手机
        /// </summary>
        [Description("验证手机")]
        VerifyPhone = 0,

        /// <summary>
        /// 验证邮箱
        /// </summary>
        [Description("验证邮箱")]
        VerifyEmail = 1,

        /// <summary>
        /// 均需验证
        /// </summary>
        [Description("均需验证")]
        VerifyAll = 2
    }

    /// <summary>
    /// 结算账户类别
    /// </summary>
    public enum SettleAccountsType : int
    {
        /// <summary>
        /// 银行账户
        /// </summary>
        [Description("银行账户")]
        SettleBank = 0,

        /// <summary>
        /// 微信账户
        /// </summary>
        [Description("微信账户")]
        SettleWeiXin = 1,

        /// <summary>
        /// 均可结算
        /// </summary>
        [Description("均需验证")]
        SettleAll = 2
    }

    /// <summary>
    /// 是否必填
    /// </summary>
    public enum VerificationStatus : int
    {
        /// <summary>
        /// 非必填
        /// </summary>
        [Description("非必填")]
        NonMust = 0,

        /// <summary>
        /// 必填
        /// </summary>
        [Description("必填")]
        Must = 1
    }
}
