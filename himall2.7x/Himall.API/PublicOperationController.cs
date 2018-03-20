using Himall.Core;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Himall.API.Model.ParamsModel;

namespace Himall.API
{
    public class PublicOperationController : BaseApiController
    {
        public object UploadPic(PublicOperationUploadPicModel value)
        {
            try
            {
                string picStr = value.picStr;
             
                byte[] bytes = Convert.FromBase64String(picStr);
                System.IO.MemoryStream ms = new System.IO.MemoryStream(bytes);
               // Image img = System.Drawing.Image.FromStream(ms);

                string filename = DateTime.Now.ToString("yyyyMMddHHmmssffffff") + ".png";
             //   string DirUrl = HttpContext.Current.Server.MapPath("~/temp/");
                //if (!System.IO.Directory.Exists(DirUrl))      //检测文件夹是否存在，不存在则创建
                //{
                //    System.IO.Directory.CreateDirectory(DirUrl);
                //}
                //string path = AppDomain.CurrentDomain.BaseDirectory + "/temp/";
                //string returnpath = "/temp/" + filename;
                //img.Save(Path.Combine(path, filename));
                var fname = "/temp/" + filename;
                var ioname = Core.HimallIO.GetImagePath(fname);
               // files.Add(ioname);
                try
                {
                    Core.HimallIO.CreateFile(fname, ms);
                    //file.SaveAs(Path.Combine(path, filename));
                }
                catch (Exception)
                {

                }

				return Json(new { Success = true, Src =ioname , RomoteImage = Core.HimallIO.GetRomoteImagePath(ioname) });
            }
            catch (HimallException he)
            {
                return Json(new { Success = false, Msg = he.Message });
            }
            catch (Exception e)
            {
                return Json(new { Success = false, Msg = e.Message });
            }
        }
    }
}
