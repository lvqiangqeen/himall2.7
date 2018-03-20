using Himall.IServices;
using Himall.Web.Areas.SellerAdmin.Models;
using Himall.Web.Framework;
using System.Linq;
using System.Web.Mvc;
using Himall.Core;
using Himall.Model;
using System.Collections.Generic;
using Himall.Model.DTO;
using Himall.Core.Helper;
using System.IO;


namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    public class ShopHomeModuleController : BaseSellerController
    {
        private IShopHomeModuleService _iShopHomeModuleService;
        public ShopHomeModuleController( IShopHomeModuleService iShopHomeModuleService )
        {
            _iShopHomeModuleService = iShopHomeModuleService;
        }

        // GET: SellerAdmin/ShopHomeModule
        public ActionResult Management()
        {
            var shopHomeModules = _iShopHomeModuleService.GetAllShopHomeModuleInfos( CurrentSellerManager.ShopId ).ToArray();

            var models = shopHomeModules.Select( item => new ShopHomeModuleBasicModel()
            {
                Id = item.Id ,
                Name = item.Name ,
                DisplaySequence = item.DisplaySequence ,
                IsEnable = item.IsEnable
            } );
            return View( models );
        }


        [HttpPost]
        public JsonResult SaveName( long id , string name )
        {
            if( string.IsNullOrWhiteSpace( name ) )
                throw new InvalidPropertyException( "名称不能为空" );
            if( id <= 0 )
                throw new InvalidPropertyException( "商品模块id必须大于0" );

            name = name.Trim();
            _iShopHomeModuleService.UpdateShopProductModuleName( CurrentSellerManager.ShopId , id , name );
            return Json( new { success = true } );
        }


        [HttpPost]
        public JsonResult AddShopHomeModule( string name )
        {
            if( string.IsNullOrWhiteSpace( name ) )
                throw new InvalidPropertyException( "名称不能为空" );

            var shopHomeModule = new ShopHomeModuleInfo()
            {
                ShopId = CurrentSellerManager.ShopId ,
                Name = name.Trim()
            };

            _iShopHomeModuleService.AddShopProductModule( shopHomeModule );
            return Json( new { success = true , name = shopHomeModule.Name , id = shopHomeModule.Id } );
        }


        [HttpPost]
        [ShopOperationLog( Message = "删除商品模块" )]
        public JsonResult Delelte( long id )
        {
            if( id <= 0 )
                throw new InvalidPropertyException( "商品模块id必须大于0" );

            _iShopHomeModuleService.Delete( CurrentSellerManager.ShopId , id );
            return Json( new { success = true } );
        }



        [HttpPost]
        [ShopOperationLog( Message = "添加商品模块" )]
        public JsonResult SaveShopModuleProducts( long id , string productIds )
        {
            if( id <= 0 )
                throw new InvalidPropertyException( "商品模块id必须大于0" );
            IEnumerable<long> productIds_long;
            try
            {
                productIds_long = productIds.Split( ',' ).Select( item => long.Parse( item ) );
            }
            catch( System.FormatException )
            {
                throw new InvalidPropertyException( "商品编号不合法，请使用半角逗号连接各商品id" );
            }

            _iShopHomeModuleService.UpdateShopProductModuleProducts( CurrentSellerManager.ShopId , id , productIds_long );
            return Json( new { success = true } );
        }


        #region by zjt

        public ActionResult EditFooter()
        {
            string footer = _iShopHomeModuleService.GetFooter(CurrentSellerManager.ShopId );
            ViewBag.Footer = footer;
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditFooter( string footer )
        {
            _iShopHomeModuleService.EditFooter( CurrentSellerManager.ShopId , footer );
            return Json( new Result { success = true } );
        }


        public ActionResult AddFloor( long? id = 0 )
        {
            ShopHomeModuleInfo info = null;
            if( id != null && id > 0 )
            {
                info = _iShopHomeModuleService.GetShopHomeModuleInfo( CurrentSellerManager.ShopId , ( int )id );
            }
            else
            {
                info = new ShopHomeModuleInfo();
            }
            return View( info );
        }

        public ActionResult EditFloor( long? id = 0 )
        {
            ShopHomeModuleInfo info = null; 
            if( id != null && id > 0 )
            {
                info = _iShopHomeModuleService.GetShopHomeModuleInfo( CurrentSellerManager.ShopId , ( int )id );
            }
            else
            {
                info = new ShopHomeModuleInfo();
            }
            return View( info );
        }

        public ActionResult AddAcion( string args )
        {
            if( string.IsNullOrEmpty( args ) )
            {
                return Json( new Result { success = false } );
            }

            AddShopHomeModuleModel model = Newtonsoft.Json.JsonConvert.DeserializeObject<AddShopHomeModuleModel>( args );

            foreach( var img in model.TopImgs )
            {
                if( !img.ImgPath.Contains( "/temp" ) )
                {
                    continue;
                }
                string source = img.ImgPath.Substring(img.ImgPath.LastIndexOf("/temp"));
                string dest = string.Format( @"/Storage/Shop/{0}/ImageAd/" , CurrentSellerManager.ShopId );
                string fullDir = dest ;
                Core.HimallIO.CopyFile(source, fullDir + Path.GetFileName(source), true);
                img.ImgPath = Path.Combine( dest , Path.GetFileName( source ) );
            }

            model.ShopId = CurrentSellerManager.ShopId;
            _iShopHomeModuleService.SaveFloor( model );
            return Json( new Result { success = true } );
        }

        [HttpPost]
        public JsonResult FloorEnable( long id , bool enable )
        {
            _iShopHomeModuleService.Enable( id , enable );
            return Json( new
            {
                success = true
            } );
        }

        [HttpPost]
        public JsonResult FloorChangeSequence( int oriRowNumber , int newRowNumber , string direction )
        {
            _iShopHomeModuleService.UpdateFloorSequence( CurrentSellerManager.ShopId , oriRowNumber , newRowNumber , direction );
            return Json( new
            {
                success = true
            } );
        }

        [HttpPost]
        public ActionResult DelFloor( long id )
        {
            _iShopHomeModuleService.DelFloor( id );
            return Json( new
            {
                success = true
            } );
        }
        #endregion
    }
}