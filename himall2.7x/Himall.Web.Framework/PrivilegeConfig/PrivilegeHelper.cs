using Himall.Model;
using Himall.Web.Framework;
using System;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using System.Linq;
using System.Collections.Generic;
using Himall.CommonModel;

namespace Himall.Web.Framework
{
    public class PrivilegeHelper
    {
        private static Privileges adminPrivileges;
        private static Privileges adminPrivilegesDefault;
        private static Privileges adminPrivilegesInternal;

        private static Privileges sellerAdminPrivileges;

        private static Privileges userPrivileges;
        public static Privileges UserPrivileges
        {
            set
            {
                userPrivileges = value;
            }
            get
            {
                if (userPrivileges == null)
                {
                    userPrivileges = GetPrivileges<UserPrivilege>();
                }
                return userPrivileges;
            }
        }

        /// <summary>
        /// 平台后台权限
        /// </summary>
        public static Privileges AdminPrivileges
        {
            set
            {
                adminPrivileges = value;
            }
            get
            {
                if (adminPrivileges == null)
                {
                    adminPrivileges = GetPrivileges<AdminPrivilege>();
                }
                return adminPrivileges;
            }
        }


        /// <summary>
        /// 平台后台导航
        /// </summary>
        public static Privileges AdminPrivilegesDefault
        {
            set
            {
                adminPrivilegesDefault = value;
            }
            get
            {
                if (adminPrivilegesDefault == null)
                {
                    adminPrivilegesDefault = GetPrivileges<AdminPrivilege>(AdminCatalogType.Default);
                }
                return adminPrivilegesDefault;
            }
        }

        /// <summary>
        /// 平台后台内部导航
        /// </summary>
        public static Privileges AdminPrivilegesInternal
        {
            set
            {
                adminPrivilegesInternal = value;
            }
            get
            {
                if (adminPrivilegesInternal == null)
                {
                    adminPrivilegesInternal = GetPrivileges<AdminPrivilege>(AdminCatalogType.Internal);
                }
                return adminPrivilegesInternal;
            }
        }

        public static Privileges SellerAdminPrivileges
        {
            set
            {
                sellerAdminPrivileges = value;
            }
            get
            {
                if (sellerAdminPrivileges == null)
                {
                    sellerAdminPrivileges = GetPrivileges<SellerPrivilege>();
                }
                return sellerAdminPrivileges;
            }
        }

        /// <summary>
        /// 相当于根目录的路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Privileges GetPrivileges<TEnum>()
        {
            Type type = typeof(TEnum);
            FieldInfo[] fields = type.GetFields();
            if (fields.Length == 1)
            {
                return null;
            }
            Privileges p = new Privileges();
            foreach (var field in fields)
            {
                var attributes = field.GetCustomAttributes(typeof(PrivilegeAttribute), true);
                if(attributes.Length==0)
                {
                    continue;
                    
                }
                GroupActionItem group = new GroupActionItem();
                ActionItem item = new ActionItem();
                List<string> actions = new List<string>();
                List<PrivilegeAttribute> attrs = new List<PrivilegeAttribute>();
                List<Controllers> ctrls = new List<Controllers>();
                
                foreach (var attr in attributes)
                {
                    Controllers ctrl=new Controllers();
                    var attribute = attr as PrivilegeAttribute;
                    ctrl.ControllerName = attribute.Controller;
                    ctrl.ActionNames.AddRange(attribute.Action.Split(','));
                    ctrls.Add(ctrl);
                    attrs.Add(attribute);
                }
                var groupInfo=attrs.FirstOrDefault(a => !string.IsNullOrEmpty(a.GroupName));
                group.GroupName = groupInfo.GroupName;
                item.PrivilegeId = groupInfo.Pid;
                item.Name = groupInfo.Name;
                item.Url = groupInfo.Url;
                item.Type = groupInfo.AdminCatalogType;
                
                item.Controllers.AddRange(ctrls);
                var currentGroup = p.Privilege.FirstOrDefault(a => a.GroupName == group.GroupName);
                if (currentGroup == null)
                {   group.Items.Add(item);
                    p.Privilege.Add(group);
                }
                else
                {
                    currentGroup.Items.Add(item);
                }
            }
            #region 门店授权
            bool isOpenStore = Application.SiteSettingApplication.GetSiteSettings() != null && Application.SiteSettingApplication.GetSiteSettings().IsOpenStore;
            if (!isOpenStore)//未授权则关闭门店管理菜单
            {
                var shopManager = p.Privilege.Where(x => x.GroupName.Equals("店铺")).FirstOrDefault();
                if (shopManager != null)
                {
                    shopManager.Items.Remove(shopManager.Items.Where(x=>x.PrivilegeId==4008).FirstOrDefault());// 4008 = "门店管理"
                }
            }
            #endregion
            return p;
        }

        /// <summary>
        /// 相当于根目录的路径
        /// </summary>
        /// <param name="path"></param>
        /// <param name="Type">导航类别</param>
        /// <returns></returns>
        public static Privileges GetPrivileges<TEnum>(AdminCatalogType Type)
        {
            Type type = typeof(TEnum);
            FieldInfo[] fields = type.GetFields();
            if (fields.Length == 1)
            {
                return null;
            }
            Privileges p = new Privileges();
            foreach (var field in fields)
            {
                var attributes = field.GetCustomAttributes(typeof(PrivilegeAttribute), true);
                if (attributes.Length == 0)
                {
                    continue;
                }
                GroupActionItem group = new GroupActionItem();
                ActionItem item = new ActionItem();
                List<string> actions = new List<string>();
                List<PrivilegeAttribute> attrs = new List<PrivilegeAttribute>();
                List<Controllers> ctrls = new List<Controllers>();
                string linkTarget = string.Empty;
                foreach (var attr in attributes)
                {
                    Controllers ctrl = new Controllers();
                    var attribute = attr as PrivilegeAttribute;
                    if (!attribute.AdminCatalogType.Equals(Type))
                    {
                        continue;
                    }
                    ctrl.ControllerName = attribute.Controller;
                    ctrl.ActionNames.AddRange(attribute.Action.Split(','));
                    ctrls.Add(ctrl);
                    attrs.Add(attribute);
                    linkTarget = attribute.LinkTarget;
                }
                if (attrs.Count.Equals(0))
                {
                    continue;
                }
                var groupInfo = attrs.FirstOrDefault(a => !string.IsNullOrEmpty(a.GroupName));
                group.GroupName = groupInfo.GroupName;
                item.PrivilegeId = groupInfo.Pid;
                item.Name = groupInfo.Name;
                item.Url = groupInfo.Url;
                item.Type = groupInfo.AdminCatalogType;
                item.LinkTarget = linkTarget;
                item.Controllers.AddRange(ctrls);
                var currentGroup = p.Privilege.FirstOrDefault(a => a.GroupName == group.GroupName);
                if (currentGroup == null)
                {
                    group.Items.Add(item);
                    p.Privilege.Add(group);
                }
                else
                {
                    currentGroup.Items.Add(item);
                }

            }
            return p;
        }
    }
}
