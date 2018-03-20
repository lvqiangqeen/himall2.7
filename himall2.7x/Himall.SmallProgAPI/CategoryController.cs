using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.SmallProgAPI.Model;
using Newtonsoft.Json;

namespace Himall.SmallProgAPI
{
    public class CategoryController : BaseApiController
    {
        public object GetAllCategories()
        {
            var json = new object();
            IEnumerable<CategoryInfo> categories = ServiceProvider.Instance<ICategoryService>.Create.GetMainCategory();
            if (categories == null)
            {
                json = GetErrorJson("没获取到相应的分类");
            }
            else
            {
                var model = categories.Select(c => new
                {
                    cid = c.Id,
                    name = c.Name,
                    subs = ServiceProvider.Instance<ICategoryService>.Create.GetCategoryByParentId(c.Id).Select(a => new
                    {
                        cid = a.Id,
                        name = a.Name
                    })
                }).ToList();
                json = new
                {
                    Status = "OK",
                    Data = model
                };
            }
            return json;
        }

        object GetErrorJson(string errorMsg)
        {
            var message =new
            {
                Status = "NO",
                Message = errorMsg
            };
            return message;
        }
    }
}
