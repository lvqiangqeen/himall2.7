using System;
using System.Collections.Generic;
using Himall.IServices;
using AutoMapper;
using Himall.Core;
using Himall.DTO;
using Himall.Core.Plugins.Message;
using Himall.Model;
using Himall.IServices.QueryModel;
using Himall.CommonModel;

namespace Himall.Application
{
    public class AppMessageApplication
    {
        private static IAppMessageService _appMessageService = ObjectContainer.Current.Resolve<IAppMessageService>();

        /// <summary>
        /// 未读消息数（30天内）
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static int GetShopNoReadMessageCount(long shopId)
        {
            return _appMessageService.GetShopNoReadMessageCount(shopId);
        }
        /// <summary>
        /// 未读消息数（30天内）
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <returns></returns>
        public static int GetBranchNoReadMessageCount(long shopBranchId)
        {
            return _appMessageService.GetBranchNoReadMessageCount(shopBranchId);
        }

        /// <summary>
        /// 获取消息列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<AppMessagesInfo> GetMessages(AppMessageQuery query)
        {
            return _appMessageService.GetMessages(query);
        }
        /// <summary>
        /// 消息状态改己读
        /// </summary>
        /// <param name="id"></param>
        public static void ReadMessage(long id)
        {
            _appMessageService.ReadMessage(id);
        }
        /// <summary>
        /// 新增消息
        /// </summary>
        /// <param name="appMessagesInfo"></param>
        public static void AddAppMessages(AppMessages appMessages)
        {
            AutoMapper.Mapper.CreateMap<AppMessages, AppMessagesInfo>();
            var appMessagesInfo = AutoMapper.Mapper.Map<AppMessages, AppMessagesInfo>(appMessages);
            _appMessageService.AddAppMessages(appMessagesInfo);
        }
    }
}
