using System.Collections.Generic;
using Himall.Core;
using Himall.IServices;
using Himall.Model;
using Himall.CommonModel;
using System.Linq;
using System;

namespace Himall.Application
{
    public class RegionApplication
    {
        private static IRegionService _iRegionService = ObjectContainer.Current.Resolve<IRegionService>();

        /// <summary>
        /// 获取全部 区域数据
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Region> GetAllRegions()
        {
            return _iRegionService.GetAllRegions();
        }

        /// <summary>
        /// 获取指定区域
        /// </summary>
        /// <param name="id">区域编号</param>
        /// <returns></returns>
        public static Region GetRegion(long id)
        {
            var model= _iRegionService.GetRegion(id);
            if(model==null)
            {
                model = _iRegionService.GetRegion(GetDefaultRegionId());
            }
            return model;
        }

        /// <summary>
        /// 获取 指定下属区域
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public static IEnumerable<Region> GetSubRegion(long parentId, bool trace = false)
        {
            return _iRegionService.GetSubs(parentId, trace);
        }

        /// <summary>
        /// 获取下属区域到三级
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public static IEnumerable<Region> GetThirdSubRegion(long parentId)
        {
            return _iRegionService.GetThridSubs(parentId);
        }

        /// <summary>
        /// 获取指定区域下属 键(编号)/值(名称)
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public static Dictionary<int, string> GetSubMap(long parentId)
        {
            return _iRegionService.GetSubs(parentId).ToDictionary(k => k.Id, v => v.Name);
        }

        /// <summary>
        ///  获取省 市 区 的编号，中间用逗号隔开
        /// </summary>
        /// <param name="regionId"></param>
        /// <returns></returns>
        public static string GetRegionPath(int regionId)
        {
            return _iRegionService.GetRegionPath(regionId);
        }
        /// <summary>
        /// 根据地址名称反查地址全路径
        /// </summary>
        /// <param name="city">城市名</param>
        /// <param name="district">区名</param>
        /// <param name="street">街道名</param>
        /// <returns></returns>
        public static string GetAddress_Components(string city,string district, string street,out string newStreet)
        {
            return _iRegionService.GetAddress_Components(city,district, street,out newStreet);
        }
        /// <summary>
        /// 根据城名称获取对应的区域模型
        /// </summary>
        /// <param name="name"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static Region GetRegionByName(string name, Region.RegionLevel level)
        {
            return _iRegionService.GetRegionByName(name, level);
        }
        /// <summary>
        /// 获取 区域全称,不同等级 空格 隔开
        /// </summary>
        /// <param name="regionId"></param>
        /// <param name="seperator">分隔符</param>
        /// <returns></returns>
        public static string GetFullName(int id, string seperator = " ")
        {
            return _iRegionService.GetFullName(id, seperator);
        }

        /// <summary>
        /// 获取区域名称
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetRegionName(int id)
        {
            var region = _iRegionService.GetRegion(id);
            return region.Name;
        }

        /// <summary>
        /// 获取 区域对应等级的名称
        /// </summary>
        /// <param name="id">区域ID</param>
        /// <param name="level">等级</param>
        /// <returns></returns>
        public static Region GetRegion(long id, Region.RegionLevel level)
        {
            return _iRegionService.GetRegion(id, level);
        }


        ///// <summary>
        ///// 添加区域
        ///// </summary>
        ///// <param name="regionName"></param>
        ///// <param name="level"></param>
        ///// <param name="path"></param>
        ///// 
        //public static void AddRegion(string regionName, Region.RegionLevel level, string path)
        //{
        //    _iRegionService.AddRegion(regionName, level, path);
        //}


        /// <summary>
        /// 添加区域
        /// </summary>
        /// <param name="regionName"></param>
        /// <param name="level"></param>
        /// <param name="path"></param>
        public static long AddRegion(string regionName, long parentId)
        {
            if(string.IsNullOrWhiteSpace(regionName))
            {
                throw new HimallException("区域名称不能为空");
            }
          return  _iRegionService.AddRegion(regionName, parentId);
        }


        /// <summary>
        /// 修改区域
        /// </summary>
        /// <param name="regionName"></param>
        /// <param name="regionId"></param>
        public static void EditRegion(string regionName, int regionId)
        {
            if (string.IsNullOrWhiteSpace(regionName))
            {
                throw new HimallException("区域名称不能为空");
            }
            _iRegionService.EditRegion(regionName, regionId);
        }

        /// <summary>
        /// 获取区域简称
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetRegionShortName(int id)
        {
            var region = _iRegionService.GetRegion(id);
            return string.IsNullOrEmpty(region.ShortName) ? region.Name : region.ShortName;
        }

        /// <summary>
        /// 获取区域名称(多个区域)
        /// </summary>
        /// <param name="ids">编号列表</param>
        /// <param name="seperator">分割符号</param>
        /// <returns></returns>
        public static string GetRegionName(string regionIds, string seperator = ",")
        {
            var ids = regionIds.Split(new string[] { seperator }, StringSplitOptions.RemoveEmptyEntries).Select(p => long.Parse(p)).ToList();
            var regions = new List<Region>();
            foreach (var item in ids)
                regions.Add(_iRegionService.GetRegion(item));

            var result = string.Empty;
            foreach (var item in regions)
            {
                if (!string.IsNullOrEmpty(result)) result += seperator;
                if (item != null) result += item.Name;
            }
            return result;
        }

        public static Region GetRegionByName(string name)
        {
            return _iRegionService.GetRegionByName(name);
        }

        /// <summary>
        /// 通过ip获取地区信息
        /// <para>(数据来源：淘宝)</para>
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static long GetRegionByIPInTaobao(string ip)
        {
            return _iRegionService.GetRegionByIPInTaobao(ip);
        }

        /// <summary>
        /// 重置为默认
        /// </summary>
        public static  void ResetRegion()
        {
            _iRegionService.ResetRegions();
        }
        /// <summary>
        /// 默认 region id
        /// </summary>
        /// <returns></returns>
        public static int GetDefaultRegionId()
        {
            return CommonConst.DEFAULT_REGION_ID;
        }
    }
}
