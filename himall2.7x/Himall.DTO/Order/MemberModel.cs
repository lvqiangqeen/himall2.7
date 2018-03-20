using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.DTO
{
    public class MemberModel : UserMemberInfo
    {
        public string StrLastLoginDate { get { return LastLoginDate.ToString("yyyy-MM-dd HH:mm"); } }

        public string StrCreateDate
        {
            get
            {
                return CreateDate.ToString( "yyyy-MM-dd HH:mm" );
            }
        }
    }
}