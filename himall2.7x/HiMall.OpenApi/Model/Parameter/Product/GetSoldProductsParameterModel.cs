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
    public class GetSoldProductsParameterModel : BasePageParameterModel
    {
        /// <summary>
        /// 起始的修改时间
        /// </summary>
        public DateTime? start_modified {get; set; }
        /// <summary>
        /// 结束的修改时间
        /// </summary>
        public DateTime? end_modified { get; set; }
        /// <summary>
        /// 商品状态
        /// <para>默认查询所有状态的数据，除了默认值外每次只能查询一种状态</para>
        /// </summary>
        public string approve_status { get; set; }
        /// <summary>
        /// 搜索关键字
        /// </summary>
        public string q { get; set; }
        /// <summary>
        /// 排序方式，格式为column:asc/desc
        /// <para>column可选值:display_sequence（默认顺序） create_time(创建时间),sold_quantity（商品销量）;默认商品排序编号升序(diplay_sequence值越小在前)。</para>
        /// </summary>
        public string order_by { get; set; }

        /// <summary>
        /// 检测参数完整性与合法性
        /// </summary>
        /// <returns></returns>
        public override bool CheckParameter()
        {
            bool result = base.CheckParameter();

            if (start_modified.HasValue)
            {
                if (start_modified.Value > DateTime.Now)
                {
                    throw new HimallOpenApiException(OpenApiErrorCode.Time_Start_Now, "start_modified");
                }
                if (end_modified.HasValue && start_modified.Value >= end_modified.Value)
                {
                    throw new HimallOpenApiException(OpenApiErrorCode.Time_Start_End, "end_modified");
                }
            }

            if (end_modified.HasValue && end_modified.Value > DateTime.Now)
            {
                //throw new HimallOpenApiException(OpenApiErrorCode.Time_End_Now, "end_modified");
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
