using Himall.CommonModel;
using Himall.Model;
using Himall.Model.Models;
using Senparc.Weixin.MP.AdvancedAPIs.Poi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.IServices
{
    public interface IAutoReplyService : IService 
    {
        /// <summary>
        /// 添加/修改规则
        /// </summary>
        /// <param name="autoReplyInfo"></param>
        /// <returns></returns>
        void ModAutoReply(AutoReplyInfo autoReplyInfo);
        /// <summary>
        /// 删除规则
        /// </summary>
        /// <param name="autoReplyInfo"></param>
        /// <returns></returns>
        void DeleteAutoReply(AutoReplyInfo autoReplyInfo);
        /// <summary>
        /// 根据关键词和消息类型获取自动回复信息
        /// </summary>
        /// <param name="keyWord">关键字</param>
        /// <param name="type">消息类型</param>
        /// <returns></returns>
        AutoReplyInfo GetAutoReplyByKey(ReplyType type, string keyWord = "", bool isList = true, bool isReply = false, bool isFollow = false);
        AutoReplyInfo GetAutoReplyById(int Id);
        /// <summary>
        /// 获取关键字回复列表
        /// </summary>
        /// <param name="type"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        ObsoletePageModel<AutoReplyInfo> GetPage(int pageIndex, int pageSize, ReplyType type);
    }
}
