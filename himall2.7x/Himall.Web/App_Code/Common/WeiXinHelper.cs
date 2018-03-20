using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using Himall.Web.Models;
using Senparc.Weixin.MP.Helpers;
using System.IO;

namespace Himall.Web.App_Code.Common
{
    public class WeiXinHelper
    {
        /// <summary>
        /// 二維碼分享
        /// </summary>
        /// <param name="sharePath">需分享的url</param>
        /// <param name="imgUrl"></param>
        /// <param name="imgCode"></param>
        public static void CreateQCode(string sharePath, out string fullPath, out string imgCode)
        {
            var curhttp = System.Web.HttpContext.Current;
            string url = curhttp.Request.Url.Scheme + "://" + curhttp.Request.Url.Authority; ;
            url = url + sharePath;
            fullPath = url;
            var map = Core.Helper.QRCodeHelper.Create(url);
            MemoryStream ms = new MemoryStream();
            map.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            //  将图片内存流转成base64,图片以DataURI形式显示  
            string strUrl = "data:image/gif;base64," + Convert.ToBase64String(ms.ToArray());
            ms.Dispose();
            imgCode = strUrl;
        }
    }
}