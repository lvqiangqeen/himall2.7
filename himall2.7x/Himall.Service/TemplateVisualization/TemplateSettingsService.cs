using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Service
{
    public class TemplateSettingsService : ServiceBase, ITemplateSettingsService
    {
        public void SetCurrentTemplate(string tName, long shopId = 0)
        {
            TemplateVisualizationSettingsInfo curr = Context.TemplateVisualizationSettingsInfo.FirstOrDefault(t => t.ShopId.Equals(shopId));
            if (null == curr)
            {
                curr = new TemplateVisualizationSettingsInfo()
                {
                    CurrentTemplateName = tName,
                    ShopId = shopId
                };
                Context.TemplateVisualizationSettingsInfo.Add(curr);
            }
            else
            {
                curr.CurrentTemplateName = tName;
            }
            Context.SaveChanges();
        }

        public TemplateVisualizationSettingsInfo GetCurrentTemplate(long shopId = 0)
        {
            var curr = Context.TemplateVisualizationSettingsInfo.FirstOrDefault(t => t.ShopId.Equals(shopId));
            return curr;
        }

        public string GetGoodTagFromCache(long page, string tName="")
        {
            string crrentTemplateName = "t1";
            string html = string.Empty;
            if (string.IsNullOrWhiteSpace(tName))
            {//从缓存获取当前模板数据
                var curr = new Himall.Model.TemplateVisualizationSettingsInfo();
                if (Core.Cache.Exists(CacheKeyCollection.MobileHomeTemplate("0")))//如果存在缓存，则从缓存中读取
                {
                    curr = Core.Cache.Get<Himall.Model.TemplateVisualizationSettingsInfo>(CacheKeyCollection.MobileHomeTemplate("0"));
                }
                else
                {
                    curr = Context.TemplateVisualizationSettingsInfo.FirstOrDefault(t => t.ShopId.Equals(0));
                    Core.Cache.Insert<Himall.Model.TemplateVisualizationSettingsInfo>(CacheKeyCollection.MobileHomeTemplate("0"), curr);
                }
                if (curr != null)
                    crrentTemplateName = curr.CurrentTemplateName;
            }
            else
            {
                crrentTemplateName = tName;
            }
            if (Core.Cache.Exists(CacheKeyCollection.MobileHomeProductInfo(crrentTemplateName, page)))//如果存在缓存，则从缓存中读取
            {
                html = Core.Cache.Get(CacheKeyCollection.MobileHomeProductInfo(crrentTemplateName, page)).ToString();
            }
            return html;
        }

        public string GetShopGoodTagFromCache(long shopId, long page,string tName="")
        {
            string crrentTemplateName = "t1";
            string html = string.Empty;
            var curr = new Himall.Model.TemplateVisualizationSettingsInfo();
            if (Core.Cache.Exists(CacheKeyCollection.MobileHomeTemplate(shopId.ToString())))//如果存在缓存，则从缓存中读取
            {
                curr = Core.Cache.Get<Himall.Model.TemplateVisualizationSettingsInfo>(CacheKeyCollection.MobileHomeTemplate(shopId.ToString()));
            }
            else
            {
                curr = Context.TemplateVisualizationSettingsInfo.FirstOrDefault(t => t.ShopId.Equals(shopId));
                Core.Cache.Insert<Himall.Model.TemplateVisualizationSettingsInfo>(CacheKeyCollection.MobileHomeTemplate(shopId.ToString()), curr);
            }
            if (curr != null)
            {
                crrentTemplateName = curr.CurrentTemplateName;
            }
            if (!string.IsNullOrWhiteSpace(tName))
            {
                crrentTemplateName = tName;
            }

            if (Core.Cache.Exists(CacheKeyCollection.MobileShopHomeProductInfo(shopId.ToString(),crrentTemplateName, page)))//如果存在缓存，则从缓存中读取
            {
                html = Core.Cache.Get(CacheKeyCollection.MobileShopHomeProductInfo(shopId.ToString(), crrentTemplateName, page)).ToString();
            }
            return html;
        }
    
    }
}
