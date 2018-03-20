using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Himall.CoreTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
         var s=   Core.Helper.ValidateHelper.IsEmail("61905693@QQ.COM");
            Assert.AreEqual(s,true);
        }
    }
}
