using Himall.IServices;
using Himall.CommonModel;
using Himall.Model;
using Himall.Core;

namespace Himall.Application
{
    public class WeixinAutoReplyApplication
    {
        private static IAutoReplyService _iAutoReplyService = ObjectContainer.Current.Resolve<IAutoReplyService>();
     
        /// <summary>
        /// 添加/修改规则
        /// </summary>
        /// <param name="autoReplyInfo"></param>
        /// <returns></returns>
        public static void ModAutoReply(AutoReplyInfo autoReplyInfo) {
            _iAutoReplyService.ModAutoReply(autoReplyInfo);
        }
        /// <summary>
        /// 删除规则
        /// </summary>
        /// <param name="autoReplyInfo"></param>
        /// <returns></returns>
        public static void DeleteAutoReply(AutoReplyInfo autoReplyInfo) {
            _iAutoReplyService.DeleteAutoReply(autoReplyInfo);
        }
        /// <summary>
        /// 根据关键词和消息类型获取自动回复信息
        /// </summary>
        /// <param name="keyWord">关键字</param>
        /// <param name="type">消息类型</param>
        /// <returns></returns>
        public static AutoReplyInfo GetAutoReplyByKey(ReplyType type, string keyWord = "", bool isList = true, bool isReply = false, bool isFollow = false)
        {
            return _iAutoReplyService.GetAutoReplyByKey(type, keyWord, isList, isReply, isFollow);
        }
        public static AutoReplyInfo GetAutoReplyById(int Id) {
            return _iAutoReplyService.GetAutoReplyById(Id);
        }
        /// <summary>
        /// 获取关键字回复列表
        /// </summary>
        /// <param name="type"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static PageModel<AutoReplyInfo> GetPage(int pageIndex, int pageSize, ReplyType type = ReplyType.Keyword)
        {
            return _iAutoReplyService.GetPage(pageIndex, pageSize, type);
        }
    }
}
