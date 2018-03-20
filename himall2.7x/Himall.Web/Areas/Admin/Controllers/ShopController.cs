using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Models;
using System.Collections.Generic;
using System.Linq;
using Himall.Core;
using System.Web.Mvc;
using Himall.Web.Framework;
using System.ComponentModel;
using System;
using Himall.Application;
using System.Threading.Tasks;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class ShopController : BaseAdminController
    {
        IShopService _iShopService;
        IProductService _iProductService;
        IOperationLogService _iOperationLogService;
        IRegionService _iRegionService;
        ICategoryService _iCategoryService;
        ISearchProductService _iSearchProductService;
        public ShopController(IShopService iShopService,
            IProductService iProductService,
            IOperationLogService iOperationLogService,
            IRegionService iRegionService,
            ICategoryService iCategoryService,
            ISearchProductService iSearchProductService)
        {
            _iShopService = iShopService;
            _iProductService = iProductService;
            _iOperationLogService = iOperationLogService;
            _iRegionService = iRegionService;
            _iCategoryService = iCategoryService;
            _iSearchProductService = iSearchProductService;
            //获取当前店铺发布商品的数量
        }
        // GET: Admin/Shop
        public ActionResult Management(string type = "", int s = 0)
        {
            Himall.Web.Models.ShopModel result = new ShopModel();
            var state = (Himall.Model.ShopInfo.ShopAuditStatus)s;
            List<string> drpstate = new List<string>();
            drpstate.Add("2");
            //drpstate.Add("3");
            drpstate.Add("5");

            var status = Himall.Model.ShopInfo.ShopAuditStatus.Open.ToSelectList();
            if (state > 0)
            {
                status = state.ToSelectList();
            }
            var StatusList = type == "Auditing" ? status.Where(c => drpstate.Contains(c.Value)) : status;
            var ssellist = StatusList.Select(d => new { Id = d.Value, Name = d.Text }).ToList();
            //if (type == "Auditing")
            //{
            ssellist.Insert(0, new { Id = "", Name = "请选择..." });
            //}
            SelectList StateSelectList = new SelectList((ssellist), "Id", "Name", s);

            ViewBag.StatusList = StateSelectList;
            var grade = _iShopService.GetShopGrades();
            List<SelectListItem> gradeList = new List<SelectListItem>{new SelectListItem
            {
                Selected = true,
                Value = 0.ToString(),
                Text = "请选择..."
            }};
            foreach (var item in grade)
            {
                gradeList.Add(new SelectListItem
                {
                    Selected = false,
                    Value = item.Id.ToString(),
                    Text = item.Name
                });
            }
            ViewBag.Type = type;
            ViewBag.Grade = gradeList;
            return View(result);
        }

        public ActionResult ShopOverview()
        {
            Himall.Web.Models.ShopModel result = new ShopModel();
            return View(result);
        }


        public JsonResult GetApplyList(int page, int rows, string shopName, BusinessCategoriesApplyInfo.BusinessCateApplyStatus? status)
        {
            BussinessCateApplyQuery query = new BussinessCateApplyQuery();
            query.PageNo = page;
            query.PageSize = rows;
            query.ShopName = shopName;
            query.Status = status;
            var model = _iShopService.GetBusinessCateApplyList(query);
            var cate = model.Models.ToList().Select(a => new { Id = a.Id, ShopName = a.ShopName, ApplyDate = a.ApplyDate.ToString("yyyy-MM-dd HH:mm"), AuditedStatus = a.AuditedStatus.ToDescription() });
            var p = new { rows = cate.ToList(), total = model.Total };
            return Json(p);
        }


        public ActionResult ApplyDetail(long Id)
        {
            var model = _iShopService.GetBusinessCategoriesApplyInfo(Id);
            return View(model);
        }

        public JsonResult AgreeOrNot(Himall.Model.BusinessCategoriesApplyInfo.BusinessCateApplyStatus type, long id)
        {
            _iShopService.AuditShopBusinessCate(id, type);
            return Json(new Result() { success = true, msg = "审核成功" });
        }

        /// <summary>
        /// 冻结
        /// </summary>
        /// <param name="id">店铺标识</param>
        /// <param name="state">是否冻结</param>
        [HttpPost]
        public JsonResult FreezeShop(long id, bool state)
        {
            _iShopService.FreezeShop(id, state);
            Task.Factory.StartNew(() =>
            {
                _iSearchProductService.UpdateSearchStatusByShop(id);
            });
            _iOperationLogService.AddPlatformOperationLog(
               new LogInfo
               {
                   Date = DateTime.Now,
                   Description = (state ? "冻结" : "解冻") + "店铺信息，Id=" + id,
                   IPAddress = Request.UserHostAddress,
                   PageUrl = "/Shop/FreezeShop/",
                   UserName = CurrentManager.UserName,
                   ShopId = 0

               });
            return Json(new Result() { success = true, msg = "操作成功" });
        }


        public ActionResult ApplyList()
        {
            return View();
        }




        [HttpPost]
        [UnAuthorize]
        public JsonResult List(long? shopGradeId, int? shopStatus, int page, int rows, string shopName, string shopAccount, string type = "")
        {
            var queryModel = new ShopQuery()
            {
                Status = (Himall.Model.ShopInfo.ShopAuditStatus?)shopStatus,
                PageSize = rows,
                PageNo = page,
                ShopAccount = shopAccount,
                ShopName = shopName,
                ShopGradeId = shopGradeId
            };
            if (type == "Auditing")
            {
                if (shopStatus == null)
                {
                    queryModel.Status = Himall.Model.ShopInfo.ShopAuditStatus.WaitAudit;
                    queryModel.MoreStatus.Add(Himall.Model.ShopInfo.ShopAuditStatus.WaitConfirm);
                }
            }

            var shops = ShopApplication.GetShops(queryModel);
            var shopGrades = ShopApplication.GetShopGrades();
            var shopAccounts = ShopApplication.GetShopAccounts(shops.Models.Select(p => p.Id));

            var models = shops.Models.Select(item =>
            {
                var shopGrade = shopGrades.FirstOrDefault(p => p.Id == item.GradeId);
                var shopAccountModel = shopAccounts.FirstOrDefault(p => p.ShopId == item.Id);

                return new ShopModel()
                {
                    Id = item.Id,
                    Account = item.ShopAccount,
                    EndDate = type == "Auditing" ? "--" : item.EndDate.HasValue ? item.EndDate.Value.ToString("yyyy-MM-dd") : "",
                    Name = item.ShopName,
                    ShopGrade = shopGrade != null ? shopGrade.Name : "",
                    Status = item.EndDate < DateTime.Now ? "已过期" : item.ShowShopAuditStatus.ToDescription(),
                    IsSelf = item.IsSelf,
                    BusinessType = item.BusinessType == null ? Himall.CommonModel.ShopBusinessType.Enterprise : item.BusinessType.Value,
                    Balance = shopAccountModel != null ? shopAccountModel.Balance : 0
                };
            });

            var dataGrid = new DataGridModel<ShopModel>() { rows = models, total = shops.Total };
            return Json(dataGrid);
        }

        /// <summary>
        /// 修改商家入驻信息
        /// </summary>
        /// <param name="shop"></param>
        /// <returns></returns>
        [HttpPost]
        [UnAuthorize]
        public JsonResult Edit(ShopModel shop)
        {
            var service = _iShopService;
            var nowShop = service.GetShop(shop.Id);

            //验证修改的店铺等级是否通过
            if (!CheckShopGrade(shop.Id, Convert.ToInt64(shop.ShopGrade)))
            {
                throw new HimallException("该店铺已使用空间数或已添加商品数大于该套餐");
            }

            if (service.ExistShop(shop.Name, shop.Id))
            {
                throw new HimallException("该店铺已存在");
            }
            //日龙修改
            //去除检查公司名称重复
            //if (service.ExistShop(nowShop.CompanyName, shop.Id))
            //{
            //    throw new HimallException("该公司名称已存在");
            //}

            if (ShopApplication.ExistBusinessLicenceNumber(shop.BusinessLicenceNumber, shop.Id))
            {
                throw new HimallException("营业执照号已存在");
            }
            bool isExpired = false;
            if (ModelState.IsValid)
            {
                isExpired = Convert.ToDateTime(shop.EndDate) < DateTime.Now.AddDays(1).Date;

                nowShop.CompanyName = shop.CompanyName;
                nowShop.CompanyAddress = shop.CompanyAddress;
                nowShop.CompanyEmployeeCount = shop.CompanyEmployeeCount;
                nowShop.ShopName = shop.Name;
                nowShop.GradeId = Convert.ToInt64(shop.ShopGrade);
                nowShop.EndDate = Convert.ToDateTime(shop.EndDate);
                nowShop.CompanyRegionId = shop.NewCompanyRegionId;
                nowShop.ShopStatus = (ShopInfo.ShopAuditStatus)(Convert.ToInt32(shop.Status));

                nowShop.BusinessLicenseCert = Request.Form["BusinessLicenseCert"];
                nowShop.ProductCert = Request.Form["ProductCert"];
                nowShop.OtherCert = Request.Form["OtherCert"];
                nowShop.BusinessLicenceNumber = shop.BusinessLicenceNumber;
                nowShop.BusinessSphere = shop.BusinessSphere;
                nowShop.BusinessLicenceNumberPhoto = shop.BusinessLicenceNumberPhoto == null ? "" : shop.BusinessLicenceNumberPhoto;
                nowShop.OrganizationCode = shop.OrganizationCode;
                nowShop.OrganizationCodePhoto = shop.OrganizationCodePhoto;
                nowShop.GeneralTaxpayerPhot = shop.GeneralTaxpayerPhot;
                if (nowShop.EndDate > DateTime.Now)
                {
                    nowShop.ShopStatus = ShopInfo.ShopAuditStatus.Open;
                }

                _iShopService.UpdateShop(nowShop);

                Task.Factory.StartNew(() =>
                {
                    _iSearchProductService.UpdateShop(shop.Id, shop.Name);
                    if (isExpired)
                        _iSearchProductService.UpdateSearchStatusByShop(shop.Id);
                });

                _iOperationLogService.AddPlatformOperationLog(
                   new LogInfo
                   {
                       Date = DateTime.Now,
                       Description = "修改店铺信息，Id=" + shop.Id,
                       IPAddress = Request.UserHostAddress,
                       PageUrl = "/Shop/Edit/" + shop.Id,
                       UserName = CurrentManager.UserName,
                       ShopId = 0

                   });
                return Json(new Result { success = true, msg = "保存成功！" });
            }
            else
            {
                List<string> sb = new List<string>();
                //获取所有错误的Key
                List<string> Keys = ModelState.Keys.ToList();
                //获取每一个key对应的ModelStateDictionary
                foreach (var key in Keys)
                {
                    var errors = ModelState[key].Errors.ToList();
                    //将错误描述添加到sb中
                    foreach (var error in errors)
                    {
                        sb.Add(error.ErrorMessage);
                    }
                }
                return Json(new Result { success = false, msg = sb[0].ToString() });
            }

        }

        /// <summary>
        /// 修改个人入驻信息
        /// </summary>
        /// <param name="shop"></param>
        /// <returns></returns>
        [HttpPost]
        [UnAuthorize]
        public JsonResult EditPersonal(ShopPersonal shop)
        {
            var service = _iShopService;
            var nowShop = service.GetShop(shop.Id);

            //验证修改的店铺等级是否通过
            if (!CheckShopGrade(shop.Id, Convert.ToInt64(shop.ShopGrade)))
            {
                throw new HimallException("该店铺已使用空间数或已添加商品数大于该套餐");
            }

            if (service.ExistShop(shop.Name, shop.Id))
            {
                throw new HimallException("该店铺已存在");
            }
            //日龙修改
            //去除检查公司名称重复
            //if (service.ExistShop(nowShop.CompanyName, shop.Id))
            //{
            //    throw new HimallException("该公司名称已存在");
            //}
            bool isExpired = false;
            if (ModelState.IsValid)
            {
                isExpired = Convert.ToDateTime(shop.EndDate) < DateTime.Now.AddDays(1).Date;
                nowShop.CompanyName = shop.CompanyName;
                nowShop.CompanyAddress = shop.CompanyAddress;
                nowShop.ShopName = shop.Name;
                nowShop.GradeId = Convert.ToInt64(shop.ShopGrade);
                nowShop.EndDate = Convert.ToDateTime(shop.EndDate);
                nowShop.IDCard = shop.IDCard;
                nowShop.IDCardUrl = shop.IDCardUrl;
                nowShop.IDCardUrl2 = shop.IDCardUrl2;
                nowShop.CompanyRegionId = shop.NewCompanyRegionId;

                _iShopService.UpdateShop(nowShop);
                Task.Factory.StartNew(() =>
                {
                    _iSearchProductService.UpdateShop(shop.Id, shop.Name);
                    if (isExpired)
                        _iSearchProductService.UpdateSearchStatusByShop(shop.Id);
                });

                _iOperationLogService.AddPlatformOperationLog(
                   new LogInfo
                   {
                       Date = DateTime.Now,
                       Description = "修改店铺信息，Id=" + shop.Id,
                       IPAddress = Request.UserHostAddress,
                       PageUrl = "/Shop/EditPersonal/" + shop.Id,
                       UserName = CurrentManager.UserName,
                       ShopId = 0

                   });
                return Json(new Result { success = true, msg = "保存成功！" });
            }
            else
            {
                List<string> sb = new List<string>();
                //获取所有错误的Key
                List<string> Keys = ModelState.Keys.ToList();
                //获取每一个key对应的ModelStateDictionary
                foreach (var key in Keys)
                {
                    var errors = ModelState[key].Errors.ToList();
                    //将错误描述添加到sb中
                    foreach (var error in errors)
                    {
                        sb.Add(error.ErrorMessage);
                    }
                }
                return Json(new Result { success = false, msg = sb[0].ToString() });
            }

        }

        /// <summary>
        /// 企业入驻信息编辑
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [UnAuthorize]
        public ActionResult Edit(long id = 0)
        {
            if (id == 0)
                return View(new ShopModel());
            var shop = _iShopService.GetShop(id);
            var shopGrade = _iShopService.GetShopGrades();
            List<SelectListItem> ShopG = new List<SelectListItem>();
            foreach (var item in shopGrade)
            {
                ShopG.Add(new SelectListItem
                {
                    Selected = item.Id == shop.GradeId,
                    Text = item.Name,
                    Value = item.Id.ToString()
                });
            }
            ViewBag.ShopGrade = ShopG;
            ViewBag.Status = (int)shop.ShopStatus;
            ViewBag.BankRegionIds = _iRegionService.GetRegionPath(shop.BankRegionId);
            ViewBag.CompanyRegionIds = _iRegionService.GetRegionPath(shop.CompanyRegionId);
            ViewBag.BusinessLicenceArea = _iRegionService.GetRegionPath(shop.BusinessLicenceRegionId);
            return View(new ShopModel(shop));
        }

        /// <summary>
        /// 个人入驻信息编辑
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [UnAuthorize]
        public ActionResult EditPersonal(long id = 0)
        {
            if (id == 0)
                return View(new ShopModel());
            var shop = _iShopService.GetShop(id);

            ShopPersonal shopPersoal = new ShopPersonal()
            {
                Id = shop.Id,
                CompanyAddress = shop.CompanyAddress,
                CompanyName = shop.CompanyName,
                CompanyRegion = ServiceHelper.Create<IRegionService>().GetFullName(shop.CompanyRegionId),
                EndDate = shop.EndDate.HasValue ? shop.EndDate.Value.ToString("yyyy-MM-dd") : "",
                IDCard = shop.IDCard,
                IDCardUrl = shop.IDCardUrl,
                IDCardUrl2 = shop.IDCardUrl2,
                NewCompanyRegionId = shop.CompanyRegionId,
                Name = shop.ShopName
            };
            var obj = ServiceHelper.Create<IShopService>().GetShopGrade(shop.GradeId);
            shopPersoal.ShopGrade = obj == null ? "" : obj.Name;


            var shopGrade = _iShopService.GetShopGrades();
            List<SelectListItem> ShopG = new List<SelectListItem>();
            foreach (var item in shopGrade)
            {
                ShopG.Add(new SelectListItem
                {
                    Selected = item.Id == shop.GradeId,
                    Text = item.Name,
                    Value = item.Id.ToString()
                });
            }
            ViewBag.ShopGrade = ShopG;
            ViewBag.Status = (int)shop.ShopStatus;
            ViewBag.BankRegionIds = _iRegionService.GetRegionPath(shop.BankRegionId);
            ViewBag.CompanyRegionIds = _iRegionService.GetRegionPath(shop.CompanyRegionId);
            ViewBag.BusinessLicenceArea = _iRegionService.GetRegionPath(shop.BusinessLicenceRegionId);
            return View(shopPersoal);
        }

        [UnAuthorize]
        [HttpPost]
        public JsonResult DeleteShop(long Id)
        {
            _iShopService.DeleteShop(Id);
            _iOperationLogService.AddPlatformOperationLog(
                   new LogInfo
                   {
                       Date = DateTime.Now,
                       Description = "删除店铺，Id=" + Id,
                       IPAddress = Request.UserHostAddress,
                       PageUrl = "/Shop/Edit/" + Id,
                       UserName = CurrentManager.UserName,
                       ShopId = 0

                   });
            return Json(new { Successful = true });
        }

        public ActionResult Details(long id)
        {
            var shop = _iShopService.GetShop(id, true);
            var model = new ShopModel(shop);
            model.BusinessCategory = new List<CategoryKeyVal>();
            foreach (var key in shop.BusinessCategory.Keys)
            {
                model.BusinessCategory.Add(new CategoryKeyVal
                {
                    CommisRate = shop.BusinessCategory[key],
                    Name = _iCategoryService.GetCategory(key).Name
                });
            }

            var shopModel = _iShopService.GetShop(id);
            ViewBag.PassStr = shopModel.ShopStatus.ToDescription();
            return View(model);
        }

        [HttpGet]
        [UnAuthorize]
        public JsonResult GetCategoryCommisRate(long id)
        {
            var comm = _iCategoryService.GetCategory(id).CommisRate;
            return Json(new { successful = true, CommisRate = comm }, JsonRequestBehavior.AllowGet);
        }

        [Description("开店审核页面")]
        [HttpGet]
        [UnAuthorize]
        public ActionResult Auditing(long id)
        {
            var shop = _iShopService.GetShop(id, true);
            ViewBag.ShopId = id;
            ViewBag.Status = (int)shop.ShopStatus;
            var model = new ShopModel(shop);
            model.BusinessCategory = new List<CategoryKeyVal>();
            foreach (var key in shop.BusinessCategory.Keys)
            {
                model.BusinessCategory.Add(new CategoryKeyVal
                {
                    CommisRate = shop.BusinessCategory[key],
                    Name = _iCategoryService.GetCategory(key).Name
                });
            }
            return View(model);
        }

        [Description("开店审核页面(POST)")]
        [HttpPost]
        [UnAuthorize]
        public JsonResult Auditing(long shopId, int status, string comment = "")
        {
            Himall.DTO.Settled mSettled = ShopApplication.GetSettled();

            _iShopService.UpdateShopStatus(shopId, (ShopInfo.ShopAuditStatus)status, comment, mSettled.TrialDays);
            _iOperationLogService.AddPlatformOperationLog(
                   new LogInfo
                   {
                       Date = DateTime.Now,
                       Description = string.Format("开店审核页面，店铺Id={0},状态为：{1}, 说明是：{2}", shopId,
                       ((ShopInfo.ShopAuditStatus)status).ToString(), comment),
                       IPAddress = Request.UserHostAddress,
                       PageUrl = "/Shop/Auditing/" + shopId,
                       UserName = CurrentManager.UserName,
                       ShopId = 0

                   });
            return Json(new { Successful = true });
        }

        [Description("类目经营页面")]
        public ActionResult BusinessCategory(long id)
        {
            var bc = _iShopService.GetBusinessCategory(id).ToList();
            ViewBag.ShopId = id;
            return View(bc);
        }

        public JsonResult CanDeleteBusinessCategory(long id, long cid)
        {
            var result = new Result { status = -1, msg = "未知异常", success = false };
            if (_iShopService.CanDeleteBusinessCategory(id, cid))
            {
                result = new Result { status = 1, msg = "可以删除", success = true };
            }
            else
            {
                result = new Result { status = -1, msg = "不可删除，有商品被购买", success = false };
            }
            return Json(result);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult SaveBusinessCategory(long shopId = 0, string bcategory = "")
        {
            Dictionary<long, decimal> businessCategory = new Dictionary<long, decimal>();
            foreach (var item in bcategory.Split(','))
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    if (!businessCategory.ContainsKey(int.Parse(item.Split('|')[0])))
                        businessCategory.Add(int.Parse(item.Split('|')[0]), decimal.Parse(item.Split('|')[1]));
                }
            }
            _iShopService.SaveBusinessCategory(shopId, businessCategory);
            _iOperationLogService.AddPlatformOperationLog(
           new LogInfo
           {
               Date = DateTime.Now,
               Description = "修改店铺经营类目，店铺Id=" + shopId,
               IPAddress = Request.UserHostAddress,
               PageUrl = "/Shop/SaveBusinessCategory?shopId=" + shopId + "&bcategory=" + bcategory,
               UserName = CurrentManager.UserName,
               ShopId = 0

           });
            return Json(new { Successful = true });
        }

        [HttpPost]
        public JsonResult UpdateShopCommisRate(long businessCategoryId, decimal commisRate)
        {
            _iShopService.SaveBusinessCategory(businessCategoryId, commisRate);
            return Json(new { success = true });
        }
        /// <summary>
        /// 验证修改的店铺等级是否符合要求
        /// </summary>
        /// <param name="shopId">店铺编号</param>
        /// <param name="newId">需修改的店铺等级编号</param>
        /// <returns></returns>
        [HttpPost]
        public bool CheckShopGrade(long shopId, long newId)
        {
            var shopservice = _iShopService;
            var productservice = _iProductService;
            //获取当前店铺发布商品的数量
            var productCount = productservice.GetShopAllProducts(shopId);
            //获取当前店铺使用的空间
            var SpaceUsage = shopservice.GetShopSpaceUsage(shopId);
            //正使用的空间
            var UseSpaceing = SpaceUsage > 0 ? SpaceUsage : 0;

            if (newId > 0)
            {
                var newshopGrade = _iShopService.GetShopGrade(newId);
                if (newshopGrade != null)
                {
                    var newproductLimit = newshopGrade.ProductLimit;
                    var newimageLimit = newshopGrade.ImageLimit;
                    //如果修改的店铺等级商品发布数、使用空间 任何一个小于正在使用的商品发布数、使用空间 则不通过
                    if (newproductLimit < productCount || newimageLimit < UseSpaceing)
                        return false;
                    else
                        return true;
                }
                else
                    return true;
            }
            else
                return true;
        }
    }
}