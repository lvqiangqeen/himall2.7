using Himall.Model;

namespace Himall.IServices
{
    /// <summary>
    /// 站点设置服务
    /// </summary>
    public interface ISiteSettingService:IService
    {
        /// <summary>
        /// 获取系统配置信息
        /// </summary>
        /// <returns></returns>
        SiteSettingsInfo GetSiteSettings();
        /// <summary>
        /// 获取系统配置信息  使用asp.net cache  windows服务使用
        /// </summary>
        /// <returns></returns>
        SiteSettingsInfo GetSiteSettingsByObjectCache();
        /// <summary>
        /// 保存系统配置信息
        /// </summary>
        /// <param name="siteSettingsInfo">待保存的系统配置（该配置必须是完整的配置）</param>
        void SetSiteSettings(SiteSettingsInfo siteSettingsInfo);

        /// <summary>
        /// 保存单个配置项
        /// </summary>
        /// <param name="key">配置项的Key（大小写敏感）</param>
        /// <param name="value">值</param>
        void SaveSetting(string key, object value);

    }
}
