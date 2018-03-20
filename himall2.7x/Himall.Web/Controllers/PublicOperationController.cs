using Himall.Application;
using Himall.Core;
using Himall.IServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Controllers
{
    public class PublicOperationController : Controller
    {
        /// <summary>
        /// 上传文件的扩展名集合
        /// </summary>
        string[] AllowFileExtensions = new string[] { ".rar", ".zip" };

        /// <summary>
        /// 上传图片文件扩展名集合
        /// </summary>
        string[] AllowImageExtensions = new string[] { ".png", ".jpg", ".jpeg", ".gif", ".bmp" };
        /// <summary>
        /// 检查图片文件扩展名
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private bool CheckImageFileType(string filename)
        {
            var fileExtension = Path.GetExtension(filename).ToLower();
            return AllowImageExtensions.Select(x => x.ToLower()).Contains(fileExtension);
        }

        /// <summary>
        /// 检查上传文件的扩展名
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private bool CheckFileType(string filename)
        {
            var fileExtension = Path.GetExtension(filename).ToLower();
            return AllowFileExtensions.Select(x => x.ToLower()).Contains(fileExtension);
        }
        // GET: PublicOperation
        [HttpPost]
        public ActionResult UploadPic()
        {
            string test = "";
            string path = "";
            string filename = "";
           // var maxRequestLength = 15360*1024;
            List<string> files = new List<string>();
            if (Request.Files.Count == 0) return Content("NoFile", "text/html");
            else
            {
                for (var i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];
                    if (null == file || file.ContentLength <= 0) return Content("格式不正确！", "text/html");
                    //if(Request.ContentLength > maxRequestLength)
                    //{
                    //    return Content("文件大小超出限制！", "text/html");
                    //}
                    Random ra = new Random();
                    filename = DateTime.Now.ToString("yyyyMMddHHmmssffffff") + i
                        + Path.GetExtension(file.FileName);
                    if (!CheckImageFileType(filename))
                    {
                        return Content("上传的图片格式不正确", "text/html");
                    }
                    //string DirUrl = Server.MapPath("~/temp/");
                    //if (!System.IO.Directory.Exists(DirUrl))      //检测文件夹是否存在，不存在则创建
                    //{
                    //    System.IO.Directory.CreateDirectory(DirUrl);
                    //}
                    //path = AppDomain.CurrentDomain.BaseDirectory + "/temp/";

                    var fname="/temp/" + filename;
                    var ioname = Core.HimallIO.GetImagePath(fname);
                    files.Add(ioname);
                    try
                    {
                        Core.HimallIO.CreateFile(fname, file.InputStream,FileCreateType.Create);
                        //file.SaveAs(Path.Combine(path, filename));
                    }
                    catch (Exception ex)
                    {
                        Log.Error("上传文件错误",ex);
                    }
                }
            }
            return Content(string.Join(",", files), "text/html");
        }
        public ActionResult UploadPicToWeiXin()
        {
            string path = "";
            string filename = "";
            // var maxRequestLength = 15360*1024;
            List<string> files = new List<string>();
            if (Request.Files.Count == 0) return Content("NoFile", "text/html");
            else
            {
                for (var i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];
                    if (null == file || file.ContentLength <= 0) return Content("格式不正确！", "text/html");
                    //if(Request.ContentLength > maxRequestLength)
                    //{
                    //    return Content("文件大小超出限制！", "text/html");
                    //}
                    Random ra = new Random();
                    filename = DateTime.Now.ToString("yyyyMMddHHmmssffffff") + i
                        + Path.GetExtension(file.FileName);
                    if (!CheckImageFileType(filename))
                    {
                        return Content("上传的图片格式不正确", "text/html");
                    }
                    string DirUrl = Server.MapPath("~/temp/");
                    if (!System.IO.Directory.Exists(DirUrl))      //检测文件夹是否存在，不存在则创建
                    {
                        System.IO.Directory.CreateDirectory(DirUrl);
                    }
                    path = AppDomain.CurrentDomain.BaseDirectory + "/temp/";
                    files.Add("/temp/" + filename);
                    try
                    {
                        file.SaveAs(Path.Combine(path, filename));
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            return Content(string.Join(",", files), "text/html");
        }

        public ActionResult UploadPictures()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UploadFile()
        {
            string strResult = "NoFile";
            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];
                if (file.ContentLength == 0)
                {
                    strResult = "文件长度为0,格式异常。";
                }
                else
                {
                    Random ra = new Random();
                    string filename = DateTime.Now.ToString("yyyyMMddHHmmssfff") + ra.Next(1000, 9999)
                        //+ file.FileName.Substring(file.FileName.LastIndexOf("\\") + 1);
                         + Path.GetExtension(file.FileName);
                    if (!CheckFileType(filename))
                    {
                        return Content("上传的文件格式不正确", "text/html");
                    }
                    string DirUrl = Server.MapPath("~/temp/");
                    if (!System.IO.Directory.Exists(DirUrl))      //检测文件夹是否存在，不存在则创建
                    {
                        System.IO.Directory.CreateDirectory(DirUrl);
                    }
                    string strfile = filename;
                    try
                    {
                        object opcount = Core.Cache.Get(CacheKeyCollection.UserImportOpCount);
                        if (opcount == null)
                        {
                            Core.Cache.Insert(CacheKeyCollection.UserImportOpCount, 1);
                        }
                        else
                        {
                            Core.Cache.Insert(CacheKeyCollection.UserImportOpCount, int.Parse(opcount.ToString()) + 1);
                        }
                        file.SaveAs(Path.Combine(DirUrl, filename));
                    }
                    catch (Exception e)
                    {
                        object opcount = Core.Cache.Get(CacheKeyCollection.UserImportOpCount);
                        if (opcount != null)
                        {
                            Core.Cache.Insert(CacheKeyCollection.UserImportOpCount, int.Parse(opcount.ToString()) - 1);
                        }
                        Core.Log.Error("商品导入上传文件异常：" + e.Message);
                        strfile = "Error";
                    }
                    strResult = strfile;
                }
            }
            return Content(strResult, "text/html");
        }


        public ActionResult TestCache()
        {
            string result = "无";
            if( Himall.Core.Cache.Get( "tt" ) == null )
            {
                result = "失效";
                Log.Info( "缓存已经失效" );
                Himall.Core.Cache.Insert( "tt" , "zhangsan" , 7000 );
            }

            return Json( result , JsonRequestBehavior.AllowGet );
        }


        public ActionResult FullDiscount()
        {
           var model=  FullDiscountApplication.GetOngoingActiveByProductIds(new long[] { 709,700,698,696,800,825},1);

            return Json(model, JsonRequestBehavior.AllowGet);
        }
    }
}