using Himall.Core;
using Himall.Core.Plugins.Message;
using Himall.Entity;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;

namespace Himall.Service
{
    /// <summary>
    /// 店铺OpenApi服务
    /// </summary>
    public class ShopOpenApiService : ServiceBase, IShopOpenApiService
    {
        /// <summary>
        /// 获取店铺的OpenApi配置
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public ShopOpenApiSettingInfo Get(long shopId)
        {
            if (shopId < 1)
            {
                throw new HimallException("错误的店铺编号");
            }
            return Context.ShopOpenApiSettingInfo.SingleOrDefault(d => d.ShopId == shopId);
        }
        /// <summary>
        /// 获取店铺的OpenApi配置
        /// </summary>
        /// <param name="appkey"></param>
        /// <returns></returns>
        public ShopOpenApiSettingInfo Get(string appkey)
        {
            if (string.IsNullOrWhiteSpace(appkey))
            {
                throw new HimallException("错误的appkey");
            }
            return Context.ShopOpenApiSettingInfo.SingleOrDefault(d => d.AppKey == appkey);
        }
        /// <summary>
        /// 生成一个OpenApi配置
        /// <para>如果店铺已生成会异常</para>
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public ShopOpenApiSettingInfo MakeOpenApi(long shopId)
        {
            if (shopId <= 0)
            {
                throw new HimallException("[OpenApi]错误的店铺编号");
            }
            if (Context.ShopOpenApiSettingInfo.Any(d => d.ShopId == shopId))
            {
                throw new HimallException("[OpenApi]店铺已生成AppKey,不可以重复生成");
            }
            ShopOpenApiSettingInfo result = new ShopOpenApiSettingInfo();
            result.ShopId = shopId;
            result.AppKey = MakeAppKey(shopId);
            result.AppSecreat = MakeAppSecreat();
            result.AddDate = DateTime.Now;
            result.LastEditDate= DateTime.Now;
            result.IsEnable = false;
            result.IsRegistered = false;
            return result;
        }
        /// <summary>
        /// 生成一个appkey
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        private string MakeAppKey(long shopId = 0)
        {
            string result = "open";
            if (shopId > 0)
            {
                result = result + shopId.ToString();
            }
            while (true)
            {
                Random rnd = new Random();
                string[] seeds = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k",
                    "l", "m", "n", "o", "p","q", "r", "s", "t", "u", "v", "w", "x", "y",
                    "z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
                int seedlen = seeds.Length;
                for (int _i = 0; _i < 10; _i++)
                {
                    result += seeds[rnd.Next(0, seedlen)];
                }
                if (!Context.ShopOpenApiSettingInfo.Any(d => d.AppKey == result))
                {
                    break;
                }
            }
            return result;
        }
        /// <summary>
        /// 生成一个AppSecreat
        /// </summary>
        /// <returns></returns>
        private string MakeAppSecreat()
        {
            string result = "oas";
            while (true)
            {
                Random rnd = new Random();
                string[] seeds = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k",
                    "l", "m", "n", "o", "p","q", "r", "s", "t", "u", "v", "w", "x", "y",
                    "z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
                int seedlen = seeds.Length;
                for (int _i = 0; _i < 18; _i++)
                {
                    result += seeds[rnd.Next(0, seedlen)];
                }
                if (!Context.ShopOpenApiSettingInfo.Any(d => d.AppKey == result))
                {
                    break;
                }
            }
            return result;
        }
        /// <summary>
        /// 添加店铺OpenApi配置
        /// </summary>
        /// <param name="data"></param>
        public void Add(ShopOpenApiSettingInfo data)
        {
            if (Context.ShopOpenApiSettingInfo.Any(d => d.ShopId == data.ShopId))
            {
                throw new HimallException("[OpenApi]店铺不可拥有多个AppKey");
            }
            if (Context.ShopOpenApiSettingInfo.Any(d => d.AppKey == data.AppKey))
            {
                throw new HimallException("[OpenApi]AppKey已存在");
            }
            if (string.IsNullOrWhiteSpace(data.AppKey))
            {
                throw new HimallException("[OpenApi]AppKey不可以为空");
            }
            if (string.IsNullOrWhiteSpace(data.AppSecreat))
            {
                throw new HimallException("[OpenApi]AppSecreat不可以为空");
            }
            Context.ShopOpenApiSettingInfo.Add(data);
            Context.SaveChanges();
        }
        /// <summary>
        /// 修改店铺OpenApi配置
        /// </summary>
        /// <param name="data"></param>
        public void Update(ShopOpenApiSettingInfo data)
        {
            if (string.IsNullOrWhiteSpace(data.AppKey))
            {
                throw new HimallException("[OpenApi]AppKey不可以为空");
            }
            if (string.IsNullOrWhiteSpace(data.AppSecreat))
            {
                throw new HimallException("[OpenApi]AppSecreat不可以为空");
            }
            var dbentry = Context.Entry<ShopOpenApiSettingInfo>(data);
            dbentry.State = EntityState.Modified;
            Context.SaveChanges();
        }
        /// <summary>
        /// 设置启用状态
        /// </summary>
        /// <param name="appkey"></param>
        /// <param name="state">true:开启,false:关闭</param>
        /// <returns></returns>
        public void SetEnableState(string appkey, bool state)
        {
            var data = Get(appkey);
            if(data==null)
            {
                throw new HimallException("[OpenApi]错误的appkey");
            }
            data.IsEnable = state;
            Update(data);
        }
        /// <summary>
        /// 设置启用状态
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="state">true:开启,false:关闭</param>
        /// <returns></returns>
        public void SetEnableState(long shopId, bool state)
        {
            var data = Get(shopId);
            if (data == null)
            {
                throw new HimallException("[OpenApi]错误的shopId");
            }
            data.IsEnable = state;
            Update(data);
        }
        /// <summary>
        /// 设置注册状态
        /// </summary>
        /// <param name="appkey"></param>
        /// <param name="state">true:成功,false:失败</param>
        /// <returns></returns>
        public void SetRegisterState(string appkey, bool state)
        {
            var data = Get(appkey);
            if (data == null)
            {
                throw new HimallException("[OpenApi]错误的appkey");
            }
            data.IsRegistered = state;
            Update(data);
        }
        /// <summary>
        /// 设置注册状态
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="state">true:成功,false:失败</param>
        /// <returns></returns>
        public void SetRegisterState(long shopId, bool state)
        {
            var data = Get(shopId);
            if (data == null)
            {
                throw new HimallException("[OpenApi]错误的shopId");
            }
            data.IsRegistered = state;
            Update(data);
        }

    }
}
