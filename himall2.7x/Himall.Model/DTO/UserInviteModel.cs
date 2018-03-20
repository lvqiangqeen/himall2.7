using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
   public class UserInviteModel
    {
       public int  InvitePersonCount { set; get; }

       public int InviteIntergralCount { set; get; }

       public string InviteLink { set; get; }

       public string QR { set; get; }
    }
}
