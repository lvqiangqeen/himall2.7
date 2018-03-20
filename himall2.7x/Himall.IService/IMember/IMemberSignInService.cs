using Himall.Model;
using Himall.IServices.QueryModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.IServices
{
    /// <summary>
    /// 用户签到服务
    /// </summary>
    public interface IMemberSignInService : IService
    {
        /// <summary>
        /// 签到
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns>已持续签到天数</returns>
        int SignIn(long memberId);
        /// <summary>
        /// 今天是否可以签到
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        bool CanSignInByToday(long memberId);
        /// <summary>
        /// 获取签到信息
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        MemberSignInInfo GetSignInInfo(long memberId);
        /// <summary>
        /// 取得签到配置
        /// </summary>
        /// <returns></returns>
        SiteSignInConfigInfo GetConfig();
        /// <summary>
        /// 保存签到配置
        /// </summary>
        /// <param name="config"></param>
        void SaveConfig(SiteSignInConfigInfo config);
    }
}
