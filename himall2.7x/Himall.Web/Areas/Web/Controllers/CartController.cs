using Himall.Core.Helper;
using Himall.IServices;
using Himall.Model;
using Himall.Web.App_Code;
using Himall.Web.Areas.Web.Models;
using Himall.Web.Framework;
using System.Linq;
using System.Web.Mvc;
using Himall.Web.Models;
using Himall.CommonModel;
using Himall.Application;
using Himall.Core;


namespace Himall.Web.Areas.Web.Controllers
{
    public class CartController : BaseWebController
    {
        CartHelper cartHelper;
        ICartService _iCartService;
        IProductService _iProductService;
        IMemberService _iMemberService;
        ISiteSettingService _iSiteSettingService;
        IOrderService _iOrderService;
        IShopService _iShopService;
        private ITypeService _iTypeService;

        public CartController(ICartService iCartService,
            IProductService iProductService,
            IMemberService iMemberService,
            ISiteSettingService iSiteSettingService,
            IOrderService iOrderService,
            IShopService iShopService, ITypeService iTypeService
            )
        {
            _iCartService = iCartService;
            _iProductService = iProductService;
            _iMemberService = iMemberService;
            _iSiteSettingService = iSiteSettingService;
            _iOrderService = iOrderService;
            _iShopService = iShopService;
            _iTypeService = iTypeService;
            cartHelper = new CartHelper(_iCartService, _iProductService);
        }
        /*
         *购物车存储说明：
         *游客访问时，点击加入购物车，购物车信息保存至Cookie中，游客点击结算时，Cookie中的购物车信息转移至数据库中并清空Cookie中购物车信息。
         *登录会员点击加入购物车时，购物车信息保存至数据库中。
         *Cookie存储格式： skuId1:count1,skuId2:count2,.....
         */


        // GET: Web/Cart
        public ActionResult AddToCart(string skuId, int count)
        {
            long userId = CurrentUser != null ? CurrentUser.Id : 0;
            try
            {

                cartHelper.AddToCart(skuId, count, userId);
            }
            catch { }
            return RedirectToAction("AddedToCart", new { skuId = skuId });
        }


        public ActionResult AddedToCart(string skuId)
        {
            long userId = CurrentUser != null ? CurrentUser.Id : 0;
            try
            {

                string productId = skuId.Split('_')[0];
                ViewBag.ProductId = productId;
                var productService = _iProductService;
                var cart = cartHelper.GetCart(userId);
                ProductInfo product;
                SKUInfo sku;
                var products = cart.Items.Select(item =>
                {
                    product = productService.GetProduct(item.ProductId);
                    sku = productService.GetSku(item.SkuId);
                    return new CartItemModel()
                    {
                        skuId = item.SkuId,
                        id = product.Id,
                        imgUrl = product.ImagePath + "/1_50.png",
                        name = product.ProductName,
                        price = sku.SalePrice,
                        count = item.Quantity
                    };
                });

                ViewBag.Current = products.FirstOrDefault(item => item.skuId == skuId);
                ViewBag.Others = products.Where(item => item.skuId != skuId);
                ViewBag.Amount = products.Sum(item => item.price * item.count);
                ViewBag.TotalCount = products.Sum(item => item.count);
            }
            catch { }
            return View("AddToCart");
        }





        [HttpPost]
        public JsonResult AddProductToCart(string skuId, int count)
        {
            long userId = CurrentUser != null ? CurrentUser.Id : 0;
            try
            {

                cartHelper.AddToCart(skuId, count, userId);
                return Json(new { success = true });
            }
            catch { return Json(new { success = false }); }
        }

        /// <summary>
        /// 验证商品是否可加入购物车
        /// </summary>
        /// <param name="id">商品ID</param>
        /// <returns>success  JSON数据,返回真表示可以加入购物车</returns>
        [HttpPost]
        public JsonResult verificationToCart(long id)
        {
            long buyid = 0;
            bool success = false;

            var iLimitService = ServiceHelper.Create<ILimitTimeBuyService>();
            var ltmbuy = iLimitService.GetLimitTimeMarketItemByProductId(id);
            if (ltmbuy != null)
            {
                buyid = ltmbuy.Id;
            }
            else
            {
                var sku = _iProductService.GetSKUs(id);
                if (sku.ToList().Count == 1 && sku.FirstOrDefault().Id.Contains("0_0_0"))
                {
                    success = true;
                }
            }

            return Json(new { success = success, id = buyid });
        }

        public ActionResult BatchAddToCart(string skuIds, string counts)
        {
            var skuIdsArr = skuIds.Split(',');
            var countsArr = counts.Split(',').Select(item => int.Parse(item));

            long userId = CurrentUser != null ? CurrentUser.Id : 0;
            for (int i = 0; i < skuIdsArr.Count(); i++)
                cartHelper.AddToCart(skuIdsArr.ElementAt(i), countsArr.ElementAt(i), userId);
            return RedirectToAction("cart");
        }



