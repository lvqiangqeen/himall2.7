using Himall.Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Himall.Model
{
    public partial class ManagerInfo:ISellerManager,IPaltManager
    {

        //管理员角色名
        [NotMapped]
        public string RoleName { get; set; }

        //平台管理员权限列表
        [NotMapped]
        public List<AdminPrivilege> AdminPrivileges { set; get; }

        //商家管理员权限列表
        [NotMapped]
        public List<SellerPrivilege> SellerPrivileges { set; get; }


        //管理员角色说明
        [NotMapped]
        public string Description { set; get; }
        /// <summary>
        /// 微店编号
        /// </summary>
        [NotMapped]
        public long VShopId { get; set; }
        /// <summary>
        /// 是否主账号
        /// </summary>
        public bool IsMainAccount { get {
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
