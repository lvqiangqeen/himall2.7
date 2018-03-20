using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntityFramework.Extensions;

using Himall.Core;
using Himall.Entity;
using Himall.IServices;
using Himall.Model;

namespace Himall.Service
{
    public class CategoryService : ServiceBase, ICategoryService
    {
		#region 静态字段
		private static object _id_locker = new object();
		#endregion

		#region 常量
		/// <summary>
		/// 分类路径分隔符
		/// </summary>
		const char CATEGORY_PATH_SEPERATOR = '|';
		#endregion

        #region 平台

        public IEnumerable<CategoryInfo> GetMainCategory()
        {
            return GetCategories().Where(t => t.ParentCategoryId == 0);
        }

        /// <summary>
        /// 获取所有分类并缓存
        /// </summary>
        /// <returns></returns>
		public List<CategoryInfo> GetCategories()
		{
			List<CategoryInfo> categories = null;

			if (Cache.Exists(CacheKeyCollection.Category))
				categories = Core.Cache.Get<List<CategoryInfo>>(CacheKeyCollection.Category);

			if (categories == null)
			{
				categories = this.NoLazyNoProxyContext.CategoryInfo.Where(p => p.IsDeleted == false).ToList();
				Cache.Insert<List<CategoryInfo>>(CacheKeyCollection.Category, categories);
			}

			return categories;
		}

        public void AddCategory(CategoryInfo model)
        {
            if (null == model)
                throw new ArgumentNullException("model", "添加一个分类时，Model为空");
            if (model.ParentCategoryId == 0)
            {
                CategoryCashDepositInfo categoryCashDeposit = new CategoryCashDepositInfo() { Id = 0, CategoryId = model.Id };
                model.Himall_CategoryCashDeposit = categoryCashDeposit;
            }
            Context.CategoryInfo.Add(model);
            Context.SaveChanges();
            Cache.Remove(CacheKeyCollection.Category);
        }

        public CategoryInfo GetCategory(long id)
        {
            if (id <= 0)
                return null;
            var model = GetCategories().Where(t => t.Id == id).FirstOrDefault();
            return model;

        }

        public void UpdateCategoryName(long id, string name)
        {
            if (id <= 0)
                throw new ArgumentNullException("id", string.Format("更新一个分类的名称时，id={0}", id));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name", "更新一个分类的名称时，name为空");

            var category = Context.CategoryInfo.FindById(id);
            if (null == category || category.Id != id)
                throw new Exception(string.Format("更新一个分类的名称时，找不到id={0} 的分类", id));

            if (null != category)
            {
                var obja = Context.CategoryInfo.Where(r => r.ParentCategoryId == category.ParentCategoryId && r.Name == name && r.Id != category.Id);
                if (obja.Count() >= 1)
                    throw new Exception(string.Format("分类名称重复", id));
            }

            category.Name = name;
            Context.SaveChanges();

            Cache.Remove(CacheKeyCollection.Category);
        }
        public void UpdateCategoryCommis(long id, decimal commis)
        {
            if (id <= 0)
                throw new ArgumentNullException("id", string.Format("更新一个分类的分佣比率时，id={0}", id));
            if (commis<0)
                throw new ArgumentNullException("commis", "更新一个分类的分佣比率时，commis小于0");

            var category = Context.CategoryInfo.FindById(id);
            if (null == category || category.Id != id)
                throw new Exception(string.Format("更新一个分类的名称时，找不到id={0} 的分类", id));

            category.CommisRate = commis;
            Context.SaveChanges();

            Cache.Remove(CacheKeyCollection.Category);
        }
        public void UpdateCategoryDisplaySequence(long id, long displaySequence)
        {
            if (id <= 0)
                throw new ArgumentNullException("id", string.Format("更新一个分类的显示顺序时，id={0}", id));
            if (0 >= displaySequence)
                throw new ArgumentNullException("displaySequence", "更新一个分类的显示顺序时，displaySequence小于等于零");

            var category = Context.CategoryInfo.FindById(id);
            if (null == category || category.Id != id)
                throw new Exception(string.Format("更新一个分类的显示顺序时，找不到id={0} 的分类", id));

            category.DisplaySequence = displaySequence;
            Context.SaveChanges();

            Cache.Remove(CacheKeyCollection.Category);
        }


        public IEnumerable<CategoryInfo> GetCategoryByParentId(long id)
        {
            if (id < 0)
                throw new ArgumentNullException("id", string.Format("获取子级分类时，id={0}", id));

            if (id == 0)
            {
                return GetCategories().Where(c => c.ParentCategoryId == 0);
            }
            else
            {
                var category = GetCategories().Where(c => c.ParentCategoryId == id);
                if (category == null) return null;
                return category.OrderBy(c => c.DisplaySequence).ToList();
            }
        }


        public IEnumerable<CategoryInfo> GetFirstAndSecondLevelCategories()
        {
            var list = GetCategories();
            var result =  list.Where(c => list.Any(cc => cc.ParentCategoryId == c.Id) || c.Depth < 3).ToList();
            return result;
        }


        public long GetMaxCategoryId()
        {
			lock (_id_locker)
			{
				return this.Context.CategoryInfo.Max(p => (long?)p.Id).GetValueOrDefault();
			}
        }



        public void UpdateCategory(CategoryInfo model)
        {
            var category = Context.CategoryInfo.FindById(model.Id);
            category.Icon = model.Icon;
            category.Meta_Description = model.Meta_Description;
            category.Meta_Keywords = model.Meta_Keywords;
            category.Meta_Title = model.Meta_Title;
            category.Name = model.Name;
            category.RewriteName = model.RewriteName;
            category.TypeId = model.TypeId;
            category.CommisRate = model.CommisRate;
            Context.SaveChanges();

            Cache.Remove(CacheKeyCollection.Category);

        }

