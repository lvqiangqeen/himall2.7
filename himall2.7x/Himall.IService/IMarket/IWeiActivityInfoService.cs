using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Model;
using Himall.CommonModel;

namespace Himall.IServices
{
    public interface IWeiActivityInfoService : IService
    {
        /// <summary>
        ///  查询刮刮卡
        /// </summary>
        ObsoletePageModel<WeiActivityInfo> Get(string text, WeiActivityType type, int pageIndex, int pageSize, bool? isIntegralActivity = null, bool isShowAll = true);
        
        /// <summary>
        ///  添加刮刮卡
        /// </summary>
        long AddActivity(WeiActivityInfo model);

        /// <summary>
        ///  修改刮刮卡
        /// </summary>
        long UpdateActivity(WeiActivityInfo model);

        /// <summary>
        /// 删除刮刮卡
        /// </summary>
        void DeleteActivity(long id);

        ///// <summary>
        ///// 获取刮刮卡
        ///// </summary>
        WeiActivityInfo GetActivityModel(long id);
        /// <summary>
        /// 查询是否全部抽完
        /// </summary>
        /// <param name="id">活动Id</param>
        /// <param name="awardId">奖等Id</param>
        /// <returns></returns>
        int GetProportion(long id, long awardId);
    }
}
