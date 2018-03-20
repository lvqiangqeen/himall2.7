using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Himall.Core;
using System.Web;
using System.Web.Mvc;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Areas.Admin.Models;
using Himall.Web.Models;
using Himall.Core.Helper;
using System.IO;
using Himall.Application;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class CouponActivityController : BaseAdminController
    {
        public CouponActivityController()
        {
        }

        public ActionResult Index()
        {
            string link = string.Format("http://{0}/m-Wap/RegisterActivity/Gift", Request.Url.Authority);
            var map = Core.Helper.QRCodeHelper.Create(link);
            MemoryStream ms = new MemoryStream();
            map.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            //  将图片内存流转成base64,图片以DataURI形式显示  
            string strUrl = "data:image/gif;base64," + Convert.ToBase64String(ms.ToArray());
            ms.Dispose();
            ViewBag.QR = strUrl;
            ViewBag.link = link;


            var model = CouponApplication.GetCouponSendByRegister();
            return View(model);
        }

        /// <summary>
        /// 修改注册赠送优惠券设置
        /// </summary>
        /// <param name="CouponRegisterId">设置主键ID</param>
        /// <param name="status">状态</param>
        /// <param name="couponIds">优惠券ID，用','隔开</param>
        /// <returns></returns>
        public JsonResult Update(long CouponRegisterId, int status, string couponIds)
        {
            couponIds = couponIds.TrimEnd(',');
            if (couponIds != "" || status.Equals(0))//当活动开启时优惠券不能为空
            {
                var model = new Himall.DTO.CouponSendByRegisterModel()
                {
                    Id = CouponRegisterId,
                    Link = "#",
                    Status = (Himall.CommonModel.CouponSendByRegisterStatus)status
                };
                if (!couponIds.Equals(""))
                {
                    string[] arrCouponId = couponIds.Split(',');
                    foreach (string item in arrCouponId)
                    {
                        model.CouponIds.Add(new Himall.DTO.CouponModel() { Id = long.Parse(item) });
                    }
                }
                CouponApplication.SetCouponSendByRegister(model);
                return Json(new Result() { success = true, msg = "设置成功" });
            }
            else
            {
                return Json(new Result() { success = false, msg = "请选择优惠券" });
            }
        }


    }
}