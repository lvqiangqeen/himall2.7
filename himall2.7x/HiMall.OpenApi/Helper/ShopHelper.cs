using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Himall.Core;
using Himall.Model;
using Himall.IServices;
using Himall.OpenApi.Model;

namespace Himall.OpenApi
{
    /// <summary>
    /// 店铺辅助工具
    /// </summary>
    public class ShopHelper
    {
        private IShopService _iShopService;
        private IRegionService _iRegionService;
        private IManagerService _iManagerService;
        private IShopOpenApiService _iShopOpenApiService;

        private string _AppKey { get; set; }
        private string _AppSecreate { get; set; }

        /// <summary>
        /// 店铺编号
        /// </summary>
        public long ShopId { get; set; }
        /// <summary>
        /// 店铺管理员
        /// </summary>
        public string SellerName { get; set; }
        /// <summary>
        /// 店铺app_key
        /// </summary>
        public string AppKey
        {
            get
            {
                return _AppKey;
            }
        }
        /// <summary>
        /// 店铺app_secreate
        /// </summary>
        public string AppSecreate
        {
            get
            {
                return _AppSecreate;
            }
        }

        public ShopHelper(string app_key)
        {
            _iShopService = Himall.ServiceProvider.Instance<IShopService>.Create;
            _iRegionService = Himall.ServiceProvider.Instance<IRegionService>.Create;
            _iManagerService = Himall.ServiceProvider.Instance<IManagerService>.Create;
            _iShopOpenApiService = Himall.ServiceProvider.Instance<IShopOpenApiService>.Create;

            _AppKey = app_key;
            if (string.IsNullOrWhiteSpace(_AppKey))
            {
                throw new HimallOpenApiException(Hishop.Open.Api.OpenApiErrorCode.Missing_App_Key, "app_key");
            }
            var shopappinfo = _iShopOpenApiService.Get(_AppKey);
            if (shopappinfo == null)
            {
                throw new HimallOpenApiException(Hishop.Open.Api.OpenApiErrorCode.Invalid_App_Key, "app_key");
            }
            if (shopappinfo.IsEnable != true)
            {
                throw new HimallOpenApiException(Hishop.Open.Api.OpenApiErrorCode.System_Error, "function not open");
            }
            _AppSecreate = shopappinfo.AppSecreat;
            if(string.IsNullOrWhiteSpace(_AppSecreate))
            {
                throw new HimallOpenApiException(Hishop.Open.Api.OpenApiErrorCode.Insufficient_ISV_Permissions, "not set app_secreat");
            }

            var shop = _iShopService.GetShop(shopappinfo.ShopId);
            if (shop == null)
            {
                throw new HimallOpenApiException(Hishop.Open.Api.OpenApiErrorCode.Invalid_App_Key,"app_key");
            }
            ShopId = shop.Id;
            var manage = _iManagerService.GetSellerManagerByShopId(ShopId);
            if(manage==null)
            {
                throw new HimallException("店铺管理信息有误，请管理员修正");
            }
            SellerName = manage.UserName;
        }
    }
}
