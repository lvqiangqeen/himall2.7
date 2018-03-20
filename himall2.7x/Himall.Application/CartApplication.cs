using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Core;
using Himall.IServices;
using Himall.Model;

namespace Himall.Application
{
   public class CartApplication
    {
        private static ICartService _iCartService = ObjectContainer.Current.Resolve<ICartService>();
        /// <summary>
        /// 添加商品至购物车
        /// </summary>
        /// <param name="skuId">商品SKU Id</param>
        /// <param name="count">商品数量</param>
        /// <param name="memberId">会员id</param>
        public static  void AddToCart(string skuId, int count, long memberId)
        {
            _iCartService.AddToCart(skuId, count, memberId);
        }

        /// <summary>
        /// 添加商品至购物车
        /// </summary>
        /// <param name="cartItems">购物车商品项</param>
        /// <param name="memberId">会员Id</param>
        public static void AddToCart(IEnumerable<ShoppingCartItem> cartItems, long memberId)
        {
            _iCartService.AddToCart(cartItems, memberId);
        }

        /// <summary>
        /// 更新购物车
        /// </summary>
        /// <param name="skuId">商品SKU Id</param>
        /// <param name="count">商品数量</param>
        /// <param name="memberId">会员id</param>
        public static void UpdateCart(string skuId, int count, long memberId)
        {
            _iCartService.UpdateCart(skuId, count, memberId);
        }

        /// <summary>
        /// 清空指定会员的购物车
        /// </summary>
        /// <param name="memeberId">会员id</param>
        public static void ClearCart(long memeberId)
        {
            _iCartService.ClearCart(memeberId);
        }

        /// <summary>
        /// 删除指定会员购物车中的指定商品
        /// </summary>
        /// <param name="skuId">待删除的商品的skuid</param>
        /// <param name="memberId">会员id</param>
        public static void DeleteCartItem(string skuId, long memberId)
        {
            _iCartService.DeleteCartItem(skuId, memberId);
        }


        /// <summary>
        /// 删除指定会员购物车中的指定商品
        /// </summary>
        /// <param name="skuIds">待删除的商品的skuid</param>
        /// <param name="memberId">会员id</param>
        public static void DeleteCartItem(IEnumerable<string> skuIds, long memberId)
        {
            _iCartService.DeleteCartItem(skuIds, memberId);
        }

        /// <summary>
        /// 获取指定会员购物车信息
        /// </summary>
        /// <param name="memeberId">会员id</param>
        /// <returns></returns>
        public static ShoppingCartInfo GetCart(long memeberId)
        {
          return   _iCartService.GetCart(memeberId);
        }

        /// <summary>
        /// 获取购物车购物项
        /// </summary>
        /// <param name="cartItemIds">购物车项Id</param>
        /// <returns></returns>
        public static IQueryable<ShoppingCartItem> GetCartItems(IEnumerable<long> cartItemIds)
        {
           return  _iCartService.GetCartItems(cartItemIds);
        }

        /// <summary>
        /// 获取购物车购物项
        /// </summary>
        /// <param name="skuIds">SKUId</param>
        /// <returns></returns>
        public static IQueryable<ShoppingCartItem> GetCartItems(IEnumerable<string> skuIds, long memberId)
        {
          return   _iCartService.GetCartItems(skuIds, memberId);
        }

        public static List<ShoppingCartItem> GetCartQuantityByIds(long memberId,IEnumerable<long> productIds)
        {
            var shopcart = _iCartService.GetCartQuantityByIds(memberId,productIds).ToList();
            return AutoMapper.Mapper.Map<List<ShoppingCartItem>>(shopcart);
        }
    }
}
