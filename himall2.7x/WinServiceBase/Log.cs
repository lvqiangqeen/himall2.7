using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WinServiceBase
{
    public class Log
    {
        /// <summary>
        /// Debug日志
        /// </summary>
        /// <param name="msg"></param>
        public static void Debug(string msg)
        {
            string path = GetWinPath() + "\\Log\\Debug\\";
            ExaminePath(path);

            StreamWriter sw = new StreamWriter(path + DateTime.Now.ToString("yyyy-MM-dd") + "-debug.txt", true);
            sw.WriteLine("记录时间：" + DateTime.Now);
            sw.WriteLine("日志级别：  DEBUG ");
            sw.WriteLine("错误描述：" + msg);
            sw.WriteLine("\r\n\r\n\r\n\r\n\r\n");
            sw.Close();
        }

        /// <summary>
        /// Error
        /// </summary>
        /// <param name="msg"></param>
        public static void Error(string msg)
        {
            string path = GetWinPath() + "\\Log\\Error\\";
            ExaminePath(path);

            StreamWriter sw = new StreamWriter(path + DateTime.Now.ToString("yyyy-MM-dd") + "-error.txt", true);
            sw.WriteLine("记录时间：" + DateTime.Now);
            sw.WriteLine("日志级别：  ERROR ");
            sw.WriteLine("错误描述：" + msg);
            sw.WriteLine("\r\n\r\n\r\n\r\n\r\n");
            sw.Close();
        }

        /// <summary>
        /// Debug日志
        /// </summary>
        /// <param name="msg"></param>
        public static void Info(string msg)
        {
            string path = GetWinPath() + "\\Log\\Info\\";
            ExaminePath(path);

            StreamWriter sw = new StreamWriter(path + DateTime.Now.ToString("yyyy-MM-dd") + "-info.txt", true);
            sw.WriteLine("记录时间：" + DateTime.Now);
            sw.WriteLine("日志级别：  INFO ");
            sw.WriteLine("错误描述：" + msg);
            sw.WriteLine("\r\n\r\n\r\n\r\n\r\n");
            sw.Close();
        }

        /// <summary>
        /// 检查目录是否存在，不存在就创建目录
        /// </summary>
        /// <param name="path"></param>
        private static void ExaminePath(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }


        /// <summary>
        /// 获取服务安装物理路径
        /// </summary>
        /// <returns></returns>
        private static string GetWinPath()
        {
            string path = "";
            var _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (_config.HasFile)
            {
                path = _config.FilePath;
                path = path.Substring(0, path.LastIndexOf('\\'));

            }
            return path;
        }
    }
}
