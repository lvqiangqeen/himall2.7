using Himall.IServices;
using Himall.Model;
using Himall.Web.Areas.Admin.Models;
using Himall.Web.Framework;

using System.Linq;
using System.Web.Mvc;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class ArticleController : BaseAdminController
    {
        private IArticleService _iArticleService;
        private IArticleCategoryService _iArticleCategoryService;
        public ArticleController(IArticleService iArticleService,IArticleCategoryService iArticleCategoryService)
        {
            _iArticleService = iArticleService;
            _iArticleCategoryService = iArticleCategoryService;
        }

        // GET: Admin/Article
        public ActionResult Management()
        {
            return View();
        }



        public ActionResult Add(long? id)
        {
            ArticleModel articleModel;
            if (id.HasValue)
            {
                var article = _iArticleService.FindById(id.Value);
                articleModel = new ArticleModel()
                {
                    CategoryId = article.CategoryId,
                    Content = article.Content,
                    IconUrl = article.IconUrl,
                    Id = article.Id,
                    IsRelease = article.IsRelease,
                    Meta_Description = article.Meta_Description,
                    Meta_Keywords = article.Meta_Keywords,
                    Meta_Title = article.Meta_Title,
                    Title = article.Title,
                    ArticleCategoryFullPath = _iArticleCategoryService.GetFullPath(article.CategoryId)
                };
            }
            else
                articleModel = new ArticleModel() {  IsRelease=true};
            return View(articleModel);
        }

        [ValidateInput(false)]
        [HttpPost]
        public JsonResult Add(ArticleModel model)
        {
            if(!ModelState.IsValid)
            {
                return Json(new { success = false });
            }
            ArticleInfo article = new ArticleInfo()
            {
                Title = model.Title,
                Meta_Title = model.Meta_Title,
                Meta_Keywords = model.Meta_Keywords,
                Meta_Description = model.Meta_Description,
                IsRelease = model.IsRelease,
                CategoryId = model.CategoryId.GetValueOrDefault(),
                Content = model.Content,
                IconUrl = model.IconUrl,
                Id = model.Id

            };
            if (article.Id > 0)
                _iArticleService.UpdateArticle(article);
            else
                _iArticleService.AddArticle(article);
            return Json(new { success = true });
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult List(long? categoryId, string titleKeyWords, int rows, int page)
        {

            var articles = _iArticleService.Find(categoryId, titleKeyWords, rows, page);
            string host = Request.Url.Host;
            host += Request.Url.Port != 80 ? ":" + Request.Url.Port.ToString() : "";
            string urltmp = string.Format(@"http://{0}{1}/", host, Url.Action("Details","Article",new{Area="Web"}));
            var articleModels = articles.Models.Select(item => new
            {
                id = item.Id,
                categoryId = item.CategoryId,
                categoryName = item.ArticleCategoryInfo.Name,
                isShow = item.IsRelease,
                title = item.Title,
                displaySequence = item.DisplaySequence,
                showurl=urltmp+item.Id.ToString()
            });
            var data = new { rows = articleModels, total = articles.Total };
            return Json(data);
        }


        [HttpPost]
        [UnAuthorize]
        public JsonResult Delete(long id)
        {
            _iArticleService.DeleteArticle(id);
            return Json(new { success = true });
        }


        [HttpPost]
        [UnAuthorize]
        public JsonResult BatchDelete(string ids)
        {
            long[] ids_long = ids.Split(',').Select(item => long.Parse(item)).ToArray();
            _iArticleService.DeleteArticle(ids_long);
            return Json(new { success = true });
        }


        [HttpPost]
        [UnAuthorize]
        public JsonResult UpdateDisplaySequence(long id,long displaySequence)
        {
            _iArticleService.UpdateArticleDisplaySequence(id, displaySequence);
            return Json(new { success = true });
        }
    }
}