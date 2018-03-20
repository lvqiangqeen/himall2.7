using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Himall.Core.Helper
{
    public static class HtmlContentHelper
    {
        #region 转移图片

        /// <summary>
        /// 将HTML中的图片转为本地图片
        /// </summary>
        /// <param name="htmlText">待转换的HTML文本</param>
        /// <param name="desDir">图片要存储到的目录</param>
        /// <param name="relativeRootPath">图片使用相对地址时的根目录</param>
        /// <param name="imgSrcPreText">修改后的图片SRC的前缀</param>
        /// <returns></returns>
        public static string TransferToLocalImage(string htmlText, string relativeRootPath, string desDir, string imgSrcPreText = "")
        {
            if (!relativeRootPath.EndsWith("/"))
                relativeRootPath += "/";

            //确认目的目录存在
            //if (!System.IO.Directory.Exists(desDir))
            //    System.IO.Directory.CreateDirectory(desDir);

            int i = 0;
            List<string> imgUrls = GetHtmlImageUrlList(htmlText).ToList();
            List<string> validUrls = imgUrls.FindAll(imgurl => !imgurl.StartsWith("data:"));
            WebClient webClient = new WebClient();
            string imgFileName, imgFullName;
            string ext;
            string[] seps;

            foreach (string oriImgUrl in validUrls)
            {

                seps = oriImgUrl.Split('.');
                ext = seps[seps.Length - 1];
                imgFileName = Guid.NewGuid().ToString("N") + "." + ext;
                imgFullName = desDir + "/" + imgFileName;
                try
                {
                    if ((oriImgUrl.StartsWith("http://")||oriImgUrl.StartsWith("https://"))&&oriImgUrl.IndexOf("/Storage")<0)//网络图片
                    {
                       var data=webClient.DownloadData(oriImgUrl);
                       Stream stream = new MemoryStream(data);
                       Core.HimallIO.CreateFile(imgFullName, stream,FileCreateType.Create);
                       htmlText = htmlText.Replace(oriImgUrl, imgSrcPreText + imgFileName);
                    }
                    else if (oriImgUrl.IndexOf("temp/") >0)
                    { 
                       Core.HimallIO.CopyFile(relativeRootPath + oriImgUrl, imgFullName, true);
                       htmlText = htmlText.Replace(oriImgUrl, imgSrcPreText + imgFileName);
                    }
                  
                }
                catch { }
                i++;
            }
            //htmlText = (new Regex(" href=([\'\"]).*?\\1").Replace(htmlText, "")).Replace("<IMG", "<img").Replace("</IMG", "</img");
            htmlText = htmlText.Replace("<IMG", "<img").Replace("</IMG", "</img");
            return htmlText;
        }


        /// <summary>   
        /// 取得HTML中所有图片的 URL。   
        /// </summary>   
        /// <param name="htmlText">HTML代码</param>   
        /// <returns>图片的URL列表</returns>   
        static IEnumerable<string> GetHtmlImageUrlList(string htmlText)
        {
            // 定义正则表达式用来匹配 img 标签   
            Regex regImg = new Regex(@"<img\b[^<>]*?\bsrc[\s\t\r\n]*=[\s\t\r\n]*[""']?[\s\t\r\n]*(?<imgUrl>[^\s\t\r\n""'<>]*)[^<>]*?/?[\s\t\r\n]*>", RegexOptions.IgnoreCase);

            // 搜索匹配的字符串   
            MatchCollection matches = regImg.Matches(htmlText);
            int i = 0;
            string[] sUrlList = new string[matches.Count];

            // 取得匹配项列表   
            foreach (Match match in matches)
                sUrlList[i++] = match.Groups["imgUrl"].Value;

            return sUrlList;
        }

        /// <summary>
        /// 清除HTML中的JS脚本和style脚本
        /// </summary>
        /// <param name="htmlText"></param>
        /// <returns></returns>
        public static string RemoveScriptsAndStyles(string htmlText)
        {
            htmlText = Regex.Replace(htmlText, @"<\s*script[^>]*?>.*?<\s*/\s*script\s*>", "", RegexOptions.IgnoreCase);
            htmlText = Regex.Replace(htmlText, @"<\s*style[^>]*?>.*?<\s*/\s*style\s*>", "", RegexOptions.IgnoreCase);
            return htmlText;
        }

        #endregion

    }
}
