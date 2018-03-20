
using System;

namespace Himall.IServices
{
    /// <summary>
    /// 缓存键值集合
    /// </summary>
    public static class CacheKeyCollection
    {
        /// <summary>
        /// 管理员账号信息缓存键
        /// </summary>
        /// <param name="managerId">管理员id</param>
        /// <returns></returns>
        public static string Manager(long managerId)
        {
            return string.Format("Cache-Manager-{0}", managerId);
        }

        /// <summary>
        /// 商家账号信息缓存键
        /// </summary>
        /// <param name="sellerId">商家id</param>
        /// <returns></returns>
        public static string Seller(long sellerId)
        {
            return string.Format("Cache-Seller-{0}", sellerId);
        }

        /// <summary>
        /// 会员信息缓存键
        /// </summary>
        /// <param name="memberId">会员id</param>
        /// <returns></returns>
        public static string Member(long memberId)
        {
            return string.Format("Cache-Member-{0}", memberId);
        }

        /// <summary>
        /// 用户导入产品计数
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public static string UserImportProductCount(long userid)
        {
            return string.Format("Cache-{0}-ImportProductCount", userid);
        }
        public static string UserImportProductTotal(long userid)
        {
            return string.Format("Cache-{0}-ImportProductTotal", userid);
        }
        /// <summary>
        /// 同时导入限制
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public const string UserImportOpCount = "Cache-UserImportOpCount";

        /// <summary>
        /// 店铺关注人数缓存
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static string ShopConcerned(long shopId)
        {
            return string.Format("Cache-ShopConcerned-{0}", shopId);
        }

        /// <summary>
        /// 店铺热销的前N件商品
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static string HotSaleProduct(long shopId)
        {
            return string.Format("Cache-HotSaleProduct-{0}", shopId);
        }
        /// <summary>
        /// 移动端首页分页商品
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public static string MobileHomeProductInfo(string name, long page)
        {
            return string.Format("Cache-M-HomeProductInfo-{0}-{1}", name, page);
        }

        /// <summary>
        /// 移动端店铺首页分页商品
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public static string MobileShopHomeProductInfo(string shopId, string name, long page)
        {
            return string.Format("Cache-M-HomeProductInfo-{0}-{1}-{2}", shopId, name, page);
        }
        /// <summary>
        /// 移动端首页模版设置信息
        /// </summary>
        public static string MobileHomeTemplate(string shopid, string client = "t1")
        {
            return string.Format("Cache-MobileHomeTemplate-{0}-{1}", shopid, client);
        }
        /// <summary>
        /// 店铺最新上架的前N件商品
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static string NewSaleProduct(long shopId)
        {
            return string.Format("Cache-NewSaleProduct-{0}", shopId);
        }


        /// <summary>
        /// 店铺最受关注的前N件商品
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static string HotConcernedProduct(long shopId)
        {
            return string.Format("Cache-HotConcernedProduct-{0}", shopId);
        }

        /// <summary>
        /// 登录错误缓存（一般保留15分钟）
        /// </summary>
        /// <param name="username">出错时用户名</param>
        /// <returns></returns>
        public static string ManagerLoginError(string username)
        {
            return string.Format("Cache-Manager-Login-{0}", username);
        }

        /// <summary>
        /// 登录错误缓存（一般保留15分钟）
        /// </summary>
        /// <param name="username">出错时用户名</param>
        /// <returns></returns>
        public static string MemberLoginError(string username)
        {
            return string.Format("Cache-Member-Login-{0}", username);
        }

        /// <summary>
        /// 验证码短信发送次数
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static string MessageSendNum(string username)
        {
            return string.Format("Cache-Message-Send-{0}", username);
        }


        public static string MemberPluginCheck(string username, string pluginId)
        {
            return string.Format("Cache-Member-{0}-{1}", username, pluginId);
        }
        public static string MemberPluginCheckTime(string username, string pluginId)
        {
            return string.Format("Cache-CheckTime-{0}-{1}", username, pluginId);
        }

        /// <summary>
        /// 找回密码
        /// </summary>
        /// <param name="username"></param>
        /// <param name="pluginId"></param>
        /// <returns></returns>
        public static string MemberFindPwd(string username)
        {
            return string.Format("Cache-MemberFindPwd-{0}", username);
        }

