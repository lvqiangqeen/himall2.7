using Himall.IServices;
using Himall.Model;
using Himall.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Service
{
    public class OperationLogService : ServiceBase, IOperationLogService
    {
        public Model.ObsoletePageModel<Model.LogInfo> GetPlatformOperationLogs(IServices.QueryModel.OperationLogQuery query)
        {
            int total = 0;
            IQueryable<LogInfo> logs = Context.LogInfo.FindBy(item => item.ShopId == query.ShopId);
            if(!string.IsNullOrWhiteSpace(query.UserName)){
                logs=logs.Where(item=>query.UserName == item.UserName);
            }
            if(query.StartDate.HasValue){
                logs=logs.Where(item=>item.Date >= query.StartDate.Value);
            }
            if(query.EndDate.HasValue){
                var end = query.EndDate.Value.AddDays(1);
                logs=logs.Where(item=>item.Date <=end);
            }
            logs = logs.GetPage(out total, query.PageNo, query.PageSize);
            ObsoletePageModel<LogInfo> pageModel = new ObsoletePageModel<LogInfo>() { Models = logs, Total = total };
            return pageModel;
        }

        public void AddPlatformOperationLog(Model.LogInfo model)
        {
            model.ShopId = 0;
            Context.LogInfo.Add(model);
            Context.SaveChanges();
        }

        public void AddSellerOperationLog(Model.LogInfo model)
        {
            if (model.ShopId != 0)
            {
                model.ShopId = model.ShopId;
                Context.LogInfo.Add(model);
                Context.SaveChanges();
            }
            else
            {
                throw new Himall.Core.HimallException("日志获取店铺ID错误");
            }
        }


        public void DeletePlatformOperationLog(long id)
        {
            throw new NotImplementedException();
        }
    }
}
