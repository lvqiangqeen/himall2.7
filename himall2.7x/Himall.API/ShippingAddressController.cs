using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Himall.API.Model.ParamsModel;
using Himall.API.Helper;
using Himall.Application;

namespace Himall.API
{
    public class ShippingAddressController : BaseApiController
    {
        public object GetShippingAddressList()
        {
            CheckUserLogin();
            var shoppingAddress = ServiceProvider.Instance<IShippingAddressService>.Create.GetUserShippingAddressByUserId(CurrentUser.Id);

            var shippingAddressList = new List<ShippingAddressInfo>();
            foreach (var item in shoppingAddress)
            {
                ShippingAddressInfo shippingAddress = new ShippingAddressInfo()
                {
                    Id = item.Id,
                    ShipTo = item.ShipTo,
                    Phone = item.Phone,
                    RegionFullName = item.RegionFullName,
                    Address = item.Address,
                    RegionId = item.RegionId,
                    RegionIdPath = item.RegionIdPath,
                    IsDefault = item.IsDefault,
                    Latitude = item.Latitude,
                    Longitude = item.Longitude
                };
                shippingAddressList.Add(shippingAddress);
            }
            return Json(new { Success = "true", ShippingAddress=shippingAddressList });
        }
        public object GetShippingAddress(long id)
        {
            CheckUserLogin();
            var shoppingAddress = ServiceProvider.Instance<IShippingAddressService>.Create.GetUserShippingAddressByUserId(CurrentUser.Id);
            var shopaddressInfo = shoppingAddress.FirstOrDefault(e => e.Id == id);
            if (shopaddressInfo != null)
            {
                var model = new ShippingAddressInfo()
                {
                    Id = shopaddressInfo.Id,
                    ShipTo = shopaddressInfo.ShipTo,
                    Phone = shopaddressInfo.Phone,
                    RegionFullName = shopaddressInfo.RegionFullName,
                    Address = shopaddressInfo.Address,
                    RegionId = shopaddressInfo.RegionId,
                    RegionIdPath = shopaddressInfo.RegionIdPath,
                    Latitude=shopaddressInfo.Latitude,
                    Longitude=shopaddressInfo.Longitude
                };
                return Json(new { Success = "true", ShippingAddress = model });
            }
            else
            {
                return Json(new { Success = "true", ShippingAddress = new ShippingAddressInfo() });
            }
            
        }
        //新增收货地址
        public object PostAddShippingAddress(ShippingAddressAddModel value)
        {
            CheckUserLogin();
            ShippingAddressInfo shippingAddr = new ShippingAddressInfo();
            shippingAddr.UserId = CurrentUser.Id;
            shippingAddr.RegionId = value.regionId;
            shippingAddr.Address = value.address;
            shippingAddr.Phone=value.phone;
            shippingAddr.ShipTo = value.shipTo;
            shippingAddr.Latitude = value.latitude;
            shippingAddr.Longitude = value.longitude;
            try
            {
                ServiceProvider.Instance<IShippingAddressService>.Create.AddShippingAddress(shippingAddr);
            }
            catch (Exception ex)
            {
                return Json(new { Success = "false", Msg = ex.Message });
            }
            return Json(new { Success = "true"});
        }
        //删除收货地址
        public object PostDeleteShippingAddress(ShippingAddressDeleteModel value)
        {
            CheckUserLogin();
            ServiceProvider.Instance<IShippingAddressService>.Create.DeleteShippingAddress(value.id, CurrentUser.Id);
            return Json(new { Success = "true" });
        }
        //编辑收货地址
        public object PostEditShippingAddress(ShippingAddressEditModel value)
        {
            CheckUserLogin();
            ShippingAddressInfo shippingAddr = new ShippingAddressInfo();
            shippingAddr.UserId = CurrentUser.Id;
            shippingAddr.Id = value.id;
            shippingAddr.RegionId = value.regionId;
            shippingAddr.Address = value.address;
            shippingAddr.Phone = value.phone;
            shippingAddr.ShipTo = value.shipTo;
            shippingAddr.Longitude = value.longitude;
            shippingAddr.Latitude = value.latitude;
            ServiceProvider.Instance<IShippingAddressService>.Create.UpdateShippingAddress(shippingAddr);
            return Json(new { Success = "true" });
        }
        //设为默认收货地址
        public object PostSetDefaultAddress(ShippingAddressSetDefaultModel value)
        {
            CheckUserLogin();
            long addId = value.addId;
            ServiceProvider.Instance<IShippingAddressService>.Create.SetDefaultShippingAddress(addId, CurrentUser.Id);
            return Json(new { Success = "true"});
        }

        /// <summary>
        /// 根据搜索地址反向匹配出区域信息
        /// </summary>
        /// <param name="fromLatLng"></param>
        /// <returns></returns>
        public object GetRegion(string fromLatLng= "")
        {
            string address = string.Empty, province = string.Empty, city = string.Empty, district = string.Empty, street = string.Empty, fullPath = string.Empty, newStreet=string.Empty;
            ShopbranchHelper.GetAddressByLatLng(fromLatLng, ref address, ref province, ref city, ref district, ref street);
            if (district == "" && street != "")
            {
                district = street;
                street = "";
            }
            fullPath = RegionApplication.GetAddress_Components(city, district, street,out newStreet);
            if (fullPath.Split(',').Length <= 3) newStreet = string.Empty;//如果无法匹配街道，则置为空
            return Json(new { fullPath = fullPath, showCity = string.Format("{0} {1} {2}", province, city, district), street = newStreet });
        }
    }
}
