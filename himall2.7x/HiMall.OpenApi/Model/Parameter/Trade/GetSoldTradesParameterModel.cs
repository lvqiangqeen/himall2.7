using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Core;
using Hishop.Open.Api;

namespace Himall.OpenApi.Model.Parameter
{
    /// <summary>
    /// 商品/SKU库存修改传入参数
    /// </summary>
    public class GetSoldTradesParameterModel : BasePageParameterModel
    {
        /// <summary>
        /// 获取时间区间内订单的开始时间。
        /// <para>yyyy-MM-dd HH:mm:ss</para>
        /// </summary>
        public DateTime? start_created { get; set; }
        /// <summary>
        /// 获取时间区间内订单的结束时间
        /// </summary>
        public DateTime? end_created { get; set; }
        /// <summary>
        /// 交易状态
        /// <para>WAIT_BUYER_PAY（等待买家付款）、 WAIT_SELLER_SEND_GOODS （等待商家发货）、 WAIT_BUYER_CONFIRM_GOODS（等待买家确认收货）、 TRADE_CLOSED （交易关闭）、TRADE_FINISHED（交易成功） 默认查询所有交易状态的数据， 除了默认值外每次只能查询一种状态</para>
        /// </summary>
        public string status { get; set; }
        /// <summary>
        /// 买家帐号
        /// </summary>
        public string buyer_uname { get; set; }

        /// <summary>
        /// 检测参数完整性与合法性
        /// </summary>
        /// <returns></returns>
        public override bool CheckParameter()
        {
            bool result = base.CheckParameter();

            if (start_created.HasValue)
            {
                if (start_created.Value > DateTime.Now)
                {
                    throw new HimallOpenApiException(OpenApiErrorCode.Time_Start_Now, "start_created");
                }
                if (end_created.HasValue && start_created.Value >= end_created.Value)
                {
                    throw new HimallOpenApiException(OpenApiErrorCode.Time_Start_End, "start_created");
                }
            }

            if (end_created.HasValue && end_created.Value > DateTime.Now)
            {
                //throw new HimallOpenApiException(OpenApiErrorCode.Time_End_Now, "end_created");
            }


            return result;
        }
        /// <summary>
        /// 值初始
        /// </summary>
        public override void ValueInit()
        {
            base.ValueInit();
        }
    }
}
