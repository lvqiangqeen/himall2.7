using Himall.Model;
using System.Collections.Generic;
using System.Linq;

namespace Himall.IServices
{
    public interface ICartService : IService
    {
        /// <summary>
        /// 添加商品至购物车
        /// </summary>
        /// <param name="skuId">商品SKU Id</param>
        /// <param name="count">商品数量</param>
        /// <param name="memberId">会员id</param>
        void AddToCart(string skuId, int count, long memberId);

        /// <summary>
        /// 添加商品至购物车
        /// </summary>
        /// <param name="cartItems">购物车商品项</param>
        /// <param name="memberId">会员Id</param>
        void AddToCart(IEnumerable<ShoppingCartItem> cartItems, long memberId);

        /// <summary>
        /// 更新购物车
        /// </summary>
        /// <param name="skuId">商品SKU Id</param>
        /// <param name="count">商品数量</param>
        /// <param name="memberId">会员id</param>
        void UpdateCart(string skuId, int count, long memberId);

        /// <summary>
        /// 清空指定会员的购物车
        /// </summary>
        /// <param name="memeberId">会员id</param>
        void ClearCart(long memeberId);

        /// <summary>
        /// 删除指定会员购物车中的指定商品
        /// </summary>
        /// <param name="skuId">待删除的商品的skuid</param>
        /// <param name="memberId">会员id</param>
        void DeleteCartItem(string skuId, long memberId);


        /// <summary>
        /// 删除指定会员购物车中的指定商品
        /// </summary>
        /// <param name="skuIds">待删除的商品的skuid</param>
        /// <param name="memberId">会员id</param>
        void DeleteCartItem(IEnumerable<string> skuIds, long memberId);

        /// <summary>
        /// 获取指定会员购物车信息
        /// </summary>
        /// <param name="memeberId">会员id</param>
        /// <returns></returns>
        ShoppingCartInfo GetCart(long memeberId);

        /// <summary>
        /// 获取购物车购物项
        /// </summary>
        /// <param name="cartItemIds">购物车项Id</param>
        /// <returns></returns>
        IQueryable<ShoppingCartItem> GetCartItems(IEnumerable<long> cartItemIds);

        /// <summary>
        /// 获取购物车购物项
        /// </summary>
        /// <param name="skuIds">SKUId</param>
        /// <returns></returns>
        IQueryable<ShoppingCartItem> GetCartItems(IEnumerable<string> skuIds, long memberId);

        int GetCartProductQuantity(long memberId, long productId = 0, string skuId = "");
        IQueryable<ShoppingCartItem> GetCartQuantityByIds(long memberId,IEnumerable<long> productIds);
    }
}
