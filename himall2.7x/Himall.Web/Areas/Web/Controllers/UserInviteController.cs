using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Model;
using System.IO;

namespace Himall.Web.Areas.Web.Controllers
{
    public class UserInviteController : BaseMemberController
    {
        private IMemberInviteService _iMemberInviteService;
        public UserInviteController(IMemberInviteService iMemberInviteService)
        {
            _iMemberInviteService = iMemberInviteService;
        }

        public ActionResult Index()
        {
            var userId = CurrentUser.Id;
            var model = _iMemberInviteService.GetMemberInviteInfo(userId);
            var rule = _iMemberInviteService.GetInviteRule();

            string host = Request.Url.Host;
            string scheme = Request.Url.Scheme;
            //host += Request.Url.Port != 80 ? ":"+Request.Url.Port.ToString() : "";
              model.InviteLink = String.Format("{0}://{1}/Register/index/{2}", scheme, host, userId);
              var map = Core.Helper.QRCodeHelper.Create(model.InviteLink);
            MemoryStream ms = new MemoryStream();
            map.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            //  将图片内存流转成base64,图片以DataURI形式显示  
            string strUrl = "data:image/gif;base64," + Convert.ToBase64String(ms.ToArray());
            ms.Dispose();
            model.QR = strUrl;
            var  m = new Tuple<UserInviteModel, InviteRuleInfo,UserMemberInfo>(model, rule,CurrentUser);
            return View(m);
        }
    }
}