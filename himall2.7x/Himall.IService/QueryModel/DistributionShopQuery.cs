using Himall.Model;
using System;
using System.Collections.Generic;

namespace Himall.IServices.QueryModel
{
    public partial class DistributionShopQuery : QueryBase
    {
        /// <summary>
        /// 排序方式
        /// </summary>
        public enum EnumShopSort
        {
            /// <summary>
            /// 默认按销量
            /// </summary>
            Default=0,
            /// <summary>
            /// 按销量
            /// </summary>
            SalesNumber=1,
            /// <summary>
            /// 按商品数
            /// </summary>
            ProductNum = 2,
            /// <summary>
            /// 按代理数
            /// </summary>
            AgentNum = 3
        }
        /// <summary>
        /// 关键字
        /// </summary>
        public string skey { get; set; }
        /// <summary>
        /// 排序方法
        /// <para>重写属性</para>
        /// </summary>
        public new EnumShopSort Sort { get; set; }
    }
}
