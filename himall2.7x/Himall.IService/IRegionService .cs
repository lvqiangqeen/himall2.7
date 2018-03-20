using System.Collections.Generic;
using Himall.Model;
using Himall.CommonModel;
using System;

namespace Himall.IServices
{
    public interface IRegionService : IService
    {
        /// <summary>
        /// 获得所有区域数据
        /// </summary>
        /// <returns></returns>
        IEnumerable<Region> GetAllRegions();

        /// <summary>
        /// 根据 编号获得区域模型
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Region GetRegion(long id);
        /// <summary>
        /// 根据 编号获得区域对应等级的区域模型(向上追溯)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        Region GetRegion(long id, Region.RegionLevel level);

        /// <summary>
        /// 根据 名称获得区域模型
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Region GetRegionByName(string name);

        /// <summary>
        /// 根据 名称获得区域模型(指定等级筛选)
        /// 
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        Region GetRegionByName(string name, Region.RegionLevel level);

        /// <summary>
        /// 获得 指定区域的下属区域
        /// </summary>
        /// <param name="parent">区域ID</param>
        /// <param name="trace">是否向下追溯所有子集</param>
        /// <returns></returns>
        IEnumerable<Region> GetSubs(long parent, bool trace = false);


        /// <summary>
        /// 获取三级子类
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        IEnumerable<Region> GetThridSubs(long parent);

        /// <summary>
        /// 重置地区数据 
        /// </summary>
        void ResetRegions();

        /// <summary>
        /// 获得 区域路径(编号路径)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="seperator"></param>
        /// <returns></returns>
        string GetRegionPath(long id, string seperator = ",");

        /// <summary>
        /// 根据地址名称反查地址全路径
        /// </summary>
        /// <param name="district">区名</param>
        /// <param name="street">街道名</param>
        /// <returns></returns>
        string GetAddress_Components(string city,string district, string street,out string newStreet);

        /// <summary>
        /// 获取 区域完整名称
        /// </summary>
        /// <param name="id"></param>
        /// <param name="seperator"></param>
        /// <returns></returns>
        string GetFullName(long id, string seperator = " ");

        /// <summary>
        /// 通过ip获取地区信息
        /// <para>(数据来源：淘宝)</para>
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        long GetRegionByIPInTaobao(string ip);

        /// <summary>
        /// 添加区域
        /// </summary>
        /// <param name="regionName"></param>
        /// <param name="level"></param>
        /// <param name="path"></param>
        /// 
        [Obsolete("请使用AddRegion有返回值的方法")]
        void AddRegion(string regionName, Region.RegionLevel level, string path);


        /// <summary>
        /// 添加区域
        /// </summary>
        /// <param name="regionName">名称</param>
        /// <param name="parentId">父ID</param>
        /// <returns></returns>
        long AddRegion(string regionName, long parentId);


        /// <summary>
        /// 修改区域
        /// </summary>
        /// <param name="regionName"></param>
        /// <param name="regionId"></param>
        void EditRegion(string regionName, int regionId);
    }
}
