using Himall.Core;
using Himall.Entity;
using Himall.IServices;
using Himall.Model;
using Newtonsoft.Json;
using Senparc.Weixin.MP.CommonAPIs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Himall.Service
{
    public class WeixinMenuService : ServiceBase, IWeixinMenuService
    {
        public IEnumerable<MenuInfo> GetMainMenu(long shopId)
        {
            return Context.MenuInfo.Where(a => a.ParentId == 0 && a.Platform == PlatformType.WeiXin && a.ShopId == shopId);
        }
        
        public IEnumerable<MenuInfo> GetMenuByParentId(long id)
        {
            return Context.MenuInfo.Where(a => a.ParentId == id && a.Platform == PlatformType.WeiXin);
        }

        public MenuInfo GetMenu(long id)
        {
            return Context.MenuInfo.Where(a => a.Id == id && a.Platform == PlatformType.WeiXin).FirstOrDefault();
        }

        public IQueryable<MenuInfo> GetAllMenu(long shopId)
        {
            return Context.MenuInfo.Where(a => a.ShopId == shopId && a.Platform == PlatformType.WeiXin);
        }

        public void AddMenu(MenuInfo model)
        {
            if (model == null)
                throw new ApplicationException("微信自定义菜单的Model不能为空");
            if (model.ParentId < 0)
                throw new Himall.Core.HimallException("微信自定义菜单的上级菜单不能为负值");
            if (model.Title.Length == 0 || (model.Title.Length > 5 && model.ParentId == 0))
                throw new Himall.Core.HimallException("一级菜单的名称不能为空且在5个字符以内");
            if (model.Title.Length == 0 || (model.Title.Length > 7 && model.ParentId != 0))
                throw new Himall.Core.HimallException("二级菜单的名称不能为空且在5个字符以内");
            if ((Context.MenuInfo.Where(item => item.ParentId == 0&&item.ShopId==model.ShopId).Count() >= 3 && model.ParentId == 0) || (GetMenuByParentId(model.ParentId).Count() >= 5&&model.ParentId!=0))
                throw new Himall.Core.HimallException("微信自定义菜单最多允许三个一级菜单，一级菜单下最多运行5个二级菜单");
            else
            {
                model.Platform = PlatformType.WeiXin;
                Context.MenuInfo.Add(model);
            }
            Context.SaveChanges();
        }

        public void UpdateMenu(MenuInfo model)
        {
            if (model.Id < 0 )
                throw new Himall.Core.HimallException("微信自定义菜单的ID有误");
            if (model.ParentId < 0)
                throw new Himall.Core.HimallException("微信自定义菜单二级菜单必须指定一个一级菜单");
            if (model.Title.Length == 0 || (model.Title.Length > 5 && model.ParentId == 0))
                throw new Himall.Core.HimallException("一级菜单的名称不能为空且在5个字符以内");
            if (model.Title.Length == 0 || (model.Title.Length > 7 && model.ParentId != 0))
                throw new Himall.Core.HimallException("二级菜单的名称不能为空且在5个字符以内");
            var menu = Context.MenuInfo.FindById(model.Id);
            if (model.ParentId == 0 && GetMenuByParentId(model.Id).Count() > 0 && model.UrlType != MenuInfo.UrlTypes.Nothing)
                throw new Himall.Core.HimallException("一级菜单下有二级菜单，不允许绑定链接");
            menu.ParentId = model.ParentId;
            menu.Title = model.Title;
            menu.Url = model.Url;
            menu.UrlType = model.UrlType;
            menu.Platform = PlatformType.WeiXin;
            Context.SaveChanges();
        }

        public void DeleteMenu(long id)
        {
            Context.MenuInfo.Remove(a => a.Id == id || a.ParentId == id);
            Context.SaveChanges();
        }

        public void ConsistentToWeixin(long shopId)
        {
            string appId=string.Empty;
            string appSecret=string.Empty;
            if (shopId == 0)
            {
                var siteSettings = ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettings();
                if (String.IsNullOrEmpty(siteSettings.WeixinAppId) || String.IsNullOrEmpty(siteSettings.WeixinAppSecret))
                    throw new Himall.Core.HimallException("您的服务号配置存在问题，请您先检查配置！");
                appId = siteSettings.WeixinAppId;
                appSecret = siteSettings.WeixinAppSecret;
            }
            if(shopId>0)
            {
                var vshopSetting = ServiceProvider.Instance<IVShopService>.Create.GetVShopSetting(shopId);
                if (String.IsNullOrEmpty(vshopSetting.AppId) || String.IsNullOrEmpty(vshopSetting.AppSecret))
                    throw new Himall.Core.HimallException("您的服务号配置存在问题，请您先检查配置！");
                appId = vshopSetting.AppId;
                appSecret = vshopSetting.AppSecret;
            }
            //TODO：统一方式取Token
            string access_token = "";
            try
            {
                access_token = AccessTokenContainer.TryGetToken(appId, appSecret);
            }
            catch(Exception ex)
            {
                Log.Error("[WXACT]appId=" + appId + ";appSecret=" + appSecret + ";" + ex.Message);
                access_token = "";
            }
            if (string.IsNullOrWhiteSpace(access_token))
            {
                //强制获取一次
                access_token = AccessTokenContainer.TryGetToken(appId, appSecret, true);
                Log.Error("[WXACT]强制-appId=" + appId + ";appSecret=" + appSecret + ";");
            }
            if (string.IsNullOrWhiteSpace(access_token))
            {
                throw new HimallException("获取Access Token失败！");
            }
            IQueryable<MenuInfo> menus = GetAllMenu(shopId);
            if (menus == null)
                throw new HimallException("你还没有添加菜单！");
            var mainMenus = menus.Where(item => item.ParentId == 0).ToList();
            foreach( var menu in mainMenus)
            {
                if (GetMenuByParentId(menu.Id).Count() == 0 && menu.UrlType == MenuInfo.UrlTypes.Nothing)
                    throw new HimallException("你有一级菜单下没有二级菜单并且也没有绑定链接");
            }
            Hishop.Weixin.MP.Domain.Menu.Menu root = new Hishop.Weixin.MP.Domain.Menu.Menu();
            foreach (var top in mainMenus)
            {
                if (GetMenuByParentId(top.Id).Count() == 0)
                {
                    root.menu.button.Add(BuildMenu(top));
                }
                else
                {
                    var btn = new Hishop.Weixin.MP.Domain.Menu.SubMenu() { name = top.Title };
                    //var menuInfos = GetMenuByParentId(top.Id);
                    foreach (var sub in GetMenuByParentId(top.Id))
                    {
                        btn.sub_button.Add(BuildMenu(sub));
                    }
                    root.menu.button.Add(btn);
                }

            }

            string json = JsonConvert.SerializeObject(root.menu);
            string resp = Hishop.Weixin.MP.Api.MenuApi.CreateMenus(access_token, json);
            Core.Log.Info("微信菜单：" + json);
            if (!resp.Contains("ok"))
            {
                Core.Log.Info("微信菜单同步错误,返回内容：" + resp);
                throw new Himall.Core.HimallException("服务号配置信息错误或没有微信自定义菜单权限，请检查配置信息以及菜单的长度。");
            }
        }

        private Hishop.Weixin.MP.Domain.Menu.SingleButton BuildMenu(MenuInfo menu)
        {

            return new Hishop.Weixin.MP.Domain.Menu.SingleViewButton
            {
                name = menu.Title,
                url = menu.Url
            };
        }
    }
}
