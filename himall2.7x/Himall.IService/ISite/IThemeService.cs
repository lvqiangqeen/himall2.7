using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/**
 * 商城主题设置
 * 2016-05-16
 * **/
namespace Himall.IServices
{
    /// <summary>
    /// 商城主题设置
    /// </summary>
    public interface IThemeService : IService
    {
        /// <summary>
        /// 商城主题设置添加
        /// </summary>
        /// <param name="mTheme">主题实体类</param>
        void AddTheme(Himall.Model.ThemeInfo mTheme);

        /// <summary>
        ///  商城主题设置修改
        /// </summary>
        /// <param name="mTheme">主题实体类</param>
        void UpdateTheme(Himall.Model.ThemeInfo mTheme);

        /// <summary>
        /// 获取商城主题
        /// </summary>
        /// <returns></returns>
        Himall.Model.ThemeInfo getTheme();
    }
}
