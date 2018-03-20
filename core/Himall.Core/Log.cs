using System;
using System.Diagnostics;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace Himall.Core
{
    public static class Log
    {

        /// <summary>
        /// 一般错误
        /// </summary>
        /// <param name="message">消息</param>
        public static void Error(object message)
        {
            try
            {
                log4net.ILog log = log4net.LogManager.GetLogger(GetCurrentMethodFullName());
                log.Error(message);
            }
            catch
            {

            }

        }

        /// <summary>
        /// 一般错误
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="exception">异常</param>
        public static void Error(object message, Exception exception)
        {
            try
            {
                log4net.ILog log = log4net.LogManager.GetLogger(GetCurrentMethodFullName());
                log.Error(message, exception);
            }
            catch
            {

            }
        }


        /// <summary>
        /// 信息
        /// </summary>
        /// <param name="message">消息</param>
        public static void Info(object message)
        {
            try
            {
                log4net.ILog log = log4net.LogManager.GetLogger(GetCurrentMethodFullName());
                log.Info(message);
            }
            catch
            {

            }
        }

        /// <summary>
        /// 信息
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="exception">异常</param>
        public static void Info(object message, Exception ex)
        {
            try
            {
                log4net.ILog log = log4net.LogManager.GetLogger(GetCurrentMethodFullName());
                log.Info(message, ex);
            }
            catch
            {

            }
        }

        /// <summary>
        /// 警告
        /// </summary>
        /// <param name="message">消息</param>
        public static void Warn(object message)
        {
            try
            {
                log4net.ILog log = log4net.LogManager.GetLogger(GetCurrentMethodFullName());
                log.Warn(message);
            }
            catch
            {

            }
        }

        /// <summary>
        /// 警告
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="exception">异常</param>
        public static void Warn(object message, Exception ex)
        {
            try
            {
                log4net.ILog log = log4net.LogManager.GetLogger(GetCurrentMethodFullName());
                log.Warn(message, ex);
            }
            catch
            {

            }
        }

        /// <summary>
        /// 调试
        /// </summary>
        /// <param name="message">消息</param>
        public static void Debug(object message)
        {
            try
            {
                log4net.ILog log = log4net.LogManager.GetLogger(GetCurrentMethodFullName());
                log.Debug(message);
            }
            catch
            {

            }
        }

        /// <summary>
        /// 调试
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="exception">异常</param>
        public static void Debug(object message, Exception ex)
        {
            try
            {
                log4net.ILog log = log4net.LogManager.GetLogger(GetCurrentMethodFullName());
                log.Debug(message, ex);
            }
            catch
            { }
        }

        static string GetCurrentMethodFullName()
        {
            try
            {
                int depth = 2;
                StackTrace st = new StackTrace();
                int maxFrames = st.GetFrames().Length;
                StackFrame sf;
                string methodName, className;
                Type classType;
                do
                {
                    sf = st.GetFrame(depth++);
                    classType = sf.GetMethod().DeclaringType;
                    className = classType.ToString();
                } while (className.EndsWith("Exception") && depth < maxFrames);
                methodName = sf.GetMethod().Name;
                return className + "." + methodName;
            }
            catch (Exception e)
            {
                log4net.LogManager.GetLogger("Core.Log").Error(e.Message, e);
                return "获取方法名失败";
            }
        }
    }
}
