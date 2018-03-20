using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;

namespace Himall.Core.Plugins.Payment
{
    /// <summary>
    /// 支付插件接口
    /// </summary>
    public interface IPaymentPlugin:IPlugin
    {
        /// <summary>
        /// 处理支付返回结果
        /// </summary>
        /// <param name="context">请求</param>
        /// <exception cref="ApplicationException"></exception>
        /// <returns></returns>
        PaymentInfo ProcessReturn(HttpRequestBase context);

        /// <summary>
        /// 处理异步通知结果
        /// </summary>
        /// <param name="queryString">请求参数</param>
        /// <exception cref="ApplicationException"></exception>
        /// <returns></returns>
        PaymentInfo ProcessNotify(HttpRequestBase context);

        /// <summary>
        /// 如果付款方式为在线付款则返回付款请求的url，否则返回空字符串
        /// </summary>
        /// <param name="returnUrl">服务器异步通知页面路径，需http://格式的完整路径，不能加?id=123这类自定义参数</param>
        /// <param name="notifyUrl">页面跳转同步通知页面路径，需http://格式的完整路径，不能加?id=123这类自定义参数，不能写成http://localhost/</param>
        /// <param name="orderId">订单编号</param>
        /// <param name="totalFee">订单总金额</param>
        /// <param name="productInfo">订单商品信息</param>
        /// <param name="openId">OpenId（部分支付需要）</param>
        /// <returns></returns>
        string GetRequestUrl(string returnUrl, string notifyUrl, string orderId, decimal totalFee, string productInfo, string openId = null);
        /// <summary>
        /// 退款处理
        /// </summary>
        /// <param name="para">退款所需参数</param>
        /// <returns>退款成功时，返回不为空</returns>
        RefundFeeReturnModel ProcessRefundFee(PaymentPara para);
        /// <summary>
        /// 处理退款返回结果
        /// </summary>
        /// <param name="context">请求</param>
        /// <exception cref="ApplicationException"></exception>
        /// <returns></returns>
        PaymentInfo ProcessRefundReturn(HttpRequestBase context);

        /// <summary>
        /// 处理退款异步通知结果
        /// </summary>
        /// <param name="queryString">请求参数</param>
        /// <exception cref="ApplicationException"></exception>
        /// <returns></returns>
        PaymentInfo ProcessRefundNotify(HttpRequestBase context);
        /// <summary>
        /// 企业付款
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        PaymentInfo EnterprisePay(EnterprisePayPara para);
        /// <summary>
        /// 网站插件列表Url
        /// </summary>
        string PluginListUrl { set; }


        /// <summary>
        /// 获取表单数据
        /// </summary>
        FormData GetFormData();

        /// <summary>
        /// 设置表单数据
        /// </summary>
        /// <param name="values">表单数据键值对集合，键为表单项的name,值为用户填写的值</param>
        void SetFormValues(IEnumerable<KeyValuePair<string, string>> values);

        /// <summary>
        /// 确认收到支付消息，获取待返回至第三方支付平台的内容
        /// </summary>
        /// <returns></returns>
        string ConfirmPayResult();

        /// <summary>
        /// Logo图片路径
        /// </summary>
        string Logo { get; set; }

        /// <summary>
        /// 支持的平台类型
        /// </summary>
        IEnumerable<PlatformType> SupportPlatforms { get; }

        /// <summary>
        /// 开启对应平台的支付方式
        /// </summary>
        /// <param name="platform">待开启支付方式的平台</param>
        void Enable(PlatformType platform);

        /// <summary>
        /// 禁用对应平台的支付方式
        /// </summary>
        /// <param name="platform">待禁用支付方式的平台</param>
        void Disable(PlatformType platform);

        /// <summary>
        /// 检查是否已经开启了对应平台的支持功能
        /// </summary>
        /// <param name="platform">待检查的平台</param>
        /// <returns></returns>
        bool IsEnable(PlatformType platform);

        /// <summary>
        /// 支付请求链接类型
        /// </summary>
        UrlType RequestUrlType { get; }

        /// <summary>
        /// 帮助图片(说明图片，仅在RequestUrlType为QRCode时有效)
        /// </summary>
        string HelpImage { get;  }
    }
}
