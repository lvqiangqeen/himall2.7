using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.IServices
{
    public interface ITemplateSettingsService:IService
    {
        /// <summary>
        /// 设置当前的可视化版，如果是平台，shopId可以设置为0
        /// </summary>
        /// <param name="tName"></param>
        /// <param name="shopId"></param>
        void SetCurrentTemplate(string tName, long shopId = 0);

        /// <summary>
        /// 获取当前的可视化模板
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        TemplateVisualizationSettingsInfo GetCurrentTemplate(long shopId=0);

        /// <summary>
        /// 从缓存取首页商品Tag
        /// </summary>
        /// <param name="page">页码</param>
        /// <returns></returns>
        string GetGoodTagFromCache(long page, string tName="");
        /// <summary>
        /// 从缓存取店铺首页商品Tag
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="page"></param>
        /// <param name="tName"></param>
        /// <returns></returns>
        string GetShopGoodTagFromCache(long shopId, long page,string tName="");
    }
}
