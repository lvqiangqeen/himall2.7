using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Web.Models;
using Himall.IServices;
using Himall.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Himall.Web.Areas.SellerAdmin.Models;
using System.Threading.Tasks;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class BrandController : BaseAdminController
    {
        private IBrandService _iBrandService;
        private ISearchProductService _iSearchProductService;
        public BrandController(IBrandService iBrandService,ISearchProductService iSearchProductService)
        {
            _iBrandService = iBrandService;
            _iSearchProductService = iSearchProductService;
        }

        [Description("品牌管理页面")]
        public ActionResult Management()
        {
            return View();
        }

        public ActionResult Verify()
        {
            return View();
        }

        public ActionResult Show(long id)
        {
            var model = _iBrandService.GetBrandApply(id);
            return View(model);
        }

        [Description("分页获取品牌列表JSON数据")]
        [HttpPost]
        // GET: Admin/Brand
        public JsonResult List(int page, int rows, string keyWords)
        {
            keyWords = keyWords.Trim();
            var result = _iBrandService.GetBrands(keyWords, page, rows);
            IEnumerable<BrandModel> brands = result.Models.ToArray().Select(item => new BrandModel()
          {
              BrandName = item.Name,
              BrandLogo =Core.HimallIO.GetImagePath(item.Logo),
              ID = item.Id,
              //AuditStatus = (int)item.AuditStatus
          });
            DataGridModel<BrandModel> model = new DataGridModel<BrandModel>() { rows = brands, total = result.Total };
            return Json(model);
        }

        public JsonResult ApplyList(int page, int rows, string keyWords)
        {
            keyWords = keyWords.Trim();
            var result = _iBrandService.GetShopBrandApplys(null, 0, page, rows, keyWords);
            IEnumerable<BrandApplyModel> brands = result.Models.ToArray().Select(item => new BrandApplyModel()
            {
                Id = item.Id,
                BrandId = item.BrandId == null ? 0 : (long)item.BrandId,
                ShopId = item.ShopId,
                BrandName = item.BrandName,
                BrandLogo = item.Logo,
                BrandDesc = item.Description == null ? "" : item.Description,
                BrandAuthPic = item.AuthCertificate,
                Remark = item.Remark,
                BrandMode = item.ApplyMode,
                AuditStatus = item.AuditStatus,
                ApplyTime = item.ApplyTime.ToString("yyyy-MM-dd"),
                ShopName = item.Himall_Shops.ShopName,
            });
            DataGridModel<BrandApplyModel> model = new DataGridModel<BrandApplyModel>() { rows = brands, total = result.Total };
            return Json(model);
        }

        [HttpPost]
        [Description("删除品牌")]
        public JsonResult Delete(int id)
        {
            _iBrandService.DeleteBrand(id);

            return Json(new Result() { success = true, msg = "删除成功！" });

        }

        [HttpPost]
        [Description("删除品牌申请")]
        public JsonResult DeleteApply(int id)
        {
            _iBrandService.DeleteApply(id);

            return Json(new Result() { success = true, msg = "删除成功！" });

        }

        [HttpPost]
        public JsonResult Audit(int id)
        {
            _iBrandService.AuditBrand(id, Himall.Model.ShopBrandApplysInfo.BrandAuditStatus.Audited);
            return Json(new Result() { success = true, msg = "审核成功！" });
        }

        [HttpPost]
        public JsonResult Refuse(int id)
        {
            _iBrandService.AuditBrand(id, Himall.Model.ShopBrandApplysInfo.BrandAuditStatus.Refused);
            return Json(new Result() { success = true, msg = "拒绝成功！" });
        }

        [HttpPost]
        // GET: Admin/Brand
        public JsonResult GetBrands(string keyWords, int? AuditStatus = 2)
        {
            var after = _iBrandService.GetBrands(keyWords);
            var values = after.Select(item => new { key = item.Id, value = item.Name, envalue = item.RewriteName });
            return Json(values);
        }

        public ActionResult Edit(long id)
        {
            var brand = _iBrandService.GetBrand(id);

            BrandModel model = new BrandModel()
            {
                ID = brand.Id,
                BrandName = brand.Name,
                BrandDesc = brand.Description,
                BrandLogo = brand.Logo,
                //BrandEnName = brand.RewriteName,
                MetaDescription = brand.Meta_Description,
                MetaKeyWord = brand.Meta_Keywords,
                MetaTitle = brand.Meta_Title,
                //ShopID = brand.ShopId,
                IsRecommend = brand.IsRecommend,
                //AuditStatus = (int)brand.AuditStatus
            };
            return View(model);
        }


        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Add(BrandModel brand)
        {
            if (ModelState.IsValid)
            {
                BrandInfo model = new BrandInfo()
                {
                    Name = brand.BrandName.Trim(),
                    Description = brand.BrandDesc,
                    Logo = brand.BrandLogo,
                    Meta_Description = brand.MetaDescription,
                    Meta_Keywords = brand.MetaKeyWord,
                    Meta_Title = brand.MetaTitle,
                    //RewriteName = brand.BrandEnName,
                    //ShopId = 0,
                    IsRecommend = brand.IsRecommend,
                    //AuditStatus = BrandInfo.BrandAuditStatus.Audited
                };
                bool flag = _iBrandService.IsExistBrand(brand.BrandName);
                if (flag == false)
                {
                    _iBrandService.AddBrand(model);
                }
                return RedirectToAction("Management");
            }
            return View(brand);
        }

        [HttpPost]
        public JsonResult IsExist(string name)
        {
            bool flag = _iBrandService.IsExistBrand(name);
            if (flag == false)
            {
                return Json(new Result() { success = false, msg = null });
            }
            else
                return Json(new Result() { success = true, msg = "该品牌已存在，请不要重复添加！" });
        }

        [HttpPost]
        [OperationLog(Message = "编辑品牌")]
        public ActionResult Edit(BrandModel brand)
        {
            if (ModelState.IsValid)
            {
                BrandInfo model = new BrandInfo()
                {
                    Id = brand.ID,
                    Name = brand.BrandName.Trim(),
                    Description = brand.BrandDesc,
                    Logo = brand.BrandLogo,
                    Meta_Description = brand.MetaDescription,
                    Meta_Keywords = brand.MetaKeyWord,
                    Meta_Title = brand.MetaTitle,
                    //RewriteName = brand.BrandEnName,
                    IsRecommend = brand.IsRecommend,
                };
                _iBrandService.UpdateBrand(model);
                //更新商品搜索冗余数据
                Task.Factory.StartNew(() =>
                {
                    _iSearchProductService.UpdateBrand(model);
                });
                return RedirectToAction("Management");
            }
            return View(brand);
        }

        [HttpPost]
        public JsonResult IsInUse(long id)
        {
            bool flag = _iBrandService.BrandInUse(id);
            if (flag == false)
            {
                return Json(new Result() { success = false, msg = "该品牌尚未使用！" });
            }
            else
                return Json(new Result() { success = true, msg = "该品牌已使用！" });
        }
    }
}












