using Himall.IServices;
using Himall.Web.Areas.SellerAdmin.Models;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Areas.Web.Controllers
{
    public class ArticleController : BaseWebController
    {
        IArticleCategoryService _iArticleCategoryService;
        IArticleService _iArticleService;
        public ArticleController(IArticleCategoryService iArticleCategoryService, IArticleService iArticleService)
        {
            _iArticleCategoryService = iArticleCategoryService;
            _iArticleService = iArticleService;
        }

        private List<CategoryJsonModel> GetArticleCate()
        {
            List<CategoryJsonModel> articleCate = new List<CategoryJsonModel>();
            #region 文章分类

            var categories = _iArticleCategoryService.GetCategories().ToArray();
            foreach (var main in categories.Where(s => s.ParentCategoryId == 0))
            {
                var topC = new CategoryJsonModel()
                {
                    Name = main.Name,
                    Id = main.Id.ToString(),
                    SubCategory = new List<SecondLevelCategory>()
                };
                foreach (var secondItem in categories.Where(s => s.ParentCategoryId == main.Id))
                {
                    var secondC = new SecondLevelCategory()
                    {
                        Name = secondItem.Name,
                        Id = secondItem.Id.ToString(),
                    };

                    topC.SubCategory.Add(secondC);
                }
                articleCate.Add(topC);
            }
            return articleCate;
            #endregion
        }

        private string GetCateNameBySecond(long id)
        {
            var pid = _iArticleCategoryService.GetCategories().FirstOrDefault(c => c.Id.Equals(id)).ParentCategoryId;
            return _iArticleCategoryService.GetArticleCategory(pid).Name;
        }

        // GET: Web/Article
        public ActionResult Category(string id = "1", int pageNo = 1)
        {
            int pageSize = 20;
            long cid = 0;
            if (!long.TryParse(id, out cid))
            {
            }
            var lstarts = _iArticleService.GetArticleByArticleCategoryId(cid);
            var arts = lstarts.Where(m => m.IsRelease == true);
            List<CategoryJsonModel> cate = GetArticleCate();
            var currCate = cate.FirstOrDefault(c => c.Id == cid.ToString());
            ViewBag.Cate = cate;

            ViewBag.ArticleCateId = cid;
            ViewBag.Articles = arts.OrderByDescending(c => c.Id).Skip((pageNo - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.FirstPath = currCate != null ? currCate.Name :
                GetCateNameBySecond(cid);
            ViewBag.SecondPath = currCate == null ? _iArticleCategoryService.GetArticleCategory(cid).Name : "";

            #region 分页控制
            PagingInfo info = new PagingInfo
            {
                CurrentPage = pageNo,
                ItemsPerPage = pageSize,
                TotalItems = arts.Count()
            };
            ViewBag.pageInfo = info;
            #endregion

            return View();
        }

        public ActionResult Details(long id)
        {
            ViewBag.Cate = GetArticleCate();
            var model = _iArticleService.GetArticle(id);
            ViewBag.ArticleCateId = model.CategoryId;
            ViewBag.FirstPath = model.ArticleCategoryInfo.ParentCategoryId == 0 ? model.ArticleCategoryInfo.Name :
                _iArticleCategoryService.GetArticleCategory(model.ArticleCategoryInfo.ParentCategoryId).Name;
            ViewBag.SecondPath = model.ArticleCategoryInfo.ParentCategoryId != 0 ? model.ArticleCategoryInfo.Name : "";
            return View(model);
        }
    }
}