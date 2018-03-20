using Himall.Core;
using Himall.Core.Plugins.Message;
using Himall.Entity;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Himall.CommonModel;

namespace Himall.Service
{
    public class AppMessageService : ServiceBase, IAppMessageService
    {
        /// <summary>
        /// 商家未读消息数（30天内）
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public int GetShopNoReadMessageCount(long shopId)
        {
            var starttime = DateTime.Now.AddDays(-30).Date;
            return Context.AppMessagesInfo.Where(d => d.ShopId == shopId && d.IsRead == false && d.sendtime>= starttime).Count();
        }
        /// <summary>
        /// 门店未读消息数（30天内）
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public int GetBranchNoReadMessageCount(long shopBranchId)
        {
            var starttime = DateTime.Now.AddDays(-30).Date;
            return Context.AppMessagesInfo.Where(d => d.ShopBranchId == shopBranchId && d.IsRead == false && d.sendtime >= starttime).Count();
        }

        /// <summary>
        /// 获取消息列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public QueryPageModel<AppMessagesInfo> GetMessages(AppMessageQuery query)
        {
            var sql = Context.AppMessagesInfo.AsQueryable();
            if (query.ShopId.HasValue)
            {
                sql = sql.Where(d => d.ShopId == query.ShopId.Value);
            }
            if (query.ShopBranchId.HasValue)
            {
                sql = sql.Where(d => d.ShopBranchId == query.ShopBranchId.Value);
            }
            if (query.StartTime.HasValue)
            {
                sql = sql.Where(d => d.sendtime >= query.StartTime.Value);
            }
            if (query.EndTime.HasValue)
            {
                sql = sql.Where(d => d.sendtime <= query.EndTime.Value);
            }
            if (query.IsRead.HasValue)
            {
                sql = sql.Where(d=> d.IsRead== query.IsRead.Value);
            }

            QueryPageModel<AppMessagesInfo> result = new QueryPageModel<AppMessagesInfo>();
            int total = 0;
            result.Models = sql.GetPage(out total,query.PageNo,query.PageSize,(o=>o.OrderByDescending(d=>d.sendtime))).ToList();
            result.Total = total;
            return result;
        }
        /// <summary>
        /// 消息状态改己读
        /// </summary>
        /// <param name="id"></param>
        public void ReadMessage(long id)
        {
            var msg = Context.AppMessagesInfo.FirstOrDefault(d=>d.Id== id);
            if (msg != null)
            {
                msg.IsRead = true;
                Context.SaveChanges();
            }
        }
        /// <summary>
        /// 新增门店App消息
        /// </summary>
        /// <param name="appMessagesInfo"></param>
        public void AddAppMessages(AppMessagesInfo appMessagesInfo)
        {
            Context.AppMessagesInfo.Add(appMessagesInfo);
            Context.SaveChanges();
        }
    }
}
