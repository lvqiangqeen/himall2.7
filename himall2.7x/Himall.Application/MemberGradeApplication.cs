using System;
using System.Collections.Generic;
using System.Linq;
using Himall.IServices;
using Himall.DTO;
using Himall.CommonModel;
using Himall.Model;
using AutoMapper;
using Himall.Core;
namespace Himall.Application
{
    public class MemberGradeApplication
    {
        /// <summary>
        /// 默认会员等级名称
        /// </summary>
        const string DEFAULT_GRADE_NAME = "vip0";

        private static IMemberGradeService _iMemberGradeService = ObjectContainer.Current.Resolve<IMemberGradeService>();


        /// <summary>
        /// 获取等级列表 
        /// </summary>
        /// <returns></returns>
        public static List<Himall.DTO.MemberGrade> GetMemberGradeList()
        {
            var list = _iMemberGradeService.GetMemberGradeList().ToList();
            var m = Mapper.Map<List<Himall.Model.MemberGrade>, List<Himall.DTO.MemberGrade>>(list);
            return m;
        }


        /// <summary>
        /// 根据会员积分获取会员等级，获取单个会员的会员等级（循环调用时禁用)
        /// </summary>
        /// <param name="integral"></param>
        /// <returns></returns>
        public static DTO.MemberGrade GetMemberGradeByUserIntegral(int integral)
        {
            List<Himall.DTO.MemberGrade> memberGrade = MemberGradeApplication.GetMemberGradeList();
            return GetMemberGradeByIntegral(memberGrade, integral);
        }



        /// <summary>
        //根据会员积分获取会员等级(批量获取时先取出所有会员等级)
        /// </summary>
        /// <param name="historyIntegrals"></param>
        /// <returns></returns>
        public static DTO.MemberGrade GetMemberGradeByIntegral(List<Himall.DTO.MemberGrade> memberGrade, int integral)
        {
            var defaultGrade = new DTO.MemberGrade() { GradeName = DEFAULT_GRADE_NAME };
            var grade = memberGrade.Where(a => a.Integral <= integral).OrderByDescending(a => a.Integral).FirstOrDefault();
            if (grade == null)
            {
                return defaultGrade;
            }
            return grade;
        }
    }
}
