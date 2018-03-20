using Himall.Core;
using Himall.Entity;
using Himall.IServices;
using Himall.Model;
using System.Linq;

namespace Himall.Service
{
    public class MobileHomeTopicService : ServiceBase, IMobileHomeTopicService
    {
        /// <summary>
        /// 最大专题数
        /// </summary>
        const int MAX_HOMETOPIC_COUNT = 10;

        public IQueryable<MobileHomeTopicsInfo> GetMobileHomeTopicInfos(PlatformType platformType, long shopId = 0)
        {
            return Context.MobileHomeTopicsInfo.Where(item => item.ShopId == shopId && item.Platform == platformType);
        }

        public MobileHomeTopicsInfo GetMobileHomeTopic(long id, long shopId = 0)
        {
            return Context.MobileHomeTopicsInfo.FirstOrDefault(item => item.Id == id && item.ShopId == shopId);
        }

        public void AddMobileHomeTopic(long topicId, long shopId, PlatformType platformType, string frontCoverImage = null)
        {
            var existTopicInfo = Context.MobileHomeTopicsInfo.Count(item => item.TopicId == topicId && item.ShopId == shopId && item.Platform == platformType);
            if (existTopicInfo > 0)
                throw new Himall.Core.HimallException("已经添加过相同的专题");

            var allCount = Context.MobileHomeTopicsInfo.Count(item =>item.ShopId == shopId && item.Platform == platformType);
            if (allCount >= MAX_HOMETOPIC_COUNT)
                throw new Himall.Core.HimallException(string.Format("最多只能添加{0}个专题", MAX_HOMETOPIC_COUNT));

            var mobileHomeTopicInfo = new MobileHomeTopicsInfo()
            {
                Platform = platformType,
                ShopId = shopId,
                TopicId = topicId,
            };
            Context.MobileHomeTopicsInfo.Add(mobileHomeTopicInfo);
            Context.SaveChanges();
        }

        public void SetSequence(long id, int sequence, long shopId = 0)
        {
            var mobileHomeTopicInfo = GetMobileHomeTopic(id, shopId);
            mobileHomeTopicInfo.Sequence = sequence;
            Context.SaveChanges();
        }

        public void Delete(long id)
        {
            Context.MobileHomeTopicsInfo.Remove(id);
            Context.SaveChanges();
        }




    }
}
