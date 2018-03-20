using Himall.IServices;
using Himall.Model;
using Himall.Web;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using Himall.API.Model;
using Himall.API.Helper;
using Himall.API.Model.ParamsModel;
using Himall.Application;
using Himall.Core;

namespace Himall.API
{
    public class CartController : BaseApiController
    {
        public object GetCartProduct()
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
                            Status = (product.AuditStatus == ProductInfo.ProductAuditStatus.Audited && product.SaleStatus == ProductInfo.ProductSaleStatus.OnSale) ? 1 : 0,
                            ColorAlias = colorAlias,
                            SizeAlias = sizeAlias,
                            VersionAlias = versionAlias,
                            AddTime=item.AddTime
                        };
                        products.Add(_tmp);
                    }
                }
            }
            products = products.OrderBy(item => item.ShopId).ThenByDescending(o => o.AddTime).ToList();
            //var invalidProducts = products.Where(item => item.Status == 0).ToArray();
            //products = products.Where(item => item.Status == 1).ToArray();
            var cartShop=products.GroupBy(item=>item.ShopId);

            var cartModel = new { Success = "true", Shop = cartShop };
            return Json(cartModel);
        }

        /// <summary>
        /// 删除购物车商品
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPost]
        public object PostDeleteCartProduct(CartDeleteCartProductModel value)
        {
            CheckUserLogin();
            var skuIdsArr = value.skuIds.ToString().Split(',');
            ServiceProvider.Instance<ICartService>.Create.DeleteCartItem(skuIdsArr, CurrentUser.Id);
            dynamic d = new System.Dynamic.ExpandoObject();
            d.Success = "true";
            return d;
        }
        //更新购物车商品数量(这里没有及时更新,而是本地更新等会员点击购买的时候再更新到数据库并且提交订单)
        [HttpPost]
        public object PostUpdateCartItem(CartUpdateCartItemModel value)
        {
            CheckUserLogin();
            string Jsonstr = value.jsonstr;
            var datavalue = Newtonsoft.Json.JsonConvert.DeserializeObject<UpdateCartSKusModel>(Jsonstr);
            var cartService = ServiceProvider.Instance<ICartService>.Create;
            foreach (var sku in datavalue.skus)
            {
                cartService.UpdateCart(sku.skuId, sku.count, CurrentUser.Id);
            }
            dynamic d = new System.Dynamic.ExpandoObject();
            d.Success = "true";
            //d.Url = "http://" + Url.Request.RequestUri.Host + "/m-IOS/Order/SubmiteByCart";
            d.Url = Core.HimallIO.GetRomoteImagePath("/m-IOS/Order/SubmiteByCart");
            return d;
        }
        //添加商品到购物车
        public object PostAddProductToCart(CartAddProductToCartModel value)
        {
            CheckUserLogin();
            string skuId = value.skuId;
            int count = value.count;
            CartHelper cartHelper = new CartHelper();
            long userId = CurrentUser != null ? CurrentUser.Id : 0;
            cartHelper.AddToCart(skuId, count, userId);
            return (new { Success = "true" });
        }
    }
}
