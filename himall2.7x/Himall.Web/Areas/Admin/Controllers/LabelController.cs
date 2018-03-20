using Himall.Application;
using Himall.Core;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Framework;
using Himall.Web.Models;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class LabelController : BaseAdminController
    {
        IMemberLabelService _iMemberLabelService;
        IMemberService _iMemberService;
        public LabelController(IMemberLabelService iMemberLabelService, IMemberService iMemberService)
        {
            _iMemberLabelService = iMemberLabelService;
            _iMemberService = iMemberService;
        }

        [Description("会员标签管理页面")]
        public ActionResult Management()
        {
            return View();
        }

        [Description("分页获取会员管理JSON数据")]
        public JsonResult List(int page, string keywords, int rows)
        {
            var result = _iMemberLabelService.GetMemberLabelList(new LabelQuery { LabelName = keywords, PageSize = rows, PageNo = page });
            var labels = result.Models.ToList().Select(item => new LabelModel()
            {
                MemberNum = _iMemberService.GetMembersByLabel(item.Id).Count(),
                LabelName = item.LabelName,
                Id = item.Id
            });
            return Json(new { rows = labels.ToList(), total = result.Total });
            //var model = new DataGridModel<LabelModel>() { rows = labels, total = result.Total };
            //return Json(model);
        }

        public ActionResult Label(long id=0)
        {
            var model = _iMemberLabelService.GetLabel(id) ?? new LabelInfo() { };
            LabelModel labelmodel = new LabelModel()
            {
                Id = model.Id,
                LabelName = model.LabelName
            };
            return View(labelmodel);
        }
        [HttpPost]
        public JsonResult Label(LabelModel model)
        {
            LabelInfo labelmodel = new LabelInfo()
            {
                Id = model.Id,
                LabelName = model.LabelName
            };

            if (MemberLabelApplication.CheckNameIsExist(model.LabelName))
            {
                throw new HimallException("标签已经存在，不能重复！");
            }
            if (labelmodel.Id > 0)
            {
                _iMemberLabelService.UpdateLabel(labelmodel);
            }
            else
            {
                _iMemberLabelService.AddLabel(labelmodel);
            }
            return Json(new { Success=true});
        }
        public JsonResult deleteLabel(long Id)
        {
            var count = _iMemberService.GetMembersByLabel(Id).Count();
            if (count>0)
            {
                throw new HimallException("标签已经在使用，不能删除！");
            }
            _iMemberLabelService.DeleteLabel(new LabelInfo() { Id = Id });
            return Json(new { Success = true });
        }

        public JsonResult CheckLabelIsExist(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new HimallException("标签名不能为空！");
            }
            var labels = MemberLabelApplication.GetLabelList(new LabelQuery
            {
                LabelName = name
            });
            if (labels.Models.Count>0)
            {
                throw new HimallException("标签已经存在，不能重复！");
            }
            return Json(new { Success = true });
        }
    }
}