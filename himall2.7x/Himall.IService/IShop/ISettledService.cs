using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.IServices
{
    /// <summary>
    /// 商家入驻设置
    /// </summary>
    public interface ISettledService:IService
    {
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="mSettledInfo"></param>
        void AddSettled(Himall.Model.SettledInfo mSettledInfo);

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="mSettledInfo"></param>
        void UpdateSettled(Himall.Model.SettledInfo mSettledInfo);

        /// <summary>
        /// 查询
        /// </summary>
        /// <returns></returns>
        Himall.Model.SettledInfo GetSettled();

    }
}
