using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.DTO
{
    public class ManagerInfoModel
    {
        public long RoleId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public string Remark { set; get; }

        public string RealName { set; get; }
    }
}