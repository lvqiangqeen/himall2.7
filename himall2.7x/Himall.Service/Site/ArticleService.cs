using Himall.Core;
using Himall.Entity;
using Himall.IServices;
using Himall.Model;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Himall.Service
{
    public class ArticleService : ServiceBase, IArticleService
    {
        public void AddArticle(ArticleInfo article)
        {

            article.AddDate = DateTime.Now;
            article.DisplaySequence = 1;
            Context.ArticleInfo.Add(article);
            Context.SaveChanges();
            string url = TransferImage(article.IconUrl, article.Id);
            article.IconUrl = url;

            string imageRealtivePath = "/Storage/Plat/Article/" + article.Id;
            article.IconUrl = TransferImage(article.IconUrl, article.Id);
            article.Content = HTMLProcess(article.Content, article.Id);//转移外站图片，去除script脚本,防止注入
            Context.SaveChanges();

        }

        public void UpdateArticle(ArticleInfo article)
        {
            var localArticle = Context.ArticleInfo.FindById(article.Id);
            localArticle.CategoryId = article.CategoryId;
            localArticle.IconUrl = article.IconUrl;
            localArticle.IsRelease = article.IsRelease;
            localArticle.Meta_Description = article.Meta_Description;
            localArticle.Meta_Keywords = article.Meta_Keywords;
            localArticle.Meta_Title = article.Meta_Title;
            localArticle.Title = article.Title;
            localArticle.IconUrl = TransferImage(localArticle.IconUrl, article.Id);
            localArticle.Content = HTMLProcess(article.Content, article.Id);//转移外站图片，去除script脚本,防止注入
            Context.SaveChanges();
        }


        /// <summary>
        /// 转移外站图片，去除script脚本
        /// </summary>
        /// <param name="content">html内容</param>
        /// <param name="id"></param>
        /// <returns></returns>
        string HTMLProcess(string content, long id)
        {
            string imageRealtivePath = "/Storage/Plat/Article/" + id;
            content = Core.Helper.HtmlContentHelper.TransferToLocalImage(content, "/", imageRealtivePath, Core.HimallIO.GetImagePath(imageRealtivePath) + "/");
            content = Core.Helper.HtmlContentHelper.RemoveScriptsAndStyles(content);
            return content;
        }


        /// <summary>
        /// 图片转移
        /// </summary>
        /// <param name="tempUrl">临时图片地址</param>
        /// <returns>实际图片路径（相对于网站根目录）</returns>
        string TransferImage(string tempUrl, long id)
        {
            if (!string.IsNullOrWhiteSpace(tempUrl) && !tempUrl.Contains("/Storage/"))
            {
                string dir = "/Storage/Plat/Article/";
                dir = dir + id;


                string ext = tempUrl.Substring(tempUrl.LastIndexOf('.'));
                //return dir + "/image" + ext;

                string resulturl = tempUrl;
                if (!string.IsNullOrWhiteSpace(resulturl))
                {
                    if (resulturl.Contains("/temp/"))
                    {
                        string logoname = resulturl.Substring(resulturl.LastIndexOf('/') + 1);
                        string oldlogo = resulturl.Substring(resulturl.LastIndexOf("/temp"));
                        string newLogo = dir + logoname;
                        Core.HimallIO.CopyFile(oldlogo, newLogo, true);
                        resulturl = newLogo;
                    }
                    else if (resulturl.Contains("/Storage/"))
                    {
                        resulturl = resulturl.Substring(resulturl.LastIndexOf("/Storage"));
                    }
                }
                return resulturl;
            }
            else
                return tempUrl;
        }



        public void DeleteArticle(params long[] ids)
        {
            var artiles = Context.ArticleInfo.FindBy(item => ids.Contains(item.Id));
            Context.ArticleInfo.RemoveRange(artiles);
            Context.SaveChanges();
            foreach (long id in ids)
            {
                string path = "/Storage/Plat/Article/" + id;
                if (HimallIO.ExistDir(path))
                    HimallIO.DeleteDir(path, true);
            }

        }

        public ObsoletePageModel<ArticleInfo> Find(long? articleCategoryId, string titleKeyWords, int pageSize, int pageNumber,bool isShowAll=true)
        {
            var articles = (from p in Context.ArticleInfo
                            join o in Context.ArticleCategoryInfo on p.CategoryId equals o.Id
                            select p
                            );
            if (!isShowAll) articles = articles.Where(d=>d.IsRelease==true);
            if (articleCategoryId.HasValue)
            {
                var categoryIds = (new ArticleCategoryService()).GetArticleCategoriesByParentId(articleCategoryId.Value, true).Select(item => item.Id).ToList();
                categoryIds.Add(articleCategoryId.Value);
                articles = articles.Where(item => categoryIds.Contains(item.CategoryId));
            }
            if (!string.IsNullOrWhiteSpace(titleKeyWords))
                articles = articles.Where(item => item.Title.Contains(titleKeyWords));
            var pageModel = new ObsoletePageModel<ArticleInfo>();
            int total;
            pageModel.Models = articles.FindBy(item => true, pageNumber, pageSize, out total, item => item.Id, false);
            pageModel.Total = total;

            return pageModel;
        }


        public ArticleInfo FindById(long id)
        {
            return Context.ArticleInfo.FindById(id);
        }


        public void UpdateArticleDisplaySequence(long id, long displaySequence)
        {
            var article = Context.ArticleInfo.FindById(id);
            if (article == null)
                throw new HimallException("未找到id为" + id + "的文章");
            article.DisplaySequence = displaySequence;
            Context.SaveChanges();
        }


        public IQueryable<ArticleInfo> GetTopNArticle<T>(int num, long categoryId, Expression<Func<ArticleInfo, T>> sort = null, bool isAsc = false)
        {
            var cate = Context.ArticleCategoryInfo.Where(a => a.ParentCategoryId == categoryId).Select(a=>a.Id).ToArray();
            var query = Context.ArticleInfo.FindBy(a => (a.CategoryId == categoryId ||cate.Contains(a.CategoryId))&& a.ArticleCategoryInfo.IsDefault && a.IsRelease == true);
            if (sort == null)
            {
                if (isAsc)
                    query = query.OrderBy(item => item.Id).Take(num);
                else
                    query = query.OrderByDescending(item => item.Id).Take(num);
            }
            else
            {
                if (isAsc)
                    query = query.OrderBy(sort).Take(num);
                else
                    query = query.OrderByDescending(sort).Take(num);

            }
            return query;
        }

        public IQueryable<ArticleInfo> GetArticleByArticleCategoryId(long articleCategoryId)
        {
            return Context.ArticleInfo.Where(item => item.CategoryId == articleCategoryId);
        }

        public ArticleInfo GetArticle(long id)
        {
            return Context.ArticleInfo.FindById(id);
        }
    }
}
