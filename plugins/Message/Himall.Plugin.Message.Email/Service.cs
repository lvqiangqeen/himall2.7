using Himall.Core;
using Himall.Core.Plugins.Message;
using Himall.MessagePlugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Himall.Plugin.Message.Email
{
    public class Service : IEmailPlugin
    {
        MessageStatus messageStatus;
        Dictionary<MessageTypeEnum, StatusEnum> dic = new Dictionary<MessageTypeEnum, StatusEnum>();
        public Service()
        {
            if (!string.IsNullOrEmpty(EmailCore.WorkDirectory))
            {
                InitMessageStatus();
            }
        }

        void InitMessageStatus()
        {
            #region 原读取本地方法
            //DirectoryInfo dir = new DirectoryInfo(EmailCore.WorkDirectory);
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
            //        // dic.Add(MessageTypeEnum.ShopSuccess, (StatusEnum)messageStatus.ShopSuccess);去掉
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
            #endregion

            messageStatus = EmailCore.GetMessageStatus();
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
                // dic.Add(MessageTypeEnum.ShopSuccess, (StatusEnum)messageStatus.ShopSuccess);去掉
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
            set { EmailCore.WorkDirectory = value; }
        }

        public void CheckCanEnable()
        {
            MessageEmailConfig config = EmailCore.GetConfig();
            if (string.IsNullOrWhiteSpace(config.SendAddress))
                throw new Himall.Core.PluginConfigException("未设置SMTP邮件地址");
            if (!Core.Helper.ValidateHelper.IsEmail(config.SendAddress))
            {
                throw new Himall.Core.PluginConfigException("SMTP用户名填写错误");
            }
            if (string.IsNullOrWhiteSpace(config.EmailName))
                throw new Himall.Core.PluginConfigException("未设置SMTP邮箱用户名");
            if (string.IsNullOrWhiteSpace(config.Password))
                throw new Himall.Core.PluginConfigException("未设置SMTP邮箱密码");
            if (!Core.Helper.ValidateHelper.IsNumeric(config.SmtpPort))
                throw new Himall.Core.PluginConfigException("SMTP端口错误");
            if (string.IsNullOrWhiteSpace(config.SmtpServer))
                throw new Himall.Core.PluginConfigException("未设置SMTP邮箱服务器");
            if (string.IsNullOrWhiteSpace(config.DisplayName))
                throw new Himall.Core.PluginConfigException("未设置SMTP邮箱显示名称");
        }


        public Core.Plugins.FormData GetFormData()
        {

            var config = EmailCore.GetConfig();

            var formData = new Core.Plugins.FormData()
            {
                Items = new Core.Plugins.FormData.FormItem[] { 
                   //SmtpServer
                   new  Core.Plugins.FormData.FormItem(){
                     DisplayName = "SMTP服务器",
                     Name = "SmtpServer",
                     IsRequired = true,
                      Type= Core.Plugins.FormData.FormItemType.text,
                      Value=config.SmtpServer
                   },
                     new  Core.Plugins.FormData.FormItem(){
                     DisplayName = "SMTP服务器端口",
                     Name = "SmtpPort",
                     IsRequired = true,
                       Type= Core.Plugins.FormData.FormItemType.text,
                       Value=config.SmtpPort
                   },
                    new  Core.Plugins.FormData.FormItem(){
                     DisplayName = "SMTP用户名",
                     Name = "EmailName",
                     IsRequired = true,
                       Type= Core.Plugins.FormData.FormItemType.text,
                       Value=config.EmailName
                   },
                    new  Core.Plugins.FormData.FormItem(){
                     DisplayName = "SMTP用户密码",
                     Name = "Password",
                     IsRequired = true,
                       Type= Core.Plugins.FormData.FormItemType.password,
                       Value=config.Password
                   },
                    new  Core.Plugins.FormData.FormItem(){
                     DisplayName = "SMTP邮箱",
                     Name = "SendAddress",
                     IsRequired = true,
                       Type= Core.Plugins.FormData.FormItemType.text,
                       Value=config.SendAddress
                   },
                    new  Core.Plugins.FormData.FormItem(){
                     DisplayName = "显示名称",
                     Name = "DisplayName",
                     IsRequired = true,
                       Type= Core.Plugins.FormData.FormItemType.text,
                       Value=config.DisplayName
                   }
                }
            };
            return formData;
        }



        public void SetFormValues(IEnumerable<KeyValuePair<string, string>> values)
        {

            var SmtpServerItem = values.FirstOrDefault(item => item.Key == "SmtpServer");
            if (string.IsNullOrWhiteSpace(SmtpServerItem.Value))
                throw new Himall.Core.PluginConfigException("SMTP服务器不能为空");

            var SmtpPortItem = values.FirstOrDefault(item => item.Key == "SmtpPort");
            if (!Core.Helper.ValidateHelper.IsNumeric(SmtpPortItem.Value))
                throw new Himall.Core.PluginConfigException("SMTP端口错误");

            var EmailNameItem = values.FirstOrDefault(item => item.Key == "EmailName");
            if (string.IsNullOrWhiteSpace(EmailNameItem.Value))
                throw new Himall.Core.PluginConfigException("SMTP用户名不能为空");

            var PasswordItem = values.FirstOrDefault(item => item.Key == "Password");
            if (string.IsNullOrWhiteSpace(PasswordItem.Value))
                throw new Himall.Core.PluginConfigException("SMTP密码不能为空");

            var SendAddressItem = values.FirstOrDefault(item => item.Key == "SendAddress");
            if (string.IsNullOrWhiteSpace(SendAddressItem.Value))
                throw new Himall.Core.PluginConfigException("SMTP邮箱不能为空");

            if (!Core.Helper.ValidateHelper.IsEmail(SendAddressItem.Value))
                throw new Himall.Core.PluginConfigException("SMTP邮箱错误");
            var DisplayNameItem = values.FirstOrDefault(item => item.Key == "DisplayName");
            if (string.IsNullOrWhiteSpace(DisplayNameItem.Value))
                throw new Himall.Core.PluginConfigException("SMTP显示名称不能为空");
            MessageEmailConfig oldConfig = EmailCore.GetConfig();
            oldConfig.SmtpPort = SmtpPortItem.Value;
            oldConfig.SmtpServer = SmtpServerItem.Value;
            oldConfig.EmailName = EmailNameItem.Value;
            oldConfig.Password = PasswordItem.Value;
            oldConfig.SendAddress = SendAddressItem.Value;
            oldConfig.DisplayName = DisplayNameItem.Value;
            EmailCore.SaveConfig(oldConfig);
        }

        public void Disable(MessageTypeEnum e)
        {
            CheckCanEnable();
            if (dic.Where(a => a.Key == e).FirstOrDefault().Value == Himall.Core.Plugins.Message.StatusEnum.Disable)
            {
                throw new Himall.Core.HimallException("该功能已被禁止，不能进行设置");
            }
            SetMessageStatus(e, StatusEnum.Close);

            EmailCore.SaveMessageStatus(messageStatus);

            //using (FileStream fs = new FileStream(EmailCore.WorkDirectory + "/Data/config.xml", FileMode.Create))
            //{
            //    XmlSerializer xs = new XmlSerializer(typeof(MessageStatus));
            //    xs.Serialize(fs, messageStatus);
            //}
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
                case MessageTypeEnum.SendCouponSuccess:
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
            //using (FileStream fs = new FileStream(EmailCore.WorkDirectory + "/Data/config.xml", FileMode.Create))
            //{
            //    XmlSerializer xs = new XmlSerializer(typeof(MessageStatus));
            //    xs.Serialize(fs, messageStatus);
            //}

            EmailCore.SaveMessageStatus(messageStatus);
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
                if (string.IsNullOrWhiteSpace(EmailCore.WorkDirectory))
                    throw new MissingFieldException("没有设置插件工作目录");
                return EmailCore.WorkDirectory + "/Data/logo.png";
            }
        }



        public Dictionary<MessageTypeEnum, StatusEnum> GetAllStatus()
        {
            InitMessageStatus();
            return dic;
        }

        private void SendMessage(string[] destination, string title, string body, bool async = false)
        {
            if (destination.Length > 0)
            {
                SendMail mail = new SendMail();
                mail.SendEmail(title, destination, body, async);
            }
        }

        private void SendMessage(string destination, string title, string body, bool async = false)
        {
            if (!string.IsNullOrWhiteSpace(destination))
            {
                SendMail mail = new SendMail();
                string[] mailto = new string[] { destination };
                mail.SendEmail(title, mailto, body, async);
            }
        }

        public string SendMessageCode(string destination, MessageUserInfo info)
        {
            var config = EmailCore.GetMessageContentConfig();
            var text = config.Bind.Replace("#userName#", info.UserName).Replace("#checkCode#", info.CheckCode).Replace("#siteName#", info.SiteName);
            SendMessage(destination, info.SiteName + "邮箱验证码", text);
            return text;
        }

        public string SendMessageOnFindPassWord(string destination, MessageUserInfo info)
        {
            var config = EmailCore.GetMessageContentConfig();
            var text = config.FindPassWord.Replace("#userName#", info.UserName).Replace("#checkCode#", info.CheckCode).Replace("#siteName#", info.SiteName);
            SendMessage(destination, info.SiteName + "找回密码验证", text);
            return text;
        }

        public string SendMessageOnOrderCreate(string destination, MessageOrderInfo info)
        {
            var config = EmailCore.GetMessageContentConfig();
            var text = config.OrderCreated.Replace("#userName#", info.UserName).Replace("#orderId#", info.OrderId).Replace("#siteName#", info.SiteName);
            SendMessage(destination, info.SiteName + "订单创建成功", text);
            return text;
        }

        public string SendMessageOnOrderPay(string destination, MessageOrderInfo info)
        {
            var config = EmailCore.GetMessageContentConfig();
            var text = config.OrderPay.Replace("#userName#", info.UserName).Replace("#orderId#", info.OrderId).Replace("#siteName#", info.SiteName).Replace("#Total#", info.TotalMoney.ToString("F2"));
            SendMessage(destination, info.SiteName + "订单支付成功", text);
            return text;
        }

        public string SendMessageOnOrderRefund(string destination, MessageOrderInfo info)
        {
            var config = EmailCore.GetMessageContentConfig();
            var text = config.OrderRefund.Replace("#userName#", info.UserName).Replace("#orderId#", info.OrderId).Replace("#siteName#", info.SiteName).Replace("#RefundMoney#", info.RefundMoney.ToString("F2"));
            SendMessage(destination, info.SiteName + "订单(" + info.OrderId + ")退款已处理", text);
            return text;
        }
        public string SendMessageOnRefundDeliver(string destination, MessageOrderInfo info)
        {
            var config = EmailCore.GetMessageContentConfig();
            var text = config.RefundDeliver.Replace("#userName#", info.UserName).Replace("#orderId#", info.OrderId).Replace("#siteName#", info.SiteName).Replace("#RefundMoney#", info.RefundMoney.ToString("F2"));
            SendMessage(destination, info.SiteName + "订单(" + info.OrderId + ")售后发货提醒已处理", text);
            return text;
        }

        public string SendMessageOnOrderShipping(string destination, MessageOrderInfo info)
        {
            var config = EmailCore.GetMessageContentConfig();
            var text = config.OrderShipping.Replace("#userName#", info.UserName).Replace("#orderId#", info.OrderId).Replace("#siteName#", info.SiteName).Replace("#shippingCompany#", info.ShippingCompany).Replace("#shippingNumber#", info.ShippingNumber);
            SendMessage(destination, info.SiteName + "订单(" + info.OrderId + ")已发货", text);
            return text;
        }

        public string SendMessageOnShopAudited(string destination, MessageShopInfo info)
        {
            var config = EmailCore.GetMessageContentConfig();
            var text = config.ShopAudited.Replace("#userName#", info.UserName).Replace("#shopName#", info.ShopName).Replace("#siteName#", info.SiteName);
            SendMessage(destination, info.SiteName + "店铺(" + info.ShopName + ")审核已审核", text);
            return text;
        }

        public string SendMessageOnShopSuccess(string destination, MessageShopInfo info)
        {
            var config = EmailCore.GetMessageContentConfig();
            var text = config.ShopSuccess.Replace("#userName#", info.UserName).Replace("#shopName#", info.ShopName).Replace("#siteName#", info.SiteName);
            SendMessage(destination, info.SiteName + "店铺(" + info.ShopName + ")已开通", text);
            return text;
        }


        public string SendMessageOnCouponSuccess(string destination, MessageCouponInfo info)
        {
            var config = EmailCore.GetMessageContentConfig();
            var text = config.SendCouponSuccess.Replace("#userName#", info.UserName).Replace("#Money#", info.Money.ToString("F2")).Replace("#Url#", info.Url);
            SendMessage(destination, info.SiteName + "优惠券已到帐", text);
            return text;
        }

        public void SendMessages(string[] destination, string content, string title)
        {
            var test = content;
            SendMessage(destination, title, test);
        }

        public string SendTestMessage(string destination, string content, string title)
        {
            string result = "发送成功";
            var test = content;
            try
            {
                SendMessage(destination, title, test, false);
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
        public void SetAllStatus(Dictionary<MessageTypeEnum, StatusEnum> dic)
        {
            foreach (var d in dic)
            {
                SetMessageStatus(d.Key, d.Value);
            }

            EmailCore.SaveMessageStatus(messageStatus);
        }


        public string ShortName
        {
            get { return "邮箱"; }
        }


        public bool EnableLog
        {
            get { return false; }
        }

        public bool CheckDestination(string destination)
        {
            return Core.Helper.ValidateHelper.IsEmail(destination);
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
    }
}
