using Himall.Core.Plugins.Payment;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    /// <summary>
    /// 预付款支付视图
    /// </summary>
    public class ChargePayModel
    {
        /// <summary>
        /// 订单充值
        /// </summary>
        public Model.ChargeDetailInfo Orders { get; set; }
        /// <summary>
        /// 订单ID
        /// </summary>
        public string OrderIds { get; set; }
        /// <summary>
        /// 总价格
        /// </summary>
        public decimal TotalAmount { get; set; }
        /// <summary>
        /// 步骤
        /// </summary>
        public int Step { get; set; }
        /// <summary>
        /// 未支付超时时间
        /// </summary>
        public int UnpaidTimeout { get; set; }
        /// <summary>
        /// 支付方式
        /// </summary>
        public List<PaymentModel> models { get; set; }

    }

    /// <summary>
    /// 第三方支付视图
    /// </summary>
    public class PaymentViewModel
    {

        /// <summary>
        /// 第三方支付信息
        /// </summary>
        public IEnumerable<PaymentModel> Models { get; set; }

        /// <summary>
        /// 订单信息
        /// </summary>
        public List<OrderInfo> Orders { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 不成功的描述
        /// </summary>
        public string Msg { get; set; }

        /// <summary>
        /// 是否有非销售中的商品
        /// </summary>
        public bool HaveNoSalePro { get; set;}

        /// <summary>
        /// 会员的预付款
        /// </summary>
        public decimal Capital { get; set; }

        /// <summary>
        /// 订单总价
        /// </summary>
        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// 第三方支付的信息
    /// </summary>
    public class PaymentModel
    {
        public bool IsSuccess { get; set; }
        /// <summary>
        /// 第三方支付地址
        /// </summary>
        public string RequestUrl { get; set; }
        /// <summary>
        /// 支付Log
        /// </summary>
        public string Logo { get; set; }
        /// <summary>
        /// 第三方支付ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 支付地址方式:0页面 1二维码 2需解析成页面提交
        /// </summary>
        public UrlType UrlType { get; set; }
    }

}
