using Himall.IServices;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Areas.Web.Controllers
{
    public class CategoryController : BaseWebController
    {
        private ICategoryService _iCategoryService;
        private IShopCategoryService _iShopCategoryService;
        public CategoryController(ICategoryService iCategoryService, IShopCategoryService iShopCategoryService)
        {
            _iCategoryService = iCategoryService;
            _iShopCategoryService = iShopCategoryService;
        }

        [HttpPost]
        public JsonResult GetCategory(long? key = null, int? level = -1)
        {
            if (level == -1)
                key = 0;

            if (key.HasValue)
            {
                var categories = _iCategoryService.GetCategoryByParentId(key.Value);
                var cateoriesPair = categories.Select(item => new KeyValuePair<long, string>(item.Id, item.Name));
                return Json(cateoriesPair);
            }
            else
                return Json(new object[] { });
        }

        /// <summary>
        /// 获取店铺授权的分类
        /// </summary>
        /// <returns></returns>
        public JsonResult GetAuthorizationCategory(long shopId,long? key = null, int? level = -1)
        {
            if (level == -1)
                key = 0;
            if (key.HasValue)
            {
                var categories = _iShopCategoryService.GetBusinessCategory(shopId).Where(r => r.ParentCategoryId == key.Value).ToArray();
                var cateoriesPair = categories.Select(item => new KeyValuePair<long, string>(item.Id, item.Name));
                return Json(cateoriesPair);
            }
            else
                return Json(new object[] { });
        }
    }
}