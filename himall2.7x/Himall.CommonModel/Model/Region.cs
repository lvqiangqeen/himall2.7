using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Himall.CommonModel
{
    /// <summary>
    /// 行政区域模型
    /// </summary>
    public class Region
    {
        /// <summary>
        /// 区域编号
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 区域名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 区域简称
        /// </summary>
        public string ShortName { get; set; }
        /// <summary>
        /// 下属区域
        /// </summary>

        //[ScriptIgnore]//使用JavaScriptSerializer序列化时不序列化此字段
        //[IgnoreDataMember]//使用DataContractJsonSerializer序列化时不序列化此字段
        //[Newtonsoft.Json.JsonIgnore]//使用JsonConvert序列化时不序列化此字段
        public List<Region> Sub { get; set; }

        /// <summary>
        /// 上属区域
        /// </summary>
        [ScriptIgnore]//使用JavaScriptSerializer序列化时不序列化此字段
        [IgnoreDataMember]//使用DataContractJsonSerializer序列化时不序列化此字段
        [Newtonsoft.Json.JsonIgnore]//使用JsonConvert序列化时不序列化此字段
        public Region Parent { get; set; }

        /// <summary>
        /// 上属区域编号
        /// </summary>
        [ScriptIgnore]//使用JavaScriptSerializer序列化时不序列化此字段
        [IgnoreDataMember]//使用DataContractJsonSerializer序列化时不序列化此字段
        [Newtonsoft.Json.JsonIgnore]//使用JsonConvert序列化时不序列化此字段
        public int ParentId
        {
            get
            {
                if (Parent == null)
                    return 0;
                else
                    return Parent.Id;
            }
        }

        /// <summary>
        /// 区域行政等级
        /// </summary>
        [ScriptIgnore]//使用JavaScriptSerializer序列化时不序列化此字段
        [IgnoreDataMember]//使用DataContractJsonSerializer序列化时不序列化此字段
        [Newtonsoft.Json.JsonIgnore]//使用JsonConvert序列化时不序列化此字段
        public RegionLevel Level { get; set; }

        /// <summary>
        /// 区域行政等级
        /// </summary>
        public enum RegionLevel : int
        {
            /// <summary>
            /// 省级
            /// </summary>
            Province = 1,
            /// <summary>
            /// 市级
            /// </summary>
            City = 2,
            /// <summary>
            /// 区级
            /// </summary>
            County = 3,
            /// <summary>
            /// 镇级
            /// </summary>
            Town = 4,
            /// <summary>
            /// 村级
            /// </summary>
            Village = 5,
        }

        /// <summary>
        /// 获取当前区域到最上级的名称串
        /// </summary>
        /// <param name="split">名称之前的分隔符，默认为空格</param>
        /// <returns></returns>
        public string GetNamePath(string split = " ")
        {
            if (Parent != null)
                return string.Format("{0}{1}{2}", Parent.GetNamePath(), split, Name);
            return Name;
        }

        /// <summary>
        /// 获取当前区域到最上级的id串
        /// </summary>
        /// <param name="split">名称之前的分隔符，默认为逗号</param>
        /// <returns></returns>
        public string GetIdPath(string split = ",")
        {
            if (Parent != null)
                return string.Format("{0}{1}{2}", Parent.GetIdPath(), split, Id);
            return Id.ToString();
        }
    }
}
