using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Core;
using Himall.Core.Helper;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Framework;
using Himall.CommonModel;

namespace Himall.SmallProgAPI.Helper
{
    public class CartHelper
    {
        ICartService _iCartService;
        IProductService _iProductService;
        public CartHelper(ICartService iCartService, IProductService iProductService)
        {
            _iCartService = iCartService;
            _iProductService = iProductService;
        }

        public CartHelper()
        {
            _iCartService = ServiceHelper.Create<ICartService>();
            _iProductService = ServiceHelper.Create<IProductService>();
        }
        /*
         *购物车存储说明：
         *游客访问时，点击加入购物车，购物车信息保存至Cookie中，游客点击结算时，Cookie中的购物车信息转移至数据库中并清空Cookie中购物车信息。
         *登录会员点击加入购物车时，购物车信息保存至数据库中。
         *Cookie存储格式： skuId1:count1,skuId2:count2,.....
         */

        /// <summary>
        /// 同步客户端购物车信息至服务器
        /// </summary>
        public void UpdateCartInfoFromCookieToServer(long memberId)
        {
            string cartInfo = WebHelper.GetCookie(CookieKeysCollection.HIMALL_CART);
            if (!string.IsNullOrWhiteSpace(cartInfo))
            {
                string[] cartItems = cartInfo.Split(',');
                var shoppingCartItems = new ShoppingCartItem[cartItems.Length];
                int i = 0;
                foreach (string cartItem in cartItems)
                {
                    var cartItemParts = cartItem.Split(':');
                    shoppingCartItems[i++] = new ShoppingCartItem() { SkuId = cartItemParts[0], Quantity = int.Parse(cartItemParts[1]) };
                }
                _iCartService.AddToCart(shoppingCartItems, memberId);
            }
            WebHelper.DeleteCookie(CookieKeysCollection.HIMALL_CART);
        }

        /// <summary>
        /// 获取购物车中的商品
        /// </summary>
        /// <returns></returns>
        public IEnumerable<long> GetCartProductIds(long memberId)
        {
            long[] productIds = new long[] { };
            if (memberId > 0)//已经登录，系统从服务器读取购物车信息，否则从Cookie获取购物车信息
            {
                var cartInfo = _iCartService.GetCart(memberId);
                productIds = cartInfo.Items.Select(item => item.ProductId).ToArray();
            }
            else
            {
                string cartInfo = WebHelper.GetCookie(CookieKeysCollection.HIMALL_CART);
                if (!string.IsNullOrWhiteSpace(cartInfo))
                {
                    string[] cartItems = cartInfo.Split(',');
                    productIds = new long[cartItems.Length];
                    int i = 0;
                    foreach (string cartItem in cartItems)
                    {
                        var cartItemParts = cartItem.Split(':');
                        productIds[i++] = long.Parse(cartItemParts[0]);//获取商品Id
                    }
                }
            }
            return productIds;
        }


        /// <summary>
        /// 获取购物车中的商品
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetCartProductSKUIds(long memberId)
        {
            string[] productIds = new string[] { };
            if (memberId > 0)//已经登录，系统从服务器读取购物车信息，否则从Cookie获取购物车信息
            {
                var cartInfo = _iCartService.GetCart(memberId);
                productIds = cartInfo.Items.Select(item => item.SkuId).ToArray();
            }
            else
            {
                string cartInfo = WebHelper.GetCookie(CookieKeysCollection.HIMALL_CART);
                if (!string.IsNullOrWhiteSpace(cartInfo))
                {
                    string[] cartItems = cartInfo.Split(',');
                    productIds = new string[cartItems.Length];
                    int i = 0;
                    foreach (string cartItem in cartItems)
                    {
                        var cartItemParts = cartItem.Split(':');
                        productIds[i++] = cartItemParts[0];//获取商品SKUId
                    }
                }
            }
            return productIds;
        }

        /// <summary>
        /// 获取购物车中的商品
        /// </summary>
        /// <returns></returns>
        public ShoppingCartInfo GetCart(long memberId)
        {
            ShoppingCartInfo shoppingCartInfo;
            if (memberId > 0)//已经登录，系统从服务器读取购物车信息，否则从Cookie获取购物车信息
                shoppingCartInfo = _iCartService.GetCart(memberId);
            else
            {
                shoppingCartInfo = new ShoppingCartInfo();

                string cartInfo = WebHelper.GetCookie(CookieKeysCollection.HIMALL_CART);
                if (!string.IsNullOrWhiteSpace(cartInfo))
                {
                    string[] cartItems = cartInfo.Split(',');
                    var cartInfoItems = new ShoppingCartItem[cartItems.Length];
                    int i = 0;
                    foreach (string cartItem in cartItems)
                    {
                        var cartItemParts = cartItem.Split(':');
                        cartInfoItems[i++] = new ShoppingCartItem() { ProductId = long.Parse(cartItemParts[0].Split('_')[0]), SkuId = cartItemParts[0], Quantity = int.Parse(cartItemParts[1]) };
                    }
                    shoppingCartInfo.Items = cartInfoItems;
                }
            }
            return shoppingCartInfo;
        }


