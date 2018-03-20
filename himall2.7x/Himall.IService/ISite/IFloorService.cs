using Himall.Model;
using System.Collections.Generic;
using System.Linq;

namespace Himall.IServices
{
    public interface IFloorService:IService
    {

        #region 首页楼层设置


        /// <summary>
        /// 添加首页楼层基本信息
        /// </summary>
        /// <param name="name">楼层名称</param>
        /// <param name="topLevelCategoryIds">楼层所包含的一级分类id</param>
        HomeFloorInfo AddHomeFloorBasicInfo(string name, IEnumerable<long> topLevelCategoryIds);

        /// <summary>
        /// 获取所有首页楼层
        /// </summary>
        /// <returns></returns>
        IQueryable<HomeFloorInfo> GetHomeFloors();

        IQueryable<HomeFloorInfo> GetAllHomeFloors();

        /// <summary>
        /// 获取指定id的首页楼层
        /// </summary>
        /// <returns></returns>
        HomeFloorInfo GetHomeFloor(long id);

        /// <summary>
        /// 更新首页楼层基本信息
        /// </summary>
        /// <param name="name">楼层名称</param>
        /// <param name="topLevelCategoryIds">楼层所包含的一级分类id</param>
        /// <param name="homeFloorId">首页楼层id</param>
        void UpdateFloorBasicInfo(long homeFloorId, string name, IEnumerable<long> topLevelCategoryIds);



        /// <summary>
        /// 修改首页楼层显示顺序
        /// </summary>
        /// <param name="sourceSequence">原分类集行号</param>
        /// <param name="destiSequence">目标行号</param>
        void UpdateHomeFloorSequence(long sourceSequence, long destiSequence);

        /// <summary>
        /// 启用或关闭楼层
        /// </summary>
        /// <param name="homeFloorId">楼层id</param>
        /// <param name="enable">开启或关闭</param>
        void EnableHomeFloor(long homeFloorId, bool enable);


        /// <summary>
        /// 删除楼层
        /// </summary>
        /// <param name="id">楼层id</param>
        void DeleteHomeFloor(long id);

        /// <summary>
        /// 保存楼层详细信息
        /// </summary>
        /// <param name="homeFloor">楼层详细信息</param>
        void UpdateHomeFloorDetail(HomeFloorInfo homeFloor);


        #endregion

    }
}
