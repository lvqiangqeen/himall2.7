using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WinServiceBase
{
    public class SettingHelper
    {
        public static string Get_ConfigValue(string strKeyName)
        {
            string root = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string xmlfile = root.Remove(root.LastIndexOf('\\') + 1) + "ServiceSetting.xml";
            if (System.IO.File.Exists(xmlfile))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlfile);
                XmlNode xn = doc.SelectSingleNode("Settings/" + strKeyName);
                doc = null;
                return xn.InnerText;
            }
            return string.Empty;
        }
    }
}
