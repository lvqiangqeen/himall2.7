using System.Collections.Generic;
using System.Linq;
using Himall.Core;
using Himall.IServices;

namespace Himall.Application
{
    /// <summary>
    /// 商品类别
    /// </summary>
    public  class CategoryApplication 
    {
        private static ICategoryService _iCategoryService= ObjectContainer.Current.Resolve<ICategoryService>();
        /// <summary>
        /// 获取类别下所有子类
        /// </summary>
        /// <returns></returns>
        public  static List<Himall.DTO.Category> GetSubCategories()
        {
            var categories = _iCategoryService.GetCategories().ToArray();
            var model = categories
                .Where(item => item.ParentCategoryId == 0)
                .Select(item => new Himall.DTO.Category()
                {
                    Id = item.Id,
                    Name = item.Name,
                    SubCategories = GetSubCategories(categories, item.Id, 1),
                    Depth = 0,
                    DisplaySequence = item.DisplaySequence
                }).OrderBy(c => c.DisplaySequence).ToList();
            return model;
        }


        /// <summary>
        /// 递归获取类别下所有子类
        /// </summary>
        /// <param name="allCategoies">所有分类</param>
        /// <param name="categoryId">分类ID</param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public static List<Himall.DTO.Category> GetSubCategories(IEnumerable<Himall.Model.CategoryInfo> allCategoies, long categoryId, int depth)
        {
            var categories = allCategoies
                .Where(item => item.ParentCategoryId == categoryId)
                .Select(item =>
                {
                    string image = string.Empty;
                    if (depth == 2)
                    {
                        image = item.Icon;
                        if (string.IsNullOrWhiteSpace(image))
                            image = string.Empty;
                    }
                    return new Himall.DTO.Category()
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Image = Himall.Core.HimallIO.GetImagePath(image),
                        SubCategories = GetSubCategories(allCategoies, item.Id, depth + 1),
                        Depth = 1,
                        DisplaySequence = item.DisplaySequence
                    };
                })
                   .OrderBy(c => c.DisplaySequence).ToList();
            return categories;
        }
    }
}
