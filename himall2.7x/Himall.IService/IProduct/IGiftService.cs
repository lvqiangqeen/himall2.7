using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.IServices.QueryModel;

namespace Himall.IServices
{
    /// <summary>
    /// 积分礼品
    /// </summary>
    public interface IGiftService : IService
    {
        /// <summary>
        /// 添加礼品
        /// </summary>
        void AddGift(GiftInfo model);
        /// <summary>
        /// 修改礼品
        /// </summary>
        /// <param name="model"></param>
        void UpdateGift(GiftInfo model);
        /// <summary>
        /// 调整排序
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sequence"></param>
        void UpdateSequence(long id, int sequence);
        /// <summary>
        /// 上下架礼品
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sequence"></param>
        void ChangeStatus(long id, bool status);
        /// <summary>
        /// 获取礼品
        /// </summary>
        /// <param name="id"></param>
        GiftInfo GetById(long id);
        /// <summary>
        /// 获取礼品
        /// <para>无追踪实体，UpdateGift前调用</para>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        GiftInfo GetByIdAsNoTracking(long id);
        /// <summary>
        /// 查询礼品
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        ObsoletePageModel<GiftModel> GetGifts(GiftQuery query);

        /// <summary>
        /// 获取广告配置
        /// </summary>
        /// <param name="adtype">活动类型</param>
        /// <param name="adplatform">显示平台</param>
        /// <returns></returns>
        IntegralMallAdInfo GetAdInfo(IntegralMallAdInfo.AdActivityType adtype, IntegralMallAdInfo.AdShowPlatform adplatform);
        /// <summary>
        /// 更新广告信息
        /// </summary>
        /// <param name="ActivityType">活动类型</param>
        /// <param name="ActivityId">活动编号</param>
        /// <param name="Cover">广告图片</param>
        /// <param name="ShowStatus">显示状态</param>
        /// <param name="ShowPlatform">显示平台</param>
        /// <returns></returns>
        IntegralMallAdInfo UpdateAdInfo(IntegralMallAdInfo.AdActivityType ActivityType, long ActivityId, string Cover, IntegralMallAdInfo.AdShowStatus? ShowStatus, IntegralMallAdInfo.AdShowPlatform? ShowPlatform);
    }
}
