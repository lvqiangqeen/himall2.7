using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.IServices
{
    public interface IManagersGrade : IService
    {
        /// </summary>
        /// 
        /// <param name="model"></param>
        void AddManagersGrade(ManagersGrade model);

        void UpdateManagersGrade(ManagersGrade model);


        void DeleteManagersGrade(long id);

        ManagersGrade GetManagersGrade(long id);

        IEnumerable<ManagersGrade> GetManagersGradeList();
        /// <summary>
        /// 通过用户编号获取用户等级编号
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        long GetManagersGradeByUserId(long userId);

    }
}
