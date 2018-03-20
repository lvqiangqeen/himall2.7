using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Himall.Model;

namespace Himall.Web.Areas.Admin.Models
{
    public class RoleInfoModel
    {
        [Required(ErrorMessage="角色名称必填")]
        [StringLength(15,ErrorMessage="角色名称在15个字符以内")]
        public string RoleName { get; set; }


        public long ID { get;set; }

        //权限列表
      public  IEnumerable<RolePrivilegeInfo> RolePrivilegeInfo { set; get; }
    }
  
}