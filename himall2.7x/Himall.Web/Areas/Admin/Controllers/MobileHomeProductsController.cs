using Himall.Core;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Framework;
using System.Linq;
using System.Web.Mvc;
using Himall.Web;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class MobileHomeProductsController : BaseAdminController
    {

        IMobileHomeProductsService _iMobileHomeProductsService;
        IBrandService _iBrandService;
        ICategoryService _iCategoryService;
        IShopCategoryService _iShopCategoryService;
        MobileHomeProducts mobileHomeproduct;
        public MobileHomeProductsController(
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
            mobileHomeproduct = new MobileHomeProducts(_iMobileHomeProductsService, _iBrandService, _iCategoryService, _iShopCategoryService);
        }


        [HttpPost]
        public JsonResult GetMobileHomeProducts(PlatformType platformType, int page, int rows, string keyWords, long? categoryId = null)
        {
            object model = mobileHomeproduct.GetMobileHomeProducts(CurrentManager.ShopId, platformType, page, rows, keyWords, categoryId);
           
            return Json(model);
        }

        [HttpPost]
        public JsonResult AddHomeProducts(string productIds, PlatformType platformType)
        {
            mobileHomeproduct.AddHomeProducts(CurrentManager.ShopId, productIds, platformType);
            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult UpdateSequence(long id, short sequence)
        {
            mobileHomeproduct.UpdateSequence(CurrentManager.ShopId, id, sequence);
            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult Delete(long id)
        {
            mobileHomeproduct.Delete(CurrentManager.ShopId, id);
            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult GetAllHomeProductIds(PlatformType platformType)
        {
            var homeProductIds = mobileHomeproduct.GetAllHomeProductIds(CurrentManager.ShopId, platformType);
            return Json(homeProductIds);
        }

    }
}