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
    /// 获取商品详情传入参数
    /// </summary>
    public class GetProductParameterModel : BaseParameterModel
    {
        /// <summary>
        /// 商品编号
        /// </summary>
        public int num_iid { get; set; }

        /// <summary>
        /// 检测参数完整性与合法性
        /// </summary>
        /// <returns></returns>
        public override bool CheckParameter()
        {
            bool result = base.CheckParameter();
            if (num_iid < 1)
            {
                throw new HimallOpenApiException(OpenApiErrorCode.Product_Not_Exists, "num_iid");
            }
            return result;
        }
    }
}