		public ActionResult Cart()
		{
			//Logo
			ViewBag.Logo = _iSiteSettingService.GetSiteSettings().Logo;
			ViewBag.Step = 1;

			CartCartModel model = new CartCartModel();

			var memberInfo = base.CurrentUser;

			ViewBag.Member = memberInfo;
			long uid = 0;
			if (CurrentUser != null)
			{
				uid = CurrentUser.Id;
			}

			model.Top3RecommendProducts = _iProductService.GetPlatHotSaleProductByNearShop(10, uid, true).ToList();
			return View(model);
		}

        [HttpPost]
        public JsonResult RemoveFromCart(string skuId)
        {
            long userId = CurrentUser != null ? CurrentUser.Id : 0;


            cartHelper.RemoveFromCart(skuId, userId);
            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult BatchRemoveFromCart(string skuIds)
        {
            long userId = CurrentUser != null ? CurrentUser.Id : 0;

            var skuIdsArr = skuIds.Split(',');

            cartHelper.RemoveFromCart(skuIdsArr, userId);
            return Json(new { success = true });
        }


        [HttpPost]
        public JsonResult UpdateCartItem(string skuId, int count)
        {
            long userId = CurrentUser != null ? CurrentUser.Id : 0;
            var orderService = _iOrderService;
            SKUInfo skuinfo = orderService.GetSkuByID(skuId);
            if (skuinfo.Stock < count)
                return Json(new { success = false, msg = "库存不足" });
            cartHelper.UpdateCartItem(skuId, count, userId);
            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult GetSkuByID(string skuId)
        {
            var orderService = _iOrderService;
            SKUInfo skuinfo = orderService.GetSkuByID(skuId);
            var json = new
            {
                Color = skuinfo.Color,
                Size = skuinfo.Size,
                Version = skuinfo.Version
            };
            return Json(json);
        }

        [HttpPost]
        public JsonResult GetCartProducts()
        {
            long userId = 0;
            decimal discount = 1M;
            if (CurrentUser != null)
            {
                userId = CurrentUser.Id;
                discount = CurrentUser.MemberDiscount;
            }

            var cart = cartHelper.GetCart(userId);
            var productService = _iProductService;
            var shopService = _iShopService;
            var typeservice = _iTypeService;


            var products = cart.Items.Select(item =>
            {
                var product = productService.GetProduct(item.ProductId);

                ProductTypeInfo typeInfo = typeservice.GetType(product.TypeId);
                string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;

                var shop = ShopApplication.GetShop(product.ShopId);
                if (null != shop)
                {
                    SKUInfo sku = productService.GetSku(item.SkuId);
                    if (sku != null)
                    {
                        var price = sku.SalePrice * discount;//
                        return new
                        {
                            cartItemId = item.Id,
                            skuId = item.SkuId,
                            id = product.Id,
                            imgUrl = Himall.Core.HimallIO.GetProductSizeImage(product.RelativePath, 1, (int)ImageSize.Size_50),
                            name = product.ProductName,
                            productstatus = product.SaleStatus,
                            productauditstatus = product.AuditStatus,
                            price = shop.IsSelf ? price : sku.SalePrice,//sku.SalePrice,
                            Color = sku.Color,
                            Size = sku.Size,
                            Version = sku.Version,
                            count = item.Quantity,
                            shopId = shop.Id,
                            shopName = shop.ShopName,
                            productcode = !(sku.Version + sku.Color + sku.Size).Equals("") ? sku.Sku : product.ProductCode,
                            ColorAlias = colorAlias,
                            SizeAlias = sizeAlias,
                            VersionAlias = versionAlias,
                            AddTime=item.AddTime
                        };
                    }
                }
                return null;
            }).Where(p => p != null).OrderBy(s => s.shopId).ThenByDescending(o=>o.AddTime);

            var cartModel = new
            {
                products = products,
                amount = products.Where(item => item.productstatus == ProductInfo.ProductSaleStatus.OnSale && item.productauditstatus != ProductInfo.ProductAuditStatus.InfractionSaleOff && item.productauditstatus != ProductInfo.ProductAuditStatus.WaitForAuditing).Sum(item => item.price * item.count),
                totalCount = products.Where(item => item.productstatus == ProductInfo.ProductSaleStatus.OnSale && item.productauditstatus != ProductInfo.ProductAuditStatus.InfractionSaleOff && item.productauditstatus != ProductInfo.ProductAuditStatus.WaitForAuditing).Sum(item => item.count)
            };
            return Json(cartModel);
        }
    }
}