using Himall.Core;
using Himall.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Application
{
    public class TemplateSettingsApplication
    {
        private static ITemplateSettingsService _iTemplateSettingsService = ObjectContainer.Current.Resolve<ITemplateSettingsService>();
        /// <summary>
        /// 设置当前的可视化版，如果是平台，shopId可以设置为0
        /// </summary>
        /// <param name="tName"></param>
        /// <param name="shopId"></param>
        public static void SetCurrentTemplate(string tName, long shopId = 0)
        {
            _iTemplateSettingsService.SetCurrentTemplate(tName, shopId);
        }

        public static string GetGoodTagFromCache(long page, string tName="")
        {
            return _iTemplateSettingsService.GetGoodTagFromCache(page, tName);
        }
        /// <summary>
        /// 取店铺首页缓存数据
        /// </summary>
        /// <param name="shopid">店铺ID（非微店）</param>
        /// <param name="page"></param>
        /// <returns></returns>
        public static string GetShopGoodTagFromCache(long shopid, long page,string tName="")
        {
            return _iTemplateSettingsService.GetShopGoodTagFromCache(shopid, page,tName);
        }

        public static Model.TemplateVisualizationSettingsInfo GetCurrentTemplate(long shopid)
        {
            return _iTemplateSettingsService.GetCurrentTemplate(shopid);
        }
    }
}
