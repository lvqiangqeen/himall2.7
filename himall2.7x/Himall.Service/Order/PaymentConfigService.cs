using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Core.Plugins.Payment;
using Himall.IServices;
using Himall.Model;

namespace Himall.Service
{
    public class PaymentConfigService : ServiceBase , IPaymentConfigService
    {

        public bool IsEnable()
        {
            var model = Context.PaymentConfigInfo.FirstOrDefault();
            return model.IsCashOnDelivery;
        }

        public void Enable()
        {
            var model = Context.PaymentConfigInfo.FirstOrDefault();
            model.IsCashOnDelivery = true;
            Context.SaveChanges();
        }

        public void Disable()
        {
            var model = Context.PaymentConfigInfo.FirstOrDefault();
            model.IsCashOnDelivery = false;
            Context.SaveChanges();
        }


        public List<Model.PaymentType> GetPaymentTypes()
        {
            var payPlugins = Core.PluginsManagement.GetPlugins<IPaymentPlugin>().Select( t => t.PluginInfo );
            List<PaymentType> result = new List<PaymentType>();
            foreach( var p in payPlugins )
            {
                result.Add( new PaymentType( p.PluginId , p.DisplayName ) );
            }
            return result;
        }


        public void Save( string addressIds , string addressids_city , long shopid )
        {
            var model = Context.ReceivingAddressInfo.FirstOrDefault( p => p.ShopId == shopid );
            if( model != null )
            {
                model.AddressId = addressIds;
                model.AddressId_City = addressids_city;
            }
            else
            {
                model = new ReceivingAddressInfo();
                model.ShopId = shopid;
                model.AddressId = addressIds;
                model.AddressId_City = addressids_city;
                Context.ReceivingAddressInfo.Add( model );
            }
            Context.SaveChanges();
        }

        public List<string> GetAddressIdByShop( long shopid )
        {
            var model = Context.ReceivingAddressInfo.Where( p => p.ShopId == shopid ).FirstOrDefault();
            if( model != null )
            {
                if( !string.IsNullOrEmpty( model.AddressId ) )
                {
                    return model.AddressId.Split( ',' ).ToList();
                }
            }
            return new List<string>();
        }

        public ReceivingAddressInfo Get( long shopid )
        {
            return Context.ReceivingAddressInfo.FirstOrDefault( p => p.ShopId == shopid );
        }

        public List<string> GetAddressIdCityByShop( long shopid )
        {
            var model = Context.ReceivingAddressInfo.Where( p => p.ShopId == shopid ).FirstOrDefault();
            if( model != null )
            {
                if( !string.IsNullOrEmpty( model.AddressId_City ) )
                {
                    return model.AddressId_City.Split( ',' ).ToList();
                }
            }
            return new List<string>();
        }

        public bool IsCashOnDelivery(long cityId, long countyId)
        {
            if( Context.PaymentConfigInfo.FirstOrDefault().IsCashOnDelivery )
            {
                return Context.ReceivingAddressInfo.Any(p => p.AddressId.Contains("'" + countyId.ToString() + "'") || p.AddressId_City.Contains("'" + cityId.ToString() + "'"));
            }
            return false;
        }

        public string GetAddressIds( long shopid )
        { 
             var model = Context.ReceivingAddressInfo.Where( p => p.ShopId == shopid ).FirstOrDefault();
             if( model != null )
             {
                 return model.AddressId;
             }
             return "";
        }
    }
}
