using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    /// <summary>
    /// 交易统计
    /// </summary>
    public class TradeStatisticModel
    {
        public System.DateTime Date { get; set; }
        public long VistiCounts { get; set; }
        public long SaleCounts { get; set; }
        public decimal SaleAmounts { get; set; }
        public long OrderUserCount { get; set; }
        public long OrderCount { get; set; }
        public long OrderProductCount { get; set; }
        public decimal OrderAmount { get; set; }
        public long OrderPayUserCount { get; set; }
        public long OrderPayCount { get; set; }

        public bool StatisticFlag { get; set; }
        /// <summary>
        /// 订单转化率
        /// </summary>
        public decimal OrderConversionsRates
        {
            get {
                if (this.VistiCounts > 0)
                    return Math.Round((Convert.ToDecimal(this.OrderUserCount) / this.VistiCounts) * 100, 2);
                else
                {
                    if (this.OrderUserCount > 0)
                    {//有订单，没浏览人数，默认为100%
                        return Math.Round((Convert.ToDecimal(this.OrderUserCount) / this.OrderUserCount) * 100, 2);
                    }
                    return 0;
                }
            }
        }
        /// <summary>
        /// 付款转化率
        /// </summary>
        public decimal PayConversionsRates
        {
            get
            {
                if (this.OrderUserCount > 0)
                    return Math.Round((Convert.ToDecimal(this.OrderPayUserCount) / this.OrderUserCount) * 100, 2);
                else
                {
                    if (this.OrderPayUserCount > 0)
                    {//有付款，没浏览人数，默认为100%
                        return Math.Round((Convert.ToDecimal(this.OrderPayUserCount) / this.OrderPayUserCount) * 100, 2);
                    }
                    return 0;
                }
            }
        }
        /// <summary>
        /// 成交转化率
        /// </summary>
        public decimal TransactionConversionRate
        {
            get
            {
                if (this.VistiCounts > 0)
                    return Math.Round((Convert.ToDecimal(this.OrderPayUserCount) / this.VistiCounts) * 100, 2);
                else
                {
                    if (this.OrderPayUserCount > 0)
                    {//有付款，没浏览人数，默认为100%
                        return Math.Round((Convert.ToDecimal(this.OrderPayUserCount) / this.OrderPayUserCount) * 100, 2);
                    }
                    return 0;
                }
            }
        }

        /// <summary>
        /// 导出时添加百分号
        /// </summary>
        public string OrderConversionsRatesString
        {
            get
            {
                return OrderConversionsRates + "%";
            }
        }
        /// <summary>
        /// 导出时添加百分号
        /// </summary>
        public string PayConversionsRatesString
        {
            get
            {
                return PayConversionsRates + "%";
            }
        }
        /// <summary>
        /// 导出时添加百分号
        /// </summary>
        public string TransactionConversionRateString
        {
            get
            {
                return TransactionConversionRate + "%";
            }
        }

        /// <summary>
        /// 付款金额线
        /// </summary>
        public LineChartDataModel<decimal> ChartModelPayAmounts { get; set; }
        /// <summary>
        /// 付款人数线
        /// </summary>
        public LineChartDataModel<long> ChartModelPayUsers { get; set; }
        /// <summary>
        /// 付款件数线
        /// </summary>
        public LineChartDataModel<long> ChartModelPayPieces { get; set; }
        /// <summary>
        /// 下单转化率线
        /// </summary>
        public LineChartDataModel<decimal> ChartModelOrderConversionsRates { get; set; }
        /// <summary>
        /// 付款转化率线
        /// </summary>
        public LineChartDataModel<decimal> ChartModelPayConversionsRates { get; set; }
        /// <summary>
        /// 成交转化率线
        /// </summary>
        public LineChartDataModel<decimal> ChartModelTransactionConversionRate { get; set; }

    }
}

