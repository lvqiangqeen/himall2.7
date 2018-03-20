using Himall.Model;
using System;
using System.Collections.Generic;

namespace Himall.IServices.QueryModel
{
    public partial class ProductBrokerageQuery : QueryBase
    {
        /// <summary>
        /// 排序方式
        /// </summary>
        public enum EnumProductSort
        {
            /// <summary>
            /// 默认按顺序+编号 排序
            /// </summary>
            Default = 0,
            /// <summary>
            /// 按销量
            /// </summary>
            SalesNumber = 1,
            /// <summary>
            /// 按代理数
            /// </summary>
            AgentNum = 2,
            /// <summary>
            /// 按佣金
            /// </summary>
            Brokerage = 3,
            /// <summary>
            /// 价格升序
            /// </summary>
            PriceAsc = 4,
            /// <summary>
            /// 价格降序
            /// </summary>
            PriceDesc = 5,
            /// <summary>
            /// 月销量降序
            /// </summary>
            MonthDesc = 6,
            /// <summary>
            /// 星期销量降序
            /// </summary>
            WeekDesc = 7
        }
        /// <summary>
        /// 关键字
        /// </summary>
        public string skey { get; set; }

        /// <summary>
        /// 一级分类
        /// </summary>
        public string CategoryPathA { get; set; }
        /// <summary>
        /// 二级分类
        /// </summary>
        public string CategoryPathB { get; set; }
        /// <summary>
        /// 三级分类
        /// </summary>
        public string CategoryPathC { get; set; }
        /// <summary>
        /// 分类
        /// </summary>
        public long? CategoryId { get; set; }

        /// <summary>
        /// 店铺
        /// </summary>
        public long? ShopId { get; set; }
        /// <summary>
        /// 销售员
        /// </summary>
        public long? AgentUserId { get; set; }
        /// <summary>
        /// 推广状态
        /// </summary>
        public ProductBrokerageInfo.ProductBrokerageStatus? ProductBrokerageState { get; set; }
        /// <summary>
        /// 是否只显示正常销售的
        /// </summary>
        public bool? OnlyShowNormal { get; set; }
        /// <summary>
        /// 排序方法
        /// <para>重写属性</para>
        /// </summary>
        public new EnumProductSort Sort { get; set; }

        /// <summary>
        /// 商品IDs
        /// </summary>
        public string ProductIds { get; set; }
    }
}
