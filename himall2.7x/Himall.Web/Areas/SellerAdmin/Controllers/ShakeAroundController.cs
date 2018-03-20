using Himall.Core;
using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using Himall.Web.Models;
using Senparc.Weixin.MP.AdvancedAPIs.ShakeAround;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    public class ShakeAroundController : BaseSellerController
    {
        IShakeAroundService _iShakeAroundService;
        IPoiService _iPoiService = null;
        WXShopInfo _settings = null;
        bool _isdeploy = true;

        public ShakeAroundController(IVShopService iVShopService, IShakeAroundService iShakeAroundService, IPoiService iPoiService)
        {
            var _ivshop = iVShopService;
            if (CurrentSellerManager != null)
            {
                this._settings = _ivshop.GetVShopSetting(CurrentSellerManager.ShopId);
                this._iShakeAroundService = iShakeAroundService;
                this._iPoiService = iPoiService;
                try
                {
                    this._iShakeAroundService.init(this._settings.AppId, this._settings.AppSecret);
                    this._iPoiService.init(this._settings.AppId, this._settings.AppSecret);
                }
                catch
                {
                    this._isdeploy = false;
                }
            }
        }

        // GET: /SellerAdmin/ShakeAround/
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

            var result = this._iShakeAroundService.UnauthorizedTest();
            if (result.errcode == Senparc.Weixin.ReturnCode.api功能未授权 || (int)result.errcode == 9001020)
            {
                return Redirect("/sellerAdmin/Poi/Unauthorized");
            }
            return View();
        }

        public ActionResult AddSa()
        {
            var poi = this._iPoiService.GetPoiList();
            ViewBag.Poi = (from p in poi
                           where p.available_state == 3
                           select p).ToList();
            return View();
        }

        [HttpPost]
        public ActionResult Save(DeviceEditModel model)
        {
            if (model.id == 0)
            {
                bool result = this._iShakeAroundService.AddDevice(model.quantity, model.apply_reason, model.comment, model.poi_id);
                return Json(new
                {
                    success = result
                });
            }
            else
            {
                bool result = this._iShakeAroundService.UpdateDevice(model.device_id, model.uuid, model.major, model.minor, model.comment, model.poi_id);
                return Json(new
                {
                    success = result
                });
            }
        }

        [HttpPost]
        public ActionResult List(int page, int rows)
        {
            List<DeviceShowModel> list = new List<DeviceShowModel>();
            var data = this._iShakeAroundService.GetDeviceAll(page, rows);
            var models = data.devices;
            var pois = this._iPoiService.GetPoiList();

            foreach (var item in models)
            {
                string name = (from p in pois
                               where p.poi_id == item.poi_id
                               select p.business_name).FirstOrDefault();
                DeviceShowModel m = new DeviceShowModel(item);
                m.poi_name = name;
                list.Add(m);
            }

            DataGridModel<DeviceShowModel> dataGrid = new DataGridModel<DeviceShowModel>()
            {
                rows = list,
                total = data.total_count
            };
            return Json(dataGrid);
        }

        [HttpPost]
        public ActionResult GetPoiList()
        {
            var poi = this._iPoiService.GetPoiList();
            var result = (from p in poi
                          where p.available_state == 3
                          select p).ToList();
            return Json(result);
        }

        [HttpPost]
        public ActionResult DeviceBindLocatoin(DeviceEditModel model)
        {
            bool result = this._iShakeAroundService.DeviceBindLocatoin(model.device_id, model.uuid, model.major, model.minor, model.poi_id);
            return Json(new
            {
                success = result
            });
        }


        public ActionResult PageIndex()
        {
            if (this._settings == null)
            {
                return Redirect("/sellerAdmin/Poi/UnConfig/您还未绑定微信公众号，需要绑定微信已认证服务号才可以使用摇一摇及门店管理相关功能。");
            }
            else if (!this._isdeploy)
            {
                return Redirect("/sellerAdmin/Poi/UnConfig/access_token错误或失效，请确认AppId与AppSecret配置正确");
            }

            var result = this._iShakeAroundService.UnauthorizedTest();
            if (result.errcode == Senparc.Weixin.ReturnCode.api功能未授权 || (int)result.errcode == 9001020)
            {
                return Redirect("/sellerAdmin/Poi/Unauthorized");
            }
            return View();
        }

        public ActionResult AddPage()
        {
            return View();
        }

        public ActionResult EditPage(long id)
        {
            long[] ids = new long[] { id };
            var model = this._iShakeAroundService.GetPageById(ids)[0];
            return View(model);
        }



        [HttpPost]
        public ActionResult SavePage(PageEditModel model)
        {
            if (!(model.icon_url.IndexOf("http") >= 0))
            {
                string path = Server.MapPath("~" + model.icon_url);
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    System.Drawing.Image image = System.Drawing.Image.FromStream(fs);
                    int width = image.Width;
                    int height = image.Height;
                    if (width != height)
                    {
                        throw new HimallException("图片必须为正方形");
                    }
                }
                model.icon_url = this._iShakeAroundService.UploadImage(path);
            }



            if (model.id == 0)
            {
                bool result = this._iShakeAroundService.AddPage(model.title, model.description, model.page_url, model.icon_url, model.comment);
                return Json(new
                {
                    success = result
                });
            }
            else
            {
                bool result = this._iShakeAroundService.UpdatePage(model.page_id, model.title, model.description, model.page_url, model.icon_url, model.comment);
                return Json(new
                {
                    success = result
                });
            }
        }

        [HttpPost]
        public ActionResult PageList(int page, int rows)
        {
            var model = this._iShakeAroundService.GetPageAll(page, rows);

            for (int i = 0; i < model.pages.Count; i++)
            {
                model.pages[0].icon_url = HimallIO.GetImagePath(model.pages[0].icon_url);
            }
            DataGridModel<SearchPages_Data_Page> dataGrid = new DataGridModel<SearchPages_Data_Page>()
            {
                rows = model.pages,
                total = model.total_count
            };
            return Json(dataGrid);
        }

        public ActionResult Relationship(long id)
        {
            var devModel = this._iShakeAroundService.GetDeviceById(id);
            List<long> pageids = this._iShakeAroundService.GetPageids(devModel);


            List<SearchPages_Data_Page> pages = new List<SearchPages_Data_Page>();

            if (pageids != null && pageids.Count > 0)
            {
                pages = this._iShakeAroundService.GetPageById(pageids.ToArray());
            }


            ViewBag.DevModel = devModel;
            ViewBag.PageModel = pages;
            return View();
        }

        [HttpPost]
        public ActionResult RemoveRelationship(RemoveRelationshipModel model)
        {
            var devModel = this._iShakeAroundService.GetDeviceById(model.id);
            DeviceApply_Data_Device_Identifiers identifiers = new DeviceApply_Data_Device_Identifiers();
            identifiers.device_id = long.Parse(devModel.device_id);
            identifiers.major = long.Parse(devModel.major);
            identifiers.uuid = devModel.uuid;
            identifiers.minor = long.Parse(devModel.minor);
            bool result = this._iShakeAroundService.SetRelationship(identifiers, new long[] { model.pageid }, Senparc.Weixin.MP.ShakeAroundBindType.解除关联关系);
            return Json(new
            {
                success = result
            });
        }

        [HttpPost]
        public ActionResult BindRelationship(BindRelationshipModel model)
        {
            var devModel = this._iShakeAroundService.GetDeviceById(model.id);
            DeviceApply_Data_Device_Identifiers identifiers = new DeviceApply_Data_Device_Identifiers();
            identifiers.device_id = long.Parse(devModel.device_id);
            identifiers.major = long.Parse(devModel.major);
            identifiers.uuid = devModel.uuid;
            identifiers.minor = long.Parse(devModel.minor);
            bool result = this._iShakeAroundService.SetRelationship(identifiers, model.pageids.Split(',').Select(p => long.Parse(p)).ToArray(), Senparc.Weixin.MP.ShakeAroundBindType.建立关联关系);
            return Json(new
            {
                success = result
            });
        }

        [HttpPost]
        public ActionResult GetPagesByNotRelationship(long id, int page, int rows)
        {
            var model = this._iShakeAroundService.GetPageAll();
            var devModel = this._iShakeAroundService.GetDeviceById(id);
            List<long> pageids = this._iShakeAroundService.GetPageids(devModel);

            string[] ids = new string[0];
            if (pageids != null && pageids.Count > 0)
            {
                ids = pageids.Select(p => p.ToString()).ToArray();
            }

            var pages = (from p in model.pages
                         where !ids.Contains(p.page_id.ToString())
                         select p).ToList();
            DataGridModel<SearchPages_Data_Page> dataGrid = new DataGridModel<SearchPages_Data_Page>()
            {
                rows = pages,
                total = pages.Count
            };
            return Json(dataGrid);
        }

        [HttpPost]
        public ActionResult DeletePage(long id)
        {
            bool result = this._iShakeAroundService.DeletePage(new List<long> { id });
            return Json(new
            {
                success = result
            });
        }

    }



    public class BindRelationshipModel
    {
        public long id
        {
            get;
            set;
        }

        public string pageids
        {
            get;
            set;
        }

    }

    public class RemoveRelationshipModel
    {
        public long id
        {
            get;
            set;
        }

        public long pageid
        {
            get;
            set;
        }
    }

    public class PageEditModel
    {
        public int id
        {
            get;
            set;
        }

        public long page_id
        {
            get;
            set;
        }

        public string title
        {
            get;
            set;
        }

        public string description
        {
            get;
            set;
        }

        public string page_url
        {
            get;
            set;
        }

        public string comment
        {
            get;
            set;
        }

        public string icon_url
        {
            get;
            set;
        }

    }

    public class DeviceShowModel : DeviceModel
    {
        public DeviceShowModel(DeviceModel model)
        {
            base.device_id = model.device_id;
            base.uuid = model.uuid;
            base.minor = model.minor;
            base.major = model.major;
            base.comment = model.comment;
            base.page_ids = model.page_ids;
            base.status = model.status;
            base.poi_id = model.poi_id;
        }
        public string poi_name
        {
            get;
            set;
        }
    }

    public class DeviceEditModel
    {
        public int id
        {
            get;
            set;
        }

        public long device_id
        {
            get;
            set;
        }

        public string uuid
        {
            get;
            set;
        }

        public long major
        {
            get;
            set;
        }

        public long minor
        {
            get;
            set;
        }

        public int quantity
        {
            get;
            set;
        }

        public string apply_reason
        {
            get;
            set;
        }

        public string comment
        {
            get;
            set;
        }

        public long poi_id
        {
            get;
            set;
        }
    }
}