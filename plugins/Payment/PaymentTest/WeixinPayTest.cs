using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Himall.Plugin.Payment.WeiXinPay_Native;
using Himall.Plugin.Payment.WeiXinPay;

namespace PaymentTest
{
    [TestClass]
    public class WeixinPayTest
    {
        [TestMethod]
        public void NativePayTest()
        {
            WeiXinPay nativePay = new WeiXinPay();

            string returnUrl=string.Empty;
            string notifyUrl="http://tomhishop.xicp.net/";
            string orderId="123213c1";
            decimal totalFee=(decimal)0.01;
            string productInfo = "测试商品支付";
            nativePay.WorkDirectory = Himall.Core.Helper.IOHelper.GetMapPath("/");
            string requestUrl = nativePay.GetRequestUrl(returnUrl, notifyUrl, orderId, totalFee, productInfo);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(requestUrl));


        }


        [TestMethod]
        public void JSAPIPayTest()
        {
            Service nativePay = new Service();

            string returnUrl = string.Empty;
            string notifyUrl = "http://tomhishop.xicp.net/";
            string orderId = "1232131vxc";
            decimal totalFee = (decimal)0.01;
            string productInfo = "测试商品支付";
            nativePay.WorkDirectory = Himall.Core.Helper.IOHelper.GetMapPath("/");
            string requestUrl = nativePay.GetRequestUrl(returnUrl, notifyUrl, orderId, totalFee, productInfo);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(requestUrl));
        }
    }
}
