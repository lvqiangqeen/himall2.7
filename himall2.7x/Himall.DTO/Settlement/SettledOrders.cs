using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    /// <summary>
    /// 已结算订单列表
    /// </summary>
    public class SettledOrders
    {
        /// <summary>
        /// 订单ID
        /// </summary>
        public long OrderId { set; get; }

        /// <summary>
        /// 订单ID字符串
        /// </summary>

        public string strOrderId { set; get; }

        /// <summary>
        /// 店铺名称
        /// </summary>
        public string ShopName { set; get; }

        /// <summary>
        /// 店铺Id
        /// </summary>
        public long ShopId { set; get; }

        /// <summary>
        /// 支付方式名称
        /// </summary>
        public string PaymentTypeName { set; get; }

        /// <summary>
        /// 结算时间
        /// </summary>
        public string SettledTime { set; get; }

        /// <summary>
        /// 订单金额
        /// </summary>
        public decimal OrderAmount { get; set; }

        /// <summary>
        /// 平台佣金
        /// </summary>
        public decimal PlatCommission { set; get; }

        /// <summary>
        /// 分销佣金
        /// </summary>
        public decimal DistributorCommission { set; get; }

        /// <summary>
        /// 开团奖励
        /// </summary>
        public decimal OpenCommission { set; get; }
        /// <summary>
        /// 成团返现
        /// </summary>
        public decimal JoinCommission { set; get; }

        /// <summary>
        /// 退款金额
        /// </summary>
        public decimal RefundAmount { set; get; }

        /// <summary>
        /// 商家结算金额
        /// </summary>
        public decimal SettlementAmount { set; get; }

        /// <summary>
        /// 订单完成时间
        /// </summary>
        public string OrderFinshTime { set; get; }

        /// <summary>
        /// 运费
        /// </summary>
        public decimal FreightAmount { get; set; }

        /// <summary>
        /// 入帐金额(入账金额=平台佣金+分销佣金+结算金额)
        /// </summary>
        public decimal RecognizedAmount { get { return PlatCommission + DistributorCommission + SettlementAmount; } }

    }
}
