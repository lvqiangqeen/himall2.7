using Himall.IServices;
using Himall.Model;
using Himall.Web.Areas.Admin.Models;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class ArticleCategoryController : BaseAdminController
    {
        private IArticleCategoryService _iArticleCategoryService;

        public ArticleCategoryController(IArticleCategoryService iArticleCategoryService)
        {
            _iArticleCategoryService = iArticleCategoryService;
        }

        // GET: Admin/ArticleCategory
        public ActionResult Management()
        {
            var service = _iArticleCategoryService;
            var articleCategories = service.GetArticleCategoriesByParentId(0);
            var models = articleCategories.ToArray().Select(item => new ArticleCategoryModel()
            {
                ParentId = item.ParentCategoryId,
                Name = item.Name,
                DisplaySequence = item.DisplaySequence,
                Id = item.Id,
                IsDefault=item.IsDefault
            }).ToArray();
            foreach (var model in models)
            {
                model.HasChild = service.GetArticleCategoriesByParentId(model.Id, false).Count() > 0;
            }

            return View(models);
        }

        [HttpPost]
        public JsonResult GetArticleCategories(long parentId)
        {
            var articleCategories = _iArticleCategoryService.GetArticleCategoriesByParentId(parentId);
            var models = articleCategories.Select(item => new ArticleCategoryModel (){ Id = item.Id, Name = item.Name, DisplaySequence = item.DisplaySequence, HasChild = false,Depth=2 }).ToArray();
            var service = _iArticleCategoryService;
            foreach (var model in models)
            {
                model.HasChild = service.GetArticleCategoriesByParentId(model.Id, false).Count() > 0;
            }

            return Json(models);
 
        }

        [HttpPost]
        public ActionResult Add(long? id, string name, long parentId)
        {
            ArticleCategoryInfo articleCategory = new ArticleCategoryInfo()
            {
                Id = id.GetValueOrDefault(),
                Name = name,
                ParentCategoryId = parentId
            };
            var service = _iArticleCategoryService;
            if(service.CheckHaveRename(articleCategory.Id,articleCategory.Name))
            {
                return Json(new { success = false, msg="不可添加、修改为同名栏目！" });
            }
            if (id > 0)
                service.UpdateArticleCategory(articleCategory);
            else
                service.AddArticleCategory(articleCategory);
            return Json(new { success = true });
        }

        [UnAuthorize]
        [HttpPost]
        public JsonResult GetArticleCategory(long id)
        {
            var articleCategory = _iArticleCategoryService.GetArticleCategory(id);
            var model = new { id = id, name = articleCategory.Name, parentId = articleCategory.ParentCategoryId };
            return Json(model);

        }

        [HttpPost]
        public JsonResult Delete(long id)
        {

            _iArticleCategoryService.DeleteArticleCategory(id);
            return Json(new { success = true });
        }


        [HttpPost]
        public JsonResult BatchDelete(string ids)
        {
            long[] ids_long = ids.Split(',').Select(item => long.Parse(item)).ToArray();
            _iArticleCategoryService.DeleteArticleCategory(ids_long);
            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult UpdateOrder(long id, int displaySequence)
        {
            _iArticleCategoryService.UpdateArticleCategoryDisplaySequence(id, displaySequence);
            return Json(new { success = true });
        }


        [HttpPost]
        public JsonResult UpdateName(long id, string name)
        {
            var service = _iArticleCategoryService;
            if (service.CheckHaveRename(id, name))
            {
                return Json(new { success = false, msg = "不可添加、修改为同名栏目！" });
            }
            _iArticleCategoryService.UpdateArticleCategoryName(id, name);
            return Json(new { success = true });
        }


        [UnAuthorize]
        [HttpPost]
        public JsonResult GetCategories(long? key = null, int? level = -1)
        {
            if (level == -1)
                key = 0;

            if (key.HasValue)
            {
                var categories = _iArticleCategoryService.GetArticleCategoriesByParentId(key.Value).ToArray();
                var cateoriesPair = categories.Select(item => new KeyValuePair<long, string>(item.Id, item.Name));
                return Json(cateoriesPair);
            }
            else
                return Json(new object[] { });
        }
    }
}