using Himall.Core.Plugins;
using Himall.Core.Plugins.Express;
using Himall.Model;
using System.Collections.Generic;
using System.Linq;

namespace Himall.IServices
{
    /// <summary>
    /// 快递服务接口
    /// </summary>
    public interface IExpressService : IService
    {
        /// <summary>
        /// 获取所有快递信息
        /// </summary>
        /// <returns></returns>
        IEnumerable<IExpress> GetAllExpress();

        /// <summary>
        /// 订阅快递100的物流信息
        /// </summary>
        /// <param name="companyCode"></param>
        /// <param name="number"></param>

        void SubscribeExpress100(string expressCompanyName, string number, string kuaidi100Key, string city, string redirectUrl);

        /// <summary>
        /// 保存快递信息
        /// </summary>
        /// <param name="model"></param>

        void SaveExpressData(OrderExpressInfo model);

        /// <summary>
        /// 根据名称查找快递单模板信息
        /// </summary>
        /// <param name="name">快递名称</param>
        /// <returns></returns>
        IExpress GetExpress(string name);

        /// <summary>
        /// 修改快递单模板打印元素信息
        /// </summary>
        /// <param name="elements">待修改快递单模板打印元素信息</param>
        /// <param name="name">快递名称</param>
        void UpdatePrintElement(string name, IEnumerable<ExpressPrintElement> elements);

        /// <summary>
        /// 获取店铺最近使用的快递信息
        /// </summary>
        /// <param name="shopId">店铺id</param>
        /// <param name="takeNumber">取最近的个数</param>
        /// <returns></returns>
        IEnumerable<IExpress> GetRecentExpress(long shopId, int takeNumber);


        /// <summary>
        /// 根据打印元素序号获取对应订单中的实际内容
        /// </summary>
        /// <param name="shopId">店铺id</param>
        /// <param name="orderId">订单id</param>
        /// <param name="printElementIndexes">打印元素序号集</param>
        /// <returns></returns>
        IDictionary<int, string> GetPrintElementIndexAndOrderValue(long shopId, long orderId, IEnumerable<int> printElementIndexes);

        /// <summary>
        /// 获取快递物流信息
        /// </summary>
        /// <param name="expressCompanyName">快递公司名称</param>
        /// <param name="shipOrderNumber">快递单号</param>
        /// <returns></returns>
        ExpressData GetExpressData(string expressCompanyName, string shipOrderNumber);
    }
}
