using Himall.CommonModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    public class ShopBranchManager
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public long ShopBranchId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string Remark { get; set; }
        public string RealName { get; set; }
        /// <summary>
        /// 管理员类型（商家、门店）
        /// </summary>
        public ManagerType UserType { get; set; }
    }
}
