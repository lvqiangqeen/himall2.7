using Himall.Core;
using Himall.Model;
using System.Linq;

namespace Himall.IServices
{
    public interface IMobileHomeTopicService : IService
    {

        /// <summary>
        /// 获取移动端首页专题设置
        /// </summary>
        /// <param name="platformType">平台类型</param>
        /// <param name="shopId">店铺Id</param>
        /// <returns></returns>
        IQueryable<MobileHomeTopicsInfo> GetMobileHomeTopicInfos(PlatformType platformType, long shopId = 0);

        /// <summary>
        /// 获取指定移动端首页专题设置Id
        /// </summary>
        /// <param name="id">专题设置Id</param>
        /// <param name="shopId">店铺Id</param>
        /// <returns></returns>
        MobileHomeTopicsInfo GetMobileHomeTopic(long id, long shopId = 0);

        /// <summary>
        /// 添加移动端首页专题设置
        /// </summary>
        /// <param name="topicId">待设置的专题编号</param>
        /// <param name="platformType">平台类型</param>
        /// <param name="shopId">店铺Id</param>
        /// <param name="frontCoverImage">封面图片</param>
        void AddMobileHomeTopic(long topicId, long shopId, PlatformType platformType, string frontCoverImage = null);

        /// <summary>
        /// 设置显示顺序
        /// </summary>
        /// <param name="id">移动端首页专题设置Id</param>
        /// <param name="sequence">顺序号</param>
        /// <param name="shopId">店铺Id</param>
        void SetSequence(long id, int sequence, long shopId = 0);

        /// <summary>
        /// 删除指定移动端首页专题设置
        /// </summary>
        /// <param name="id">移动端首页专题设置Id</param>
        void Delete(long id);

    }
}
