using Himall.Model;
using System.Collections.Generic;
using System.Linq;


namespace Himall.IServices
{
    public interface IHomeCategoryService : IService
    {

        #region 首页分类设置

        /// <summary>
        /// 总首页分类集个数（行数）
        /// </summary>
        int TotalRowsCount { get;  }

        /// <summary>
        /// 获取所有首页分类集
        /// </summary>
        /// <returns></returns>
        IEnumerable<HomeCategorySet> GetHomeCategorySets();

        /// <summary>
        /// 获取指定首页分类集
        /// </summary>
        /// <param name="rowNumber">首页分类集行号</param>
        /// <returns></returns>
        HomeCategorySet GetHomeCategorySet(int rowNumber);

        /// <summary>
        /// 修改首页分类
        /// </summary>
        /// <param name="homeCategoryset">待修改的首页分类</param>
        void UpdateHomeCategorySet(HomeCategorySet homeCategoryset);


        /// <summary>
        /// 修改首页分类
        /// </summary>
        /// <param name="rowNumber">行号</param>
        /// <param name="categoryIds">分类id 集合</param>
        void UpdateHomeCategorySet(int rowNumber, IEnumerable<long> categoryIds);


        /// <summary>
        /// 修改首页分类
        /// </summary>
        /// <param name="rowNumber">行号</param>
        /// <param name="homeCategoryTopic">首页分类专题集合</param>
        void UpdateHomeCategorySet(int rowNumber, IEnumerable<HomeCategorySet.HomeCategoryTopic> homeCategoryTopic);


        /// <summary>
        /// 修改首页分类集显示顺序
        /// </summary>
        /// <param name="sourceRowNumber">原分类集行号</param>
        /// <param name="destiRowNumber">目标行号</param>
        void UpdateHomeCategorySetSequence(int sourceRowNumber, int destiRowNumber);


        #endregion





    }
}
