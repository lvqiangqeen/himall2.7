using Himall.Application;
using Himall.CommonModel;
using Himall.Core;
using Himall.IServices;
using Himall.Model;
using Himall.SmallProgAPI.Helper;
using Himall.SmallProgAPI.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.SmallProgAPI.Helper;
using Himall.SmallProgAPI.Model;
using System.Web.Mvc;

namespace Himall.SmallProgAPI
{
    public class CartController : BaseApiController
    {
        /// <summary>
        /// 添加到购物车
        /// </summary>
        /// <param name="context"></param>
        public object GetAddToCart(string openId, string SkuID, int Quantity, int GiftID = 0)
        {
            //验证用户
            CheckUserLogin();
            CartHelper cartHelper = new CartHelper();
            long userId = CurrentUser != null ? CurrentUser.Id : 0;
            var msg=cartHelper.AddToCart(SkuID, Quantity, userId);
            if (!string.IsNullOrEmpty(msg))
            {
                return Json(new { Status = "NO", Message = msg });
            }
            else
            {
                return Json(new { Status = "OK" });
            }
        }
        /// <summary>
        /// 更新购物车数量
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="SkuID"></param>
        /// <param name="Quantity"></param>
        /// <param name="GiftID"></param>
        /// <returns></returns>
        public object GetUpdateToCart(string openId, string SkuID, int Quantity, int GiftID = 0)
        {
            //验证用户
            CheckUserLogin();
            CartHelper cartHelper = new CartHelper();
            long userId = CurrentUser != null ? CurrentUser.Id : 0;

            ShoppingCartInfo CartInfo = new ShoppingCartInfo();
            if (CurrentUser != null)
            {
                CartInfo = ServiceProvider.Instance<ICartService>.Create.GetCart(CurrentUser.Id);
            }

            var oldQuantity= GetCartProductQuantity(CartInfo, skuId: SkuID);
            oldQuantity = oldQuantity + Quantity;

            cartHelper.UpdateCartItem(SkuID, oldQuantity, userId);
            //调用查询购物车数据
            return Json(new { Status = "OK" });
        }

        /// <summary>
        /// 从购物车移除
        /// </summary>
        /// <param name="context"></param> 
        public object GetdelCartItem(string openId, string SkuIds, int GiftID=0)
        {
            //验证用户
            CheckUserLogin();
            CartHelper cartHelper = new CartHelper();
            long userId = CurrentUser != null ? CurrentUser.Id : 0;
            var skuIdsArr = SkuIds.ToString().Split(',');
            cartHelper.RemoveFromCart(skuIdsArr, userId);
            //调用查询购物车数据

            return Json(new { Status = "OK" });
        }

