using Himall.Core;
using Himall.Entity;
using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Himall.Service
{
    public class ArticleCategoryService : ServiceBase, IArticleCategoryService
    {
        public IQueryable<ArticleCategoryInfo> GetArticleCategoriesByParentId(long parentId, bool recursive = false)
        {
            var articleCategories = Context.ArticleCategoryInfo.Where(item => item.ParentCategoryId == parentId);
            if (recursive)
            {
                var subIds = articleCategories.Select(item => item.Id).ToArray();
                foreach (var subId in subIds)
                    articleCategories = articleCategories.Concat(GetArticleCategoriesByParentId(subId, true));
            }
            return articleCategories;
        }

        public void AddArticleCategory(ArticleCategoryInfo articleCategory)
        {
            if (string.IsNullOrWhiteSpace(articleCategory.Name))
                throw new HimallException("未指定文章分类名称");

            //检查文章分类是否存在,排除0的父分类，0为根级
            if (articleCategory.ParentCategoryId != 0 && Context.ArticleCategoryInfo.Count(item => item.Id == articleCategory.ParentCategoryId) == 0)
                throw new HimallException("不存在父级为" + articleCategory.ParentCategoryId + "的文章分类");

            articleCategory.IsDefault = false;//默认项禁止用户添加
            articleCategory.DisplaySequence = 1;//设置默认顺序为1
            articleCategory = Context.ArticleCategoryInfo.Add(articleCategory);
            Context.SaveChanges();
        }

        public void UpdateArticleCategoryName(long id, string name)
        {
            ArticleCategoryInfo articleCategory = Context.ArticleCategoryInfo.FindById(id);
            if (articleCategory != null)
            {
                articleCategory.Name = name;
                Context.SaveChanges();
            }
            else
                throw new HimallException("未找到id为" + id + "的对象");
        }

        public void UpdateArticleCategoryDisplaySequence(long id, long displaySequence)
        {
            ArticleCategoryInfo articleCategory = Context.ArticleCategoryInfo.FindById(id);
            if (articleCategory != null)
            {
                articleCategory.DisplaySequence = displaySequence;
                Context.SaveChanges();
            }
            else
                throw new HimallException("未找到id为" + id + "的对象");
        }


        public void UpdateArticleCategory(ArticleCategoryInfo articleCategory)
        {
            if (articleCategory == null)
                throw new HimallException("未指定ArticleCategoryInfo实例");
            if (string.IsNullOrWhiteSpace(articleCategory.Name))
                throw new HimallException("未指定文章分类名称");

            //检查文章分类是否存在,排除0的父分类，0为根级
            if (articleCategory.ParentCategoryId != 0 && Context.ArticleCategoryInfo.Count(item => item.Id == articleCategory.ParentCategoryId) == 0)
                throw new HimallException("不存在父级为" + articleCategory.ParentCategoryId + "的文章分类");
            ArticleCategoryInfo oriArticleCategory = Context.ArticleCategoryInfo.FindById(articleCategory.Id);
            if (oriArticleCategory == null)
                throw new HimallException("未找到id为" + articleCategory.Id + "的对象");

            //修改
            oriArticleCategory.Name = articleCategory.Name;
            oriArticleCategory.ParentCategoryId = articleCategory.ParentCategoryId;
            Context.SaveChanges();//保存更改

        }


        public void DeleteArticleCategory(params long[] ids)
        {
            var articleCategories = Context.ArticleCategoryInfo.Where(item => ids.Contains(item.Id));
            foreach (long id in ids)
                articleCategories.Concat(GetArticleCategoriesByParentId(id, true));

            if (articleCategories.Count(item => item.IsDefault) > 0)
                throw new HimallException("系统内置分类不能删除");

            foreach (var item in articleCategories.ToList())
            {
                if (item.ArticleInfo.Count() > 0)
                    throw new HimallException("请先删除分类'" + item.Name + "'下的文章！");
            }

            Context.ArticleCategoryInfo.RemoveRange(articleCategories);
            Context.SaveChanges();
        }



        public ArticleCategoryInfo GetArticleCategory(long id)
        {
            return Context.ArticleCategoryInfo.FindById(id);
        }


        public string GetFullPath(long id, string seperator = ",")
        {
            StringBuilder path = new StringBuilder(id.ToString());
            long parentId = id;
            do
            {
                var articleCategory = GetArticleCategory(parentId);
                parentId = articleCategory.ParentCategoryId;
                path.Insert(0, parentId + seperator);
            } while (parentId != 0);
            return path.ToString();
        }


        public ArticleCategoryInfo GetSpecialArticleCategory(SpecialCategory categoryType)
        {
            //int id = 0;
            //switch (categoryType)
            //{
            //    case SpecialCategory.PageFootService:
            //        id = 1;
            //        break;
            //    case SpecialCategory.PlatformAd:
            //        id = 2;
            //        break;
            //}
            return GetArticleCategory((int)categoryType);
        }

        public IQueryable<ArticleCategoryInfo> GetCategories()
        {
            return Context.ArticleCategoryInfo.FindAll();
        }
        /// <summary>
        /// 检测是否存在重名栏目
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool CheckHaveRename(long id, string name)
        {
            bool result = false;
            result = (Context.ArticleCategoryInfo.Count(d=>d.Id!=id && d.Name==name)>0);
            return result;
        }
    }
}