        /// <summary>
        /// 验证管理员身份
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="pluginId">插件域</param>
        /// <returns></returns>
        public static string MemberPluginAuthenticate(string username, string pluginId)
        {
            return string.Format("Cache-Authenticate-{0}-{1}", username, pluginId);
        }
        /// <summary>
        /// 验证管理员身份，时间
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="pluginId">插件域</param>
        /// <returns></returns>
        public static string MemberPluginAuthenticateTime(string username, string pluginId)
        {
            return string.Format("Cache-AuthenticateTime-{0}-{1}", username, pluginId);
        }

        public static string MemberPluginFindPassWordTime(string username, string pluginId)
        {
            return string.Format("Cache-FindPasswordTime-{0}-{1}", username, pluginId);
        }

        public static string MemberPluginReBindTime(string username, string pluginId)
        {
            return string.Format("Cache-ReBindTime-{0}-{1}", username, pluginId);
        }
        public static string MemberPluginReBindStepTime(string username, string pluginId)
        {
            return string.Format("Cache-ReBindStepTime-{0}-{1}", username, pluginId);
        }

        public static string MemberFindPasswordCheck(string username, string pluginId)
        {
            return string.Format("Cache-Member-PassWord-{0}-{1}", username, pluginId);
        }




        public static string ShopPluginAuthenticate(string username, string pluginId)
        {
            return string.Format("Cache-ShopAut-{0}-{1}", username, pluginId);
        }
        public static string ShopPluginAuthenticateTime(string username, string pluginId)
        {
            return string.Format("Cache-ShopAutTime-{0}-{1}", username, pluginId);
        }

        /// <summary>
        /// 绑定银行卡
        /// </summary>
        /// <param name="username"></param>
        /// <param name="pluginId"></param>
        /// <returns></returns>
        public static string ShopPluginBindBankTime(string username, string pluginId)
        {
            return string.Format("Cache-ShopBankTime-{0}-{1}", username, pluginId);
        }
        public static string ShopPluginBindBank(string username, string pluginId)
        {
            return string.Format("Cache-ShopBank-{0}-{1}", username, pluginId);
        }

        /// <summary>
        /// 支付状态缓存
        /// </summary>
        /// <param name="orderIds">订单编号</param>
        /// <returns></returns>
        public static string PaymentState(string orderIds)
        {
            return string.Format("Cache-PaymentState-{0}", orderIds);
        }

        /// <summary>
        /// 场景值缓存
        /// </summary>
        /// <param name="sceneid"></param>
        /// <returns></returns>
        public static string SceneState(string sceneid)
        {
            return string.Format("Cache-SceneState-{0}", sceneid);
        }
        public static string ChargeOrderKey(string id)
        {
            return string.Format("Cache-ChargeOrder-{0}", id);
        }

        /// <summary>
        /// 场景返回结果
        /// </summary>
        /// <param name="sceneid"></param>
        /// <returns></returns>
        public static string SceneReturn(string sceneid)
        {
            return string.Format("Cache-SceneReturn-{0}", sceneid);
        }

        /// <summary>
        /// 绑定微信
        /// </summary>
        /// <param name="sceneid"></param>
        /// <returns></returns>
        public static string BindingReturn(string sceneid)
        {
            return string.Format("Cache-BindingReturn-{0}", sceneid);
        }
        /// <summary>
        /// 省市区
        /// </summary>
        public const string Region = "Cache-Regions";

        /// <summary>
        /// 商品分类
        /// </summary>
        public const string Category = "Cache-Categories";

        public const string HomeCategory = "Cache-HomeCategories";

        /// <summary>
        /// 品牌
        /// </summary>
        public const string Brand = "Cache-Brands";

        /// <summary>
        /// 站点设置
        /// </summary>
        public const string SiteSettings = "Cache-SiteSettings";

        /// <summary>
        /// 首页菜单导航
        /// </summary>
        public const string Banners = "Cache-Banners";

        /// <summary>
        /// 广告
        /// </summary>
        public const string Advertisement = "Cache-Adverts";

        /// <summary>
        /// 询问菜单
        /// </summary>
        public const string BottomHelpers = "Cache-Helps";

        /// <summary>
        /// 快递单模板
        /// </summary>
        public const string ExpressTemplate = "Cache-ExperssTemplate";

