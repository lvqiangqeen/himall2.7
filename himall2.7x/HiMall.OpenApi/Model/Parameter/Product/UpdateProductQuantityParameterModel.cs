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
    public class UpdateProductQuantityParameterModel : BaseParameterModel
    {
        /// <summary>
        /// 商品编号
        /// </summary>
        public int num_iid { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int quantity { get; set; }
        /// <summary>
        /// 更新方式
        /// <para>1全量更新(默认) 2增量更新</para>
        /// </summary>
        public int? type { get; set; }
        /// <summary>
        /// 规格编号
        /// </summary>
        public string sku_id { get; set; }

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
        /// <summary>
        /// 值初始
        /// </summary>
        public override void ValueInit()
        {
            base.ValueInit();
            if (!this.type.HasValue)
            {
                this.type = 1;
            }
        }
    }
}
