using Himall.Core;
using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Himall.PaymentPlugin
{
    public static class Utility<T> where T : ConfigBase, new()
    {
        /// <summary>
        /// 获取配置文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetConfig(string workDirectory)
        {
            string sDirectory = Himall.Core.Helper.IOHelper.urlToVirtual(workDirectory) + "/data.config";
            T config = new T();

            if (HimallIO.ExistFile(sDirectory))
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));
                byte[] b = Himall.Core.HimallIO.GetFileContent(sDirectory);
                string str = System.Text.Encoding.Default.GetString(b);
                MemoryStream fs = new MemoryStream(b);
                config = (T)xs.Deserialize(fs);
            }
            else
            {
                SaveConfig(config, workDirectory);
            }

            return config;
        }

        /// <summary>
        /// 保存配置文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="config"></param>
        public static void SaveConfig(T config, string workDirectory)
        {
            string sDirectory = Himall.Core.Helper.IOHelper.urlToVirtual(workDirectory) + "/data.config";
            XmlSerializer xml = new XmlSerializer(typeof(T));
            MemoryStream Stream = new MemoryStream();
            xml.Serialize(Stream, config);

            byte[] b = Stream.ToArray();
            MemoryStream stream2 = new MemoryStream(b);
            Himall.Core.HimallIO.CreateFile(sDirectory, stream2, Core.FileCreateType.Create);

        }
    }
}
