namespace Hishop.Weixin.MP.Test
{
    using Hishop.Weixin.MP.Api;
    using Hishop.Weixin.MP.Domain.Menu;
    using System;
    using System.Web.Script.Serialization;

    public class Tests
    {
        private const string AppID = "wxe7322013e6e964b8";
        private const string AppSecret = "9e4e5617c1b543e3164befd1952716b0";

        public string CreateMenu()
        {
            string token = this.GetToken();
            string menuJson = this.GetMenuJson();
            return MenuApi.CreateMenus(token, menuJson);
        }

        public string DeleteMenu()
        {
            return MenuApi.DeleteMenus(this.GetToken());
        }

        public string GetMenu()
        {
            return MenuApi.GetMenus(this.GetToken());
        }

        public string GetMenuJson()
        {
            Hishop.Weixin.MP.Domain.Menu.Menu menu = new Hishop.Weixin.MP.Domain.Menu.Menu();
            SingleClickButton item = new SingleClickButton {
                name = "热卖商品",
                key = "123"
            };
            SingleClickButton button2 = new SingleClickButton {
                name = "推荐商品",
                key = "SINGER"
            };
            SingleViewButton button3 = new SingleViewButton {
                name = "会员卡",
                url = "www.baidu.com"
            };
            SingleViewButton button4 = new SingleViewButton {
                name = "积分商城",
                url = "www.baidu.com"
            };
            SubMenu menu2 = new SubMenu {
                name = "个人中心"
            };
            menu2.sub_button.Add(button3);
            menu2.sub_button.Add(button4);
            menu.menu.button.Add(item);
            menu.menu.button.Add(button2);
            menu.menu.button.Add(menu2);
            return new JavaScriptSerializer().Serialize(menu.menu);
        }

        public string GetToken()
        {
            string token = TokenApi.GetToken("wxe7322013e6e964b8", "9e4e5617c1b543e3164befd1952716b0");
            return new JavaScriptSerializer().Deserialize<Token>(token).access_token;
        }
    }
}

