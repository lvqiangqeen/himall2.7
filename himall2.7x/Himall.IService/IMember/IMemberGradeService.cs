using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.IServices
{
    public interface IMemberGradeService : IService
    {
        /// </summary>
        /// 
        /// <param name="model"></param>
        void AddMemberGrade(MemberGrade model);

        void UpdateMemberGrade(MemberGrade model);


        void DeleteMemberGrade(long id);

        MemberGrade GetMemberGrade(long id);

        IEnumerable<MemberGrade> GetMemberGradeList();
        /// <summary>
        /// 通过用户编号获取用户等级编号
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        long GetMemberGradeByUserId(long userId);
        /// <summary>
        /// 前一个等级是否大于后一个等级
        /// </summary>
        /// <param name="oneId"></param>
        /// <param name="twoId"></param>
        /// <returns></returns>
        bool IsOneGreaterOrEqualTwo(long oneId, long twoId);
    }
}
