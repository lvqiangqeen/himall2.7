using Himall.Core;
using Himall.Entity;
using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Himall.Service
{
	public class ShopCategoryService : ServiceBase, IShopCategoryService
	{
		public IEnumerable<ShopCategoryInfo> GetMainCategory(long shopId)
		{
			return GetCategories().Where(t => t.ParentCategoryId == 0 && t.ShopId == shopId);
		}

		/// <summary>
		/// 获取所有商品分类并缓存
		/// </summary>
		/// <returns></returns>
		IEnumerable<ShopCategoryInfo> GetCategories()
		{
			IEnumerable<ShopCategoryInfo> categories = Context.ShopCategoryInfo.FindAll().ToArray();

			//if (Cache.Get(CacheKeyCollection.Category) != null)
			//{
			//    categories = (IEnumerable<ShopCategoryInfo>)Core.Cache.Get(CacheKeyCollection.Category);
			//}
			//else
			//{
			//    categories = context.ShopCategoryInfo.FindAll().ToArray();
			//    Cache.Insert(CacheKeyCollection.Category, categories);
			//}

			return categories;
		}

		public void AddCategory(ShopCategoryInfo model)
		{
			if (null == model)
				throw new ArgumentNullException("model", "添加一个商品分类时，Model为空");

			var obja = Context.ShopCategoryInfo.Where(r => r.Name.Equals(model.Name) && r.ShopId == model.ShopId && r.ParentCategoryId == model.ParentCategoryId);
			if (obja.Count() > 0)
				throw new HimallException("分类名称已经存在");

			Context.ShopCategoryInfo.Add(model);
			Context.SaveChanges();
			Cache.Remove(CacheKeyCollection.Category);
		}

		public ShopCategoryInfo GetCategoryByProductId(long id)
		{
			if (id <= 0)
				throw new ArgumentNullException("id", string.Format("获取一个商品分类时，id={0}", id));

			//var model = GetCategories().Where(t => t.Id == id).FirstOrDefault();

			var model = Context.ProductShopCategoryInfo.Where(p => p.ProductId == id).FirstOrDefault();
			if (model != null)
			{
				return model.ShopCategoryInfo;
			}
			else
			{
				return new ShopCategoryInfo()
				{
					Name = ""
				};
			}
		}
		public ShopCategoryInfo GetCategory(long id)
		{
			if (id <= 0)
				throw new ArgumentNullException("id", string.Format("获取一个商品分类时，id={0}", id));

			var model = GetCategories().Where(t => t.Id == id).FirstOrDefault();
			//var model = context.ProductShopCategoryInfo.Where(p => p.ProductId == id).FirstOrDefault().ShopCategoryInfo;
			return model;

		}

		public void UpdateCategoryName(long id, string name)
		{
			if (id <= 0)
				throw new ArgumentNullException("id", string.Format("更新一个商品分类的名称时，id={0}", id));
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException("name", "更新一个商品分类的名称时，name为空");

			var category = Context.ShopCategoryInfo.FindById(id);
			if (null == category || category.Id != id)
				throw new HimallException(string.Format("更新一个商品分类的名称时，找不到id={0} 的商品分类", id));


			var obja = Context.ShopCategoryInfo.Where(r => r.Name.Equals(name) && r.ShopId == category.ShopId && r.ParentCategoryId == category.ParentCategoryId && r.Id != id);
			if (obja.Count() > 0)
				throw new HimallException("分类名称已经存在");

			category.Name = name;
			Context.SaveChanges();

			Cache.Remove(CacheKeyCollection.Category);
		}

		public void UpdateCategoryDisplaySequence(long id, long displaySequence)
		{
			if (id <= 0)
				throw new ArgumentNullException("id", string.Format("更新一个商品分类的显示顺序时，id={0}", id));
			if (0 >= displaySequence)
				throw new ArgumentNullException("displaySequence", "更新一个商品分类的显示顺序时，displaySequence小于等于零");

			var category = Context.ShopCategoryInfo.FindById(id);
			if (null == category || category.Id != id)
				throw new Exception(string.Format("更新一个商品分类的显示顺序时，找不到id={0} 的商品分类", id));

			category.DisplaySequence = displaySequence;
			Context.SaveChanges();

			Cache.Remove(CacheKeyCollection.Category);
		}


		public IEnumerable<ShopCategoryInfo> GetCategoryByParentId(long id)
		{
			if (id < 0)
				throw new ArgumentNullException("id", string.Format("获取子级商品分类时，id={0}", id));

			if (id == 0)
			{
				return GetCategories().Where(c => c.ParentCategoryId == 0);
			}
			else
			{
				var category = GetCategories().Where(c => c.ParentCategoryId == id);
				if (category == null)
					return null;
				return category.OrderByDescending(c => c.DisplaySequence).ToList();
			}
		}




		public long GetMaxCategoryId()
		{
			return GetCategories().Count() == 0 ? 0 : GetCategories().Max(c => c.Id);
		}


		public void UpdateCategory(ShopCategoryInfo model)
		{
			var category = Context.ShopCategoryInfo.FindById(model.Id);
			category.Name = model.Name;
			Context.SaveChanges();

			Cache.Remove(CacheKeyCollection.Category);

		}

		private void ProcessingDeleteCategory(long id, long shopId)
		{
			var subIds = Context.ShopCategoryInfo.FindBy(c => c.ParentCategoryId == id && c.ShopId == shopId).Select(c => c.Id);
			if (subIds.Count() == 0)
			{
				Context.ShopCategoryInfo.Remove(Context.ShopCategoryInfo.FindById(id));
				return;
			}
			else
			{
				foreach (var item in subIds.ToList())
				{
					ProcessingDeleteCategory(item, shopId);
				}
			}
			Context.ShopCategoryInfo.Remove(Context.ShopCategoryInfo.FindById(id));
		}

		public void DeleteCategory(long id, long shopId)
		{
			ProcessingDeleteCategory(id, shopId);
			Context.SaveChanges();
			Cache.Remove(CacheKeyCollection.Category);

		}

		public IQueryable<CategoryInfo> GetBusinessCategory(long shopId)
		{
			var isSelf = Context.ShopInfo.FindById(shopId).IsSelf;
			return this.GetBusinessCategory(shopId, isSelf);
		}

		public IQueryable<CategoryInfo> GetBusinessCategory(long shopId, bool isSelf)
		{
			if (isSelf)
			{
				return this.Context.CategoryInfo.Where(p => p.IsDeleted == false);
			}
			else
			{
				var bcs=Context.BusinessCategoryInfo.Where(p => p.ShopId == shopId);
				var info = Context.CategoryInfo.Where(c => c.IsDeleted == false && bcs.Any(cc => cc.CategoryId == c.Id)).ToList();
				List<long> ids = new List<long>();
				foreach (var c in info)
				{
					var paths = c.Path.Split('|');
					for (int i = 0; i < paths.Length; i++)
					{
						long id = long.Parse(paths[i]);
						if (!ids.Contains(id))
						{
							ids.Add(id);
						}
					}
				}
				var categories = Context.CategoryInfo.Where(item => item.IsDeleted == false && ids.Contains(item.Id));

				return categories;
			}
		}


		public IQueryable<ShopCategoryInfo> GetShopCategory(long shopId)
		{
			return this.Context.ShopCategoryInfo.FindBy(s => s.ShopId == shopId);
		}



		public IEnumerable<ShopCategoryInfo> GetCategoryByParentId(long id, long shopId)
		{
			if (id < 0)
				throw new HimallException(string.Format("获取子级分类时，id={0}", id));
			return GetCategories().Where(c => c.ShopId == shopId && c.ParentCategoryId == id).OrderByDescending(t => t.DisplaySequence);
		}
	}
}
