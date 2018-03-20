using Himall.Application;
using Himall.CommonModel;
using Himall.Core;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.SmallProgAPI.Helper;
using Himall.SmallProgAPI.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Himall.SmallProgAPI
{
    /// <summary>
    /// 收货地址
    /// </summary>
    public class ShippingAddressController : BaseApiController
    {
        #region 获取
        /// <summary>
        /// 获取收货地址列表
        /// </summary>
        /// <param name="openId"></param>
        /// <returns></returns>
        public object GetList(string openId)
        {
            CheckUserLogin();
            var shoppingAddress = ServiceProvider.Instance<IShippingAddressService>.Create.GetUserShippingAddressByUserId(CurrentUser.Id);

            var result = shoppingAddress.ToList().Select(item => new
            {
                ShippingId = item.Id,
                ShipTo = item.ShipTo,
                CellPhone = item.Phone,
                FullRegionName = item.RegionFullName,
                Address = item.Address,
                RegionId = item.RegionId,
                FullRegionPath = item.RegionIdPath,
                IsDefault = item.IsDefault,
                LatLng = item.Latitude + "," + item.Longitude,
                FullAddress = item.RegionFullName + " " + item.Address,
                TelPhone = "",
                RegionLocation = ""
            }).ToList();
            return Json(new
            {
                Status = "OK",
                Data = result
            });

        }
        /// <summary>
        /// 获取收货地址
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public object GetShippingAddress(long shippingId)
        {
            CheckUserLogin();
            var shoppingAddress = ServiceProvider.Instance<IShippingAddressService>.Create.GetUserShippingAddressByUserId(CurrentUser.Id);
            var shopaddressInfo = shoppingAddress.FirstOrDefault(e => e.Id == shippingId);
            if (shopaddressInfo != null)
            {
                dynamic model = new ExpandoObject();

                model.ShippingId = shopaddressInfo.Id;
                model.ShipTo = shopaddressInfo.ShipTo;
                model.CellPhone = shopaddressInfo.Phone;
                model.FullRegionName = shopaddressInfo.RegionFullName;
                model.Address = shopaddressInfo.Address;
                model.RegionId = shopaddressInfo.RegionId;
                model.FullRegionPath = shopaddressInfo.RegionIdPath;
                model.IsDefault = shopaddressInfo.IsDefault;
                model.LatLng = shopaddressInfo.Latitude + "," + shopaddressInfo.Longitude;
                model.FullAddress = shopaddressInfo.RegionFullName + " " + shopaddressInfo.Address;
                model.TelPhone = "";
                model.RegionLocation = "";
                model.UserId = CurrentUserId;
                model.Zipcode = "";
                return Json(new { Status = "OK", Data = new { ShippingAddressInfo = model } });
            }
            else
            {
                return Json(new { Status = "NO", Message = "参数错误" });
            }

        }
        #endregion

        /// <summary>
        /// 新增收货地址
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public object PostAddAddress(ShippingAddressOperaAddressPModel value)
        {
            CheckUserLogin();
            ShippingAddressInfo shippingAddr = new ShippingAddressInfo();
            shippingAddr.UserId = CurrentUser.Id;
            shippingAddr.RegionId = value.regionId;
            shippingAddr.Address = value.address;
            shippingAddr.Phone = value.cellphone;
            shippingAddr.ShipTo = value.shipTo;
            try
            {
                ServiceProvider.Instance<IShippingAddressService>.Create.AddShippingAddress(shippingAddr);
                if (value.isDefault)
                {
                    ServiceProvider.Instance<IShippingAddressService>.Create.SetDefaultShippingAddress(shippingAddr.Id, CurrentUserId);
                }
            }
            catch (Exception ex)
            {
                return Json(new { Status = "NO", Message = ex.Message });
            }
            return Json(new { Status = "OK", Message = shippingAddr.Id });
        }
        /// <summary>
        /// 新增收货地址(微信地址)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public object PostAddWXAddress(ShippingAddressOperaAddressPModel value)
        {
            CheckUserLogin();
            try
            {
                if (string.IsNullOrWhiteSpace(value.address))
                {
                    throw new HimallException("请填写详细地址");
                }
                if (value.regionId <= 0 && (string.IsNullOrWhiteSpace(value.city) || string.IsNullOrWhiteSpace(value.county)))
                {
                    throw new HimallException("参数错误");
                }
                if (value.regionId <= 0)
                {
                    var _region = ServiceProvider.Instance<IRegionService>.Create.GetRegionByName(value.county, Region.RegionLevel.County);
                    if (_region != null)
                    {
                        value.regionId = _region.Id;
                    }
                }
                if (value.regionId <= 0)
                {
                    throw new HimallException("错误的地区信息");
                }
            }
            catch (Exception ex)
            {
                return Json(new { Status = "NO", Message = ex.Message });
            }

            #region 如果存在相同地址就返回
            var shoppingAddress = ServiceProvider.Instance<IShippingAddressService>.Create.GetUserShippingAddressByUserId(CurrentUser.Id);
            var _tmp = shoppingAddress.FirstOrDefault(d => d.RegionId == value.regionId && d.Address == value.address);
            if (_tmp != null)
            {
                return Json(new { Status = "OK", Message = _tmp.Id });
            }
            #endregion

            ShippingAddressInfo shippingAddr = new ShippingAddressInfo();
            shippingAddr.UserId = CurrentUser.Id;
            shippingAddr.RegionId = value.regionId;
            shippingAddr.Address = value.address;
            shippingAddr.Phone = value.cellphone;
            shippingAddr.ShipTo = value.shipTo;
            try
            {
                ServiceProvider.Instance<IShippingAddressService>.Create.AddShippingAddress(shippingAddr);
                if (value.isDefault)
                {
                    ServiceProvider.Instance<IShippingAddressService>.Create.SetDefaultShippingAddress(shippingAddr.Id, CurrentUserId);
                }
            }
            catch (Exception ex)
            {
                return Json(new { Status = "NO", Message = ex.Message });
            }
            return Json(new { Status = "OK", Message = shippingAddr.Id });
        }
        /// <summary>
        /// 修改收货地址
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public object PostUpdateAddress(ShippingAddressOperaAddressPModel value)
        {
            CheckUserLogin();
            CheckUserLogin();
            ShippingAddressInfo shippingAddr = new ShippingAddressInfo();
            shippingAddr.UserId = CurrentUser.Id;
            shippingAddr.Id = value.shippingId;
            shippingAddr.RegionId = value.regionId;
            shippingAddr.Address = value.address;
            shippingAddr.Phone = value.cellphone;
            shippingAddr.ShipTo = value.shipTo;
            try
            {
                ServiceProvider.Instance<IShippingAddressService>.Create.UpdateShippingAddress(shippingAddr);
                if (value.isDefault)
                {
                    ServiceProvider.Instance<IShippingAddressService>.Create.SetDefaultShippingAddress(shippingAddr.Id, CurrentUserId);
                }
            }
            catch (Exception ex)
            {
                return Json(new { Status = "NO", Message = ex.Message });
            }
            return Json(new { Status = "OK", Message = shippingAddr.Id });
        }
        /// <summary>
        /// 设置默认地址
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public object GetSetDefault(long shippingId)
        {
            CheckUserLogin();
            ServiceProvider.Instance<IShippingAddressService>.Create.SetDefaultShippingAddress(shippingId, CurrentUserId);
            return Json(new { Status = "OK", Message = "设置成功" });
        }
        /// <summary>
        /// 删除收货地址
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public object GetDeleteAddress(long shippingId)
        {
            CheckUserLogin();
            ServiceProvider.Instance<IShippingAddressService>.Create.DeleteShippingAddress(shippingId, CurrentUser.Id);
            return Json(new { Status = "OK", Message = "删除成功" });
        }
    }
}
