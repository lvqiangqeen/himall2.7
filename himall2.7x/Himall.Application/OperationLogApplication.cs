using Himall.Core;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
namespace Himall.Application
{
    public class OperationLogApplication
    {
        private static IOperationLogService _iOperationLogService = ObjectContainer.Current.Resolve<IOperationLogService>();
        /// <summary>
        /// 根据查询条件分页获取操作日志信息
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static ObsoletePageModel<LogInfo> GetPlatformOperationLogs(OperationLogQuery query)
        {
            return _iOperationLogService.GetPlatformOperationLogs(query);
        }

        /// <summary>
        /// 增加平台日志信息
        /// </summary>
        /// <param name="info"></param>
        public static void AddPlatformOperationLog(LogInfo info)
        {
             _iOperationLogService.AddPlatformOperationLog(info);
        }
        /// <summary>
        /// 增加平台日志信息
        /// </summary>
        /// <param name="info"></param>
        public static void AddSellerOperationLog(LogInfo info)
        {
            _iOperationLogService.AddSellerOperationLog(info);
        }

        /// <summary>
        ///根据ID删除平台日志信息
        /// </summary>
        /// <param name="id"></param>
        public static void DeletePlatformOperationLog(long id)
        {
            _iOperationLogService.DeletePlatformOperationLog(id);
        }
    }
}
