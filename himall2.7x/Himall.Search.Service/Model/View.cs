using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Search.Service.Model
{
    /// <summary>
    /// 商品数据
    /// </summary>
    public class ProductView
    {
        /// <summary>
        /// 商品标识
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 店铺ID
        /// </summary>
        public long ShopId { get; set; }

        /// <summary>
        /// 图片路径
        /// </summary>
        public string ImagePath { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName
        {
            get; set;
        }
        /// <summary>
        /// 店铺名称
        /// </summary>
        public string ShopName { get; set; }
        /// <summary>
        /// 店铺所在地
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 目录分类标识
        /// </summary>
        public long CategoryId { get; set; }
        /// <summary>
        /// 出售数量
        /// </summary>
        public int SaleCount { get; set; }

        /// <summary>
        /// 评价数
        /// </summary>
        public int Comments { get; set; }
    }

    /// <summary>
    /// 商品信息集合
    /// </summary>
    public class ProductViewResult
    {
        public ProductViewResult()
        {
            Data = new List<ProductView>();
            Keys = new List<string>();
        }
        /// <summary>
        /// 当前页集合
        /// </summary>
        public List<ProductView> Data { get; set; }
        /// <summary>
        /// 总数
        /// </summary>
        public int Total { get; set; }
        /// <summary>
        /// 分词后的关键字
        /// </summary>
        public List<string> Keys { get; set; }
    }

    /// <summary>
    /// 商品属性信息
    /// </summary>
    public class AttributeView
    {

        public AttributeView()
        {
            AttrValues = new List<AttributeValue>();
        }
        /// <summary>
        /// 属性标识
        /// </summary>
        public long AttrId { get; set; }

        /// <summary>
        /// 属性名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 属性值集合
        /// </summary>
        public List<AttributeValue> AttrValues { get; set; }

    }

    public class AttributeValue
    {
        /// <summary>
        /// 属性标识
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 属性名称
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// 品牌信息
    /// </summary>
    public class BrandView
    {
        /// <summary>
        /// 名称
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 标识
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 品牌Logo
        /// </summary>
        public string Logo { get; set; }
    }

    /// <summary>
    /// 分类信息
    /// </summary>
    public class CategoryView
    {
        public CategoryView()
        {
            SubCategory = new List<CategoryView>();
        }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 子分类
        /// </summary>
        public List<CategoryView> SubCategory { get; set; }
    }
}
