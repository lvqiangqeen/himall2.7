using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Entity;
using System.Data.Entity;
using Himall.Core;

namespace Himall.Service
{
    public class PrivilegesService : ServiceBase, IPrivilegesService
    {
        public void AddPlatformRole(Model.RoleInfo model)
        {
            model.ShopId = 0L;
            if (string.IsNullOrEmpty(model.Description))
            {
                model.Description = model.RoleName;
            }
            var ex = Context.RoleInfo.Any(a => a.RoleName == model.RoleName && a.ShopId == model.ShopId);
            if (ex)
            {
                throw new HimallException("已存在相同名称的权限组");
            }
            Context.RoleInfo.Add(model);
            Context.SaveChanges();
        }

        public void UpdatePlatformRole(Model.RoleInfo model)
        {
            var updatemodel = Context.RoleInfo.FindBy(a => a.ShopId == 0 && a.Id == model.Id).FirstOrDefault();
            if (updatemodel == null)
                throw new HimallException("找不到该权限组");

            var ex = Context.RoleInfo.Any(a => a.RoleName == model.RoleName && a.ShopId ==0&&a.RoleName!=updatemodel.RoleName);
            if (ex)
            {
                throw new HimallException("已存在相同名称的权限组");
            }

            updatemodel.RoleName = model.RoleName;
            updatemodel.Description = model.Description;
            if (string.IsNullOrEmpty(model.Description))
            {
                updatemodel.Description = model.RoleName;
            }
            Context.RolePrivilegeInfo.RemoveRange(updatemodel.RolePrivilegeInfo);
            updatemodel.RolePrivilegeInfo = model.RolePrivilegeInfo;
            Context.SaveChanges();
        }

        public void UpdateSellerRole(Model.RoleInfo model)
        {
            var updatemodel = Context.RoleInfo.FindBy(a => a.ShopId == model.ShopId && a.Id == model.Id).FirstOrDefault();
            if (updatemodel == null)
                throw new HimallException("找不到该权限组");

            var ex = Context.RoleInfo.Any(a => a.RoleName == model.RoleName && a.ShopId==model.ShopId&&a.RoleName!=updatemodel.RoleName);
            if (ex)
            {
                throw new HimallException("已存在相同名称的权限组");
            }
            updatemodel.RoleName = model.RoleName;
            updatemodel.Description = model.Description;
            if (string.IsNullOrEmpty(model.Description))
            {
                updatemodel.Description = model.RoleName;
            }
            Context.RolePrivilegeInfo.RemoveRange(updatemodel.RolePrivilegeInfo);
            updatemodel.RolePrivilegeInfo = model.RolePrivilegeInfo;
            Context.SaveChanges();
        }

        public void DeletePlatformRole(long id)
        {
            var model = Context.RoleInfo.Where(a => a.Id == id && a.ShopId == 0).FirstOrDefault();
            Context.RoleInfo.Remove(model);
            Context.SaveChanges();
        }
        public RoleInfo GetPlatformRole(long id)
        {
            return Context.RoleInfo.Where(a => a.Id == id && a.ShopId == 0).FirstOrDefault();
        }

        public RoleInfo GetSellerRole(long id,long shopid)
        {
            return Context.RoleInfo.Where(a => a.Id == id && a.ShopId == shopid).FirstOrDefault();
        }

        public IQueryable<RoleInfo> GetPlatformRoles()
        {
            return Context.RoleInfo.FindBy(item => item.ShopId == 0);
        }

        public IQueryable<RoleInfo> GetSellerRoles(long shopId)
        {
            return Context.RoleInfo.Where(item=>item.ShopId == shopId&&item.ShopId!=0);
        }

        public void AddSellerRole(RoleInfo model)
        {
            if (string.IsNullOrEmpty(model.Description))
            {
                model.Description = model.RoleName;
            }
            var ex = Context.RoleInfo.Any(a => a.RoleName == model.RoleName&&a.ShopId==model.ShopId);
            if(ex)
            {
                throw new HimallException("已存在相同名称的权限组");
            }
            Context.RoleInfo.Add(model);
            Context.SaveChanges();
        }


        public void DeleteSellerRole(long id,long shopId)
        {
            var model = Context.RoleInfo.Where(a => a.Id == id && a.ShopId ==shopId).FirstOrDefault();
            Context.RoleInfo.Remove(model);
            Context.SaveChanges();
        }
    }
}
