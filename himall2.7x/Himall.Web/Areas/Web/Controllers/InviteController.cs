using Himall.IServices;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Areas.Web.Controllers
{
    public class InviteController : Controller
    {
        private IMemberInviteService _iMemberInviteService;
        public InviteController(IMemberInviteService iMemberInviteService)
        {
            _iMemberInviteService = iMemberInviteService;
        }
        // GET: Web/Invite
        public ActionResult Index()
        {
            var rule = _iMemberInviteService.GetInviteRule();
            return View(rule);
        }
    }
}