using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Search.Service.Model
{
    /// <summary>
    /// 查询条件
    /// </summary>
    public class SearchCondition
    {
        public SearchCondition()
        {
            AttrIds = new List<string>();
            PageNumber = 1;
            PageSize = 10;
            OrderKey = 1;
            OrderType = true;
        }
        /// <summary>
        /// 查询关键字
        /// </summary>
        public string Keyword { get; set; }
        public string Ex_Keyword { get; set; }
        /// <summary>
        /// 品牌标识
        /// </summary>
        public long BrandId { get; set; }
        /// <summary>
        /// 属性集合
        /// </summary>
        public List<string> AttrIds { get; set; }
        /// <summary>
        /// 分类标识
        /// </summary>
        public long CategoryId { get; set; }
        /// <summary>
        /// 排序关键字/* 排序项（1：默认，2：销量，3：价格，4：评论数，5：上架时间） */
        /// </summary>
        public int OrderKey { get; set; }
        /// <summary>
        /// 排序类型
        /// </summary>
        public bool OrderType { get; set; }
        /// <summary>
        /// 店铺标识
        /// </summary>
        public long shopId { get; set; }
        /// <summary>
        /// 价格区间 低
        /// </summary>
        public decimal StartPrice { get; set; }
        /// <summary>
        /// 价格区间 高
        /// </summary>
        public decimal EndPrice { get; set; }
        /// <summary>
        /// 当前页
        /// </summary>
        public int PageNumber { get; set; }
        /// <summary>
        /// 每页数据
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// 店铺商品分类
        /// </summary>
        public long ShopCategoryId { get; set; }
    }
}
