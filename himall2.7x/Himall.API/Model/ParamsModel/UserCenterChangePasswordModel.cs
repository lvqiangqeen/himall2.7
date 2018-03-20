using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.API.Model.ParamsModel
{
    public class UserCenterChangePasswordModel
    {
        public string oldPassword { get; set; }
        public string password { get; set; }
    }
}
