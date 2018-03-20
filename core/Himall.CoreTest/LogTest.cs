using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Himall.Core;

namespace Himall.CoreTest
{
    [TestClass]
    public class LogTest
    {
        [TestMethod]
        public void Core_Log_Error_Test()
        {
            Log.Error("这是错误内容");
        }

        [TestMethod]
        public void Core_Log_ErrorWithException_Test()
        {
            Log.Error("这是错误内容", new ApplicationException("这是一个测试异常"));
        }

        [TestMethod]
        public void Core_Log_Info_Test()
        {
            Log.Info("这是消息内容");
        }

        [TestMethod]
        public void Core_Log_InfoWithException_Test()
        {
            Log.Info("这是消息内容", new ApplicationException("这是一个消息异常"));
        }

        [TestMethod]
        public void Core_Debug_Info_Test()
        {
            Log.Info("这是调试内容");
        }

        [TestMethod]
        public void Core_Log_DebugWithException_Test()
        {
            Log.Info("这是调试内容", new ApplicationException("这是一个消息异常"));
        }

        [TestMethod]
        public void Core_Log_ExceptionError_Test()
        {
            try
            {
                throw new CacheRegisterException("出错");
            }
            catch { }

        }

    }
}
