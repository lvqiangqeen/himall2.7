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
    /// 基础传入参数
    /// </summary>
    public class BasePageParameterModel : BaseParameterModel
    {
        /// <summary>
        /// 页码。
        /// <para>大于零的整数; 默认值:1</para>
        /// </summary>
        public int? page_no { get; set; }
        /// <summary>
        /// 每页条数。
        /// <para>大于零的整数; 默认值:40;最大值:100</para>
        /// </summary>
        public int? page_size { get; set; }

        /// <summary>
        /// 检测参数完整性与合法性
        /// </summary>
        /// <returns></returns>
        public override bool CheckParameter()
        {
            bool result = base.CheckParameter();
            if (result)
            {
                if (this.page_no < 1)
                {
                    throw new HimallOpenApiException(Hishop.Open.Api.OpenApiErrorCode.Invalid_Arguments, "page_no");
                }
                if (this.page_size < 1)
                {
                    throw new HimallOpenApiException(Hishop.Open.Api.OpenApiErrorCode.Invalid_Arguments, "page_size");
                }
                if (this.page_size > 100)
                {
                    throw new HimallOpenApiException(Hishop.Open.Api.OpenApiErrorCode.Page_Size_Too_Long, "page_size");
                }
            }
            return result;
        }
        /// <summary>
        /// 值初始
        /// </summary>
        public override void ValueInit()
        {
            base.ValueInit();
            if (!this.page_no.HasValue)
            {
                this.page_no = 1;
            }
            if (this.page_no < 1)
            {
                this.page_no = 1;
            }
            if (!this.page_size.HasValue)
            {
                this.page_size = 40;
            }
        }
    }
}
