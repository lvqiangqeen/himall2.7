using Himall.Core;
using Himall.IServices;
using Himall.Model;
using System.Linq;
using Himall.Entity;

namespace Himall.Service
{
    public class ShippingAddressService : ServiceBase, IShippingAddressService
    {
        public void AddShippingAddress(ShippingAddressInfo shipinfo)
        {
            shipinfo.IsDefault = false;
            shipinfo.IsQuick = false;
            if (Context.ShippingAddressInfo.Where(a => a.UserId == shipinfo.UserId).Count() >= 20)
            {
                throw new HimallException("收货地址最多只能创建20个！");
            }
            Context.ShippingAddressInfo.Add(shipinfo);
            Context.SaveChanges();
            //SetDefaultShippingAddress(shipinfo.Id, shipinfo.UserId);
        }

        public void SetDefaultShippingAddress(long id, long userId)
        {
            var model = Context.ShippingAddressInfo.Where(a => a.UserId == userId);
            foreach (var m in model.ToList())
            {
                if (m.Id == id)
                {
                    m.IsDefault = true;
                }
                else
                {
                    m.IsDefault = false;
                }
            }
            Context.SaveChanges();
        }


        public void SetQuickShippingAddress(long id, long userId)
        {
            var model = Context.ShippingAddressInfo.Where(a => a.UserId == userId);
            foreach (var m in model)
            {
                if (m.Id == id)
                {
                    m.IsQuick = true;
                }
                else
                {
                    m.IsQuick = false;
                }
            }
            Context.SaveChanges();
        }


        public void UpdateShippingAddress(ShippingAddressInfo shipinfo)
        {
            var model = Context.ShippingAddressInfo.Where(a => a.Id == shipinfo.Id && a.UserId == shipinfo.UserId).FirstOrDefault();
            if (model == null)
            {
                throw new Himall.Core.HimallException("该收货地址不存在或已被删除！");
            }
            model.Phone = shipinfo.Phone;
            model.RegionId = shipinfo.RegionId;
            model.ShipTo = shipinfo.ShipTo;
            model.Address = shipinfo.Address;
            model.Latitude = shipinfo.Latitude;
            model.Longitude = shipinfo.Longitude;
            Context.SaveChanges();
            Cache.Remove(CacheKeyCollection.CACHE_SHIPADDRESS(shipinfo.Id));
        }

        public void DeleteShippingAddress(long id, long userId)
        {
            var model = Context.ShippingAddressInfo.Where(a => a.Id == id && a.UserId == userId).FirstOrDefault();
            if (model == null)
            {
                throw new Himall.Core.HimallException("该收货地址不存在或已被删除！");
            }
            bool isDefault = model.IsDefault;
            Context.ShippingAddressInfo.Remove(model);

            if( isDefault )
            {
                var newModel = Context.ShippingAddressInfo.FirstOrDefault();
                if( newModel != null )
                {
                    newModel.IsDefault = true;
                }
            }
            Context.SaveChanges();
            
        }

        public IQueryable<ShippingAddressInfo> GetUserShippingAddressByUserId(long userId)
        {
            var regionService = ServiceProvider.Instance<IRegionService>.Create;
            var model = Context.ShippingAddressInfo.Where(a => a.UserId == userId).OrderByDescending(a => a.Id);
            foreach (var m in model)
            {
                m.RegionFullName = regionService.GetFullName(m.RegionId);
                m.RegionIdPath = regionService.GetRegionPath(m.RegionId);
            }
            return model;
        }


        public ShippingAddressInfo GetUserShippingAddress(long shippingAddressId)
        {
            string cacheKey = CacheKeyCollection.CACHE_SHIPADDRESS(shippingAddressId);
            if (Cache.Exists(cacheKey))
                return Cache.Get<ShippingAddressInfo>(cacheKey);
            var regionService = ServiceProvider.Instance<IRegionService>.Create;
            var address = Context.ShippingAddressInfo.Find(shippingAddressId);
            if (address==null)
            {
                throw new HimallException("错误的收货地址！");
            }
            address.RegionFullName = regionService.GetFullName(address.RegionId);
            address.RegionIdPath = regionService.GetRegionPath(address.RegionId);
            Cache.Insert<ShippingAddressInfo>(cacheKey, address, 1800);
            return address;
        }


        public ShippingAddressInfo GetDefaultUserShippingAddressByUserId(long userId)
        {
            //优先选择默认地址
            ShippingAddressInfo defaultShippingAddressInfo = Context.ShippingAddressInfo.FirstOrDefault(item => item.UserId == userId && item.IsDefault);
            
            //默认地址不存在时，选择最后一个添加的地址
            if (defaultShippingAddressInfo == null)
                defaultShippingAddressInfo = Context.ShippingAddressInfo.Where(item => item.UserId == userId).OrderByDescending(item => item.Id).FirstOrDefault();

            if (defaultShippingAddressInfo != null) {
                var regionService = ServiceProvider.Instance<IRegionService>.Create;
               // defaultShippingAddressInfo.RegionFullName = regionService.GetRegionFullName(defaultShippingAddressInfo.RegionId);
               // defaultShippingAddressInfo.RegionIdPath = regionService.GetRegionIdPath(defaultShippingAddressInfo.RegionId);
                defaultShippingAddressInfo.RegionFullName = regionService.GetFullName(defaultShippingAddressInfo.RegionId);
                defaultShippingAddressInfo.RegionIdPath = regionService.GetRegionPath(defaultShippingAddressInfo.RegionId);
            }
            return defaultShippingAddressInfo;
        }
    }
}
