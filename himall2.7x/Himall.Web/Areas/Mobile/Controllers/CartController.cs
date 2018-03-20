using Himall.Application;
using Himall.CommonModel;
using Himall.Core;
using Himall.Core.Helper;
using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Areas.Mobile.Controllers
{
    public class CartController : BaseMobileMemberController
    {
        IProductService _iProductService;
        IShopService _iShopService;
        IVShopService _iVShopService;
        public CartController(IProductService iProductService, IShopService iShopService, IVShopService iVShopService)
        {
            _iProductService = iProductService;
            _iShopService = iShopService;
            _iVShopService = iVShopService;
        }
        // GET: Mobile/Cart
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Cart()
        {
            return View();
        }

        [HttpPost]
        public JsonResult AddProductToCart(string skuId, int count)
        {
            CartHelper cartHelper = new CartHelper();
            long userId = CurrentUser != null ? CurrentUser.Id : 0;
            cartHelper.AddToCart(skuId, count, userId);
            return Json(new { success = true });
        }

        [HttpPost]


        public JsonResult GetCartProducts()
        {
            var cartHelper = new CartHelper();
            var cart = cartHelper.GetCart(CurrentUser.Id);
            var productService = _iProductService;
            var shopService = _iShopService;
            var vshopService = _iVShopService;
            //会员折扣
            decimal discount = 1.0M;//默认折扣为1（没有折扣）
            if (CurrentUser != null)
            {
                discount = CurrentUser.MemberDiscount;
            }
            List<long> pids = new List<long>();
            decimal prodPrice = 0.0M;//优惠价格
            var limitProducts = LimitTimeApplication.GetPriceByProducrIds(cart.Items.Select(e => e.ProductId).ToList());//限时购价格
            var products = cart.Items.Select(item =>
            {
                var product = productService.GetProduct(item.ProductId);
                var shop = shopService.GetShop(product.ShopId);
                SKUInfo sku = null;
                string colorAlias = "";
                string sizeAlias = "";
                string versionAlias = "";
                string skuDetails = "";
                if (null != shop)
                {
                    var vshop = vshopService.GetVShopByShopId(shop.Id);
                    sku = productService.GetSku(item.SkuId);
                    if (sku == null)
                    {
                        return null;
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
                    var typeInfo = TypeApplication.GetType(product.TypeId);
                    colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                    sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                    versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                    skuDetails = "";
                    if (!string.IsNullOrWhiteSpace(sku.Size))
                    {
                        skuDetails += sizeAlias + "：" + sku.Size + "&nbsp;&nbsp;";
                    }
                    if (!string.IsNullOrWhiteSpace(sku.Color))
                    {
                        skuDetails += colorAlias + "：" + sku.Color + "&nbsp;&nbsp;";
                    }
                    if (!string.IsNullOrWhiteSpace(sku.Version))
                    {
                        skuDetails += versionAlias + "：" + sku.Version + "&nbsp;&nbsp;";
                    }
                    return new
                    {
                        cartItemId = item.Id,
                        skuId = item.SkuId,
                        id = product.Id,
                        imgUrl = Himall.Core.HimallIO.GetProductSizeImage(product.RelativePath, 1, (int)ImageSize.Size_150),
                        name = product.ProductName,
                        price = prodPrice,
                        count = item.Quantity,
                        shopId = shop.Id,
                        vshopId = vshop == null ? 0 : vshop.Id,
                        shopName = shop.ShopName,
                        shopLogo = vshop == null ? "" : vshop.Logo,
                        status = (product.AuditStatus == ProductInfo.ProductAuditStatus.Audited && product.SaleStatus == ProductInfo.ProductSaleStatus.OnSale) ? 1 : 0,
                        Size = sku.Size,
                        Color = sku.Color,
                        Version = sku.Version,
                        ColorAlias = colorAlias,
                        SizeAlias = sizeAlias,
                        VersionAlias = versionAlias,
                        skuDetails = skuDetails,
                        AddTime = item.AddTime
                    };
                }
                else
                {
                    return null;
                }
            }).Where(d=>d!=null).OrderBy(s => s.vshopId).ThenByDescending(o => o.AddTime);

            var cartModel = new { products = products, amount = products.Sum(item => item.price * item.count), totalCount = products.Sum(item => item.count) };
            return Json(cartModel);
        }

        [HttpPost]
        public JsonResult UpdateCartItem(string skuId, int count)
        {
            long userId = CurrentUser != null ? CurrentUser.Id : 0;
            var cartHelper = new CartHelper();
            cartHelper.UpdateCartItem(skuId, count, userId);
            return Json(new { success = true });
        }

        public JsonResult UpdateCartItem(Dictionary<string, int> skus, long userId)
        {
            var cartHelper = new CartHelper();
            foreach (var sku in skus)
            {
                cartHelper.UpdateCartItem(sku.Key, sku.Value, userId);
            }
            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult BatchRemoveFromCart(string skuIds)
        {
            long userId = CurrentUser != null ? CurrentUser.Id : 0;

            var skuIdsArr = skuIds.Split(',');
            var cartHelper = new CartHelper();
            cartHelper.RemoveFromCart(skuIdsArr, userId);
            return Json(new { success = true });
        }
        [HttpPost]
        public JsonResult RemoveFromCart(string skuId)
        {
            long userId = CurrentUser != null ? CurrentUser.Id : 0;

            var cartHelper = new CartHelper();
            cartHelper.RemoveFromCart(skuId, userId);
            return Json(new { success = true });
        }
    }
}