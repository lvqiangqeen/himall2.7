using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Model;

namespace Himall.IServices
{
    public  interface IWeiActivityWinService : IService
    {
        /// <summary>
        ///  查询中奖信息
        /// </summary>
        ObsoletePageModel<WeiActivityWinInfo> Get(string text,long id,int pageIndex, int pageSize);

        /// <summary>
        /// 添加中奖信息
        /// </summary>
        /// <param name="info">中奖信息实体</param>
        void AddWinner(WeiActivityWinInfo info);
        /// <summary>
        /// 查询中奖人数
        /// </summary>
        /// <param name="activityId">活动id</param>
        /// <param name="text">查询条件</param>
        /// <returns>人数</returns>
        string GetWinNumber(long activityId, string text);

        /// <summary>
        /// 查询用户中奖信息
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        List<WeiActivityWinInfo> GetWinInfo(long userId);
    }
}
