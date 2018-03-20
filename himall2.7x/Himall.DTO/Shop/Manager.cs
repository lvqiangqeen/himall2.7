using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Model;

namespace Himall.DTO
{
    /// <summary>
    /// 店铺管理员
    /// </summary>
    public class Manager
    {
        /// <summary>
        /// 商店管理员ID
        /// </summary>
        public long Id { get; set; }
		public long ShopId { get; set; }
		public long RoleId { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		public string PasswordSalt { get; set; }
		public System.DateTime CreateDate { get; set; }
		public string Remark { get; set; }
		public string RealName { get; set; }
		public string RoleName { get; set; }

		/// <summary>
		/// 平台管理员权限列表
		/// </summary>
		public List<AdminPrivilege> AdminPrivileges { set; get; }

		/// <summary>
		/// 商家管理员权限列表
		/// </summary>
		public List<SellerPrivilege> SellerPrivileges { set; get; }

		public string Description { set; get; }

		/// <summary>
		/// 微店编号
		/// </summary>
		public long VShopId { get; set; }
        /// <summary>
        /// 是否主账号
        /// </summary>
        public bool IsMainAccount
        {
            get
            {
                if (this.UserName.Contains(":"))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
    }
}
