using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.IServices
{
    public interface IPrivilegesService : IService
    {
        /// <summary>
        /// 添加一个平台管理员角色
        /// </summary>
        void AddPlatformRole(RoleInfo model);

        /// <summary>
        /// 添加一个商家管理员角色
        /// </summary>
        void AddSellerRole(RoleInfo model);

        /// <summary>
        /// 修改平台管理员角色
        /// </summary>
        void UpdatePlatformRole(RoleInfo model);


        /// <summary>
        /// 修改商家管理员角色
        /// </summary>
        void UpdateSellerRole(RoleInfo model);

        /// <summary>
        /// 删除一个平台角色
        /// </summary>
        /// <param name="id"></param>

        void DeletePlatformRole(long id);
        /// <summary>
        /// 获取一个平台角色的详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        RoleInfo GetPlatformRole(long id);

         /// <summary>
        /// 获取一个商家角色的详情
        /// </summary>
        /// <param name="id"></param>
        /// <param name="shopId">店铺ID</param>
        /// <returns></returns>
        RoleInfo GetSellerRole(long id, long shopId);

        /// <summary>
        /// 获取商家角色列表
        /// </summary>
        /// <param name="shopID">商家的店铺ID</param>
        /// <returns></returns>
        IQueryable<RoleInfo> GetSellerRoles(long shopId);
        /// <summary>
        /// 获取平台角色列表
        /// </summary>
        /// <returns></returns>
        IQueryable<RoleInfo> GetPlatformRoles();

        void DeleteSellerRole(long id,long shopId);
    }
}