        public void RemoveFromCart(string skuId, long memberId)
        {
            if (memberId > 0)
                _iCartService.DeleteCartItem(skuId, memberId);
            else
            {
                string cartInfo = WebHelper.GetCookie(CookieKeysCollection.HIMALL_CART);
                if (!string.IsNullOrWhiteSpace(cartInfo))
                {
                    string[] cartItems = cartInfo.Split(',');
                    string newCartInfo = string.Empty;
                    foreach (string cartItem in cartItems)
                    {
                        string[] cartItemParts = cartItem.Split(':');
                        string cartItemSkuId = cartItemParts[0];
                        if (cartItemSkuId != skuId)
                            newCartInfo += "," + cartItem;
                    }
                    if (!string.IsNullOrWhiteSpace(newCartInfo))
                        newCartInfo = newCartInfo.Substring(1);
                    WebHelper.SetCookie(CookieKeysCollection.HIMALL_CART, newCartInfo);
                }
            }
        }

        public void RemoveFromCart(IEnumerable<string> skuIds, long memberId)
        {
            if (memberId > 0)
                _iCartService.DeleteCartItem(skuIds, memberId);
            else
            {
                string cartInfo = WebHelper.GetCookie(CookieKeysCollection.HIMALL_CART);
                if (!string.IsNullOrWhiteSpace(cartInfo))
                {
                    string[] cartItems = cartInfo.Split(',');
                    string newCartInfo = string.Empty;
                    foreach (string cartItem in cartItems)
                    {
                        string[] cartItemParts = cartItem.Split(':');
                        string cartItemSkuId = cartItemParts[0];
                        if (!skuIds.Contains(cartItemSkuId))
                            newCartInfo += "," + cartItem;
                    }
                    if (!string.IsNullOrWhiteSpace(newCartInfo))
                        newCartInfo = newCartInfo.Substring(1);
                    WebHelper.SetCookie(CookieKeysCollection.HIMALL_CART, newCartInfo);
                }
            }
        }



        public void UpdateCartItem(string skuId, int count, long memberId)
        {
            if (memberId > 0)
                _iCartService.UpdateCart(skuId, count, memberId);
            else
            {
                string cartInfo = WebHelper.GetCookie(CookieKeysCollection.HIMALL_CART);
                if (!string.IsNullOrWhiteSpace(cartInfo))
                {
                    string[] cartItems = cartInfo.Split(',');
                    string newCartInfo = string.Empty;
                    foreach (string cartItem in cartItems)
                    {
                        var cartItemParts = cartItem.Split(':');
                        if (cartItemParts[0] == skuId)
                            newCartInfo += "," + skuId + ":" + count;
                        else
                            newCartInfo += "," + cartItem;
                    }

                    if (!string.IsNullOrWhiteSpace(newCartInfo))
                        newCartInfo = newCartInfo.Substring(1);
                    WebHelper.SetCookie(CookieKeysCollection.HIMALL_CART, newCartInfo);
                }
                else
                {
                    WebHelper.SetCookie(CookieKeysCollection.HIMALL_CART, string.Format("{0}:{1}", skuId, count));
                }
            }
        }


        public string AddToCart(string skuId, int count, long memberId)
        {
            CheckSkuIdIsValid(skuId);
            //判断库存
            string msg = "";
            var sku = _iProductService.GetSku(skuId);
            //获取购物车已购买数量

            if (sku == null)
            {
                msg="错误的SKU";
            }
            if (count > sku.Stock)
            {
                msg="库存不足";
            }

            if (memberId > 0)
                _iCartService.AddToCart(skuId, count, memberId);
            else
            {
                string cartInfo = WebHelper.GetCookie(CookieKeysCollection.HIMALL_CART);
                if (!string.IsNullOrWhiteSpace(cartInfo))
                {
                    string[] cartItems = cartInfo.Split(',');
                    string newCartInfo = string.Empty;
                    bool exist = false;
                    foreach (string cartItem in cartItems)
                    {
                        var cartItemParts = cartItem.Split(':');
                        if (cartItemParts[0] == skuId)
                        {
                            newCartInfo += "," + skuId + ":" + (int.Parse(cartItemParts[1]) + count);
                            exist = true;
                        }
                        else
                            newCartInfo += "," + cartItem;
                    }
                    if (!exist)
                        newCartInfo += "," + skuId + ":" + count;

                    if (!string.IsNullOrWhiteSpace(newCartInfo))
                        newCartInfo = newCartInfo.Substring(1);
                    WebHelper.SetCookie(CookieKeysCollection.HIMALL_CART, newCartInfo);
                }
                else
                {
                    WebHelper.SetCookie(CookieKeysCollection.HIMALL_CART, string.Format("{0}:{1}", skuId, count));
                }
            }
            return msg;
        }

