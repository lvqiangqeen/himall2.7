using Himall.Core;
using Himall.Entity;
using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Himall.Service.Site
{
    /// <summary>
    /// 商城主题设置
    /// </summary>
    public class ThemeService : ServiceBase, IThemeService
    {
        /// <summary>
        /// 主题设置添加
        /// </summary>
        /// <param name="mTheme">主题设置实体</param>
        public void AddTheme(Himall.Model.ThemeInfo mTheme)
        {
            string imgUrl = string.Empty;
            var data = Context.ThemeInfo.ToList();
            if (data.Count == 0)
            {
                Context.ThemeInfo.Add(mTheme);
                Context.SaveChanges();
                Core.Cache.Remove(CacheKeyCollection.Themes);//清除缓存
            }
        }

        /// <summary>
        /// 主题设置数据更改
        /// </summary>
        /// <param name="mTheme">主题设置实体</param>
        public void UpdateTheme(Himall.Model.ThemeInfo mTheme)
        {
            var model = Context.ThemeInfo.FirstOrDefault(s => s.ThemeId == mTheme.ThemeId);
            model.TypeId = mTheme.TypeId;
            model.WritingColor = mTheme.WritingColor;
            model.SecondaryColor = mTheme.SecondaryColor;
            model.MainColor = mTheme.MainColor;
            model.FrameColor = mTheme.FrameColor;
            model.ClassifiedsColor = mTheme.ClassifiedsColor;
            Context.SaveChanges();
            Core.Cache.Remove(CacheKeyCollection.Themes);//清除缓存
        }

        /// <summary>
        /// 获取主题设置实体集合
        /// </summary>
        /// <returns></returns>
        public Himall.Model.ThemeInfo getTheme()
        {
            Himall.Model.ThemeInfo MThemeInfo;
            if (Core.Cache.Exists(CacheKeyCollection.Themes))//如果存在缓存，则从缓存中读取
                MThemeInfo = Core.Cache.Get<Himall.Model.ThemeInfo>(CacheKeyCollection.Themes);
            else
            {
                MThemeInfo = Context.ThemeInfo.FirstOrDefault();
                Core.Cache.Insert(CacheKeyCollection.Themes, MThemeInfo);
            }
            return MThemeInfo;
        }
    }
}
