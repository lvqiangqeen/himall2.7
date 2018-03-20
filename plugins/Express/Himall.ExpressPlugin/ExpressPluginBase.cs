using Himall.Core.Plugins.Express;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Himall.ExpressPlugin
{
    public abstract class ExpressPluginBase
    {
        ExpressInfo expressInfo;

        public ExpressPluginBase()
        {
            if (WorkDirectory != null)
                RefreshExpressInfo();
        }

        public string Name
        {
            get { return expressInfo.Name; }
        }

        public string DisplayName
        {
            get { return expressInfo.DisplayName; }
        }

        /// <summary>
        /// 淘宝Code
        /// </summary>
        public string TaobaoCode { get { return expressInfo.TaobaoCode; } }

        /// <summary>
        /// 快递100Code
        /// </summary>
        public string Kuaidi100Code { get { return expressInfo.Kuaidi100Code; } }

        /// <summary>
        /// 快递鸟Code
        /// </summary>
        public string KuaidiNiaoCode { get { return expressInfo.KuaidiNiaoCode; } }

        /// <summary>
        /// 快递单宽
        /// </summary>
        public int Width { get { return expressInfo.Width; } }

        /// <summary>
        /// 快递单高
        /// </summary>
        public int Height { get { return expressInfo.Height; } }

        /// <summary>
        /// 快递单图片
        /// </summary>
        public string BackGroundImage { get { return expressInfo.BackGroundImage; } set { expressInfo.BackGroundImage = value; } }

        /// <summary>
        /// Logo
        /// </summary>
        public string Logo { get { return expressInfo.Logo; } set { expressInfo.Logo = value; } }

        /// <summary>
        /// 所包含的打印元素
        /// </summary>
        public IEnumerable<ExpressPrintElement> Elements { get { return expressInfo.Elements; } }


        static Dictionary<string, string> workDirectories = new Dictionary<string, string>();

        public string WorkDirectory
        {
            get
            {
                string name = this.GetType().FullName;
                if (!string.IsNullOrWhiteSpace(name) && workDirectories.ContainsKey(name))
                    return workDirectories[name];
                else
                    return null;
            }
            set
            {
                string name = this.GetType().FullName;
                if (!workDirectories.ContainsKey(name))
                    workDirectories.Add(name, value);
            }
        }



        public void UpdatePrintElement(IEnumerable<ExpressPrintElement> printElements)
        {
            expressInfo.Elements = printElements.ToArray();

            string sDirectory = Himall.Core.Helper.IOHelper.urlToVirtual(WorkDirectory) + "/config.xml";
            XmlSerializer xml = new XmlSerializer(typeof(ExpressInfo));
            MemoryStream Stream = new MemoryStream();
            xml.Serialize(Stream, expressInfo);

            byte[] b = Stream.ToArray();
            MemoryStream stream2 = new MemoryStream(b);
            Himall.Core.HimallIO.CreateFile(sDirectory, stream2, Core.FileCreateType.Create);


            //using (FileStream fs = new FileStream(WorkDirectory + "/config.xml", FileMode.Create))
            //{
            //    XmlSerializer xs = new XmlSerializer(typeof(ExpressInfo));
            //    xs.Serialize(fs, expressInfo);
            //}
        }


        public void CheckCanEnable()
        {
            throw new NotImplementedException();
        }

        void RefreshExpressInfo()
        {
            DirectoryInfo dir = new DirectoryInfo(WorkDirectory);
            //查找该目录下的
            var configFile = dir.GetFiles("config.xml").FirstOrDefault();
            if (configFile != null)
            {

                string sDirectory = Himall.Core.Helper.IOHelper.urlToVirtual(configFile.FullName);

                XmlSerializer xs = new XmlSerializer(typeof(ExpressInfo));
                byte[] b = Himall.Core.HimallIO.GetFileContent(sDirectory);
                string str = System.Text.Encoding.Default.GetString(b);
                MemoryStream fs = new MemoryStream(b);
                expressInfo = (ExpressInfo)xs.Deserialize(fs);


                //using (FileStream fs = new FileStream(configFile.FullName, FileMode.Open))
                //{
                //    XmlSerializer xs = new XmlSerializer(typeof(ExpressInfo));
                //    expressInfo = (ExpressInfo)xs.Deserialize(fs);
                //}
            }
        }

        /// <summary>
        /// 计算下一快递单号
        /// </summary>
        /// <param name="currentExpressCode">当前快递单号</param>
        /// <returns></returns>
        public virtual string NextExpressCode(string currentExpressCode)
        {
            long current;
            bool isValid = long.TryParse(currentExpressCode, out current);
            if (!isValid)
                throw new FormatException("快递单号格式不正确,正确的格式为数字");
            return (current + 1).ToString();
        }

        /// <summary>
        /// 检查快递单号是否合法
        /// </summary>
        /// <param name="expressCode">待检查的快递单号</param>
        /// <returns></returns>
        public virtual bool CheckExpressCodeIsValid(string expressCode)
        {
            long current;
            return long.TryParse(expressCode, out current);
        }

    }
}
