using Himall.Core;
using Himall.Core.Helper;
using Himall.Core.Plugins.Message;
using Himall.Entity;
using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace Himall.Service
{
    public class MessageService : ServiceBase, IMessageService
    {

        public void UpdateMemberContacts(MemberContactsInfo info)
        {
            var exist = Context.MemberContactsInfo.FirstOrDefault(a => a.ServiceProvider == info.ServiceProvider && a.UserId == info.UserId && a.UserType == info.UserType);
            if (exist != null)
            {
                exist.Contact = info.Contact;
            }
            else
            {
                Context.MemberContactsInfo.Add(info);
            }
            var user = Context.UserMemberInfo.Where(a => a.Id == info.UserId).FirstOrDefault();
            if (user != null)
            {
                if (info.ServiceProvider == "Himall.Plugin.Message.SMS")
                {
                    user.CellPhone = info.Contact;
                }
                else
                {
                    user.Email = info.Contact;
                }
            }
            Context.SaveChanges();
            Core.Cache.Remove(CacheKeyCollection.Member(info.UserId));//移除用户缓存
        }

        public string GetDestination(long userId, string pluginId, MemberContactsInfo.UserTypes type)
        {
            var Destination = Context.MemberContactsInfo.Where(a => a.UserId == userId && a.ServiceProvider == pluginId && a.UserType == type).FirstOrDefault();
            if (Destination != null)
            {
                return Destination.Contact;
            }
            return "";
        }

        public string GetDestination(long userId, string pluginId)
        {
            var Destination = Context.MemberContactsInfo.Where(a => a.UserId == userId && a.ServiceProvider == pluginId).FirstOrDefault();
            if (Destination != null)
            {
                return Destination.Contact;
            }
            return "";
        }
        public MemberContactsInfo GetMemberContactsInfo(string pluginId, string contact, MemberContactsInfo.UserTypes type)
        {
            return Context.MemberContactsInfo.Where(a => a.ServiceProvider == pluginId && a.UserType == type && a.Contact == contact).FirstOrDefault();
        }

        public List<MemberContactsInfo> GetMemberContactsInfo(long UserId)
        {
            return Context.MemberContactsInfo.Where(a => a.UserId == UserId).ToList();
        }

        public void SendMessageCode(string destination, string pluginId, Core.Plugins.Message.MessageUserInfo info)
        {

            var messagePlugin = PluginsManagement.GetPlugin<IMessagePlugin>(pluginId);
            if (string.IsNullOrEmpty(destination) || !messagePlugin.Biz.CheckDestination(destination))
                throw new HimallException(messagePlugin.Biz.ShortName + "错误");
            var content = messagePlugin.Biz.SendMessageCode(destination, info);
            if (messagePlugin.Biz.EnableLog)
            {
                Context.MessageLog.Add(new MessageLog() { SendTime = DateTime.Now, ShopId = 0, MessageContent = content, TypeId = "短信" });
                Context.SaveChanges();
            }
        }

        public void SendMessageOnFindPassWord(long userId, Core.Plugins.Message.MessageUserInfo info)
        {
            var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
            foreach (var msg in message)
            {
                if (msg.Biz.GetStatus(MessageTypeEnum.FindPassWord) == StatusEnum.Open)
                {
                    string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactsInfo.UserTypes.General);
                    if (!msg.Biz.CheckDestination(destination))
                        throw new HimallException(msg.Biz.ShortName + "错误");
                    var content = msg.Biz.SendMessageOnFindPassWord(destination, info);
                    if (msg.Biz.EnableLog)
                    {
                        Context.MessageLog.Add(new MessageLog() { SendTime = DateTime.Now, ShopId = 0, MessageContent = content, TypeId = "短信" });
                        Context.SaveChanges();
                    }
                }
            }
        }
        /// <summary>
        /// 创建订单通知
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        public void SendMessageOnOrderCreate(long userId, MessageOrderInfo info)
        {
            var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();

            foreach (var msg in message)
            {
                if (msg.Biz.GetStatus(MessageTypeEnum.OrderCreated) == StatusEnum.Open)
                {
                    string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactsInfo.UserTypes.General);
                    if (!msg.Biz.CheckDestination(destination))
                        throw new HimallException(msg.Biz.ShortName + "错误");
                    var content = msg.Biz.SendMessageOnOrderCreate(destination, info);
                    if (msg.Biz.EnableLog)
                    {
                        Context.MessageLog.Add(new MessageLog() { SendTime = DateTime.Now, ShopId = info.ShopId, MessageContent = content, TypeId = "短信" });
                        Context.SaveChanges();
                    }
                }
            }

            #region 发送模板消息
            if (info.MsgOrderType == MessageOrderType.Normal)
            {
                var userinfo = Context.UserMemberInfo.FirstOrDefault(d => d.Id == userId);
                if (userinfo != null)
                {
                    var msgdata = new WX_MsgTemplateSendDataModel();
                    msgdata.first.value = "尊敬的（" + userinfo.Nick + "），您的订单信息如下：";
                    msgdata.first.color = "#000000";
                    msgdata.keyword1.value = info.OrderId.ToString();
                    msgdata.keyword1.color = "#FF0000";
                    msgdata.keyword2.value = info.Quantity.ToString();
                    msgdata.keyword2.color = "#000000";
                    msgdata.keyword3.value = info.TotalMoney.ToString();
                    msgdata.keyword3.color = "#FF0000";
                    msgdata.remark.value = "感谢您的光顾，支付完成后，我们将火速为您发货~~";
                    msgdata.remark.color = "#000000";
                    //处理url
                    var _iwxtser = Himall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                    string url = _iwxtser.GetMessageTemplateShowUrl(MessageTypeEnum.OrderCreated);
                    url = url.Replace("{id}", info.OrderId.ToString());
                    _iwxtser.SendMessageByTemplate(MessageTypeEnum.OrderCreated, userId, msgdata, url);
                }
            }
            else
            {
                var userinfo = Context.UserMemberInfo.FirstOrDefault(d => d.Id == userId);
                if (userinfo != null)
                {
                    var msgdata = new WX_MsgTemplateSendDataModel();     
            
                    msgdata.keyword1.value = info.OrderTime.ToString("yyyy-MM-dd HH:mm:ss");
                    msgdata.keyword1.color = "#173177";
                    msgdata.keyword2.value = info.TotalMoney.ToString("F2");
                    msgdata.keyword2.color = "#ff3300";
                    msgdata.keyword3.value = info.ProductName + " 等...";
                    msgdata.keyword3.color = "#173177";
                    msgdata.keyword4.value = info.OrderId;
                    msgdata.keyword4.color = "#173177";
                    msgdata.keyword5.value = "待支付";
                    msgdata.keyword5.color = "#173177";

                    //处理url
                    var _iwxtser = Himall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                    var _iwxmember = Himall.ServiceProvider.Instance<IMemberService>.Create;
                    string page = _iwxtser.GetWXAppletMessageTemplateShowUrl(MessageTypeEnum.OrderCreated);//小程序跳转地址
                    page = page.Replace("{id}", info.OrderId.ToString());

                    string openId = _iwxmember.GetMemberOpenIdInfoByuserIdAndType(userId, "WeiXinSmallProg").OpenId;//登录小程序的OpenId

                    string formid = "";
                    var formmodel = _iwxtser.GetWXAppletFromDataById(MessageTypeEnum.OrderCreated, info.OrderId.ToString());
                    if ( formmodel != null)
                        formid = formmodel.FormId;//根据OrderId获取FormId

                    Log.Info("小程序发送 page:" + page);

                    _iwxtser.SendAppletMessageByTemplate(MessageTypeEnum.OrderCreated, userId, msgdata, page, openId, formid);
                }

            }

            #endregion
        }
        /// <summary>
        /// 支付通知
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        public void SendMessageOnOrderPay(long userId, MessageOrderInfo info)
        {
            var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
            foreach (var msg in message)
            {
                if (msg.Biz.GetStatus(MessageTypeEnum.OrderPay) == StatusEnum.Open)
                {
                    string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactsInfo.UserTypes.General);
                    if (!msg.Biz.CheckDestination(destination))
                        throw new HimallException(msg.Biz.ShortName + "错误");
                    var content = msg.Biz.SendMessageOnOrderPay(destination, info);
                    if (msg.Biz.EnableLog)
                    {
                        Context.MessageLog.Add(new MessageLog() { SendTime = DateTime.Now, ShopId = info.ShopId, MessageContent = content, TypeId = "短信" });
                        Context.SaveChanges();
                    }
                }
            }

            #region 发送模板消息
            if (info.MsgOrderType == MessageOrderType.Normal)
            {
                var userinfo = Context.UserMemberInfo.FirstOrDefault(d => d.Id == userId);
                if (userinfo != null)
                {
                    var msgdata = new WX_MsgTemplateSendDataModel();
                    msgdata.first.value = "您好，您有一笔订单已经支付成功";
                    msgdata.first.color = "#000000";
                    msgdata.keyword1.value = info.OrderId.ToString();
                    msgdata.keyword1.color = "#000000";
                    msgdata.keyword2.value = info.PayTime.ToString();
                    msgdata.keyword2.color = "#000000";
                    msgdata.keyword3.value = info.TotalMoney.ToString();
                    msgdata.keyword3.color = "#FF0000";
                    msgdata.keyword4.value = info.PaymentType.ToString();
                    msgdata.keyword4.color = "#000000";
                    msgdata.remark.value = "感谢您的惠顾";
                    msgdata.remark.color = "#000000";
                    var _iwxtser = Himall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                    string url = _iwxtser.GetMessageTemplateShowUrl(MessageTypeEnum.OrderPay);
                    url = url.Replace("{id}", info.OrderId.ToString());
                    _iwxtser.SendMessageByTemplate(MessageTypeEnum.OrderPay, userId, msgdata, url);
                }
            }
            else
            {
                var userinfo = Context.UserMemberInfo.FirstOrDefault(d => d.Id == userId);
                if (userinfo != null)
                {
                    var msgdata = new WX_MsgTemplateSendDataModel();
                    msgdata.keyword1.value = info.PayTime.ToString("yyyy-MM-dd HH:mm:ss");
                    msgdata.keyword1.color = "#173177";
                    msgdata.keyword2.value = info.TotalMoney.ToString("f2")+"元";
                    msgdata.keyword2.color = "#ff3300";
                    msgdata.keyword3.value = info.ProductName.ToString() + "等...";
                    msgdata.keyword3.color = "#173177";
                    msgdata.keyword4.value = info.OrderId.ToString();
                    msgdata.keyword4.color = "#173177";
                    msgdata.keyword5.value = info.OrderTime.ToString("yyyy-MM-dd HH:mm:ss");
                    msgdata.keyword5.color = "#173177";
                    //处理url
                    var _iwxtser = Himall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                    var _iwxmember = Himall.ServiceProvider.Instance<IMemberService>.Create;
                    string page = _iwxtser.GetWXAppletMessageTemplateShowUrl(MessageTypeEnum.OrderPay);//小程序跳转地址
                    page = page.Replace("{id}", info.OrderId.ToString());

                    string openId = _iwxmember.GetMemberOpenIdInfoByuserIdAndType(userId, "WeiXinSmallProg").OpenId;//登录小程序的OpenId

                    string formid = "";
                    var formmodel = _iwxtser.GetWXAppletFromDataById(MessageTypeEnum.OrderPay, info.OrderId.ToString());
                    if (formmodel != null)
                    {
                        formid = formmodel.FormId;//根据OrderId获取FormId
                    }
                    else
                    {
                        Log.Info("订单：" + info.OrderId + " FormId为空");
                    }
                    _iwxtser.SendAppletMessageByTemplate(MessageTypeEnum.OrderPay, userId, msgdata, page, openId, formid);
                }
            }
            #endregion
        }

        /// <summary>
        /// 店铺有新订单
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        public void SendMessageOnShopHasNewOrder(long shopId, MessageOrderInfo info)
        {
            #region 发送模板消息
            //卖家收信息
            var shopinfo = Context.ManagerInfo.FirstOrDefault(d => d.ShopId == shopId);
            if (shopinfo != null)
            {
                var sellerinfo = Context.UserMemberInfo.FirstOrDefault(d => d.UserName == shopinfo.UserName);
                if (sellerinfo != null)
                {
                    var msgdata = new WX_MsgTemplateSendDataModel();

#if DEBUG
                    Core.Log.Info("[模板消息]卖家新订单用户编号：" + sellerinfo.Id.ToString());
#endif
                    msgdata = new WX_MsgTemplateSendDataModel();
                    msgdata.first.value = "您的店铺有新的订单生成。";
                    msgdata.first.color = "#000000";
                    msgdata.keyword1.value = info.ShopName;
                    msgdata.keyword1.color = "#000000";
                    msgdata.keyword2.value = info.ProductName.ToString() + "等...";
                    msgdata.keyword2.color = "#000000";
                    msgdata.keyword3.value = info.OrderTime.ToString();
                    msgdata.keyword3.color = "#000000";
                    msgdata.keyword4.value = info.TotalMoney.ToString();
                    msgdata.keyword4.color = "#FF0000";
                    msgdata.keyword5.value = "已付款(" + info.PaymentType + ")";
                    msgdata.keyword5.color = "#000000";
                    msgdata.remark.value = "感谢您的使用,祝您生意兴荣。";
                    msgdata.remark.color = "#000000";

#if DEBUG
                    Core.Log.Info("[模板消息]卖家新订单开始前：" + sellerinfo.Id.ToString() + "_" + info.OrderId.ToString());
#endif
                    var _iwxtser = Himall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                    _iwxtser.SendMessageByTemplate(MessageTypeEnum.ShopHaveNewOrder, sellerinfo.Id, msgdata);
                }
            }
            #endregion
        }

        /// <summary>
        /// 退款/退货成功通知
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        /// <param name="refundid"></param>
        public void SendMessageOnOrderRefund(long userId, MessageOrderInfo info, long refundid = 0)
        {
            var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
            foreach (var msg in message)
            {
                if (msg.Biz.GetStatus(MessageTypeEnum.OrderRefund) == StatusEnum.Open)
                {
                    string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactsInfo.UserTypes.General);
                    if (!msg.Biz.CheckDestination(destination))
                        throw new HimallException(msg.Biz.ShortName + "错误");
                    var content = msg.Biz.SendMessageOnOrderRefund(destination, info);
                    if (msg.Biz.EnableLog)
                    {
                        Context.MessageLog.Add(new MessageLog() { SendTime = DateTime.Now, ShopId = info.ShopId, MessageContent = content, TypeId = "短信" });
                        Context.SaveChanges();
                    }
                }
            }

            #region 发送模板消息
            if (info.MsgOrderType == MessageOrderType.Normal)
            {
                var userinfo = Context.UserMemberInfo.FirstOrDefault(d => d.Id == userId);
                if (userinfo != null)
                {
                    var msgdata = new WX_MsgTemplateSendDataModel();
                    msgdata.first.value = "您的订单已经完成退款，¥" + info.RefundMoney.ToString("F2") + "已经退回您的付款账户（或预存款账户），请留意查收。";
                    msgdata.first.color = "#000000";
                    msgdata.keyword1.value = "¥" + info.RefundMoney.ToString("F2");
                    msgdata.keyword1.color = "#FF0000";
                    msgdata.keyword2.value = info.ProductName + "";
                    msgdata.keyword2.color = "#000000";
                    msgdata.keyword3.value = info.OrderId.ToString();
                    msgdata.keyword3.color = "#000000";
                    //msgdata.remark.value = "请您耐心等候";
                    //msgdata.remark.color = "#000000";
                    var _iwxtser = Himall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                    if (refundid > 0)
                    {
                        string url = _iwxtser.GetMessageTemplateShowUrl(MessageTypeEnum.OrderRefund);
                        url = url.Replace("{id}", refundid.ToString());
                    }
                    _iwxtser.SendMessageByTemplate(MessageTypeEnum.OrderRefund, userId, msgdata);
                }
            }
            else
            {

                //小程序发送
                string status = "退货/退款成功";
                string remark = "您的订单已经完成退款,¥" + info.RefundMoney.ToString("F2") + "已经退回您的付款账户（或预存款账户），请留意查收.";
                
                var userinfo = Context.UserMemberInfo.FirstOrDefault(d => d.Id == userId);
                if (userinfo != null)
                {
                    var msgdata = new WX_MsgTemplateSendDataModel();
                    msgdata.keyword1.value = "¥" + info.RefundMoney.ToString("F2") + "元";
                    msgdata.keyword1.color = "#173177";
                    msgdata.keyword2.value = status;
                    msgdata.keyword2.color = "#173177";
                    msgdata.keyword3.value = remark;
                    msgdata.keyword3.color = "#173177";
                    msgdata.keyword4.value = info.RefundTime.ToString("yyyy-MM-dd HH:mm:ss");
                    msgdata.keyword4.color = "#173177";
                    msgdata.keyword5.value = info.OrderId.ToString();
                    msgdata.keyword5.color = "#173177";

                    //处理url
                    var _iwxtser = Himall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                    var _iwxmember = Himall.ServiceProvider.Instance<IMemberService>.Create;
                    string page = _iwxtser.GetWXAppletMessageTemplateShowUrl(MessageTypeEnum.OrderRefund);//小程序跳转地址
                    page = page.Replace("{id}", info.OrderId.ToString());

                    string openId = _iwxmember.GetMemberOpenIdInfoByuserIdAndType(userId, "WeiXinSmallProg").OpenId;//登录小程序的OpenId

                    string formid = "";
                    var formmodel = _iwxtser.GetWXAppletFromDataById(MessageTypeEnum.OrderRefund, info.OrderId.ToString());
                    if (formmodel != null)
                        formid = formmodel.FormId;//根据OrderId获取FormId
                    _iwxtser.SendAppletMessageByTemplate(MessageTypeEnum.OrderRefund, userId, msgdata, page, openId, formid);
                }
            }
            #endregion
        }
        /// <summary>
        /// 售后发货信息提醒
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        /// <param name="refundid"></param>
        public void SendMessageOnRefundDeliver(long userId, MessageOrderInfo info, long refundid = 0)
        {
            var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
#if DEBUG
            Core.Log.Info("[发送消息]数据：" + Newtonsoft.Json.JsonConvert.SerializeObject(info) + "[售后发货]");
#endif
            foreach (var msg in message)
            {
                if (msg.Biz.GetStatus(MessageTypeEnum.RefundDeliver) == StatusEnum.Open)
                {
                    string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactsInfo.UserTypes.General);
                    if (!msg.Biz.CheckDestination(destination))
                    {
#if DEBUG
                        Core.Log.Info("[发送消息]失败：" + msg.PluginInfo.PluginId + "未发送," + destination + "格式检测未通过[售后发货]");
#endif
                        throw new HimallException(msg.Biz.ShortName + "错误：实例失败。");
                    }
                    try
                    {
                        var content = msg.Biz.SendMessageOnRefundDeliver(destination, info);
#if DEBUG
                        Core.Log.Info("[发送消息]发送结束：" + destination + " : " + msg.PluginInfo.PluginId + "[售后发货]");
#endif
                        if (msg.Biz.EnableLog)
                        {
                            Context.MessageLog.Add(new MessageLog() { SendTime = DateTime.Now, ShopId = info.ShopId, MessageContent = content, TypeId = "短信" });
                            Context.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Info("[发送消息]发送失败：" + msg.PluginInfo.PluginId + "[售后发货]", ex);
                    }
                }
            }

            #region 发送模板消息
            var userinfo = Context.UserMemberInfo.FirstOrDefault(d => d.Id == userId);
            if (userinfo != null)
            {
                var msgdata = new WX_MsgTemplateSendDataModel();
                msgdata.first.value = "您的订单(" + info.OrderId + ")售后已审核通过，请及时发货。";
                msgdata.first.color = "#000000";
                msgdata.keyword1.value = "审核通过，请您发货";
                msgdata.keyword1.color = "#FF0000";
                msgdata.keyword2.value = info.ProductName + "";
                msgdata.keyword2.color = "#000000";
                msgdata.keyword3.value = info.RefundMoney.ToString("F2");
                msgdata.keyword3.color = "#000000";
                msgdata.keyword4.value = info.Remark;
                msgdata.keyword4.color = "#000000";
                msgdata.keyword5.value = info.RefundAuditTime.ToString("yyyy-MM-dd HH:mm:ss");
                msgdata.keyword5.color = "#000000";
                msgdata.remark.value = "感谢您的使用！";
                msgdata.remark.color = "#000000";
                var _iwxtser = Himall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                if (refundid > 0)
                {
                    string url = _iwxtser.GetMessageTemplateShowUrl(MessageTypeEnum.RefundDeliver);
                    url = url.Replace("{id}", refundid.ToString());
                }
                _iwxtser.SendMessageByTemplate(MessageTypeEnum.RefundDeliver, userId, msgdata);
            }
            #endregion
        }

        /// <summary>
        /// 发货通知
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        public void SendMessageOnOrderShipping(long userId, MessageOrderInfo info)
        {
            var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
            foreach (var msg in message)
            {
                if (msg.Biz.GetStatus(MessageTypeEnum.OrderShipping) == StatusEnum.Open)
                {
                    string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactsInfo.UserTypes.General);
                    if (!msg.Biz.CheckDestination(destination))
                        throw new HimallException(msg.Biz.ShortName + "错误");
                    var content = msg.Biz.SendMessageOnOrderShipping(destination, info);
                    if (msg.Biz.EnableLog)
                    {
                        Context.MessageLog.Add(new MessageLog() { SendTime = DateTime.Now, ShopId = info.ShopId, MessageContent = content, TypeId = "短信" });
                        Context.SaveChanges();
                    }
                }
            }
            #region 发送模板消息
            if (info.MsgOrderType == MessageOrderType.Normal)
            {
                var userinfo = Context.UserMemberInfo.FirstOrDefault(d => d.Id == userId);
                if (userinfo != null)
                {
                    var msgdata = new WX_MsgTemplateSendDataModel();
                    msgdata.first.value = "嗖嗖，您的订单已发货，正加速送到您的手上。";
                    msgdata.first.color = "#000000";
                    msgdata.keyword1.value = info.ProductName + "等...";
                    msgdata.keyword1.color = "#000000";
                    msgdata.keyword2.value = info.ShippingCompany.ToString();
                    msgdata.keyword2.color = "#000000";
                    msgdata.keyword3.value = info.ShippingNumber.ToString();
                    msgdata.keyword3.color = "#FF0000";
                    msgdata.keyword4.value = info.ShipTo;
                    msgdata.keyword4.color = "#000000";
                    msgdata.remark.value = "请您耐心等候";
                    msgdata.remark.color = "#000000";
                    var _iwxtser = Himall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                    string url = _iwxtser.GetMessageTemplateShowUrl(MessageTypeEnum.OrderShipping);
                    url = url.Replace("{id}", info.OrderId.ToString());
                    _iwxtser.SendMessageByTemplate(MessageTypeEnum.OrderShipping, userId, msgdata);
                }
            }
            #endregion
            #region 小程序模版
            else
            {
                var userinfo = Context.UserMemberInfo.FirstOrDefault(d => d.Id == userId);
                if (userinfo != null)
                {
                    var msgdata = new WX_MsgTemplateSendDataModel();
                    msgdata.keyword1.value = info.ShippingCompany;//快递公司
                    msgdata.keyword1.color = "#173177";
                    msgdata.keyword2.value = info.ShippingNumber;//快递单号
                    msgdata.keyword2.color = "#173177";
                    msgdata.keyword3.value = info.OrderTime.ToString("yyyy-MM-dd HH:mm:ss");
                    msgdata.keyword3.color = "#173177";
                    msgdata.keyword4.value = info.ProductName + "等...";
                    msgdata.keyword4.color = "#173177";
                    msgdata.keyword5.value = info.ShipTo;
                    msgdata.keyword5.color = "#173177";

                    //处理url
                    var _iwxtser = Himall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                    var _iwxmember = Himall.ServiceProvider.Instance<IMemberService>.Create;
                    string page = _iwxtser.GetWXAppletMessageTemplateShowUrl(MessageTypeEnum.OrderShipping);//小程序跳转地址
                    page = page.Replace("{id}", info.OrderId.ToString());

                    string openId = _iwxmember.GetMemberOpenIdInfoByuserIdAndType(userId, "WeiXinSmallProg").OpenId;//登录小程序的OpenId

                    string formid = "";
                    var formmodel = _iwxtser.GetWXAppletFromDataById(MessageTypeEnum.OrderPay, info.OrderId.ToString());
                    if (formmodel != null)
                        formid = formmodel.FormId;//根据OrderId获取FormId
                    _iwxtser.SendAppletMessageByTemplate(MessageTypeEnum.OrderShipping, userId, msgdata, page, openId, formid);
                }
            }
            #endregion
        }

        public void SendMessageOnShopAudited(long userId, MessageShopInfo info)
        {
            var messages = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
            foreach (var msg in messages)
            {
                if (msg.Biz.GetStatus(MessageTypeEnum.ShopAudited) == StatusEnum.Open)
                {
                    string destination = GetDestination(userId, msg.PluginInfo.PluginId);
                    if (!msg.Biz.CheckDestination(destination))
                        throw new HimallException(msg.Biz.ShortName + "错误");
                    var content = msg.Biz.SendMessageOnShopAudited(destination, info);
                    if (msg.Biz.EnableLog)
                    {
                        Context.MessageLog.Add(new MessageLog() { SendTime = DateTime.Now, ShopId = 0, MessageContent = content, TypeId = "短信" });
                        Context.SaveChanges();
                    }
                }
            }
        }

        /// <summary>
        /// 发送优惠券成功时发送消息
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>

        public void SendMessageOnCouponSuccess(long userId, MessageCouponInfo info)
        {
            var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
            foreach (var msg in message)
            {
                if (msg.Biz.GetStatus(MessageTypeEnum.SendCouponSuccess) == StatusEnum.Open)
                {
                    string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactsInfo.UserTypes.General);
                    if (!msg.Biz.CheckDestination(destination))
                        throw new HimallException(msg.Biz.ShortName + "错误");
                    var content = msg.Biz.SendMessageOnCouponSuccess(destination, info);
                    if (msg.Biz.EnableLog)
                    {
                        Context.MessageLog.Add(new MessageLog() { SendTime = DateTime.Now, ShopId = 0, MessageContent = content, TypeId = "短信" });
                        Context.SaveChanges();
                    }
                }
            }
        }


        //2.4去除
        //public void SendMessageOnShopSuccess(long userId, MessageShopInfo info)
        //{
        //    var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
        //    foreach (var msg in message)
        //    {
        //        if (msg.Biz.GetStatus(MessageTypeEnum.ShopSuccess) == StatusEnum.Open)
        //        {
        //            string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactsInfo.UserTypes.ShopManager);
        //            if (!msg.Biz.CheckDestination(destination))
        //                throw new HimallException(msg.Biz.ShortName + "错误");
        //            var content = msg.Biz.SendMessageOnShopSuccess(destination, info);
        //            if (msg.Biz.EnableLog)
        //            {
        //                context.MessageLog.Add(new MessageLog() { SendTime = DateTime.Now, ShopId = 0, MessageContent = content, TypeId = "短信" });
        //                context.SaveChanges();
        //            }
        //        }
        //    }
        //}

        //原结算发送消息 150624
        //public void SendMessageOnShopSettlement(long userId, MessageSettlementInfo info)
        //{
        //    var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
        //    foreach (var msg in message)
        //    {
        //        if (msg.Biz.GetStatus(MessageTypeEnum.ShopSettlement) == StatusEnum.Open)
        //        {
        //            string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactsInfo.UserTypes.ShopManager);
        //            if (!msg.Biz.CheckDestination(destination))
        //                throw new HimallException(msg.Biz.ShortName + "错误");
        //            var content = msg.Biz.SendMessageOnShopSettlement(destination, info);
        //            if (msg.Biz.EnableLog)
        //            {
        //                context.MessageLog.Add(new MessageLog() { SendTime = DateTime.Now, ShopId = 0, MessageContent = content, TypeId = "短信" });
        //                context.SaveChanges();
        //            }
        //        }
        //    }
        //}


        public void AddSendMessageRecord(dynamic model)
        {
            throw new NotImplementedException();
        }

        public ObsoletePageModel<object> GetSendMessageRecords(object querymodel)
        {
            throw new NotImplementedException();
        }
    }
}

