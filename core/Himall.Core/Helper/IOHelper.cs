using System;
using System.IO;
using System.Web;
using System.Xml.Serialization;

namespace Himall.Core.Helper
{
    public class IOHelper
    {
        /// <summary>
        /// 获取当前目录
        /// （网站为网站根目录，测试时为dll所在目录）
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetMapPath(string path)
        {
            if (HttpContext.Current != null)
            {
                return HttpContext.Current.Server.MapPath(path);
            }
            else
            {
                string root = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                if (!string.IsNullOrWhiteSpace(path))
                {
                    path = path.Replace("/", "\\");
                    if (!path.StartsWith("\\"))
                        path = "\\" + path;
                    path = path.Substring(path.IndexOf('\\') + (root.EndsWith("\\") ? 1 : 0));
                }
                return root + path;
            }
        }

        /// <summary>
        /// 复制指定的文件到指定目录
        /// </summary>
        /// <param name="fileFullPath">源文件的全路径</param>
        /// <param name="destination">目标目录</param>
        /// <param name="isDeleteSourceFile">是否删除源文件</param>
        /// <param name="fileName">目标文件名称,默认是原名称</param>
        /// <exception cref="ArgumentNullException">源文件全路径为空</exception>
        /// <exception cref="FileNotFoundException">找不到源文件</exception>
        /// <exception cref="DirectoryNotFoundException">找不到目标目录</exception>
        /// <exception cref="Exception">复制文件异常</exception>
        public static void CopyFile(string fileFullPath, string destination, bool isDeleteSourceFile = false, string fileName = "")
        {
            if (string.IsNullOrWhiteSpace(fileFullPath))
                throw new ArgumentNullException("fileFullPath", "源文件全路径不能为空");

            if (!File.Exists(fileFullPath))
                throw new FileNotFoundException("找不到源文件", fileFullPath);

            if (!Directory.Exists(destination))
                throw new DirectoryNotFoundException("找不到目标目录 " + destination);

            try
            {
                fileName = string.IsNullOrWhiteSpace(fileName) ? Path.GetFileName(fileFullPath) : fileName;
                File.Copy(fileFullPath, Path.Combine(destination, fileName), true);
                if (isDeleteSourceFile)
                    File.Delete(fileFullPath);
            }
            catch (Exception)
            {
                throw;
            }


        }

        /// <summary>
        /// 获取文件夹大小单位KB
        /// </summary>
        /// <param name="dirPath">文件夹路径</param>
        /// <returns></returns>
        public static long GetDirectoryLength(string dirPath)
        {
            if (!Directory.Exists(dirPath))
                return 0;
            long len = 0;
            DirectoryInfo di = new DirectoryInfo(dirPath);
            foreach (FileInfo fi in di.GetFiles())
            {
                len += fi.Length;
            }
            DirectoryInfo[] dis = di.GetDirectories();
            if (dis.Length > 0)
            {
                for (int i = 0; i < dis.Length; i++)
                {
                    len += GetDirectoryLength(dis[i].FullName);
                }
            }
            return len;
        }


        /// <summary>
        /// 将物理路径转换成相对路径
        /// </summary>
        /// <param name="url">物理路径名称</param>
        /// <returns>返回绝对路径</returns>
        public static string urlToVirtual(string url)
        {
            string tmpRootDir = GetMapPath("/");//获取程序根目录  
            string imagesurl2 = url.Replace(tmpRootDir, ""); //转换成相对路径  
            imagesurl2 = imagesurl2.Replace(@"\", @"/");
            return "/" + imagesurl2;
        }
    }
}
