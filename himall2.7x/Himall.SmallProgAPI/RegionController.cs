using Himall.Application;
using Himall.CommonModel;
using Himall.Core;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.SmallProgAPI.Helper;
using Himall.SmallProgAPI.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Himall.SmallProgAPI
{
    /// <summary>
    /// 地区
    /// </summary>
    public class RegionController : BaseApiController
    {
        private IRegionService iRegionService;
        public RegionController()
        {
            iRegionService = ServiceProvider.Instance<IRegionService>.Create;
        }
        /// <summary>
        /// 获取所有子级地址
        /// </summary>
        /// <param name="parentRegionId">此参数无实际意义，仅为了兼容云商城挖的坑，在云商城系统里此参数也未参与任务实际业务</param>
        /// <returns></returns>
        public object GetAll(long? parentRegionId=null)
        {
            var regions = iRegionService.GetSubs(0,true);
            var models = regions.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                city =(p.Sub!=null?p.Sub.Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    area = (c.Sub != null ? c.Sub.Select(a => new
                    {
                        id = a.Id,
                        name = a.Name
                    }) : null)
                }):null)
            }).ToList();

            return Json(new
            {
                Status = "OK",
                province = models
            });
        }
        /// <summary>
        /// 获取直属子级
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public object GetSub(long parentId)
        {
            var region = iRegionService.GetRegion(parentId);
            if (region == null)
            {
                return Json(new BaseResultModel(false) { Message = "错误的参数：parentId" });
            }
            var models = region.Sub.Select(p => new
            {
                id = p.Id,
                name = p.Name,
            }).ToList();

            return Json(new
            {
                Status = "OK",
                Depth = region.Level.GetHashCode(),
                Regions = models
            });
        }
    }
}
