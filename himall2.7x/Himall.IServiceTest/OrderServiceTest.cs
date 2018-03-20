using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Himall.Service;
using System.Threading;
using System.Collections.Generic;
using Himall.IServices;

namespace Himall.IServiceTest
{
    [TestClass]
    public class OrderServiceTest
    {
        static List<long> list = new List<long>();
        [TestMethod]
        public void CreateOrderTest()
        {
            Get();
            var t = list.Count;
        }


        public void Get()
        {
            for (int i = 0; i < 70; i++)
            {
                A a = new A();
                list.Add(a.GetOrderId());
            }
        }


        [TestMethod]

        public void PaySucceed()
        {
            var service = ServiceProvider.Instance<IOrderService>.Create;
            var orderIds=new long[]{2016070775717359};
            service.PaySucceed(orderIds, "Himall.Plugin.Payment.WeiXinPay_Native", DateTime.Now, "4003922001201607078527197576", 20160707757173590);
        }

    }

    public class A
    {
        readonly static object obj = new object();
        public long GetOrderId()
        {
            //订单生成算法
            /*
             * 订单号一共14位，由固定值（1位） 年（2位），月（2位），天（2位） ，当前订单数（5位），随机数（2）
             * 
             * 
             *       这里一共是5位当前订单数，也就是说一天最大支持99999个订单。
             */
            lock (obj)
            {
                var year = DateTime.Now.Year.ToString().Substring(2, 2);
                var _day = DateTime.Now.Day;
                var day = _day < 10 ? "0" + _day.ToString() : _day.ToString();
                var _month = DateTime.Now.Month;
                var month = _month < 10 ? "0" + _month.ToString() : _month.ToString();
                string randomStr = "";
                string orderCountStr = "";
                string orderId = "";

                Thread.Sleep(2);
                long tick = DateTime.Now.Ticks;
                Random ran = new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));
                var ranString = ran.Next(10, 99).ToString();
                randomStr = ranString.ToString();
                orderCountStr = (5 + 1).ToString().PadLeft(5, '0');
                orderId = (String.Format("1{0}{1}{2}{3}{4}", year, month, day, orderCountStr, randomStr));
                return long.Parse(orderId);
            }
        }
    }
}
