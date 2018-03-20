using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Core;
using Himall.IServices;

namespace Himall.Application
{
    public class SettledApplication
    {
        private static ISettledService _iSettledService = ObjectContainer.Current.Resolve<ISettledService>();
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="mSettledInfo"></param>
        public static void AddSettled(Himall.Model.SettledInfo mSettledInfo)
        {
            _iSettledService.AddSettled(mSettledInfo);
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="mSettledInfo"></param>
        public static void UpdateSettled(Himall.Model.SettledInfo mSettledInfo)
        {
            _iSettledService.UpdateSettled(mSettledInfo);
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <returns></returns>
      public static   Himall.Model.SettledInfo GetSettled()
        {
            return _iSettledService.GetSettled();
        }
    }
}
