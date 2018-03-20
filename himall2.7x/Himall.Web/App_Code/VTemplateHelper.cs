using Himall.CommonModel;
using Himall.Core;
using Himall.IServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Himall.Web
{
    /// <summary>
    /// 可视化模板辅助类
    /// </summary>
    public static class VTemplateHelper
    {
        /// <summary>
        /// 获取模板目录
        /// <para>相对路径</para>
        /// </summary>
        /// <param name="client">模板名称</param>
        /// <param name="type"></param>
        /// <param name="shopId">商铺编号,0表示平台</param>
        /// <returns></returns>
        public static string GetTemplatePath(string client, VTemplateClientTypes type, long shopId = 0)
        {
            string result = "";
            switch (type)
            {
                case VTemplateClientTypes.WapIndex:
                    result = "/Areas/Admin/Templates/vshop/" + client + "/";
                    break;
                case VTemplateClientTypes.WapSpecial:
                case VTemplateClientTypes.SellerWapSpecial:
                    if (string.IsNullOrWhiteSpace(client) || client == "0")
                    {
                        client = "empty";
                    }
                    result = "/Special/" + client + "/";
                    break;
                case VTemplateClientTypes.SellerWapIndex:
                    result = "/Areas/SellerAdmin/Templates/vshop/" + shopId + "/" + client + "/";
                    break;
                case VTemplateClientTypes.WXSmallProgram:
                    result = "/AppletHome/";
                    break;
            }
            return result;
        }
        /// <summary>
        /// 获取模板信息
        /// <para>不走缓存</para>
        /// </summary>
        /// <param name="client">模板名称</param>
        /// <param name="type"></param>
        /// <param name="shopId">商铺编号,0表示平台</param>
        /// <returns></returns>
        public static string GetTemplate(string client, VTemplateClientTypes type, long shopId = 0)
        {
            string result = "";
            string tmpurl = GetTemplatePath(client, type, shopId);
            string dataurl = tmpurl + "data/default.json";
            StreamReader sr = new StreamReader(HttpContext.Current.Server.MapPath(dataurl), System.Text.Encoding.UTF8);
            try
            {
                string input = sr.ReadToEnd();
                sr.Close();
                input = input.Replace("\r\n", "").Replace("\n", "");
                result = input;
            }
            catch { }
            return result;
        }
        /// <summary>
        /// 从缓存获取模板JSON信息
        /// </summary>
        /// <param name="client"></param>
        /// <param name="type"></param>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static JObject GetTemplateByCache(string client, VTemplateClientTypes type, long shopId = 0)
        {
            JObject result = null;
            string cachename = CacheKeyCollection.MobileHomeTemplate(shopId.ToString(), client);
            if (Cache.Exists(cachename))
            {
                result = Cache.Get<JObject>(cachename);
            }
            if (result == null)
            {
                string _tmpl = GetTemplate(client, type, shopId);
                result = (JObject)JsonConvert.DeserializeObject(_tmpl);
                Cache.Insert<JObject>(cachename, result);
            }
            return result;
        }
        /// <summary>
        /// 清理模板缓存
        /// </summary>
        /// <param name="client"></param>
        /// <param name="type"></param>
        /// <param name="shopId"></param>
        public static void ClearCache(string client, VTemplateClientTypes type, long shopId = 0)
        {
            string cachename = CacheKeyCollection.MobileHomeTemplate(shopId.ToString(), client);
            Cache.Remove(cachename);
        }
        /// <summary>
        /// 获取模板节点
        /// </summary>
        /// <param name="id"></param>
        /// <param name="shopid"></param>
        /// <returns></returns>
        public static string GetTemplateItemById(string id, string client, VTemplateClientTypes type, long shopId = 0)
        {
            string result = "";
            JObject tmpljo = GetTemplateByCache(client, type, shopId);
            JToken pjt = tmpljo["PModules"];
            JToken ljt = tmpljo["LModules"];
            JToken curr = null;
            bool isfinded = false;
            foreach (var item in pjt)
            {
                if (TryGetJsonString(item, "id") == id)
                {
                    curr = item;
                    isfinded = true;
                    break;
                }
            }
            if (!isfinded)
            {
                foreach (var item in ljt)
                {
                    if (TryGetJsonString(item, "id") == id)
                    {
                        curr = item;
                        isfinded = true;
                        break;
                    }
                }

            }
            if (curr != null)
            {
                result = Base64Decode(TryGetJsonString(curr, "dom_conitem"));
            }
            return result;
        }
        /// <summary>  
        /// Base64加密  
        /// </summary>  
        /// <param name="message"></param>  
        /// <returns></returns>  
        public static string Base64Code(string message)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            return Convert.ToBase64String(bytes);
        }
        /// <summary>  
        /// Base64解密  
        /// </summary>  
        /// <param name="message"></param>  
        /// <returns></returns>  
        public static string Base64Decode(string message)
        {
            byte[] bytes = Convert.FromBase64String(message);
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// 复制目录与文件
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public static void CopyFolder(string from, string to)
        {
            if (!Directory.Exists(to))
                Directory.CreateDirectory(to);

            // 子文件夹
            foreach (string sub in Directory.GetDirectories(from))
                CopyFolder(sub + "\\", to + Path.GetFileName(sub) + "\\");

            // 文件
            foreach (string file in Directory.GetFiles(from))
                File.Copy(file, to + Path.GetFileName(file), true);
        }

        #region 私有
        /// <summary>
        /// 获取json对应值
        /// </summary>
        /// <param name="jt"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string TryGetJsonString(this JToken jt, string name)
        {
            string result = "";
            var _tmp = jt[name];
            if (_tmp != null)
            {
                result = _tmp.ToString();
            }
            return result;
        }
        /// <summary>
        /// 获取json对应值
        /// </summary>
        /// <param name="jt"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string TryGetJsonString(this JObject jt, string name)
        {
            string result = "";
            var _tmp = jt[name];
            if (_tmp != null)
            {
                result = _tmp.ToString();
            }
            return result;
        }
        #endregion
    }
}