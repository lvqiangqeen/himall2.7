using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using Himall.Web.Models;
using Newtonsoft.Json;
using Senparc.Weixin.MP.AdvancedAPIs.Poi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Model.Models;

namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    public class PoiController : BaseSellerController
    {
        private IPoiService _iPoiService;
        private IVShopService _iVShopService;
        private WXShopInfo _settings;
        bool _isdeploy = true;

        public PoiController(IPoiService iPoiService, IVShopService iVShopService)
        {
            _iVShopService = iVShopService;
            if (CurrentSellerManager != null)
            {
                this._settings = _iVShopService.GetVShopSetting(CurrentSellerManager.ShopId);
                if (this._settings != null)
                {
                    this._iPoiService = iPoiService;
                    try
                    {
                        this._iPoiService.init(this._settings.AppId, this._settings.AppSecret);
                    }
                    catch
                    {
                        this._isdeploy = false;
                    }
                }
            }
        }

        public ActionResult UnConfig(string id)
        {
            ViewBag.Name = id;
            return View();
        }

        public ActionResult Unauthorized()
        {
            return View();
        }

        public ActionResult Index()
        {
            if (this._settings == null)
            {
                return Redirect("/sellerAdmin/Poi/UnConfig/您还未绑定微信公众号，需要绑定微信已认证服务号才可以使用摇一摇及门店管理相关功能。");
            }
            else if (!this._isdeploy)
            {
                return Redirect("/sellerAdmin/Poi/UnConfig/access_token错误或失效，请确认AppId与AppSecret配置正确");
            }

            var result = this._iPoiService.GetPoiList(0, 1);
            if( result.errcode == Senparc.Weixin.ReturnCode.api功能未授权 || ( int )result.errcode == 9001020 )
            {
                return Redirect("/sellerAdmin/Poi/Unauthorized");
            }

            return View();
        }

        public ActionResult Detail(string id)
        {
            GetStoreBaseInfo model = this._iPoiService.GetPoi(id);
            ViewBag.Id = id;
            return View(model);
        }

        public ActionResult AddPage()
        {
            try
            {
                ViewBag.Category = this._iPoiService.GetCategory();
            }
            catch( Exception e )
            {
                ViewBag.Category = new List<WXCategory>();
            }
            return View();
        }

        public ActionResult EditPage(string id)
        {
            GetStoreBaseInfo model = this._iPoiService.GetPoi(id);
            ViewBag.Id = id;
            ViewBag.Category = this._iPoiService.GetCategory();
            return View(model);
        }

        [HttpPost]
        public ActionResult Save(PoiEditModel model)
        {
            string wxPath = "";
            if (!string.IsNullOrEmpty(model.photo_list) && !(model.photo_list.IndexOf("http") >= 0))
            {
                string path = Server.MapPath("~" + model.photo_list);
                wxPath = this._iPoiService.UploadImage(path);
            }


            if (model.id == 0)
            {
                StoreBaseInfo data = new StoreBaseInfo();
                data.sid = Guid.NewGuid().ToString();

                var province = Himall.Application.RegionApplication.GetRegion(model.RegionId, CommonModel.Region.RegionLevel.Province);
                var city = Himall.Application.RegionApplication.GetRegion(model.RegionId, CommonModel.Region.RegionLevel.City);
                var county = Himall.Application.RegionApplication.GetRegion(model.RegionId, CommonModel.Region.RegionLevel.County);
                data.province = province.Name;
                data.city = city.Name;
                data.district =county.Name;
                data.address = model.address;
                data.business_name = model.business_name;
                data.branch_name = model.branch_name;
                if (string.IsNullOrEmpty(model.categoryTwo))
                {
                    data.categories = new string[] { model.categoryOne };
                }
                else
                {
                    data.categories = new string[] { model.categoryOne + "," + model.categoryTwo };
                }

                data.photo_list = new List<Store_Photo> { new Store_Photo { photo_url = wxPath } };
                data.telephone = model.telephone;
                data.avg_price = model.avg_price;
                data.open_time = model.open_time;
                data.offset_type = 1;
                data.recommend = model.recommend;
                data.special = model.special;
                data.longitude = "0";
                data.latitude = "0";
                data.introduction = model.introduction;
                CreateStoreData cd = new CreateStoreData();
                cd.business = new CreateStore_Business();
                cd.business.base_info = data;
                bool result = this._iPoiService.AddPoi(cd);
                return Json(new
                {
                    success = result
                });
            }
            else
            {
                UpdateStore_BaseInfo data = new UpdateStore_BaseInfo();
                data.poi_id = model.poi_id;
                data.telephone = model.telephone;
                data.recommend = model.recommend;
                data.special = model.special;
                data.introduction = model.introduction;
                data.open_time = model.open_time;
                data.avg_price = model.avg_price;
                data.photo_list = new List<Store_Photo> { new Store_Photo { photo_url = wxPath } };

                UpdateStoreData ud = new UpdateStoreData();
                ud.business = new UpdateStore_Business();
                ud.business.base_info = data;
                bool result = this._iPoiService.UpdatePoi(ud);
                return Json(new
                {
                    success = result
                });
            }
        }

        public ActionResult AddPoi(StoreBaseInfo poidata)
        {
            CreateStoreData data = new CreateStoreData();
            data.business = new CreateStore_Business();
            data.business.base_info = poidata;

            bool result = this._iPoiService.AddPoi(data);
            return Json(new
            {
                success = result
            });
        }

        [HttpPost]
        public ActionResult List(int page, int rows)
        {
            var models = this._iPoiService.GetPoiList(page, rows);
            var obj = (from l in models.business_list
                       select l.base_info).ToList();
            DataGridModel<GetStoreList_BaseInfo> dataGrid = new DataGridModel<GetStoreList_BaseInfo>()
            {
                rows = obj,
                total = int.Parse(models.total_count)
            };
            return Json(dataGrid);
        }

        [HttpPost]
        public ActionResult DelPoi(string id)
        {
            bool result = this._iPoiService.DeletePoi(id);
            return Json(new
            {
                success = result
            });
        }
    }

    public class PoiEditModel
    {
        public PoiEditModel()
        {
            province = "";
            city = "";
            district = "";
            address = "";
            business_name = "";
            branch_name = "";
            categoryOne = "";
            categoryTwo = "";
            photo_list = "";
            telephone = "";
            open_time = "";
            recommend = "";
            special = "";
            introduction = "";
            RegionId = 0;
        }

        /// <summary>
        /// 区域ID
        /// </summary>
        public int RegionId { set; get; }

        public string poi_id
        {
            get;
            set;
        }
        public int id
        {
            get;
            set;
        }
        public string province
        {
            get;
            set;
        }
        public string city
        {
            get;
            set;
        }
        public string district
        {
            get;
            set;
        }
        public string address
        {
            get;
            set;
        }
        public string business_name
        {
            get;
            set;
        }
        public string branch_name
        {
            get;
            set;
        }
        public string categoryOne
        {
            get;
            set;
        }
        public string categoryTwo
        {
            get;
            set;
        }
        public string photo_list
        {
            get;
            set;
        }
        public string telephone
        {
            get;
            set;
        }
        public int avg_price
        {
            get;
            set;
        }
        public string open_time
        {
            get;
            set;
        }
        public string recommend
        {
            get;
            set;
        }
        public string special
        {
            get;
            set;
        }
        public string introduction
        {
            get;
            set;
        }


        public string latitude { get; set; }
    }
}