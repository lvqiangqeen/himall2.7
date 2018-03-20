using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Himall.Entity;
using Himall.Core;
using Himall.Model;
using Himall.IServices;
using Himall.Service;
using Himall.Web.Framework;
using System.Threading;
using System.Configuration;

namespace Himall.Web.App_Code.Common
{
    /// <summary>
    /// 加载缓存处理类
    /// </summary>
    public class InitCacheHelper
    {
        /// <summary>
        /// 初始化缓存
        /// </summary>
        public static void InitCache() {
            //加载移动端当前首页模版
            var curr = ServiceHelper.Create<ITemplateSettingsService>().GetCurrentTemplate(0);
            Core.Cache.Insert<Himall.Model.TemplateVisualizationSettingsInfo>(CacheKeyCollection.MobileHomeTemplate("0"), curr);
        }
    }
}