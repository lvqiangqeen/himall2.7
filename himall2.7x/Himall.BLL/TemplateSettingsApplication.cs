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
        private static ITemplateSettingsService _iMemberService = ObjectContainer.Current.Resolve<ITemplateSettingsService>();
        /// <summary>
        /// 设置当前的可视化版，如果是平台，shopId可以设置为0
        /// </summary>
        /// <param name="tName"></param>
        /// <param name="shopId"></param>
        public static void SetCurrentTemplate(string tName, long shopId = 0)
        {
            _iMemberService.SetCurrentTemplate(tName, shopId);
        }

        public static string GetGoodTagFromCache(long page)
        {
            return _iMemberService.GetGoodTagFromCache(page);
        }
    }
}
