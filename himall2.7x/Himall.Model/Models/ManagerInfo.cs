using Himall.Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Himall.Model
{
    public partial class ManagerInfo : ISellerManager, IPaltManager
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

        /// <summary>
        /// 会员等级名称
        /// </summary>
        public string GradeName { set; get; }

        /// <summary>
        /// 会员可用积分
        /// </summary>
        public long AvailableIntegral { get; set; }

        /// <summary>
        /// 会员历史积分
        /// </summary>
        public long HistoryIntegral { set; get; }
       

    }
}
