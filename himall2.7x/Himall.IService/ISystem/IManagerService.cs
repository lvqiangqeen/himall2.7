using Himall.IServices.QueryModel;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.IServices
{
    public interface IManagerService : IService
    {
        /// <summary>
        /// 更新店铺状态
        /// </summary>
         void UpdateShopStatus();

        /// <summary>
        /// 根据查询条件分页获取平台管理员信息
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        ObsoletePageModel<ManagerInfo> GetPlatformManagers(ManagerQuery query);

        /// <summary>
        /// 根据查询条件分页获取商家管理员信息
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        ObsoletePageModel<ManagerInfo> GetSellerManagers(ManagerQuery query);

        /// <summary>
        /// 根据角色ID获取平台角色下的管理员
        /// </summary>
        /// <param name="roleid"></param>
        /// <returns></returns>
        IQueryable<ManagerInfo> GetPlatformManagerByRoleId(long roleId);
        /// <summary>
        /// 根据ShopId获取对应管理信息
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        ManagerInfo GetSellerManagerByShopId(long shopId);

        /// <summary>
        /// 获取当前登录的平台管理员
        /// </summary>
        /// <returns></returns>
        ManagerInfo GetPlatformManager(long userId);

        /// <summary>
        /// 根据角色ID获取商家角色下的管理员
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="shopId"></param>
        /// <returns></returns>
        IQueryable<ManagerInfo> GetSellerManagerByRoleId(long roleId, long shopId);

        /// <summary>
        /// 获取当前登录的商家管理员
        /// </summary>
        /// <returns></returns>
        ManagerInfo GetSellerManager(long userId);
        /// <summary>
        /// 添加一个平台管理员
        /// </summary>
        /// <param name="model"></param>
        void AddPlatformManager(ManagerInfo model);
        /// <summary>
        /// 添加一个商家管理员
        /// </summary>
        /// <param name="password">密码</param>
        /// <param name="username">用户名</param>
        ManagerInfo AddSellerManager(string username, string password, string salt);

        /// <summary>
        /// 添加商家子帐号（不创建店铺）
        /// </summary>
        /// <param name="model"></param>
        ///<param name="currentSellerName">当前商家帐号用户名</param>

        void AddSellerManager(ManagerInfo model, string currentSellerName);
        /// <summary>
        /// 修改平台管理员密码和角色
        /// </summary>
        /// <param name="model"></param>
        void ChangePlatformManagerPassword(long id, string password, long roleId);

        /// <summary>
        /// 修改商家管理员密码
        /// </summary>
        /// <param name="model"></param>
        void ChangeSellerManagerPassword(long id, long shopId, string password, long roleId);


        /// <summary>
        /// 修改商家管理员
        /// </summary>
        /// <param name="model"></param>
        void ChangeSellerManager(ManagerInfo info);

        /// <summary>
        /// 删除平台管理员
        /// </summary>
        /// <param name="id"></param>
        void DeletePlatformManager(long id);


        /// <summary>
        /// 删除商家管理员
        /// </summary>
        /// <param name="id">管理员ID</param>
        /// <param name="shopId">店铺ID</param>
        void DeleteSellerManager(long id,long shopId);

        /// <summary>
        /// 批量删除平台管理员
        /// </summary>
        /// <param name="ids"></param>
        void BatchDeletePlatformManager(long[] ids);


        /// <summary>
        /// 批量删除商家管理员
        /// </summary>
        /// <param name="ids"></param>
        void BatchDeleteSellerManager(long[] ids,long shopId);

        /// <summary>
        /// 是否存在相同的用户名
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        bool CheckUserNameExist(string userName,bool isPlatFormManager = false);


        /// <summary>
        /// 获取查询的管理员列表
        /// </summary>
        /// <param name="keyWords">关键字</param>
        /// <returns></returns>
        IQueryable<ManagerInfo> GetManagers(string keyWords);


        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码（明文）</param>
        /// <param name="isPlatFormManager">是否为平台管理员</param>
        /// <returns></returns>
        ManagerInfo Login(string username, string password, bool isPlatFormManager = false);

        /// <summary>
        /// 根据商家名称获取商家信息
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        ManagerInfo GetSellerManager(string userName);
    }
}
