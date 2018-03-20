using System.Collections.Generic;
using System.Linq;

namespace Himall.Model
{
    public class Privileges
    {
        public Privileges()
        {
            Privilege = new List<GroupActionItem>();
        }
        public List<GroupActionItem> Privilege { get; set; }
    }

    public class GroupActionItem
    {
        public GroupActionItem()
        {
            Items = new List<ActionItem>();
        }
        public string GroupName { set; get; }
        public List<ActionItem> Items { get; set; }
    }
    public class ActionItem
    {
        public ActionItem()
        {
            Controllers = new List<Controllers>();
        }
        public string Name { get; set; }
        public string Url { get; set; }
        public List<Controllers> Controllers { set; get; }
        public int PrivilegeId { get; set; }

        public Himall.CommonModel.AdminCatalogType Type { get; set; }
        /// <summary>
        /// 链接打开方式，blank,parent,self,top
        /// </summary>
        public string LinkTarget { get; set; }
    }
    public class Controllers
    {
        public Controllers()
        {
            ActionNames = new List<string>();
        }
        public string ControllerName { set; get; }
        public List<string> ActionNames { set; get; }
    }
}
