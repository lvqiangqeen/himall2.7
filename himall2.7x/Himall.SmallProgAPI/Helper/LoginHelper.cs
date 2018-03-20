using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.SmallProgAPI.Helper
{
    public class LoginHelper
    {
        IMemberService _iMemberService;

        public LoginHelper(IMemberService iMemberService)
        {
            _iMemberService = iMemberService;
        }

        public static void ChangeOpenIdBindMember(MemberOpenIdInfo model)
        {
            ServiceHelper.Create<IMemberService>().UpdateOpenIdBindMember(model);
        }
    }
}
