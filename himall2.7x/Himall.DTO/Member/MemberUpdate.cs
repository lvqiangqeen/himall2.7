using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.DTO
{
    public class MemberUpdate
    {

        /// <summary>
        /// 会员ID
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 真实姓名
        /// </summary>
        public string RealName { get; set; }


        /// <summary>
        /// 呢称
        /// </summary>
        public string Nick { set; get; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; }


        /// <summary>
        /// QQ
        /// </summary>
        public string QQ { get; set; }

        /// <summary>
        /// 电话
        /// </summary>
        public string CellPhone { get; set; }


        public Himall.CommonModel.SexType  Sex { get; set; }

        /// <summary>
        /// 生日
        /// </summary>
        public DateTime? BirthDay { get; set; }

        /// <summary>
        /// 职业
        /// </summary>
        public string Occupation { set; get; }


    }
}