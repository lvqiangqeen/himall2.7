using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Core;
using Himall.IServices;
using Himall.Model;

namespace Himall.Application
{

    public class PaymentConfigApplication

    {

        private static IPaymentConfigService _iPaymentConfigService = ObjectContainer.Current.Resolve<IPaymentConfigService>();

        /// <summary>
        /// 是否开启
        /// </summary>
        public static bool IsEnable()
        {
            return _iPaymentConfigService.IsEnable();
        }

        /// <summary>
        /// 开启
        /// </summary>
        public static  void Enable()
        {
          _iPaymentConfigService.Enable();
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public static  void Disable()
        {
            _iPaymentConfigService.Disable();
        }


        /// <summary>
        /// 保存商家的配置
        /// addressIds = "id,id,id,id....."
        /// </summary>
        public static  void Save(string addressIds, string addressids_city, long shopid)
        {
            _iPaymentConfigService.Save(addressIds, addressids_city, shopid);
        }

        public static  ReceivingAddressInfo Get(long shopid)
        {
           return  _iPaymentConfigService.Get(shopid);
        }

        public static  List<string> GetAddressIdByShop(long shopid)
        {
           return  _iPaymentConfigService.GetAddressIdByShop(shopid);
        }
        public static  List<string> GetAddressIdCityByShop(long shopid)
        {
            return _iPaymentConfigService.GetAddressIdCityByShop(shopid);
        }

        public static  string GetAddressIds(long shopid)
        {
            return _iPaymentConfigService.GetAddressIds(shopid);
        }

        public static  bool IsCashOnDelivery(long regionId)
        {
            var city = RegionApplication.GetRegion(regionId, CommonModel.Region.RegionLevel.City);
            var county = RegionApplication.GetRegion(regionId, CommonModel.Region.RegionLevel.County);
            var cityId = city == null ? 0 : city.Id;
            var countyId = county == null ? 0 : county.Id;
            return _iPaymentConfigService.IsCashOnDelivery(cityId,countyId);
        }

        public static  List<PaymentType> GetPaymentTypes()
        {
            return _iPaymentConfigService.GetPaymentTypes();
        }
    }
}
