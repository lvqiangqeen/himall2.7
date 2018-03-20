using Himall.Core;
using Himall.Core.Helper;
using Himall.Entity;
using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace Himall.Service
{
    public class ManagerService : ServiceBase, IManagerService
    {
        public ObsoletePageModel<ManagerInfo> GetPlatformManagers(IServices.QueryModel.ManagerQuery query)
        {
            int total = 0;
            IQueryable<ManagerInfo> users = Context.ManagerInfo.FindBy(item => item.ShopId == 0, query.PageNo, query.PageSize, out total, item => item.RoleId, true);
            ObsoletePageModel<ManagerInfo> pageModel = new ObsoletePageModel<ManagerInfo>()
            {
                Models = users,
                Total = total
            };
            return pageModel;
        }
        public ObsoletePageModel<ManagerInfo> GetSellerManagers(IServices.QueryModel.ManagerQuery query)
        {
            int total = 0;
            IQueryable<ManagerInfo> users = Context.ManagerInfo.FindBy(item => item.ShopId == query.ShopID && item.RoleId != 0 && item.Id != query.userID, query.PageNo, query.PageSize, out total);
            ObsoletePageModel<ManagerInfo> pageModel = new ObsoletePageModel<ManagerInfo>()
            {
                Models = users,
                Total = total
            };
            return pageModel;
        }

        public IQueryable<ManagerInfo> GetPlatformManagerByRoleId(long roleId)
        {
            return Context.ManagerInfo.FindBy(item => item.ShopId == 0 && item.RoleId == roleId);
        }

        public ManagerInfo GetPlatformManager(long userId)
        {
            ManagerInfo manager = null;
            string CACHE_MANAGER_KEY = CacheKeyCollection.Manager(userId);

            if (Cache.Exists(CACHE_MANAGER_KEY))
            {
                manager = Core.Cache.Get<ManagerInfo>(CACHE_MANAGER_KEY);
            }
            else
            {
                manager = Context.ManagerInfo.FirstOrDefault(item => item.Id == userId && item.ShopId == 0);
                if (manager == null)
                    return null;
                if (manager.RoleId == 0)
                {
                    List<AdminPrivilege> AdminPrivileges = new List<AdminPrivilege>();
                    AdminPrivileges.Add((AdminPrivilege)0);
                    manager.RoleName = "系统管理员";
                    manager.AdminPrivileges = AdminPrivileges;
                    manager.Description = "系统管理员";
                }
                else
                {
                    var model = Context.RoleInfo.FindById(manager.RoleId);
                    if (model != null)
                    {
                        List<AdminPrivilege> AdminPrivileges = new List<AdminPrivilege>();
                        model.RolePrivilegeInfo.ToList().ForEach(a => AdminPrivileges.Add((AdminPrivilege)a.Privilege));
                        manager.RoleName = model.RoleName;
                        manager.AdminPrivileges = AdminPrivileges;
                        manager.Description = model.Description;
                    }
                }
                Cache.Insert(CACHE_MANAGER_KEY, manager);
            }
            return manager;
        }

        public IQueryable<ManagerInfo> GetSellerManagerByRoleId(long roleId, long shopId)
        {
            return Context.ManagerInfo.FindBy(item => item.ShopId == shopId && item.RoleId == roleId);
        }

        /// <summary>
        /// 根据ShopId获取对应系统管理信息
        /// <para>仅获取首页店铺系统管理员</para>
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public ManagerInfo GetSellerManagerByShopId(long shopId)
        {
            return Context.ManagerInfo.SingleOrDefault(item => item.ShopId == shopId && item.RoleId == 0);
        }

        public ManagerInfo GetSellerManager(long userId)
        {
            ManagerInfo manager = null;

            string CACHE_MANAGER_KEY = CacheKeyCollection.Seller(userId);

            if (Cache.Exists(CACHE_MANAGER_KEY))
            {
                manager = Core.Cache.Get<ManagerInfo>(CACHE_MANAGER_KEY);
            }
            else
            {
                manager = Context.ManagerInfo.Where(item => item.Id == userId && item.ShopId != 0).FirstOrDefault();
                if (manager == null)
                    return null;
                if (manager.RoleId == 0)
                {
                    List<SellerPrivilege> SellerPrivileges = new List<SellerPrivilege>();
                    SellerPrivileges.Add(0);
                    manager.RoleName = "系统管理员";
                    manager.SellerPrivileges = SellerPrivileges;
                    manager.Description = "系统管理员";
                }
                else
                {
                    var model = Context.RoleInfo.FindById(manager.RoleId);
                    if (model != null)
                    {
                        List<SellerPrivilege> SellerPrivileges = new List<SellerPrivilege>();
                        model.RolePrivilegeInfo.ToList().ForEach(a => SellerPrivileges.Add((SellerPrivilege)a.Privilege));
                        manager.RoleName = model.RoleName;
                        manager.SellerPrivileges = SellerPrivileges;
                        manager.Description = model.Description;
                    }
                }
                Cache.Insert(CACHE_MANAGER_KEY, manager);
            }
            if (manager != null)
            {
                var vshop = Context.VShopInfo.FirstOrDefault(item => item.ShopId == manager.ShopId);
                manager.VShopId = -1;
                if (vshop != null)
                {
                    manager.VShopId = vshop.Id;
                }
            }
            return manager;
        }

        public void AddPlatformManager(ManagerInfo model)
        {
            if (model.RoleId == 0)
                throw new HimallException("权限组选择不正确!");
            if (CheckUserNameExist(model.UserName, true))
            {
                throw new HimallException("该用户名已存在！");
            }
            model.ShopId = 0;
            model.PasswordSalt = Guid.NewGuid().ToString();
            model.CreateDate = DateTime.Now;
            var pwd = SecureHelper.MD5(model.Password);
            model.Password = SecureHelper.MD5(pwd + model.PasswordSalt);
            Context.ManagerInfo.Add(model);
            Context.SaveChanges();
        }

        public void AddSellerManager(ManagerInfo model, string currentSellerName)
        {
            if (model.RoleId == 0)
                throw new HimallException("权限组选择不正确!");
            if (CheckUserNameExist(model.UserName))
            {
                throw new HimallException("该用户名已存在！");
            }
            if (model.ShopId == 0)
            {
                throw new HimallException("没有权限进行该操作！");
            }
            model.PasswordSalt = Guid.NewGuid().ToString();
            model.CreateDate = DateTime.Now;
            var pwd = SecureHelper.MD5(model.Password);
            model.Password = SecureHelper.MD5(pwd + model.PasswordSalt);
            Context.ManagerInfo.Add(model);
            Context.SaveChanges();
        }


        public void ChangePlatformManagerPassword(long id, string password, long roleId)
        {
            var model = Context.ManagerInfo.FindBy(item => item.Id == id && item.ShopId == 0).FirstOrDefault();
            if (model == null)
                throw new HimallException("该管理员不存在，或者已被删除!");
            if (roleId != 0 && model.RoleId != 0)
                model.RoleId = roleId;
            if (!string.IsNullOrWhiteSpace(password))
            {
                var pwd = SecureHelper.MD5(password);
                model.Password = SecureHelper.MD5(pwd + model.PasswordSalt);
            }

            Context.SaveChanges();
            string CACHE_MANAGER_KEY = CacheKeyCollection.Manager(id);
            Core.Cache.Remove(CACHE_MANAGER_KEY);
        }


        public void ChangeSellerManager(ManagerInfo info)
        {
            var model = Context.ManagerInfo.FindBy(item => item.Id == info.Id && item.ShopId == info.ShopId).FirstOrDefault();
            if (model == null)
                throw new HimallException("该管理员不存在，或者已被删除!");
            if (info.RoleId != 0 && model.RoleId != 0)
                model.RoleId = info.RoleId;
            if (!string.IsNullOrWhiteSpace(info.Password))
            {
                var pwd = SecureHelper.MD5(info.Password);
                model.Password = SecureHelper.MD5(pwd + model.PasswordSalt);
            }
            model.RealName = info.RealName;
            model.Remark = info.Remark;
            Context.SaveChanges();
            string CACHE_MANAGER_KEY = CacheKeyCollection.Seller(info.Id);
            Core.Cache.Remove(CACHE_MANAGER_KEY);
        }

        public void ChangeSellerManagerPassword(long id, long shopId, string password, long roleId)
        {
            var model = Context.ManagerInfo.FindBy(item => item.Id == id && item.ShopId == shopId).FirstOrDefault();
            if (model == null)
                throw new HimallException("该管理员不存在，或者已被删除!");
            if (roleId != 0 && model.RoleId != 0)
                model.RoleId = roleId;
            if (!string.IsNullOrWhiteSpace(password))
            {
                var pwd = SecureHelper.MD5(password);
                model.Password = SecureHelper.MD5(pwd + model.PasswordSalt);
            }
            Context.SaveChanges();
            string CACHE_MANAGER_KEY = CacheKeyCollection.Seller(id);
            Core.Cache.Remove(CACHE_MANAGER_KEY);
        }


        public void DeletePlatformManager(long id)
        {
            var model = Context.ManagerInfo.FindBy(item => item.Id == id && item.ShopId == 0 && item.RoleId != 0).FirstOrDefault();
            Context.ManagerInfo.Remove(model);
            Context.SaveChanges();
            string CACHE_MANAGER_KEY = CacheKeyCollection.Manager(id);
            Core.Cache.Remove(CACHE_MANAGER_KEY);
        }


        public void BatchDeletePlatformManager(long[] ids)
        {
            var model = Context.ManagerInfo.FindBy(item => item.ShopId == 0 && item.RoleId != 0 && ids.Contains(item.Id));
            Context.ManagerInfo.RemoveRange(model);
            Context.SaveChanges();
            foreach (var id in ids)
            {
                string CACHE_MANAGER_KEY = CacheKeyCollection.Manager(id);
                Core.Cache.Remove(CACHE_MANAGER_KEY);
            }
        }


        public void DeleteSellerManager(long id, long shopId)
        {
            var model = Context.ManagerInfo.FindBy(item => item.Id == id && item.ShopId == shopId && item.RoleId != 0).FirstOrDefault();
            //日龙修改
            //var user = context.UserMemberInfo.FirstOrDefault( a => a.UserName == model.UserName );
            //context.ManagerInfo.Remove( user );
            Context.ManagerInfo.Remove(model);
            Context.SaveChanges();
            string CACHE_MANAGER_KEY = CacheKeyCollection.Seller(id);
            Core.Cache.Remove(CACHE_MANAGER_KEY);
        }

        public void BatchDeleteSellerManager(long[] ids, long shopId)
        {
            var model = Context.ManagerInfo.FindBy(item => item.ShopId == shopId && item.RoleId != 0 && ids.Contains(item.Id));
            //    var username = model.Select( a => a.UserName ).ToList();
            //日龙修改
            //var user = context.UserMemberInfo.FindBy( item => username.Contains( item.UserName ) );
            //context.UserMemberInfo.Remove( user );
            Context.ManagerInfo.RemoveRange(model);
            Context.SaveChanges();
            foreach (var id in ids)
            {
                string CACHE_MANAGER_KEY = CacheKeyCollection.Seller(id);
                Core.Cache.Remove(CACHE_MANAGER_KEY);
            }
        }


        public IQueryable<ManagerInfo> GetManagers(string keyWords)
        {
            IQueryable<ManagerInfo> managers = Context.ManagerInfo.FindBy(item =>
                         (keyWords == null || keyWords == "" || item.UserName.Contains(keyWords)));
            return managers;
        }



        public ManagerInfo Login(string username, string password, bool isPlatFormManager = false)
        {
            var _iSiteSettingService = ObjectContainer.Current.Resolve<ISiteSettingService>();
            ManagerInfo manager;
            if (isPlatFormManager)
                manager = Context.ManagerInfo.FindBy(item => item.UserName == username && item.ShopId == 0).FirstOrDefault();
            else
                manager = Context.ManagerInfo.FindBy(item => item.UserName == username && item.ShopId != 0).FirstOrDefault();
            if (manager != null)
            {
                //#if !DEBUG
                //string msg = "", host = System.Web.HttpContext.Current.Request.Url.Host;
                //bool isOpenStore;
                //bool isOpenShopApp;
                //if (isPlatFormManager)
                //{
                //    if (!LicenseChecker.Check(out msg, out isOpenStore, out isOpenShopApp, host))
                //    {
                //        throw new Himall.Core.HimallException(msg);
                //    }
                //    else
                //    {
                //        _iSiteSettingService.SaveSetting("IsOpenStore", isOpenStore);
                //        _iSiteSettingService.SaveSetting("IsOpenShopApp", isOpenShopApp);
                //    }
                //}
                //#endif
                string encryptedWithSaltPassword = GetPasswrodWithTwiceEncode(password, manager.PasswordSalt);
                if (encryptedWithSaltPassword.ToLower() != manager.Password)//比较密码是否一致
                    manager = null;//不一致，则置空，表示未找到指定的管理员
                else//一致，则表示登录成功，更新登录时间
                {
                    if (manager.ShopId > 0)//不处理平台
                    {
                        var shop = ServiceProvider.Instance<IShopService>.Create.GetShop(manager.ShopId);
                        if (shop == null)
                            throw new HimallException("未找到帐户对应的店铺");

                        if (!shop.IsSelf)//只处理非官方店铺
                        {
                            if (shop.ShopStatus == ShopInfo.ShopAuditStatus.Freeze)//冻结店铺
                            {
                                //throw new HimallException("帐户所在店铺已被冻结");
                            }
                        }
                    }
                }
            }
            return manager;
        }




        string GetPasswrodWithTwiceEncode(string password, string salt)
        {
            string encryptedPassword = Core.Helper.SecureHelper.MD5(password);//一次MD5加密
            string encryptedWithSaltPassword = Core.Helper.SecureHelper.MD5(encryptedPassword + salt);//一次结果加盐后二次加密
            return encryptedWithSaltPassword;
        }

        public ManagerInfo AddSellerManager(string username, string password, string salt = "")
        {
            var model = Context.ManagerInfo.FirstOrDefault(p => p.UserName == username && p.ShopId != 0);
            if (model != null)
            {
                return new ManagerInfo()
                {
                    Id = model.Id
                };
            }
            if (string.IsNullOrEmpty(salt))
            {
                salt = Core.Helper.SecureHelper.MD5(Guid.NewGuid().ToString("N"));
            }
            ManagerInfo manager;
            using (TransactionScope scope = new TransactionScope())
            {
                ShopInfo shopInfo = ServiceProvider.Instance<IShopService>.Create.CreateEmptyShop();
                manager = new ManagerInfo()
                {
                    CreateDate = DateTime.Now,
                    UserName = username,
                    Password = password,
                    PasswordSalt = salt,
                    ShopId = shopInfo.Id,
                    SellerPrivileges = new List<SellerPrivilege>() { (SellerPrivilege)0 },
                    AdminPrivileges = new List<AdminPrivilege>()
                    {
                    },
                    RoleId = 0,
                };
                Context.ManagerInfo.Add(manager);
                Context.SaveChanges();
                scope.Complete();
            }
            return manager;
        }

        public bool CheckUserNameExist(string username, bool isPlatFormManager = false)
        {
            if (isPlatFormManager)
            {
                return Context.ManagerInfo.Any(item => item.UserName.ToLower() == username.ToLower() && item.ShopId == 0);
            }
            var sellerManager = Context.ManagerInfo.Any(item => item.UserName.ToLower() == username.ToLower() && item.ShopId != 0);
            return Context.UserMemberInfo.Any(item => item.UserName.ToLower() == username.ToLower()) || sellerManager;
        }

        public ManagerInfo GetSellerManager(string userName)
        {
            var manager = Context.ManagerInfo.Where(item => item.UserName == userName && item.ShopId != 0).FirstOrDefault();
            return manager;
        }

        public void UpdateShopStatus()
        {
            List<ShopInfo> models = Context.ShopInfo.Where(s => s.EndDate < DateTime.Now).ToList();
            foreach (var m in models)
            {
                if (m.IsSelf)
                {
                    //TODO:DZY[150729] 官方自营店到期自动延期
                    /* zjt  
                     * TODO可移除，保留注释即可
                     */
                    m.EndDate = DateTime.Now.AddYears(10);
                    m.ShopStatus = ShopInfo.ShopAuditStatus.Open;
                }
                else
                {
                    m.ShopStatus = ShopInfo.ShopAuditStatus.Unusable;
                }
            }

            Context.SaveChanges();

        }
    }
}

