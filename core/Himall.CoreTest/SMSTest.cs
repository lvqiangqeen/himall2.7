using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Himall.Core.Plugins.Message;

namespace Himall.CoreTest
{
    [TestClass]
    public class SMSTest : TestRoot
    {
        [TestMethod]
        public void TestMethod1()
        {


            var messagePlugins = Core.PluginsManagement.GetPlugins<Core.Plugins.Message.IMessagePlugin>();
            // var validMessagers = messagePlugins.Where(item => item.Biz.GetStatus());
            // messagePlugins.ElementAt(0).Biz.set
            //Dictionary<MessageTypeEnum, StatusEnum> dic = new Dictionary<MessageTypeEnum, StatusEnum>();
            //dic.Add(MessageTypeEnum.FindPassWord, StatusEnum.Close);
            //dic.Add(MessageTypeEnum.FindPassWord, StatusEnum.Close);
            //dic.Add(MessageTypeEnum.OrderCreated, StatusEnum.Close);
            //dic.Add(MessageTypeEnum.OrderPay, StatusEnum.Close);
            //dic.Add(MessageTypeEnum.OrderRefund, StatusEnum.Open);
            //dic.Add(MessageTypeEnum.OrderShipping, StatusEnum.Close);
            //dic.Add(MessageTypeEnum.ShopAudited, StatusEnum.Close);
            //dic.Add(MessageTypeEnum.ShopSuccess, StatusEnum.Open);
            //  var result= messagePlugins.ElementAt(0).Biz.SendTestMessage("61905693@qq.com","测试邮件","测试邮件");
            var test = messagePlugins.ElementAt(1).Biz.SendTestMessage("18673368930", "【商城】测试发送短信");
            //  Assert.AreEqual("发送成功", result);
            Assert.AreEqual("发送成功465", test);
        }

        [TestMethod]
        public void TestEmail()
        {
            //SendMail mail = new SendMail();
            //string s1 = "147905090@qq.com,61905693@qq.com";
            //string[] arr = s1.Split(',');
            //mail.ASynSendMail("你好吗？我是来扁你的。你这个二货", arr,"12347897897");
        }
    }
}
