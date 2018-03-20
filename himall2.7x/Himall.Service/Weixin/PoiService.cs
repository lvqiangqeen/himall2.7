using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.IServices;
using Senparc.Weixin.MP.CommonAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.Poi;
using Himall.Core;
using Senparc.Weixin.Exceptions;
using Himall.Model.Models;

namespace Himall.Service
{
    public class PoiService : ServiceBase , IPoiService
    {
        private string _accessToken = string.Empty;

        public void init( string appid , string secret )
        {
            this._accessToken = AccessTokenContainer.TryGetToken( appid , secret );
        }

        public bool AddPoi( CreateStoreData createStoreData )
        {
            var result = PoiApi.AddPoi( this._accessToken , createStoreData );
            if( result.errcode != 0 )
            {
                throw new Exception( result.errmsg );
            }
            return true;
        }

        public bool UpdatePoi( UpdateStoreData updateStoreData )
        {
            var result = PoiApi.UpdatePoi( this._accessToken , updateStoreData );
            if( result.errcode != 0 )
            {
                throw new Exception( result.errmsg );
            }
            else if( ( int )result.errcode == 65107 )
            {
                throw new HimallException( "暂时不允许修改" );
            }
            return true;
        }

        public bool DeletePoi( string poiId )
        {

            var result = PoiApi.DeletePoi( this._accessToken , poiId );
            if( ( int )result.errcode == 65107 || result.errcode == Senparc.Weixin.ReturnCode.系统繁忙此时请开发者稍候再试 )
            {
                throw new HimallException( "系统繁忙，请稍后尝试！" );
            }
            else if( result.errcode == 0 )
            {
                return true;
            }
            else
            {
                throw new Exception( result.errmsg );
            }


        }

        public GetStoreListResultJson GetPoiList( int page , int rows )
        {
            int begin = 0;
            if( page > 1 )
            {
                begin = ( page - 1 ) * rows;
            }
            
            var result = PoiApi.GetPoiList( this._accessToken , begin , rows );
            if( result.errcode != 0 && result.errcode != Senparc.Weixin.ReturnCode.api功能未授权)
            {
                throw new Exception( result.errmsg );
            }
            return result;
        }

        public List<GetStoreList_BaseInfo> GetPoiList()
        {

            var result = PoiApi.GetPoiList( this._accessToken , 0 , 50 );
            if( result.errcode != 0 )
            {
                throw new Exception( result.errmsg );
            }
            var obj = ( from l in result.business_list
                        select l.base_info ).ToList();
            return obj;
        }

        public GetStoreBaseInfo GetPoi( string poiId )
        {
            var result = PoiApi.GetPoi( this._accessToken , poiId );
            if( result.errcode != 0 )
            {
                throw new Exception( result.errmsg );
            }

            return result.business.base_info;
        }

        public List<WXCategory> GetCategory()
        {
            var result = PoiApi.GetCategory( this._accessToken );
            if( result.errcode != 0 )
            {
                Log.Debug("[WXPoi]" + result.errcode +":"+ result.errmsg);
                throw new Exception( result.errmsg );
            }

            List<WXCategory> c = new List<WXCategory>();
            foreach( string str in result.category_list )
            {
                string[] cateArr = str.Split( ',' );
                if( cateArr.Count() == 1 )
                {
                    c.Add( new WXCategory
                    {
                        Id = Guid.NewGuid().ToString() ,
                        Name = cateArr[ 0 ]
                    } );
                }
                else if( c.Exists( p => p.Name == cateArr[0] ) )
                {
                    var parent = c.FirstOrDefault( p => p.Name == cateArr[ 0 ] );
                    if( !parent.Child.Exists( p => p.Name == cateArr[ 1 ] ) )
                    {
                        WXCategory child = new WXCategory();
                        child.Id = parent.Id;
                        child.Name = cateArr[ 1 ];
                        parent.Child.Add( child );
                    }
                   
                }
                else
                {
                    WXCategory item = new WXCategory();
                    item.Id = Guid.NewGuid().ToString();
                    item.Name = cateArr[ 0 ];

                    WXCategory child = new WXCategory();
                    child.Id = item.Id;
                    child.Name = cateArr[ 1 ];
                    item.Child.Add( child );
                    c.Add( item );
                }

            }
            return c;
        }

        public new void Dispose()
        {
            
        }


        public string UploadImage( string filePath )
        {
            var result = PoiApi.UploadImage( this._accessToken , filePath );
            if( result.errcode != 0 )
            {
                throw new Exception( result.errmsg );
            }
            return result.url;
        }
    }
}
