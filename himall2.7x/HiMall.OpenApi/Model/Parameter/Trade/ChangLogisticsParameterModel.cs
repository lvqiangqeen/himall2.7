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
    public class ChangLogisticsParameterModel : BaseParameterModel
    {
        /// <summary>
        /// 交易编号
        /// </summary>
        public string tid { get; set; }
        /// <summary>
        /// 物流公司名称
        /// </summary>
        public string company_name { get; set; }
        /// <summary>
        /// 运单号
        /// </summary>
        public string out_sid { get; set; }
        /// <summary>
        /// 检测参数完整性与合法性
        /// </summary>
        /// <returns></returns>
        public override bool CheckParameter()
        {
            bool result = base.CheckParameter();

            long orderId = 0;
            if (string.IsNullOrWhiteSpace(tid))
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

            if(string.IsNullOrWhiteSpace(company_name))
            {
                throw new HimallOpenApiException(OpenApiErrorCode.Missing_Required_Arguments, "company_name");
            }
            if (string.IsNullOrWhiteSpace(out_sid))
            {
                throw new HimallOpenApiException(OpenApiErrorCode.Missing_Required_Arguments, "out_sid");
            }

            if (this.out_sid.Length > 100)
            {
                throw new HimallOpenApiException(OpenApiErrorCode.Out_Sid_Too_Long, "out_sid");
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
