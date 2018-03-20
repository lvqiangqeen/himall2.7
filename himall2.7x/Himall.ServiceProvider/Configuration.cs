using System.Collections.Generic;
using System.Configuration;
using System.Xml;
using System.Linq;
using System;

namespace Himall.ServiceProvider
{
    public class ServiceProviderConfig : IConfigurationSectionHandler
    {

        public List<Item> Items { get; set; }

        public ServiceProviderConfig() { Items = new List<Item>(); }

        public object Create(object parent, object configContext, XmlNode section)
        {
            ServiceProviderConfig config = new ServiceProviderConfig();
            foreach (XmlNode node in section.SelectNodes("item"))
            {
                if (node != null && node.Attributes != null)
                {
                    Item item = new Item();
                    var attribute = node.Attributes["interface"];
                    if (attribute != null)
                    {
                        item.Interface = attribute.Value;
                    }
                    else
                        throw new ApplicationException("配置文件中存在未指定接口的项");


                    var assembly = node.Attributes["assembly"];
                    var namespaceName = node.Attributes["namespace"];

                    if (assembly == null)
                        throw new ApplicationException("配置文件接口" + item.Interface + "未指定程序集");
                    if (namespaceName == null)
                        throw new ApplicationException("配置文件接口" + item.Interface + "未指定命名空间");

                    item.Assembly = assembly.Value;
                    item.NameSpace = namespaceName.Value;
                    config.Items.Add(item);
                }
            }
            return config;
        }
    }


    public class Item
    {
        public string Interface { get; set; }

        public string Assembly { get; set; }

        public string NameSpace { get; set; }
    }

}
