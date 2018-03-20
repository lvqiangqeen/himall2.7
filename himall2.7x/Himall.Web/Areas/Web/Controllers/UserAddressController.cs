using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using System.Web.Mvc;

namespace Himall.Web.Areas.Web.Controllers
{
    public class UserAddressController : BaseMemberController
    {
        private IShippingAddressService _iShippingAddressService;
        public UserAddressController(IShippingAddressService iShippingAddressService)
        {
            _iShippingAddressService = iShippingAddressService;
        }
        // GET: Web/UserAddress
        public ActionResult Index()
        {
            var userId = CurrentUser.Id;
            var m = _iShippingAddressService.GetUserShippingAddressByUserId(userId);
            return View(m);
        }

        [HttpPost]
        public JsonResult AddShippingAddress(ShippingAddressInfo info)
        {
            info.UserId = CurrentUser.Id;
            _iShippingAddressService.AddShippingAddress(info);
            return Json(new { success = true, msg = "添加成功", id = info.Id });
        }

        [HttpPost]
        public JsonResult DeleteShippingAddress(long id)
        {
            var userId = CurrentUser.Id;
            _iShippingAddressService.DeleteShippingAddress(id, userId);
            return Json(new Result() { success = true, msg = "删除成功" });
        }

        [HttpPost]
        public JsonResult EditShippingAddress(ShippingAddressInfo info)
        {
            info.UserId = CurrentUser.Id;
            _iShippingAddressService.UpdateShippingAddress(info);
            return Json(new { success = true, msg = "修改成功", id = info.Id });
        }

        [HttpPost]
        public JsonResult SetQuickShippingAddress(long id)
        {
            var userId = CurrentUser.Id;
            _iShippingAddressService.SetQuickShippingAddress(id, userId);
            return Json(new Result() { success = true, msg = "设置成功" });
        }

        [HttpPost]
        public JsonResult SetDefaultShippingAddress(long id)
        {
            var userId = CurrentUser.Id;
            _iShippingAddressService.SetDefaultShippingAddress(id, userId);
            return Json(new Result() { success = true, msg = "设置成功" });
        }

        [HttpPost]
        public JsonResult GetShippingAddress(long id)
        {
            var address = _iShippingAddressService.GetUserShippingAddress(id);
            var json = new
            {

                id = address.Id,
                fullRegionName = address.RegionFullName,
                address = address.Address,
                phone = address.Phone,
                shipTo = address.ShipTo,
                fullRegionIdPath = address.RegionIdPath

            };
            return Json(json);
        }

        [HttpGet]
        public ActionResult InitRegion(string fromLatLng)
        {
            string address = string.Empty, province = string.Empty, city = string.Empty, district = string.Empty, street = string.Empty, newStreet = string.Empty;
            Himall.Web.Common.ShopbranchHelper.GetAddressByLatLng(fromLatLng, ref address, ref province, ref city, ref district, ref street);
            if (district == "" && street != "")
            {
                district = street;
                street = "";
            }
            string fullPath = Himall.Application.RegionApplication.GetAddress_Components(city, district, street,out newStreet);
            return Json(new { fullPath = fullPath, showCity = string.Format("{0} {1} {2}", province, city, district), street = street }, JsonRequestBehavior.AllowGet);
        }
    }
}