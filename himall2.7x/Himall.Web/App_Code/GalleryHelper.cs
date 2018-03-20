using Himall.IServices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;

namespace Himall.Web
{
    public class GalleryHelper
    {
        public GalleryHelper()
        {

        }
        private string tempLatePath = "";
        private const string adminTemplateDic = "/Areas/Admin/Templates/vshop/";
        private const string sellerAdminTemplateDic = "/Areas/SellerAdmin/Templates/vshop/{0}/";
        private string templateCuName = "";

        public void UpdateTemplateName(string tName, string name, long shopId = 0)
        {
            string imgPath = adminTemplateDic;
            if (shopId != 0)
            {
                imgPath = string.Format(sellerAdminTemplateDic, shopId);
            }
            var dirs = Directory.GetDirectories(HttpContext.Current.Server.MapPath(imgPath));
            foreach (var item in dirs)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(item);
                if(dirInfo.Name.ToLower().Equals(tName.ToLower()))
                {
                    FileInfo file = dirInfo.GetFiles("template.xml").FirstOrDefault();
                    FileStream stream = file.OpenWrite();
                    XmlDocument doc = new XmlDocument();
                    doc.Load(stream);
                    doc.SelectSingleNode("root/Name").InnerText = name;
                    stream.Close();
                }
            }
        }

        public List<ManageThemeInfo> LoadThemes(long shopId = 0)
        {
            string imgPath = adminTemplateDic;
            if (shopId != 0)
            {
                imgPath = string.Format(sellerAdminTemplateDic, shopId);
            }
            XmlDocument doc = new XmlDocument();
            List<ManageThemeInfo> themesList = new List<ManageThemeInfo>();
            string[] temps = Directory.Exists(HttpContext.Current.Server.MapPath(imgPath))
                ? Directory.GetDirectories(HttpContext.Current.Server.MapPath(imgPath)) : null;
            foreach (string dic in temps)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(dic);
                string dirName = dirInfo.Name.ToLower(CultureInfo.InvariantCulture);
                // 为了区分公共样式和专有样式，在公共样式前加"*"号加以区分
                //
                if ((dirName.Length > 0) && (!dirName.StartsWith("_")))
                {
                    FileInfo[] files = dirInfo.GetFiles("template.xml");
                    foreach (FileInfo file in files)
                    {
                        ManageThemeInfo theme = new ManageThemeInfo();
                        FileStream stream = file.OpenRead();
                        doc.Load(stream);
                        stream.Close();
                        theme.Name = doc.SelectSingleNode("root/Name").InnerText;
                        theme.ThemeName = dirName;
                        theme.ThemeImgUrl = string.Format("{0}{1}/{2}", imgPath, dirName, dirInfo.GetFiles("default.*")[0].Name);
                        if (dirName == tempLatePath)
                        {
                            templateCuName = doc.SelectSingleNode("root/Name").InnerText;
                        }
                        themesList.Add(theme);
                    }
                }
            }
            return themesList;
        }


        /// <summary>
        /// 获取模板图片
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string GetImgName(string fileName)
        {

            return "/Templates/vshop/" + fileName + "/default.png";

        }


    }

}