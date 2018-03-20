using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    /// <summary>  
    ///描述枚举的属性  
    /// </summary>  
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class PrivilegeAttribute : Attribute
    {
        /// <summary>
        /// 权限组名
        /// </summary>
        public string GroupName { set; get; }
        /// <summary>
        /// 权限名
        /// </summary>
        public string Name { set; get; }
        public string Url { set; get; }
        /// <summary>
        /// 权限包含的控制器名
        /// </summary>
        public string Controller { set; get; }
        /// <summary>
        /// 方法名
        /// </summary> 
        public string Action { set; get; }
        public int Pid { set; get; }

        public Himall.CommonModel.AdminCatalogType AdminCatalogType { get; set; }
        /// <summary>
        /// 链接打开方式，blank,parent,self,top
        /// </summary>
        public string LinkTarget { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="GroupName">权限组名</param>
        /// <param name="Name">权限名</param>
        /// <param name="Pid">权限Id</param>
        /// <param name="Url">权限地址</param>
        /// <param name="Controller">权限包含的控制器</param>
        /// <param name="Action">权限包含的action</param>
        /// <param name="type">是否显示在导航中</param>
        public PrivilegeAttribute(string groupName, string name, int pid, string url, string controller, string action = "", Himall.CommonModel.AdminCatalogType type = Himall.CommonModel.AdminCatalogType.Default,string target="")
        {
            this.Name = name;
            this.GroupName = groupName;
            this.Pid = pid;
            this.Url = url;
            this.Controller = controller;
            this.Action = action;
            this.AdminCatalogType = type;
            this.LinkTarget = target;
        }
        public PrivilegeAttribute(string controller, string action = "")
        {
            this.Controller = controller;
            this.Action = action;
        }
    }
}
