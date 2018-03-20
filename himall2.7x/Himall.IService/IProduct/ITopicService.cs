
using Himall.Core;
using Himall.IServices.QueryModel;
using Himall.Model;
namespace Himall.IServices
{
    /// <summary>
    /// 商品专题服务接口
    /// </summary>
    public interface ITopicService:IService
    {
        /// <summary>
        /// 获取所有专题
        /// </summary>
        /// <param name="pageNo">页号</param>
        /// <param name="pageSize">每页行数</param>
        /// <param name="platformType">平台类型</param>
        /// <returns></returns>
        ObsoletePageModel<TopicInfo> GetTopics(int pageNo, int pageSize, PlatformType platformType = PlatformType.PC);

        /// <summary>
        /// 根据条件获取专题
        /// </summary>
        /// <param name="topicQuery">条件</param>
        /// <returns></returns>
        ObsoletePageModel<TopicInfo> GetTopics(TopicQuery topicQuery);

        /// <summary>
        /// 删除专题
        /// </summary>
        /// <param name="id">专题编号</param>
        void DeleteTopic(long id);


        /// <summary>
        /// 新增商品专题
        /// </summary>
        /// <param name="topicInfo">商品专题实体</param>
        void AddTopic(TopicInfo topicInfo);

        /// <summary>
        /// 更新商品专题
        /// </summary>
        /// <param name="topicInfo">商品专题实体</param>
        void UpdateTopic(TopicInfo topicInfo);

        /// <summary>
        /// 获取商品专题
        /// </summary>
        /// <param name="id">商品专题编号</param>
        /// <returns></returns>
        TopicInfo GetTopicInfo(long id);
    }
}
