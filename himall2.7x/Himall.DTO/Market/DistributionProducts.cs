using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Core;

namespace Himall.DTO
{
    /// <summary>
    /// 分销商品
    /// </summary>
    public class DistributionProducts
    {
        /// <summary>
        /// 设置ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 分销ID
        /// </summary>
        public long ProductbrokerageId { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sequence { get; set; }

        /// <summary>
        /// 商品ID
        /// </summary>
        public long ProductId { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 商品图片
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// 商品价格
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 品牌
        /// </summary>
        public string Brand { get; set; }


        /// <summary>
        /// 三级分类名称
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// 分销状态
        /// </summary>
        public Himall.Model.ProductBrokerageInfo.ProductBrokerageStatus Status { get; set; }

        /// <summary>
        /// 分销状态显示值
        /// </summary>
        public string ProDisStatus { get { return Status.ToDescription(); } }

        /// <summary>
        /// 分销佣金
        /// </summary>
        public decimal Commission { get; set; }

        /// <summary>
        /// 是否已代理
        /// </summary>
        public bool isHasAgent { get; set; }

        /// <summary>
        /// 成交数
        /// </summary>
        public long SaleNum { get; set; }

        /// <summary>
        /// 代理次数
        /// </summary>
        public int AgentNum { get; set; }
        /// <summary>
        /// 广告语
        /// </summary>
        public string ShortDescription { get; set; }
    }
}
