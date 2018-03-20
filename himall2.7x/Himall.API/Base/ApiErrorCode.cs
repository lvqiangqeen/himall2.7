using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Himall.API
{
    public enum ApiErrorCode
    {
        /// <summary>
        /// 开发者权限不足
        /// </summary>
        [Description("开发者权限不足")]
        Insufficient_ISV_Permissions = 1,
        /// <summary>
        /// 用户权限不足
        /// </summary>
        [Description("用户权限不足")]
        Insufficient_User_Permissions = 2,
        /// <summary>
        /// 远程服务错误
        /// </summary>
        [Description("远程服务出错")]
        Remote_Service_Error = 3,
        /// <summary>
        /// 缺少方法名参数
        /// </summary>
        [Description("缺少方法名参数")]
        Missing_Method = 4,
        /// <summary>
        /// 不存在的方法名
        /// </summary>
        [Description("不存在的方法名")]
        Invalid_Method = 5,
        /// <summary>
        /// 非法数据格式
        /// </summary>
        [Description("非法数据格式")]
        Invalid_Format = 6,
        /// <summary>
        /// 缺少签名参数
        /// </summary>
        [Description("缺少签名参数")]
        Missing_Signature = 7,
        /// <summary>
        /// 非法签名
        /// </summary>
        [Description("非法签名")]
        Invalid_Signature = 8,
        /// <summary>
        /// 缺少AppKey参数
        /// </summary>
        [Description("缺少AppKey参数")]
        Missing_App_Key = 9,
        /// <summary>
        /// 非法的AppKey参数
        /// </summary>
        [Description("非法的AppKey参数")]
        Invalid_App_Key = 10,
        /// <summary>
        /// 缺少时间戳参数
        /// </summary>
        [Description("缺少时间戳参数")]
        Missing_Timestamp = 12,
        /// <summary>
        /// 非法的时间戳参数
        /// </summary>
        [Description("非法的时间戳参数")]
        Invalid_Timestamp = 13,
        /// <summary>
        /// 缺少必选参数
        /// </summary>
        [Description("缺少必选参数")]
        Missing_Required_Arguments = 14,
        /// <summary>
        /// 非法的参数
        /// </summary>
        [Description("非法的参数")]
        Invalid_Arguments = 15,
        /// <summary>
        /// 请求被禁止
        /// </summary>
        [Description("请求被禁止")]
        Forbidden_Request = 16,
        /// <summary>
        /// 参数错误
        /// </summary>
        [Description("参数错误")]
        Parameter_Error = 17,
        /// <summary>
        /// 系统错误
        /// </summary>
        [Description("系统错误")]
        System_Error = 18,
        /// <summary>
        /// 缺少参数
        /// </summary>
        [Description("缺少参数")]
        Missing_Parameters = 501,
        /// <summary>
        /// 非法的用户信息
        /// </summary>
        [Description("非法的用户信息")]
        Invalid_User_Key_Info = 502,
        /// <summary>
        /// 参数格式错误
        /// </summary>
        [Description("参数格式错误")]
        Parameters_Format_Error = 503,
    }
}