        public object GetCartProduct(string openId="")
        {
            CheckUserLogin();
            var cartHelper = ServiceProvider.Instance<ICartService>.Create;
            var cart = cartHelper.GetCart(CurrentUser.Id);
            var productService = ServiceProvider.Instance<IProductService>.Create;
            var shopService = ServiceProvider.Instance<IShopService>.Create;
            var vshopService = ServiceProvider.Instance<IVShopService>.Create;
            var siteSetting = ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettings();
            var typeservice = ServiceProvider.Instance<ITypeService>.Create;
            List<CartProductModel> products = new List<CartProductModel>();
            //会员折扣
            decimal discount = 1.0M;//默认折扣为1（没有折扣）
            if (CurrentUser != null)
            {
                discount = CurrentUser.MemberDiscount;
            }
            decimal prodPrice = 0.0M;//优惠价格
            var limitProducts = LimitTimeApplication.GetPriceByProducrIds(cart.Items.Select(e => e.ProductId).ToList());//限时购价格
            foreach (var item in cart.Items)
            {
                var product = productService.GetProduct(item.ProductId);
                var shop = shopService.GetShop(product.ShopId);

                if (null != shop)
                {
                    var vshop = vshopService.GetVShopByShopId(shop.Id);
                    SKUInfo sku = productService.GetSku(item.SkuId);
                    if (sku == null)
                    {
                        continue;
                    }
                    //处理限时购、会员折扣价格
                    var prod = limitProducts.FirstOrDefault(e => e.ProductId == item.ProductId);
                    prodPrice = sku.SalePrice;

                    if (prod != null)
                    {
                        prodPrice = prod.MinPrice;
                    }
                    else
                    {
                        if (shop.IsSelf)
                        {//官方自营店才计算会员折扣
                            prodPrice = sku.SalePrice * discount;
                        }
                    }
                    ProductTypeInfo typeInfo = typeservice.GetType(product.TypeId);
                    string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                    string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                    string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;

                    if (sku != null)
                    {
                        var _tmp = new CartProductModel
                        {
                            CartItemId = item.Id.ToString(),
                            SkuId = item.SkuId,
                            Id = product.Id.ToString(),
                            ImgUrl = Core.HimallIO.GetRomoteProductSizeImage(product.RelativePath, 1, (int)Himall.CommonModel.ImageSize.Size_150),
                            Name = product.ProductName,
                            Price = prodPrice.ToString("F2"),
                            Count = item.Quantity.ToString(),
                            ShopId = shop.Id.ToString(),
                            Size = sku.Size,
                            Color = sku.Color,
                            Version = sku.Version,
                            VShopId = vshop == null ? "0" : vshop.Id.ToString(),
                            ShopName = shop.ShopName,
                            ShopLogo = vshop == null ? "" : Core.HimallIO.GetRomoteImagePath(vshop.Logo),
                            Url = Core.HimallIO.GetRomoteImagePath("/m-IOS/product/detail/") + item.ProductId,
                            IsValid = (product.AuditStatus == ProductInfo.ProductAuditStatus.Audited && product.SaleStatus == ProductInfo.ProductSaleStatus.OnSale) ? 1 : 0,
                            ColorAlias = colorAlias,
                            SizeAlias = sizeAlias,
                            VersionAlias = versionAlias,
                            AddTime = item.AddTime
                        };
                        products.Add(_tmp);
                    }
                }
            }

            products = products.OrderBy(item => item.ShopId).ThenByDescending(o => o.AddTime).ToList();
            var cartShop = products.GroupBy(item => item.ShopId);
            var cartModel = new { Status = "OK", Data = cartShop };
            return Json(cartModel);//原api返回
        }

        /// <summary>
        /// 检查失效商品
        /// </summary>
        /// <param name="skus"></param>
        /// <param name="memeberId"></param>
        /// <returns></returns>
        public object GetCanSubmitOrder(string openId,string skus)
        {
            CheckUserLogin();
            var json = new object();
            if (!string.IsNullOrEmpty(skus))
            {
                bool status = true;
                var SkuIds = skus.Split(',').Where(d => !string.IsNullOrWhiteSpace(d)).ToList();
                CartHelper cartHelper = new CartHelper();
                foreach (var item in SkuIds)
	            {
                    if (!cartHelper.CheckSkuId(item))
                    {
                        status = false;
                    }
	            }
                if (status)
                {
                    json = GetOKJson("OK");
                }
                else
                {
                    json = GetErrorJson("有失效商品");
                }
            }
            else
            {
                json = GetErrorJson("请选择商品");
            }
            return json;
        }

        private int GetCartProductQuantity(ShoppingCartInfo cartInfo, long productId = 0, string skuId = "")
        {
            int cartQuantity = 0;
            if (cartInfo == null)
            {
                return 0;
            }
            else
            {
                if (productId > 0)
                {
                    cartQuantity += cartInfo.Items.Where(p => p.ProductId == productId).Sum(d => d.Quantity);
                }
                else
                {
                    cartQuantity += cartInfo.Items.Where(p => p.SkuId == skuId).Sum(d => d.Quantity);
                }
            }
            return cartQuantity;
        }
        object GetOKJson(string okMsg)
        {
            var message = new
            {
                Status = "OK",
                Message = okMsg
            };
            return message;
        }
        object GetErrorJson(string errorMsg)
        {
            var message = new
            {
                Status = "NO",
                Message = errorMsg
            };
            return message;
        }
    }
}
