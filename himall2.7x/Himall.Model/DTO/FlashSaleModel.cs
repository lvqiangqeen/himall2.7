using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public class FlashSaleModel
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 店铺ID
        /// </summary>
        public long ShopId { get; set; }

        /// <summary>
        /// 店铺名称
        /// </summary>
        public string ShopName { get; set; }

        /// <summary>
        /// 商品ID
        /// </summary>
        public long ProductId { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public FlashSaleInfo.FlashSaleStatus Status { get; set; }

        /// <summary>
        /// 状态文字
        /// </summary>
        public string StatusStr { get; set; }

        /// <summary>
        /// 状态整形
        /// </summary>
        public int StatusNum { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public string BeginDate { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public string EndDate { get; set; }

        /// <summary>
        /// 限制每人购买的数量
        /// </summary>
        public int LimitCountOfThePeople { get; set; }

        /// <summary>
        /// 仅仅只计算在限时购里的销售数
        /// </summary>
        public int SaleCount { get; set; }
        
        /// <summary>
        /// 限时购商品SKU
        /// </summary>
        public List<FlashSaleDetailModel> Details { get; set; }

        /// <summary>
        /// 类别名称
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// 商品图片
        /// </summary>
        public string ProductImg { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        public decimal MinPrice { get; set; }

        /// <summary>
        /// 市场价格
        /// </summary>
        public decimal MarketPrice { get; set; }

        /// <summary>
        /// 是否开始
        /// </summary>
        public bool IsStarted { get; set; }

        /// <summary>
        /// 库存
        /// </summary>
        public long Quantity { get; set; }
    }

    public class FlashSalePrice
    {
        public long ProductId { get; set; }

        public decimal MinPrice { get; set; }
    }
}
