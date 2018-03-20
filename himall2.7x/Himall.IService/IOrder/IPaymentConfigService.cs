using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Model;

namespace Himall.IServices
{
    public interface IPaymentConfigService : IService
    {
        /// <summary>
        /// 是否开启
        /// </summary>
        bool IsEnable();

        /// <summary>
        /// 开启
        /// </summary>
        void Enable();

        /// <summary>
        /// 关闭
        /// </summary>
        void Disable();


        /// <summary>
        /// 保存商家的配置
        /// addressIds = "id,id,id,id....."
        /// </summary>
        void Save( string addressIds , string addressids_city , long shopid );

        ReceivingAddressInfo Get( long shopid );

        List<string> GetAddressIdByShop( long shopid );
        List<string> GetAddressIdCityByShop( long shopid ); 

        string GetAddressIds( long shopid );
        /// <summary>
        /// 
        /// </summary>
        /// <param name="countyId">区级ID</param>
        /// <param name="cityId">市级ID</param>
        /// <returns></returns>
        bool IsCashOnDelivery(long cityId, long countyId);

        List<PaymentType> GetPaymentTypes();
    }
}
