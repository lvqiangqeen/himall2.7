using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Web;

[assembly: PreApplicationStartMethod(typeof(Himall.Core.PluginsManagement), "RegistAll")]
namespace Himall.CoreTest
{
    [TestClass]
    public class CacheTest
    {
        [TestMethod]
        public void Core_Cache_InsertAndGet_Test()
        {
            string key = "key"+DateTime.Now.ToString("yyyyMMddHHmmssffffff"), value = "001";
            Himall.Core.Cache.Insert(key, value);
            string actualValue = (string)Himall.Core.Cache.Get(key);
            Assert.AreEqual(value, actualValue);
        }


        [TestMethod]
        public void Core_Cache_Remove_Test()
        {
            string key = "key" + DateTime.Now.ToString("yyyyMMddHHmmssffffff"), value = "001";
            Himall.Core.Cache.Insert(key, value);
            string actualValue = (string)Himall.Core.Cache.Get(key);
            Assert.AreEqual(value, actualValue);//检查是否存在
            Himall.Core.Cache.Remove(key);//移除
            object actual = Himall.Core.Cache.Get(key);
            Himall.Core.Cache.Remove("hahaha");

            Assert.IsNull(actual);
        }


        [TestMethod]
        public void Core_Cache_InsertAndGetObject_Test()
        {
            string key = "key110";
            var value = new List<int>() { 1, 3, 4 };
            Himall.Core.Cache.Insert(key, value);
            var actualValue = (List<int>)Himall.Core.Cache.Get(key);
            Assert.AreEqual(value, actualValue);
        }
    }
}