        public void DeleteCategory(long id)
        {
			var path = this.Context.CategoryInfo.Where(p => p.Id == id).Select(p => new { p.Path }).FirstOrDefault();
			if (path == null)
			{
				Cache.Remove(CacheKeyCollection.Category);
				return;
			}

			var pathStart = path.Path + CATEGORY_PATH_SEPERATOR;
			var allChildIds=this.Context.CategoryInfo.Where(p => p.Path.StartsWith(pathStart)).Select(p => p.Id).ToList();

			var existProduct = this.Context.ProductInfo.Exist(p => (p.CategoryId == id || allChildIds.Contains(p.CategoryId)) && p.IsDeleted == false);
			if (existProduct)
				throw new HimallException("删除失败，因为有商品与该分类或子分类关联");

			allChildIds.Add(id);
			//this.Context.CategoryInfo.Where(p => p.Id == id || allChildIds.Contains(p.Id)).Update(p => new CategoryInfo { IsDeleted = true });
			var sql = string.Format("UPDATE himall_categories SET IsDeleted=@p0 WHERE Id IN ({0})",string.Join(",",allChildIds));
			this.Context.Database.ExecuteSqlCommand(sql, true, id);

			//this.Context.BusinessCategoryInfo.Where(p => p.CategoryId == id || allChildIds.Contains(p.CategoryId)).Delete();
			sql = string.Format("DELETE FROM himall_businesscategories WHERE CategoryId IN ({0})", string.Join(",", allChildIds));
			this.Context.Database.ExecuteSqlCommand(sql, id);

            sql = "DELETE FROM himall_homecategories WHERE CategoryId Not IN (select Id from himall_categories where IsDeleted=0)";  //清理己删分类在首页的显示
            this.Context.Database.ExecuteSqlCommand(sql, id);

            Cache.Remove(CacheKeyCollection.Category);
        }


        public IEnumerable<CategoryInfo> GetTopLevelCategories(IEnumerable<long> categoryIds)
        {
            var categories = GetCategories().Where(item => categoryIds.Contains(item.Id));
            List<long> topLevelIds = new List<long>();
            foreach (var cateogry in categories.ToList())
            {
                if (cateogry.Depth == 1)
                    topLevelIds.Add(cateogry.Id);
                else
                {
                    var path = cateogry.Path;
                    var topLevelId = long.Parse(path.Split(CATEGORY_PATH_SEPERATOR)[0]);//取全路径的第一级转换，即所属一级分类id
                    topLevelIds.Add(topLevelId);
                }
            }
            return GetCategories().Where(item => topLevelIds.Contains(item.Id));
        }


        public IEnumerable<CategoryInfo> GetSecondAndThirdLevelCategories(params long[] ids)
        {
            var categoies = GetCategories().Where(item => ids.Contains(item.ParentCategoryId));
            var categoryList = new List<CategoryInfo>(categoies);

            foreach (var categoryId in categoies.Select(item => item.Id).ToList())
            {
                var category = GetCategories().Where(item => item.ParentCategoryId == categoryId);
                categoryList.AddRange(category);
            }
            return categoryList;
        }

        public string GetEffectCategoryName(long shopId, long typeId)
        {
            StringBuilder name = new StringBuilder();
			var cates = Context.CategoryInfo.Where(c => c.TypeId == typeId && c.IsDeleted == false).ToList();
            var businessCate = Context.BusinessCategoryInfo.FindBy(b => b.ShopId == shopId);
            foreach (var item in cates)
            {
                if (businessCate.Any(b => b.CategoryId == item.Id))
                {
                    name.Append(item.Name);
                    name.Append(',');
                }
            }
            return name.ToString().TrimEnd(',');
        }

        #endregion

        public IEnumerable<CategoryInfo> GetValidBusinessCategoryByParentId(long id)
        {
            var allCategories = GetCategories().ToArray();
            var categories = allCategories.Where(item => item.ParentCategoryId == id).ToArray();
            if (id == 0)//表示第一级，只筛选出包含第三级分类的一级分类
            {
                var topCateogryIds = allCategories.Where(item => item.Path.Split('|').Length == 3).Select(item => long.Parse(item.Path.Split('|')[0]));
                categories = categories.Where(item => topCateogryIds.Contains(item.Id)).ToArray();
            }
            else//只筛选出包含第三级分类的二级分类
            {
                var currentCategory = allCategories.FirstOrDefault(item => item.Id == id);
                if (currentCategory != null && currentCategory.Depth == 1)//判断是否第一级，如果是第一级则表示获取第二级数据
                {
                    var secondCateogryIds = allCategories.Where(item => item.Path.Split('|').Length == 3).Select(item => long.Parse(item.Path.Split('|')[1]));
                    categories = categories.Where(item => secondCateogryIds.Contains(item.Id)).ToArray();
                }
            }
            return categories;
        }


        public ObsoletePageModel<CategoryInfo> GetCategoryByName(string name, int pageNo, int pageSize)
        {
            int total = 0;
			IQueryable<CategoryInfo> categories = Context.CategoryInfo.FindBy(item =>
				(name == null || name == "" || item.Name.Contains(name)) && item.IsDeleted == false, pageNo, pageSize, out total, a => a.Id, false);
            ObsoletePageModel<CategoryInfo> pageModel = new ObsoletePageModel<CategoryInfo>() { Models = categories, Total = total };
            return pageModel;
        }
    }
}
