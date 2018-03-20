using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    public class SearchProductQuery
    {
        public SearchProductQuery()
        {
            AttrValIds = new List<string>();
            PageNumber = 1;
            PageSize = 20;
            OrderKey = 1;
            OrderType = true;
        }
        /// <summary>
        /// 查询关键字
        /// </summary>
        public string Keyword { get; set; }
        /// <summary>
        /// 品牌标识
        /// </summary>
        public long BrandId { get; set; }
        /// <summary>
        /// 属性集合
        /// </summary>
        public List<string> AttrValIds { get; set; }
        /// <summary>
        /// 分类标识
        /// </summary>
        public long FirstCateId { get; set; }
        /// <summary>
        /// 分类标识
        /// </summary>
        public long SecondCateId { get; set; }
        /// <summary>
        /// 分类标识
        /// </summary>
        public long ThirdCateId { get; set; }
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
        public long ShopId { get; set; }
        /// <summary>
        /// 微店标识
        /// </summary>
        public long VShopId { get; set; }
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
       
        bool isLikeSearch = false;

        /// <summary>
        /// 当全文搜索查不到时是否使用like查询
        /// </summary>
        public bool IsLikeSearch
        {
            get
            {
                return isLikeSearch;
            }

            set
            {
                isLikeSearch = value;
            }
        }        
    }
}
