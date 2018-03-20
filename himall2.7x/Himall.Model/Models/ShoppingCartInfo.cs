using System;
using System.Collections.Generic;

namespace Himall.Model
{
    public class ShoppingCartInfo
    {

        public ShoppingCartInfo()
        {
            Items = new ShoppingCartItem[] { };
        }

        /// <summary>
        /// 购物车 商品项
        /// </summary>
        public IEnumerable<ShoppingCartItem> Items { get; set; }

        /// <summary>
        /// 会员Id
        /// </summary>
        public long MemberId { get; set; }

    }

    /// <summary>
    /// 购物车商品项
    /// </summary>
    public class ShoppingCartItem
    {
        /// <summary>
        /// 购物车商品项Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 商品Id
        /// </summary>
        public long ProductId { get; set; }

        /// <summary>
        /// 商品SKUID
        /// </summary>
        public string SkuId { get; set; }

        /// <summary>
        /// 商品数量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 添加时间
        /// </summary>
        public DateTime AddTime { get; set; }
    }


}
