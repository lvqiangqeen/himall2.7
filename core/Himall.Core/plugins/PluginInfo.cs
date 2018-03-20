using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

namespace Himall.Core.Plugins
{
    public class PluginInfo
    {
        /// <summary>
        /// 插件标识
        /// </summary>
        public string PluginId { get; set; }

        /// <summary>
        /// 插件显示名称
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 程序集类全名
        /// </summary>
        public string ClassFullName { get; set; }

        /// <summary>
        /// 插件描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 插件所在目录
        /// </summary>
        public string PluginDirectory { get; set; }

        /// <summary>
        /// 插件类型(0-开放登录插件,1-支付插件,2-配送插件，3-消息插件，4-微信支付插件，5-短信插件，6-邮件插件)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 插件类型
        /// </summary>
        [XmlIgnoreAttribute]
        public IEnumerable<PluginType> PluginTypes { get { return this.Type.Split(',').Select(item => (PluginType)int.Parse(item)); } }

        public string Author { get; set; }

        public string Version { get; set; }

        public DateTime? AddedTime { get; set; }

        public string MinHimallVersion { get; set; }

        public string MaxHimallVersion { get; set; }

        public string Logo { get; set; }

        /// <summary>
        /// 是否开启
        /// </summary>
        public bool Enable { get; set; }

        /// <summary>
        /// 插件显示顺序
        /// </summary>
        public int DisplayIndex { get; set; }
    }
}