        public bool CheckSkuId(string skuId)
        {
            bool status = true;
            long productId = 0;
            long.TryParse(skuId.Split('_')[0], out productId);
            if (productId == 0)
                status = false;
            var skuItem = _iProductService.GetSku(skuId);
            if (skuItem == null)
                status = false;
            var sku = _iProductService.GetSku(skuId);
            if (sku == null)
            {
                status = false;
            }
            return status;
        }


        void CheckSkuIdIsValid(string skuId)
        {
            long productId = 0;
            long.TryParse(skuId.Split('_')[0], out productId);
            if (productId == 0)
                throw new Himall.Core.InvalidPropertyException("SKUId无效");

            var skuItem = _iProductService.GetSku(skuId);
            if (skuItem == null)
                throw new Himall.Core.InvalidPropertyException("SKUId无效");

        }


    }

    public class MobileHomeProducts
    {
        IMobileHomeProductsService _iMobileHomeProductsService;
        IBrandService _iBrandService;
        ICategoryService _iCategoryService;
        IShopCategoryService _iShopCategoryService;

        public MobileHomeProducts(
            IMobileHomeProductsService iMobileHomeProductsService,
            IBrandService iBrandService,
            ICategoryService iCategoryService,
            IShopCategoryService iShopCategoryService
            )
        {
            _iBrandService = iBrandService;
            _iCategoryService = iCategoryService;
            _iMobileHomeProductsService = iMobileHomeProductsService;
            _iShopCategoryService = iShopCategoryService;
        }

        public object GetMobileHomeProducts(long shopId, PlatformType platformType, int page, int rows, string keyWords, long? categoryId = null)
        {

            ProductQuery productQuery = new ProductQuery()
            {
                CategoryId = categoryId,
                KeyWords = keyWords,
                PageSize = rows,
                PageNo = page
            };

            var homeProducts = _iMobileHomeProductsService.GetMobileHomePageProducts(shopId, platformType, productQuery);
            var brandService = _iBrandService;
            var categoryService = _iCategoryService;
            var model = homeProducts.Models.ToArray().Select(item =>
            {
                var brand = brandService.GetBrand(item.Himall_Products.BrandId);
                return new
                {
                    name = item.Himall_Products.ProductName,
                    image = item.Himall_Products.GetImage(ImageSize.Size_50),
                    price = item.Himall_Products.MinSalePrice.ToString("F2"),
                    brand = brand == null ? "" : brand.Name,
                    sequence = item.Sequence,
                    categoryName = categoryService.GetCategory(long.Parse(categoryService.GetCategory(item.Himall_Products.CategoryId).Path.Split('|').Last())).Name,
                    id = item.Id,
                    productId = item.ProductId
                };
            });
            return new { rows = model, total = homeProducts.Total };
        }

        public object GetSellerMobileHomePageProducts(long shopId, PlatformType platformType, int page, int rows, string brandName, long? categoryId = null)
        {
            ProductQuery productQuery = new ProductQuery()
            {
                ShopCategoryId = categoryId,
                KeyWords = brandName,
                PageSize = rows,
                PageNo = page
            };

            var homeProducts = _iMobileHomeProductsService.GetSellerMobileHomePageProducts(shopId, platformType, productQuery);
            var brandService = _iBrandService;
            var shopCategoryService = _iShopCategoryService;

            var model = homeProducts.Models.ToArray().Select(item =>
            {
                var brand = brandService.GetBrand(item.Himall_Products.BrandId);
                var cate = item.Himall_Products.Himall_ProductShopCategories.FirstOrDefault();
                return new
                {
                    name = item.Himall_Products.ProductName,
                    image = item.Himall_Products.GetImage(ImageSize.Size_50),
                    price = item.Himall_Products.MinSalePrice.ToString("F2"),
                    brand = brand == null ? "" : brand.Name,
                    sequence = item.Sequence,
                    id = item.Id,
                    categoryName = cate == null ? "" : cate.ShopCategoryInfo.Name,
                    productId = item.ProductId,
                };
            });


            return new { rows = model, total = homeProducts.Total };
        }

        public void AddHomeProducts(long shopId, string productIds, PlatformType platformType)
        {
            var productIdsArr = productIds.Split(',').Select(item => long.Parse(item));
            _iMobileHomeProductsService.AddProductsToHomePage(shopId, platformType, productIdsArr);
        }
        public void UpdateSequence(long shopId, long id, short sequence)
        {
            _iMobileHomeProductsService.UpdateSequence(shopId, id, sequence);
        }

        public void Delete(long shopId, long id)
        {
            _iMobileHomeProductsService.Delete(shopId, id);
        }

        public object GetAllHomeProductIds(long shopId, PlatformType platformType)
        {
            var homeProductIds = _iMobileHomeProductsService.GetMobileHomePageProducts(shopId, platformType).Select(item => item.ProductId);
            return homeProductIds;
        }
    }
}
