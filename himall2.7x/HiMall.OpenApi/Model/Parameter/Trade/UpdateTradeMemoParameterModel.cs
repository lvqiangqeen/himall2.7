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
    public class UpdateTradeMemoParameterModel : BaseParameterModel
    {
        /// <summary>
        /// 交易编号
        /// </summary>
        public string tid { get; set; }
        /// <summary>
        /// 备注文本
        /// </summary>
        public string memo { get; set; }
        /// <summary>
        /// 备注标记
        /// <para>云商城系统的可选值分别是1~6的正整数，实际情况请参考对接的系统</para>
        /// </summary>
        public int? flag { get; set; }
        /// <summary>
        /// 检测参数完整性与合法性
        /// </summary>
        /// <returns></returns>
        public override bool CheckParameter()
        {
            bool result = base.CheckParameter();

            long orderId = 0;
            if(string.IsNullOrWhiteSpace(tid))
            {
                throw new HimallOpenApiException(OpenApiErrorCode.Biz_Order_ID_is_Invalid, "tid");
            }
            if (!long.TryParse(this.tid, out orderId))
            {
                throw new HimallOpenApiException(OpenApiErrorCode.Biz_Order_ID_is_Invalid, "tid");
            }
            if (orderId < 1)
            {
                throw new HimallOpenApiException(OpenApiErrorCode.Biz_Order_ID_is_Invalid, "tid");
            }
            if (memo.Length > 1000)
            {
                throw new HimallOpenApiException(OpenApiErrorCode.Trade_Memo_Too_Long, "memo");
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
