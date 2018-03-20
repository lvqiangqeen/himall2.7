using Himall.Entity;
using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;

namespace Himall.Service
{
    public class SiteSettingService : ServiceBase, ISiteSettingService
    {
        public SiteSettingsInfo GetSiteSettings()
        {
            SiteSettingsInfo siteSettingsInfo=null;

            if (Core.Cache.Exists(CacheKeyCollection.SiteSettings))//如果存在缓存，则从缓存中读取
                siteSettingsInfo = Core.Cache.Get<SiteSettingsInfo>(CacheKeyCollection.SiteSettings);

			if (siteSettingsInfo == null)
			{
				//否则从数据库中读取，并将配置存入至缓存

				//通过反射获取值
				var values = Context.SiteSettingsInfo.FindAll().ToArray();
				siteSettingsInfo = new SiteSettingsInfo();

				var properties = siteSettingsInfo.GetType().GetProperties();
				foreach (var property in properties)
				{
					if (property.Name != "Id")
					{
						var temp = values.FirstOrDefault(item => item.Key == property.Name);
						if (temp != null)
							property.SetValue(siteSettingsInfo, Convert.ChangeType(temp.Value, property.PropertyType));
					}
				}

				Core.Cache.Insert(CacheKeyCollection.SiteSettings, siteSettingsInfo);
			}

            return siteSettingsInfo;
        }
        public SiteSettingsInfo GetSiteSettingsByObjectCache()
        {
            SiteSettingsInfo siteSettingsInfo = null;
            ObjectCache cache = MemoryCache.Default;
                if (cache.Contains(CacheKeyCollection.SiteSettings))//如果存在缓存，则从缓存中读取
                    siteSettingsInfo = cache.Get(CacheKeyCollection.SiteSettings) as SiteSettingsInfo;

            if (siteSettingsInfo == null)
            {
                //否则从数据库中读取，并将配置存入至缓存

                //通过反射获取值
                var values = Context.SiteSettingsInfo.FindAll().ToArray();
                siteSettingsInfo = new SiteSettingsInfo();

                var properties = siteSettingsInfo.GetType().GetProperties();
                foreach (var property in properties)
                {
                    if (property.Name != "Id")
                    {
                        var temp = values.FirstOrDefault(item => item.Key == property.Name);
                        if (temp != null)
                            property.SetValue(siteSettingsInfo, Convert.ChangeType(temp.Value, property.PropertyType));
                    }
                }

               // Core.Cache.Insert(CacheKeyCollection.SiteSettings, siteSettingsInfo);
                var policy = new CacheItemPolicy() { AbsoluteExpiration = DateTime.Now.AddSeconds(300) };
                cache.Add(CacheKeyCollection.SiteSettings, siteSettingsInfo, policy);
            }

            return siteSettingsInfo;
        }

        public void SetSiteSettings(SiteSettingsInfo siteSettingsInfo)
        {
            PropertyInfo[] properties = siteSettingsInfo.GetType().GetProperties();
            SiteSettingsInfo temp;
            string value;
            object obj;
            IEnumerable<SiteSettingsInfo> siteSettingInfos = Context.SiteSettingsInfo.FindAll().ToArray();
            foreach (var property in properties)
            {
                obj = property.GetValue(siteSettingsInfo);
                if (obj != null)
                    value = obj.ToString();
                else
                    value = "";
                if (property.Name != "Id")
                {
                    temp = siteSettingInfos.FirstOrDefault(item => item.Key == property.Name);
                    if (temp == null)//数据库中不存在，则创建
                        Context.SiteSettingsInfo.Add(new SiteSettingsInfo() { Key = property.Name, Value = value });
                    else//存在则更新
                        temp.Value = value;
                }
            }

            //删除不存在的项
            var propertyNames = properties.Select(item => item.Name);
            Context.SiteSettingsInfo.RemoveRange(siteSettingInfos.Where(item => !propertyNames.Contains(item.Key)));
            Context.SaveChanges();
            Core.Cache.Remove(CacheKeyCollection.SiteSettings);
        }

        public void SaveSetting(string key, object value)
        {
            if (value == null)
                throw new ArgumentNullException("值不能为null");

            //检查Key是否存在
            PropertyInfo[] properties = typeof(SiteSettingsInfo).GetProperties();
            if (properties.FirstOrDefault(item => item.Name == key) == null)
                throw new ApplicationException("未找到" + key + "对应的配置项");

            var siteSetting = Context.SiteSettingsInfo.FindBy(item => item.Key == key).FirstOrDefault() ;
            if (siteSetting == null)
            {
                siteSetting = new SiteSettingsInfo();
                Context.SiteSettingsInfo.Add(siteSetting);
            }
            siteSetting.Key = key;
            siteSetting.Value = value.ToString();
            Context.SaveChanges();
            Core.Cache.Remove(CacheKeyCollection.SiteSettings);
        }


    }
}