        /// <summary>
        /// 主题设置
        /// </summary>
        public const string Themes = "Cache-Themes";

        /// <summary>
        /// 商家入驻设置
        /// </summary>
        public const string Settled = "Cache-Settled";

        /// <summary>
        /// 昨天订单数
        /// </summary>
        /// 

        public static string YesterDayOrdersNum(long? shopId = null)
        {
            return string.Format("Cache-YesterDayOrdersNum-{0}", shopId);
        }

        public static string YesterDayPayOrdersNum(long? shopId = null)
        {
            return string.Format("Cache-YesterDayPayOrdersNum-{0}", shopId);
        }

        public static string YesterDaySaleAmount(long? shopId = null)
        {
            return string.Format("YesterDaySaleAmount-{0}", shopId);
        }

        /// <summary>
        /// 商铺续费提醒，每天提醒一次
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static string isPromptKey(long shopId)
        {
            return string.Format("isPromptKey-{0}", shopId);
        }

        /// <summary>
        /// 位置地址信息缓存
        /// </summary>
        /// <param name="fromLatLng"></param>
        /// <returns></returns>
        public static string LatlngCacheKey(string fromLatLng)
        {
            return string.Format("DataCache-latlngAddress-{0}", fromLatLng);
        }


        #region 3.0优化
        //PC信任登录缓存
        public const string CACHE_OAUTHLIST = "KEY_OAUTHLIST";
        public const string CACH_FLASHSALECONFIG = "CACH_FLASHSALECONFIG";
        public const string CACHE_ATTRIBUTE_LIST = "CACHE_ATTRIBUTE_LIST";
        public const string CACHE_ATTRIBUTEVALUE_LIST = "CACHE_ATTRIBUTEVALUE_LIST";
        public const string CACHE_SELFSHOP = "CACHE_SELFSHOP";
        public const string CACHE_LIMITPRODUCTS = "CACHE_LIMITPRODUCTS";
        public const string CACHE_FIGHTGROUP = "CACHE_FIGHTGROUP";
        public const string CACHE_DISTRIBUTORSETTING = "CACHE_DISTRIBUTORSETTING";
        public static string CACHE_CART(long userId)
        {
            return string.Format("CART_{0}", userId);
        }
        //商品详情获取店铺信息
        public static string CACHE_SHOPDTO(long id, bool businessCategoryOn)
        {
            return string.Format("CACHE_SHOPDTO_{0}_{1}", id, businessCategoryOn);
        }
        //商品详情获取店铺信息
        public static string CACHE_SHOP(long id, bool businessCategoryOn)
        {
            return string.Format("CACHE_SHOP_{0}_{1}", id, businessCategoryOn);
        }
        //商品详情缓存
        public static string CACHE_PRODUCTDESC(long id)
        {
            return string.Format("CACHE_SHOP_{0}", id);
        }
        public static string CACHE_SHOPINFO(long sid, long productId)
        {
            return string.Format("CACHE_SHOPINFO_{0}_{1}", sid, productId);
        }
        public static string CACHE_PRODUCTMARK(long id)
        {
            return string.Format("CACHE_PRODUCTMARK_{0}", id);
        }
        public static string CACHE_PRODUCTLIMITNOTSTART(long id)
        {
            return string.Format("CACHE_PRODUCTLIMITNOTSTART_{0}", id);
        }
        public static string CACHE_SEARCHFILTER(string keyword, long cid, long b_id, string a_id)
        {
            return string.Format("CACHE_SEARCHFILTER_{0}_{1}_{2}_{3}", keyword, cid, b_id, a_id);
        }
        public static string CACHE_SHIPADDRESS(long addressId)
        {
            return string.Format("CACHE_SHIPADDRESS_{0}", addressId);
        }
        public static string CACHE_FREIGHTTEMPLATE(long templateId)
        {
            return string.Format("CACHE_FREIGHTTEMPLATE_{0}", templateId);
        }
        public static string CACHE_FREIGHTAREADETAIL(long templateId)
        {
            return string.Format("CACHE_FREIGHTAREADETAIL_{0}", templateId);
        }
        public static string CACHE_PRODUCT(long productId)
        {
            return string.Format("CACHE_PRODUCT_{0}", productId);
        }
        #endregion

    }
}
