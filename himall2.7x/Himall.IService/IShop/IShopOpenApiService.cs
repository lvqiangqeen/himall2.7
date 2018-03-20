using Himall.IServices.QueryModel;
using Himall.Model;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Himall.IServices
{
    /// <summary>
    /// 店铺OpenApi服务接口
    /// </summary>
    public interface IShopOpenApiService : IService
    {
        /// <summary>
        /// 获取店铺的OpenApi配置
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        ShopOpenApiSettingInfo Get(long shopId);
        /// <summary>
        /// 获取店铺的OpenApi配置
        /// </summary>
        /// <param name="appkey"></param>
        /// <returns></returns>
        ShopOpenApiSettingInfo Get(string appkey);
        /// <summary>
        /// 生成一个OpenApi配置
        /// <para>如果店铺已生成会异常</para>
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        ShopOpenApiSettingInfo MakeOpenApi(long shopId);
        /// <summary>
        /// 添加店铺OpenApi配置
        /// </summary>
        /// <param name="data"></param>
        void Add(ShopOpenApiSettingInfo data);
        /// <summary>
        /// 修改店铺OpenApi配置
        /// </summary>
        /// <param name="data"></param>
        void Update(ShopOpenApiSettingInfo data);
        /// <summary>
        /// 设置启用状态
        /// </summary>
        /// <param name="appkey"></param>
        /// <param name="state">true:开启,false:关闭</param>
        /// <returns></returns>
        void SetEnableState(string appkey,bool state);
        /// <summary>
        /// 设置启用状态
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="state">true:开启,false:关闭</param>
        /// <returns></returns>
        void SetEnableState(long shopId, bool state);
        /// <summary>
        /// 设置注册状态
        /// </summary>
        /// <param name="appkey"></param>
        /// <param name="state">true:成功,false:失败</param>
        /// <returns></returns>
        void SetRegisterState(string appkey, bool state);
        /// <summary>
        /// 设置注册状态
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="state">true:成功,false:失败</param>
        /// <returns></returns>
        void SetRegisterState(long shopId, bool state);
    }
}
