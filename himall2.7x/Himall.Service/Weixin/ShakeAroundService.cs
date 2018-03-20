using Himall.Core;
using Himall.IServices;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.AdvancedAPIs.Poi;
using Senparc.Weixin.MP.AdvancedAPIs.ShakeAround;
using Senparc.Weixin.MP.CommonAPIs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Service
{
    public class ShakeAroundService : ServiceBase , IShakeAroundService
    {
        private string _accessToken = string.Empty;

        public void init( string appid , string secret )
        {
            this._accessToken = AccessTokenContainer.TryGetToken( appid , secret );
        }

        public bool AddDevice( int quantity , string applyReason , string comment = null , long? poiId = null )
        {
            var result = ShakeAroundApi.DeviceApply( this._accessToken , quantity , applyReason , comment , poiId );
            if( result.errcode != 0 )
            {
                throw new Exception( result.errmsg );
            }
            return true;
        }

        public bool UpdateDevice( long deviceId , string uuId , long major , long minor , string comment , long poiId = 0 )
        {
            var result = ShakeAroundApi.DeviceUpdate( this._accessToken , deviceId , uuId , major , minor , comment , poiId );
            if( result.errcode != 0 )
            {
                throw new Exception( result.errmsg );
            }
            return true;
        }

        public List<DeviceModel> GetDeviceAll()
        {
            var result = ShakeAroundApi.SearchDeviceByRange( this._accessToken , 0 , 49 );
            if( result.errcode != 0 )
            {
                throw new Exception( result.errmsg );
            }
            return result.data.devices;
        }

        public DeviceList2ResultJson GetDeviceAll( int page , int rows )
        {
            int begin = 0;
            if( page > 1 )
            {
                begin = ( page - 1 ) * rows;
            }

            var result = ShakeAroundApi.SearchDeviceByRange( this._accessToken , begin , rows );
            if( result.errcode != 0 )
            {
                throw new Exception( result.errmsg );
            }
            return result.data;
        }

        public DeviceListResultJson UnauthorizedTest() 
        {
            var result = ShakeAroundApi.SearchDeviceByRange( this._accessToken , 0 , 1 );
            if( result.errcode != 0 && result.errcode != Senparc.Weixin.ReturnCode.api功能未授权 && ( int )result.errcode != 9001020 )
            {
                throw new HimallException( result.errmsg );
            }
            return result;
        }

        public List<long> GetPageids( DeviceModel model )
        {
            DeviceApply_Data_Device_Identifiers d = new DeviceApply_Data_Device_Identifiers();
            d.device_id = long.Parse( model.device_id );
            d.major = long.Parse( model.major );
            d.minor = long.Parse( model.minor );
            d.uuid = model.uuid;

            var result = ShakeAroundApi.SearchPagesByDeviceId( this._accessToken , 1 , d );
            return result.data.relations.Select( p => p.page_id ).ToList();
        }

        public DeviceModel GetDeviceById( long id )
        {
            DeviceApply_Data_Device_Identifiers d = new DeviceApply_Data_Device_Identifiers();
            d.device_id = id;
            var result = ShakeAroundApi.SearchDeviceById( this._accessToken , new List<DeviceApply_Data_Device_Identifiers>
            {
                d
            } );
            if( result.errcode != 0 )
            {
                throw new Exception( result.errmsg );
            }
            if( result.data.devices.Count > 0 )
            {
                return result.data.devices[ 0 ];
            }
            return null;
        }

        public bool DeviceBindLocatoin( long deviceId , string uuId , long major , long minor , long poiId )
        {
            var result = ShakeAroundApi.DeviceBindLocatoin( this._accessToken , deviceId , uuId , major , minor , poiId );
            if( result.errcode != 0 )
            {
                throw new Exception( result.errmsg );
            }
            return true;
        }

        public string UploadImage( string file )
        {
            var result = ShakeAroundApi.UploadImage( this._accessToken , file );
            if( result.errcode != 0 )
            {
                throw new Exception( result.errmsg );
            }
            return result.data.pic_url;
        }

        public bool AddPage( string title , string description , string pageUrl , string iconUrl , string comment = null )
        {
            var result = ShakeAroundApi.AddPage( this._accessToken , title , description , pageUrl , iconUrl , comment );
            if( result.errcode != 0 )
            {
                throw new Exception( result.errmsg );
            }
            return true;
        }

        public bool UpdatePage( long pageId , string title , string description , string pageUrl , string iconUrl , string comment = null )
        {
            var result = ShakeAroundApi.UpdatePage( this._accessToken , pageId , title , description , pageUrl , iconUrl , comment );
            if( result.errcode != 0 )
            {
                throw new Exception( result.errmsg );
            }
            return true;
        }

        public bool DeletePage( List<long> ids )
        {
            var result = ShakeAroundApi.DeletePage( this._accessToken , ids.ToArray() );
            if( result.errcode != 0 )
            {
                if( result.errmsg.Equals( "the page has been binded to device ID" ) )
                {
                    throw new HimallException( "页面已配置到设备" );
                }
                throw new Exception( result.errmsg );
            }
            return true;
        }

        public SearchPages_Data GetPageAll()
        {
            var result = ShakeAroundApi.SearchPagesByRange( this._accessToken , 0 , 49 );
            if( result.errcode != 0 )
            {
                throw new Exception( result.errmsg );
            }
            return result.data;
        }

        public SearchPages_Data GetPageAll( int page , int rows )
        {
            int begin = 0;
            if( page > 1 )
            {
                begin = ( page - 1 ) * rows;
            }

            var result = ShakeAroundApi.SearchPagesByRange( this._accessToken , begin , rows );
            if( result.errcode != 0 )
            {
                throw new Exception( result.errmsg );
            }
            return result.data;
        }

        public List<SearchPages_Data_Page> GetPageById( long[] ids )
        {
            var result = ShakeAroundApi.SearchPagesByPageId( this._accessToken , ids );
            if( result.errcode != 0 )
            {
                throw new Exception( result.errmsg );
            }
            return result.data.pages;
        }

        public bool SetRelationship( DeviceApply_Data_Device_Identifiers deviceIdentifier , long[] pageIds , ShakeAroundBindType type )
        {
            var result = ShakeAroundApi.BindPage( this._accessToken , deviceIdentifier , pageIds , type , ShakeAroundAppendType.新增 );
            if( result.errcode != 0 )
            {
                throw new Exception( result.errmsg );
            }
            return true;
        }


    }
}
