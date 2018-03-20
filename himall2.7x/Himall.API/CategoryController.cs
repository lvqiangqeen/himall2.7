using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.API.Model;

namespace Himall.API
{
    public class CategoryController : BaseApiController
    {
        public object GetCategories()
        {
            var categories = ServiceProvider.Instance<ICategoryService>.Create.GetCategories().ToArray();
            var model = categories
                .Where(item => item.ParentCategoryId == 0)
                .Select(item => new CategoryModel()
                {
                    Id = item.Id,
                    Name = item.Name,
                    SubCategories = GetSubCategories(categories, item.Id, 1),
                    Depth = 0,
                    DisplaySequence = item.DisplaySequence
                }).OrderBy(c => c.DisplaySequence);
            return Json(new { Success = "True", Category = model });
        }


        IEnumerable<CategoryModel> GetSubCategories(IEnumerable<CategoryInfo> allCategoies, long categoryId, int depth)
        {
            var categories = allCategoies
                .Where(item => item.ParentCategoryId == categoryId)
                .Select(item =>
                {
                    string image = string.Empty;
                    if (depth == 2)
                    {
                        //image ="http://" + Url.Request.RequestUri.Host + item.Icon;
                        if (!string.IsNullOrWhiteSpace(item.Icon))
                            image = Core.HimallIO.GetRomoteImagePath(item.Icon);
                    }
                    return new CategoryModel()
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Image = image,
                        SubCategories = GetSubCategories(allCategoies, item.Id, depth + 1),
                        Depth = 1,
                        DisplaySequence = item.DisplaySequence
                    };
                })
                   .OrderBy(c => c.DisplaySequence);
            return categories;
        }
    }
}
