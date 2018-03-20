
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Himall.Core.Plugins.OAuth
{
    /// <summary>
    /// 信任登录插件接口
    /// </summary>
    public interface IOAuthPlugin:IPlugin
    {
        /// <summary>
        /// 用于显示在界面上的短名称 
        /// </summary>
        string ShortName { get;}

        /// <summary>
        /// 获取表单数据
        /// </summary>
        FormData GetFormData();

        /// <summary>
        /// 设置表单数据
        /// </summary>
        /// <param name="values">表单数据键值对集合，键为表单项的name,值为用户填写的值</param>
        void SetFormValues(IEnumerable<KeyValuePair<string, string>> values);

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="returnParams">用户登录后返回的参数</param>
        /// <returns></returns>
        OAuthUserInfo GetUserInfo(NameValueCollection queryString);

        /// <summary>
        /// 登录链接地址
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        string GetOpenLoginUrl(string returnUrl);

        /// <summary>
        /// 登录图标（鼠标悬停图片）(16x16)
        /// </summary>
        string Icon_Hover { get; }

        /// <summary>
        /// 登录图标（默认样式图片）
        /// </summary>
        string Icon_Default { get; }


        /// <summary>
        /// 获取需要放置在首页Head中的验证内容
        /// </summary>
        /// <returns></returns>
        string GetValidateContent();

    }
}
