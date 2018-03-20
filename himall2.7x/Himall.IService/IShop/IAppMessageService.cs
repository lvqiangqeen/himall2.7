using Himall.IServices.QueryModel;
using Himall.Model;
using System.Linq;
using System;
using System.Collections.Generic;
using Himall.CommonModel;

namespace Himall.IServices
{
    public interface IAppMessageService : IService
    {
        /// <summary>
        /// 商家未读消息数（30天内）
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        int GetShopNoReadMessageCount(long shopId);
        /// <summary>
        /// 门店未读消息数（30天内）
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        int GetBranchNoReadMessageCount(long shopBranchId);
        /// <summary>
        /// 获取消息列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        QueryPageModel<AppMessagesInfo> GetMessages(AppMessageQuery query);
        /// <summary>
        /// 消息状态改己读
        /// </summary>
        /// <param name="id"></param>
        void ReadMessage(long id);
        /// <summary>
        /// 新增App消息
        /// </summary>
        /// <param name="appMessagesInfo"></param>
        void AddAppMessages(AppMessagesInfo appMessagesInfo);
    }
}
