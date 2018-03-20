using Himall.Core.Plugins.Message;
using Himall.MessagePlugin;
using Himall.Plugin.Message.SMS;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Himall.Plugin.Message.SMS
{
    public class Service : ISMSPlugin
    {
        MessageStatus messageStatus;
        Dictionary<MessageTypeEnum, StatusEnum> dic = new Dictionary<MessageTypeEnum, StatusEnum>();
        public Service()
        {
            if (!string.IsNullOrEmpty(SMSCore.WorkDirectory))
            {
                InitMessageStatus();
            }
        }

        void InitMessageStatus()
        {
            //DirectoryInfo dir = new DirectoryInfo(SMSCore.WorkDirectory);
            ////查找该目录下的
            //var configFile = dir.GetFiles("Data/config.xml").FirstOrDefault();
            //if (configFile != null)
            //{
            //    using (FileStream fs = new FileStream(configFile.FullName, FileMode.Open))
            //    {
            //        XmlSerializer xs = new XmlSerializer(typeof(MessageStatus));
            //        messageStatus = (MessageStatus)xs.Deserialize(fs);
            //        dic.Clear();
            //        dic.Add(MessageTypeEnum.FindPassWord, (StatusEnum)messageStatus.FindPassWord);
            //        dic.Add(MessageTypeEnum.OrderCreated, (StatusEnum)messageStatus.OrderCreated);
            //        dic.Add(MessageTypeEnum.OrderPay, (StatusEnum)messageStatus.OrderPay);
            //        dic.Add(MessageTypeEnum.OrderRefund, (StatusEnum)messageStatus.OrderRefund);
            //        dic.Add(MessageTypeEnum.OrderShipping, (StatusEnum)messageStatus.OrderShipping);
            //        dic.Add(MessageTypeEnum.ShopAudited, (StatusEnum)messageStatus.ShopAudited);
            //        //2.4删除
            //        //  dic.Add(MessageTypeEnum.ShopSuccess, (StatusEnum)messageStatus.ShopSuccess);去掉
            //        dic.Add(MessageTypeEnum.ShopHaveNewOrder, (StatusEnum)messageStatus.ShopHaveNewOrder);
            //        dic.Add(MessageTypeEnum.ReceiveBonus, (StatusEnum)messageStatus.ReceiveBonus);
            //        dic.Add(MessageTypeEnum.LimitTimeBuy, (StatusEnum)messageStatus.LimitTimeBuy);
            //        dic.Add(MessageTypeEnum.SubscribeLimitTimeBuy, (StatusEnum)messageStatus.SubscribeLimitTimeBuy);
            //        dic.Add(MessageTypeEnum.RefundDeliver, (StatusEnum)messageStatus.RefundDeliver);

            //        #region 拼团
            //        dic.Add(MessageTypeEnum.FightGroupOpenSuccess, (StatusEnum)messageStatus.FightGroupOpenSuccess);
            //        dic.Add(MessageTypeEnum.FightGroupJoinSuccess, (StatusEnum)messageStatus.FightGroupJoinSuccess);
            //        dic.Add(MessageTypeEnum.FightGroupNewJoin, (StatusEnum)messageStatus.FightGroupNewJoin);
            //        dic.Add(MessageTypeEnum.FightGroupFailed, (StatusEnum)messageStatus.FightGroupFailed);
            //        dic.Add(MessageTypeEnum.FightGroupSuccess, (StatusEnum)messageStatus.FightGroupSuccess);
            //        #endregion
            //        //发送优惠券
            //        dic.Add(MessageTypeEnum.SendCouponSuccess, (StatusEnum)messageStatus.SendCouponSuccess);
            //    }
            //}

            DirectoryInfo dir = new DirectoryInfo(SMSCore.WorkDirectory);
            //查找该目录下的
            messageStatus = SMSCore.GetMessageStatus();

            if (messageStatus != null)
            {
                dic.Clear();
                dic.Add(MessageTypeEnum.FindPassWord, (StatusEnum)messageStatus.FindPassWord);
                dic.Add(MessageTypeEnum.OrderCreated, (StatusEnum)messageStatus.OrderCreated);
                dic.Add(MessageTypeEnum.OrderPay, (StatusEnum)messageStatus.OrderPay);
                dic.Add(MessageTypeEnum.OrderRefund, (StatusEnum)messageStatus.OrderRefund);
                dic.Add(MessageTypeEnum.OrderShipping, (StatusEnum)messageStatus.OrderShipping);
                dic.Add(MessageTypeEnum.ShopAudited, (StatusEnum)messageStatus.ShopAudited);
                //2.4删除
                //  dic.Add(MessageTypeEnum.ShopSuccess, (StatusEnum)messageStatus.ShopSuccess);去掉
                dic.Add(MessageTypeEnum.ShopHaveNewOrder, (StatusEnum)messageStatus.ShopHaveNewOrder);
                dic.Add(MessageTypeEnum.ReceiveBonus, (StatusEnum)messageStatus.ReceiveBonus);
                dic.Add(MessageTypeEnum.LimitTimeBuy, (StatusEnum)messageStatus.LimitTimeBuy);
                dic.Add(MessageTypeEnum.SubscribeLimitTimeBuy, (StatusEnum)messageStatus.SubscribeLimitTimeBuy);
                dic.Add(MessageTypeEnum.RefundDeliver, (StatusEnum)messageStatus.RefundDeliver);

                #region 拼团
                dic.Add(MessageTypeEnum.FightGroupOpenSuccess, (StatusEnum)messageStatus.FightGroupOpenSuccess);
                dic.Add(MessageTypeEnum.FightGroupJoinSuccess, (StatusEnum)messageStatus.FightGroupJoinSuccess);
                dic.Add(MessageTypeEnum.FightGroupNewJoin, (StatusEnum)messageStatus.FightGroupNewJoin);
                dic.Add(MessageTypeEnum.FightGroupFailed, (StatusEnum)messageStatus.FightGroupFailed);
                dic.Add(MessageTypeEnum.FightGroupSuccess, (StatusEnum)messageStatus.FightGroupSuccess);
                #endregion
                //发送优惠券
                dic.Add(MessageTypeEnum.SendCouponSuccess, (StatusEnum)messageStatus.SendCouponSuccess);
            }
        }

        public string WorkDirectory
        {
            set { SMSCore.WorkDirectory = value; }
        }

        public void CheckCanEnable()
        {
            MessageSMSConfig config = SMSCore.GetConfig();
            if (string.IsNullOrWhiteSpace(config.AppKey))
                throw new Himall.Core.PluginConfigException("未设置AppKey");

            if (string.IsNullOrWhiteSpace(config.AppSecret))
                throw new Himall.Core.PluginConfigException("未设置AppSecret");
        }


        public Core.Plugins.FormData GetFormData()
        {

            var config = SMSCore.GetConfig();

            var formData = new Core.Plugins.FormData()
            {
                Items = new Core.Plugins.FormData.FormItem[] { 
                   //AppKey
                   new  Core.Plugins.FormData.FormItem(){
                     DisplayName = "AppKey",
                     Name = "AppKey",
                     IsRequired = true,
                      Type= Core.Plugins.FormData.FormItemType.text,
                      Value=config.AppKey
                   },
                     new  Core.Plugins.FormData.FormItem(){
                     DisplayName = "AppSecret",
                     Name = "AppSecret",
                     IsRequired = true,
                       Type= Core.Plugins.FormData.FormItemType.text,
                       Value=config.AppSecret
                   }
                }
            };
            return formData;
        }



        public void SetFormValues(IEnumerable<KeyValuePair<string, string>> values)
        {
            var appKeyItem = values.FirstOrDefault(item => item.Key == "AppKey");
            if (string.IsNullOrWhiteSpace(appKeyItem.Value))
                throw new Himall.Core.PluginConfigException("AppKey不能为空");
            var appSecretItem = values.FirstOrDefault(item => item.Key == "AppSecret");
            if (string.IsNullOrWhiteSpace(appSecretItem.Value))
                throw new Himall.Core.PluginConfigException("AppSecret不能为空");
            MessageSMSConfig oldConfig = SMSCore.GetConfig();
            oldConfig.AppKey = appKeyItem.Value;
            oldConfig.AppSecret = appSecretItem.Value;
            SMSCore.SaveConfig(oldConfig);
        }

        public void Disable(MessageTypeEnum e)
        {
            CheckCanEnable();
            if (dic.Where(a => a.Key == e).FirstOrDefault().Value == Himall.Core.Plugins.Message.StatusEnum.Disable)
            {
                throw new Himall.Core.HimallException("该功能已被禁止，不能进行设置");
            }
            SetMessageStatus(e, StatusEnum.Close);
            SMSCore.SaveMessageStatus(messageStatus);

        }
        void SetMessageStatus(MessageTypeEnum e, StatusEnum s)
        {
            switch (e)
            {
                case MessageTypeEnum.OrderCreated:
                    messageStatus.OrderCreated = (int)s;
                    break;
                case MessageTypeEnum.FindPassWord:
                    messageStatus.FindPassWord = (int)s;
                    break;
                case MessageTypeEnum.OrderPay:
                    messageStatus.OrderPay = (int)s;
                    break;
                case MessageTypeEnum.OrderRefund:
                    messageStatus.OrderRefund = (int)s;
                    break;
                case MessageTypeEnum.OrderShipping:
                    messageStatus.OrderShipping = (int)s;
                    break;
                case MessageTypeEnum.ShopAudited:
                    messageStatus.ShopAudited = (int)s;
                    break;
                //2.4删除
                //case MessageTypeEnum.ShopSuccess:
                //    messageStatus.ShopSuccess = (int)s;
                //    break;
                case MessageTypeEnum.RefundDeliver:
                    messageStatus.RefundDeliver = (int)s;
                    break;
                case MessageTypeEnum.FightGroupOpenSuccess:
                    messageStatus.FightGroupOpenSuccess = (int)s;
                    break;
                case MessageTypeEnum.FightGroupJoinSuccess:
                    messageStatus.FightGroupJoinSuccess = (int)s;
                    break;
                case MessageTypeEnum.FightGroupNewJoin:
                    messageStatus.FightGroupNewJoin = (int)s;
                    break;
                case MessageTypeEnum.FightGroupFailed:
                    messageStatus.FightGroupFailed = (int)s;
                    break;
                case MessageTypeEnum.FightGroupSuccess:
                    messageStatus.FightGroupSuccess = (int)s;
                    break;
                case MessageTypeEnum.SendCouponSuccess://发送优惠券
                    messageStatus.SendCouponSuccess = (int)s;
                    break;
            }
        }
        public void Enable(MessageTypeEnum e)
        {
            CheckCanEnable();
            if (dic.Where(a => a.Key == e).FirstOrDefault().Value == Himall.Core.Plugins.Message.StatusEnum.Disable)
            {
                throw new Himall.Core.HimallException("该功能已被禁止，不能进行设置");
            }
            SetMessageStatus(e, StatusEnum.Open);
            SMSCore.SaveMessageStatus(messageStatus);
        }

        public StatusEnum GetStatus(MessageTypeEnum e)
        {
            InitMessageStatus();
            return dic.FirstOrDefault(a => a.Key == e).Value;
        }

        public string Logo
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SMSCore.WorkDirectory))
                    throw new MissingFieldException("没有设置插件工作目录");
                return SMSCore.WorkDirectory + "/Data/logo.png";
            }
        }

        public Dictionary<MessageTypeEnum, StatusEnum> GetAllStatus()
        {
            InitMessageStatus();
            return dic;
        }

        public string SendMessageCode(string destination, MessageUserInfo info)
        {
            var config = SMSCore.GetMessageContentConfig();
            var text = config.Bind.Replace("#userName#", info.UserName).Replace("#checkCode#", info.CheckCode).Replace("#siteName#", info.SiteName);
            SendMessage(destination, text, "2");
            return text;
        }

        private string SendMessage(string destination, string text, string speed = "0")
        {
            if (!string.IsNullOrWhiteSpace(destination))
            {
                var config = SMSCore.GetConfig();
                SortedDictionary<string, string> tmpParas = new SortedDictionary<string, string>();
                tmpParas.Add("mobiles", destination);
                tmpParas.Add("text", text);
                tmpParas.Add("appkey", config.AppKey);
                tmpParas.Add("sendtime", DateTime.Now.ToString());
                tmpParas.Add("speed", speed);
                Dictionary<string, string> paras = SMSAPiHelper.Parameterfilter(tmpParas);
                string sign = SMSAPiHelper.BuildSign(paras, config.AppSecret, "MD5", "utf-8");
                paras.Add("sign", sign);
                paras.Add("sign_type", "MD5");
                string postdata = SMSAPiHelper.CreateLinkstring(paras);
                return SMSAPiHelper.PostData("http://sms.kuaidiantong.cn/SendMsg.aspx", postdata);
            }
            return "发送目标不能为空！";
        }

        private void BatchSendMessage(string[] destination, string text)
        {
            if (destination.Length > 0)
            {
                var config = SMSCore.GetConfig();
                SortedDictionary<string, string> tmpParas = new SortedDictionary<string, string>();
                tmpParas.Add("mobiles", string.Join(",", destination));
                tmpParas.Add("text", text);
                tmpParas.Add("appkey", config.AppKey);
                tmpParas.Add("sendtime", DateTime.Now.ToString());
                tmpParas.Add("speed", "1");
                Dictionary<string, string> paras = SMSAPiHelper.Parameterfilter(tmpParas);
                string sign = SMSAPiHelper.BuildSign(paras, config.AppSecret, "MD5", "utf-8");
                paras.Add("sign", sign);
                paras.Add("sign_type", "MD5");
                string postdata = SMSAPiHelper.CreateLinkstring(paras);
                SMSAPiHelper.PostData("http://sms.kuaidiantong.cn/SendMsg.aspx", postdata);
            }
        }


        public string SendMessageOnFindPassWord(string destination, MessageUserInfo info)
        {
            var config = SMSCore.GetMessageContentConfig();
            var text = config.FindPassWord.Replace("#userName#", info.UserName).Replace("#checkCode#", info.CheckCode).Replace("#siteName#", info.SiteName);
            SendMessage(destination, text, "2");
            return text;
        }

        public string SendMessageOnOrderCreate(string destination, MessageOrderInfo info)
        {
            var config = SMSCore.GetMessageContentConfig();
            var text = config.OrderCreated.Replace("#userName#", info.UserName).Replace("#orderId#", info.OrderId).Replace("#siteName#", info.SiteName);
            SendMessage(destination, text);
            return text;
        }

        public string SendMessageOnOrderPay(string destination, MessageOrderInfo info)
        {
            var config = SMSCore.GetMessageContentConfig();
            var text = config.OrderPay.Replace("#userName#", info.UserName).Replace("#orderId#", info.OrderId).Replace("#siteName#", info.SiteName).Replace("#Total#", info.TotalMoney.ToString("F2"));
            SendMessage(destination, text);
            return text;
        }

        public string SendMessageOnOrderRefund(string destination, MessageOrderInfo info)
        {
            var config = SMSCore.GetMessageContentConfig();
            var text = config.OrderRefund.Replace("#userName#", info.UserName).Replace("#orderId#", info.OrderId).Replace("#siteName#", info.SiteName).Replace("#RefundMoney#", info.RefundMoney.ToString("F2"));
            SendMessage(destination, text);
            return text;
        }
        public string SendMessageOnRefundDeliver(string destination, MessageOrderInfo info)
        {
            var config = SMSCore.GetMessageContentConfig();
            var text = config.RefundDeliver.Replace("#userName#", info.UserName).Replace("#orderId#", info.OrderId).Replace("#siteName#", info.SiteName).Replace("#RefundMoney#", info.RefundMoney.ToString("F2"));
            SendMessage(destination, text);
            return text;
        }

        public string SendMessageOnOrderShipping(string destination, MessageOrderInfo info)
        {
            var config = SMSCore.GetMessageContentConfig();
            var text = config.OrderShipping.Replace("#userName#", info.UserName).Replace("#orderId#", info.OrderId).Replace("#siteName#", info.SiteName).Replace("#shippingCompany#", info.ShippingCompany).Replace("#shippingNumber#", info.ShippingNumber);
            SendMessage(destination, text);
            return text;
        }

        public string SendMessageOnShopAudited(string destination, MessageShopInfo info)
        {
            var config = SMSCore.GetMessageContentConfig();
            var text = config.ShopAudited.Replace("#userName#", info.UserName).Replace("#shopName#", info.ShopName).Replace("#siteName#", info.SiteName);
            SendMessage(destination, text);
            return text;
        }

        public string SendMessageOnShopSuccess(string destination, MessageShopInfo info)
        {
            var config = SMSCore.GetMessageContentConfig();
            var text = config.ShopSuccess.Replace("#userName#", info.UserName).Replace("#shopName#", info.ShopName).Replace("#siteName#", info.SiteName);
            SendMessage(destination, text);
            return text;
        }

        public string SendMessageOnCouponSuccess(string destination, MessageCouponInfo info)
        {
            var config = SMSCore.GetMessageContentConfig();
            var text = config.SendCouponSuccess.Replace("#userName#", info.UserName).Replace("#Money#", info.Money.ToString("F2")).Replace("#Url#", info.Url).Replace("#siteName#", info.SiteName);
            SendMessage(destination, text);
            return text;
        }


        public void SendMessages(string[] destination, string content, string title)
        {
            var test = content;
            BatchSendMessage(destination, test);
        }

        public string SendTestMessage(string destination, string content, string title)
        {
            var test = content;
            return SendMessage(destination, test);
        }
        public void SetAllStatus(Dictionary<MessageTypeEnum, StatusEnum> dic)
        {
            foreach (var d in dic)
            {
                SetMessageStatus(d.Key, d.Value);
            }
            SMSCore.SaveMessageStatus(messageStatus);
        }
        public string ShortName
        {
            get { return "手机"; }
        }
        public bool EnableLog
        {
            get { return true; }
        }

        public string GetBuyLink()
        {
            return "http://sms.kuaidiantong.cn/SMSPackList.aspx";
        }

        public string GetLoginLink()
        {
            return "http://sms.kuaidiantong.cn/";
        }

        public string GetSMSAmount()
        {
            var config = SMSCore.GetConfig();
            string postdata = "method=getAmount&appkey=" + config.AppKey;
            return SMSAPiHelper.PostData("http://sms.kuaidiantong.cn/GetAmount.aspx", postdata);
        }


        public bool IsSettingsValid
        {
            get
            {
                try
                {
                    CheckCanEnable();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool CheckDestination(string destination)
        {
            return Core.Helper.ValidateHelper.IsMobile(destination);
        }



    }
}


