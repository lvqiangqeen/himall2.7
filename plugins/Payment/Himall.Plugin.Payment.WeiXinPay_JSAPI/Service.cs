using Himall.Core;
using Himall.Core.Plugins;
using Himall.Core.Plugins.Payment;
using Himall.PaymentPlugin;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Himall.Plugin.Payment.WeiXinPay_JSAPI
{
    public class Service : PaymentBase<Config>, IPaymentPlugin
    {
        public string ConfirmPayResult()
        {
            return "SUCCESS";
        }

        public FormData GetFormData()
        {
            Config config = Utility<Config>.GetConfig();

            var formData = new FormData()
            {
                Items = new FormData.FormItem[] { 
                    //AppId
                   new  FormData.FormItem(){
                     DisplayName = "AppId",
                     Name = "AppId",
                     IsRequired = true,
                      Type= FormData.FormItemType.text,
                      Value=config.AppId
                   },
                   //AppSecret
                   new  FormData.FormItem(){
                     DisplayName = "AppSecret",
                     Name = "AppSecret",
                     IsRequired = true,
                      Type= FormData.FormItemType.text,
                      Value=config.AppSecret
                   },
                   //MCHID
                   new  FormData.FormItem(){
                     DisplayName = "Key",
                     Name = "Key",
                     IsRequired = true,
                      Type= FormData.FormItemType.text,
                      Value=config.Key
                   },
                  //AppId
                   new  FormData.FormItem(){
                     DisplayName = "MCHID",
                     Name = "MCHID",
                     IsRequired = true,
                      Type= FormData.FormItemType.text,
                      Value=config.MCHID
                   },
                }
            };
            return formData;
        }

        public string GetRequestUrl(string returnUrl, string notifyUrl, string orderId, decimal totalFee, string productInfo)
        {
            throw new System.NotImplementedException();
        }

        string _logo;

        public string Logo
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_logo))
                    _logo = Utility<Config>.GetConfig().Logo;
                return _logo;
            }
            set
            {
                _logo = value;
            }
        }

        public string PluginListUrl
        {
            set { throw new System.NotImplementedException(); }
        }

        public PaymentInfo ProcessNotify(NameValueCollection queryString)
        {
            throw new System.NotImplementedException();
        }

        public PaymentInfo ProcessReturn(NameValueCollection queryString)
        {
            throw new System.NotImplementedException();
        }

        public void SetFormValues(IEnumerable<KeyValuePair<string, string>> values)
        {
            var appIdItem = values.FirstOrDefault(item => item.Key == "AppId");
            if (string.IsNullOrWhiteSpace(appIdItem.Value))
                throw new ArgumentNullException("合作者身份AppId不能为空");

            var appSecretItem = values.FirstOrDefault(item => item.Key == "AppSecret");
            if (string.IsNullOrWhiteSpace(appSecretItem.Value))
                throw new ArgumentNullException("AppSecret不能为空");

            var keyItem = values.FirstOrDefault(item => item.Key == "Key");
            if (string.IsNullOrWhiteSpace(keyItem.Value))
                throw new ArgumentNullException("Key不能为空");

            var MCHIDItem = values.FirstOrDefault(item => item.Key == "MCHID");
            if (string.IsNullOrWhiteSpace(MCHIDItem.Value))
                throw new ArgumentNullException("MCHID不能为空");


            Config oldConfig = Utility<Config>.GetConfig();
            oldConfig.AppId = appIdItem.Value;
            oldConfig.Key = keyItem.Value;
            oldConfig.AppSecret = appSecretItem.Value;
            oldConfig.MCHID = MCHIDItem.Value;
            Utility<Config>.SaveConfig(oldConfig);
        }

        public void CheckCanEnable()
        {
            Config config = Utility<Config>.GetConfig();
            if (string.IsNullOrWhiteSpace(config.AppId))
                throw new PluginConfigException("未设置AppId");
            if (string.IsNullOrWhiteSpace(config.AppSecret))
                throw new PluginConfigException("未设置AppSecret");
            if (string.IsNullOrWhiteSpace(config.Key))
                throw new PluginConfigException("未设置Key");
            if (string.IsNullOrWhiteSpace(config.MCHID))
                throw new PluginConfigException("未设置MCHID");
        }

        public string WorkDirectory
        {
            set { Config.WorkDirectory = value; }
        }
    }
}
