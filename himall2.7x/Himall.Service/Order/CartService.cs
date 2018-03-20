using Himall.Core;
using Himall.Entity;
using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Himall.Service
{
    public class CartService : ServiceBase, ICartService
    {
        public void AddToCart(string skuId, int count, long memberId)
        {
            if (count != 0)
            {
                CheckCartItem(skuId, count, memberId);
                var cartItem = Context.ShoppingCartItemInfo.FirstOrDefault(item => item.UserId == memberId && item.SkuId == skuId);
                if (cartItem != null)//首先查询，如果已经存在则直接更新，否则新建
                {
                    cartItem.Quantity += count;//否则更新数量
                }
                else if (count > 0)
                {
                    long productId = long.Parse(skuId.Split('_')[0]);//SKU第一节为商品Id
                    Context.ShoppingCartItemInfo.Add(new ShoppingCartItemInfo() { UserId = memberId, Quantity = count, SkuId = skuId, ProductId = productId, AddTime = DateTime.Now });
                }
                Context.SaveChanges();
                Cache.Remove(CacheKeyCollection.CACHE_CART(memberId));
            }
        }

        public void AddToCart(IEnumerable<ShoppingCartItem> cartItems, long memberId)
        {
            foreach (var cartItem in cartItems.ToList())
            {
                CheckCartItem(cartItem.SkuId, cartItem.Quantity, memberId);
                var oriCartItem = Context.ShoppingCartItemInfo.FirstOrDefault(item => item.UserId == memberId && item.SkuId == cartItem.SkuId);
                if (oriCartItem != null)//首先查询，如果已经存在则直接更新，否则新建
                    oriCartItem.Quantity += cartItem.Quantity;
                else
                {
                    long productId = long.Parse(cartItem.SkuId.Split('_')[0]);//SKU第一节为商品Id
                    Context.ShoppingCartItemInfo.Add(new ShoppingCartItemInfo() { UserId = memberId, Quantity = cartItem.Quantity, SkuId = cartItem.SkuId, ProductId = productId, AddTime = DateTime.Now });
                }
            }
            Context.SaveChanges();
            Cache.Remove(CacheKeyCollection.CACHE_CART(memberId));
        }

        public void UpdateCart(string skuId, int count, long memberId)
        {
            CheckCartItem(skuId, count, memberId);
            var cartItem = Context.ShoppingCartItemInfo.FirstOrDefault(item => item.UserId == memberId && item.SkuId == skuId);
            if (cartItem != null)//首先查询，如果已经存在则直接更新，否则新建
            {
                if (count == 0)//数量为0时，删除对应项
                    Context.ShoppingCartItemInfo.Remove(cartItem.Id);
                else
                    cartItem.Quantity = count;//否则更新数量
            }
            else if (count > 0)
            {
                long productId = long.Parse(skuId.Split('_')[0]);//SKU第一节为商品Id
                Context.ShoppingCartItemInfo.Add(new ShoppingCartItemInfo() { UserId = memberId, Quantity = count, SkuId = skuId, ProductId = productId, AddTime = DateTime.Now });
            }
            Context.SaveChanges();
            Cache.Remove(CacheKeyCollection.CACHE_CART(memberId));
        }

        public void ClearCart(long memeberId)
        {
            Context.ShoppingCartItemInfo.Remove(item => item.UserId == memeberId);
            Context.SaveChanges();
            Cache.Remove(CacheKeyCollection.CACHE_CART(memeberId));
        }

        public void DeleteCartItem(string skuId, long memberId)
        {
            Context.ShoppingCartItemInfo.Remove(item => item.SkuId == skuId && item.UserId == memberId);
            Context.SaveChanges();
            Cache.Remove(CacheKeyCollection.CACHE_CART(memberId));
        }


        public void DeleteCartItem(IEnumerable<string> skuIds, long memberId)
        {
            Context.ShoppingCartItemInfo.Remove(item => skuIds.Contains(item.SkuId) && item.UserId == memberId);
            Context.SaveChanges();
            Cache.Remove(CacheKeyCollection.CACHE_CART(memberId));
        }

        public ShoppingCartInfo GetCart(long memeberId)
        {
            if (Cache.Exists(CacheKeyCollection.CACHE_CART(memeberId)))
                return Cache.Get<ShoppingCartInfo>(CacheKeyCollection.CACHE_CART(memeberId));

            ShoppingCartInfo shoppingCartInfo = new ShoppingCartInfo() { MemberId = memeberId };
            //var prosql = Context.ProductInfo.Where(d=>d.IsDeleted == false).Select(d=>d.Id);
            var cartItems = Context.ShoppingCartItemInfo.Where(item => item.UserId == memeberId);
            shoppingCartInfo.Items = cartItems.Select(item => new ShoppingCartItem()
            {
                Id = item.Id,
                SkuId = item.SkuId,
                Quantity = item.Quantity,
                AddTime = item.AddTime,
                ProductId = item.ProductId
            });
            Cache.Insert<ShoppingCartInfo>(CacheKeyCollection.CACHE_CART(memeberId), shoppingCartInfo, 600);
            return shoppingCartInfo;
        }




        void CheckCartItem(string skuId, int count, long memberId)
        {
            if (string.IsNullOrWhiteSpace(skuId))
                throw new InvalidPropertyException("SKUId不能为空");
            else if (count < 0)
                throw new InvalidPropertyException("商品数量不能小于0");
            else
            {
                var member = Context.UserMemberInfo.FirstOrDefault(item => item.Id == memberId);
                if (member == null)
                    throw new InvalidPropertyException("会员Id" + memberId + "不存在");
            }
        }

        public IQueryable<ShoppingCartItem> GetCartItems(IEnumerable<long> cartItemIds)
        {
            var shoppingCartItems = Context.ShoppingCartItemInfo
                .FindBy(item => cartItemIds.Contains(item.Id))
                .Select(item => new ShoppingCartItem()
                {
                    Id = item.Id,
                    SkuId = item.SkuId,
                    Quantity = item.Quantity,
                    ProductId = item.ProductId,
                    AddTime = item.AddTime
                });
            return shoppingCartItems;
        }


        public IQueryable<ShoppingCartItem> GetCartItems(IEnumerable<string> skuIds, long memberId)
        {
            return Context.ShoppingCartItemInfo.Where(item => item.UserId == memberId && skuIds.Contains(item.SkuId))
                                               .Select(item => new ShoppingCartItem()
                                               {
                                                   Id = item.Id,
                                                   SkuId = item.SkuId,
                                                   Quantity = item.Quantity,
                                                   ProductId = item.ProductId,
                                                   AddTime = item.AddTime
                                               });
        }
        /// <summary>
        /// 获取购物车对应商品数量
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="skuId"></param>
        /// <returns></returns>
        public int GetCartProductQuantity(long memberId, long productId = 0, string skuId = "")
        {
            int cartQuantity = 0;
            if (productId > 0)
            {
                var shopInfo = Context.ShoppingCartItemInfo.Where(p => p.UserId == memberId && p.ProductId == productId);
                if (shopInfo != null)
                {
                    cartQuantity += shopInfo.Sum(d => d.Quantity);
                }
            }
            else
            {
                var shopInfo = Context.ShoppingCartItemInfo.Where(p => p.UserId == memberId && p.SkuId == skuId);
                if (shopInfo != null)
                {
                    cartQuantity += shopInfo.Sum(d => d.Quantity);
                }
            }
            return cartQuantity;
        }

        public IQueryable<ShoppingCartItem> GetCartQuantityByIds(long memberId, IEnumerable<long> productIds)
        {
            return Context.ShoppingCartItemInfo.Where(item => item.UserId == memberId && productIds.Contains(item.ProductId))
                .Select(item => new ShoppingCartItem()
                {
                    Id = item.Id,
                    SkuId = item.SkuId,
                    Quantity = item.Quantity,
                    ProductId = item.ProductId,
                    AddTime = item.AddTime
                });
        }
    }
}
