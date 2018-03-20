using Himall.IServices;
using Himall.Model;
using Himall.Web.Areas.SellerAdmin.Models;
using Himall.Web.Framework;

using Himall.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    public class ArticleController : BaseSellerController
    {

        private IArticleService _iArticleService;
        private IArticleCategoryService _iArticleCategoryService;
        public ArticleController(IArticleService iArticleService, IArticleCategoryService iArticleCategoryService)
        {
            _iArticleService = iArticleService;
            _iArticleCategoryService = iArticleCategoryService;
        }

        [UnAuthorize]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult List(int rows, int page)
        {
            var articleCategoryService = _iArticleCategoryService;
            var platformCategory = articleCategoryService.GetSpecialArticleCategory(SpecialCategory.PlatformNews);
            var articles = _iArticleService.Find(platformCategory.Id, "", rows, page,false);
            var articleModels = articles.Models.ToArray().Select(item => new
            {
                id = item.Id,
                addDate = item.AddDate.ToString("yyyy-MM-dd"),
                title = item.Title,
            });
            var dataGrid = new { rows = articleModels, total = articles.Total };
            return Json(dataGrid);
        }
        [UnAuthorize]
        public ActionResult Details(long id)
        {
            var model= _iArticleService.GetArticle(id);
            return View(model);
        }
    }
}