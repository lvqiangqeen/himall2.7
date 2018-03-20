using System;
using System.Collections.Generic;
using System.Text;
using ICSharpCode.SharpZipLib;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using Ionic.Zip;
namespace Himall.Core.Helper
{
    public class ZipHelper
    {
        public class ZipInfo
        {
            /// <summary>
            /// 解/压缩成功状态
            /// </summary>
            public bool Success { get; set; }
            /// <summary>
            /// 解/压缩结果信息
            /// </summary>
            public string InfoMessage { set; get; }
            /// <summary>
            /// 输出目录
            /// </summary>
            public string UnZipPath { get;set;}
        }
        /// <summary>
        /// 压缩文件(Zip)
        /// </summary>
        /// <param name="filesPath">待压缩文件目录</param>
        /// <param name="zipFilePath">压缩文件输出目录</param>
        /// <returns></returns>
        public static ZipInfo CreateZipFile(string filesPath, string zipFilePath)
        {
            if (!Directory.Exists(filesPath))
            {
                return new ZipInfo
                {
                    Success = false,
                    InfoMessage = "没有找到文件"
                };
            }
            try
            {
                string[] filenames = Directory.GetFiles(filesPath);
                using (ICSharpCode.SharpZipLib.Zip.ZipOutputStream s = new ICSharpCode.SharpZipLib.Zip.ZipOutputStream(File.Create(zipFilePath)))
                {
                    s.SetLevel(9); // 压缩级别 0-9
                    //s.Password = "123"; //Zip压缩文件密码
                    byte[] buffer = new byte[4096]; //缓冲区大小
                    foreach (string file in filenames)
                    {
                        ICSharpCode.SharpZipLib.Zip.ZipEntry entry = new ICSharpCode.SharpZipLib.Zip.ZipEntry(Path.GetFileName(file));
                        entry.DateTime = DateTime.Now;
                        s.PutNextEntry(entry);
                        using (FileStream fs = File.OpenRead(file))
                        {
                            int sourceBytes;
                            do
                            {
                                sourceBytes = fs.Read(buffer, 0, buffer.Length);
                                s.Write(buffer, 0, sourceBytes);
                            } while (sourceBytes > 0);
                        }
                    }
                    s.Finish();
                    s.Close();
                }
                return new ZipInfo
                {
                    Success = true,
                    InfoMessage = "压缩成功"
                };
            }
            catch (Exception ex)
            {
                return new ZipInfo
                {
                    Success = false,
                    InfoMessage = ex.Message
                };
            }
        }
        /// <summary>
        /// 解压Zip文件
        /// </summary>
        /// <param name="zipFilePath"></param>
        /// <returns></returns>
        /*
        public static ZipInfo UnZipFile(string zipFilePath)
        {
            if (!File.Exists(zipFilePath))
            {
                return new ZipInfo
                {
                    Success = false,
                    InfoMessage = "没有找到解压文件"
                };
            }
            try
            {
                string path = zipFilePath.Replace(Path.GetExtension(zipFilePath), string.Empty) + "_" + System.DateTime.Now.ToString("yyyyMMddHHmmssfff");
                string directoryName = string.Empty;
                string fileName = string.Empty;
                using (ICSharpCode.SharpZipLib.Zip.ZipInputStream s = new ICSharpCode.SharpZipLib.Zip.ZipInputStream(File.OpenRead(zipFilePath)))
                {
                    ICSharpCode.SharpZipLib.Zip.ZipEntry theEntry;
                    while ((theEntry = s.GetNextEntry()) != null)
                    {
                        directoryName = Path.GetDirectoryName(theEntry.Name);
                        fileName = Path.GetFileName(theEntry.Name);

                        // create directory
                        if (directoryName.Length > 0)
                        {
                            directoryName = Path.Combine(path, directoryName);
                            if (!Directory.Exists(directoryName))
                                Directory.CreateDirectory(directoryName);
                        }
                        else
                        {
                            directoryName = path;
                        }

                        if (fileName != String.Empty)
                        {
                            fileName = Path.Combine(directoryName, fileName);
                            using (FileStream streamWriter = File.Create(fileName))
                            {
                                int size = 2048;
                                byte[] data = new byte[2048];
                                while (true)
                                {
                                    size = s.Read(data, 0, data.Length);
                                    if (size > 0)
                                    {
                                        streamWriter.Write(data, 0, size);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                return new ZipInfo
                {
                    Success = true,
                    InfoMessage = "解压成功",
                    UnZipPath=path
                };
            }
            catch (Exception ex)
            {
                return new ZipInfo
                {
                    Success = false,
                    InfoMessage = "解压文件:" + ex.Message
                };
            }
        }
        */
        public static ZipInfo UnZipFile(string zipFilePath)
        {
            string path = zipFilePath.Replace(Path.GetExtension(zipFilePath), string.Empty) + "_" + System.DateTime.Now.ToString("yyyyMMddHHmmssfff");
            string directoryName = string.Empty;
            string fileName = string.Empty;
            using (Ionic.Zip.ZipFile zip = Ionic.Zip.ZipFile.Read(Path.Combine(zipFilePath)))
            {
                foreach (Ionic.Zip.ZipEntry entry in zip)
                {
                    directoryName = Path.GetDirectoryName(entry.FileName);

                    // create directory
                    if (directoryName.Length > 0)
                    {
                        directoryName = Path.Combine(path, directoryName);
                        if (!Directory.Exists(directoryName))
                            Directory.CreateDirectory(directoryName);
                    }
                    else
                    {
                        directoryName = path;
                    }

                    if (!entry.IsDirectory)
                        entry.Extract(path, Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);
                }
            }

            if (!File.Exists(zipFilePath))
            {
                return new ZipInfo
                {
                    Success = false,
                    InfoMessage = "没有找到解压文件"
                };
            }
            return new ZipInfo
            {
                Success = true,
                InfoMessage = "解压成功",
                UnZipPath = path
            };

        }
    }
    
}
