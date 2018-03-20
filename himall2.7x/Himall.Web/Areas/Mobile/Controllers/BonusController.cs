using System;
using System.Web;
using System.Web.Mvc;
using Himall.Core;
using Himall.IServices;
using Himall.Model;
using Himall.Web.App_Code.Common;
using Himall.Web.Areas.Admin.Models.Market;
using Himall.Web.Framework;
using Himall.Web.Models;
using Senparc.Weixin.Exceptions;
using Senparc.Weixin.MP.AdvancedAPIs.OAuth;
using Senparc.Weixin.MP.CommonAPIs;
using Senparc.Weixin.MP.Helpers;

namespace Himall.Web.Areas.Mobile.Controllers
{
    public class BonusController : BaseMobileTemplatesController
    {
        private IBonusService _iBonusService;
        private SiteSettingsInfo _siteSetting;
        private ISiteSettingService _iSiteSettingService;
        public BonusController(IBonusService iBonusService, ISiteSettingService iSiteSettingService)
        {
            _iBonusService = iBonusService;
            _iSiteSettingService = iSiteSettingService;
            this._siteSetting = _iSiteSettingService.GetSiteSettings();
            if( string.IsNullOrWhiteSpace( this._siteSetting.WeixinAppId ) || string.IsNullOrWhiteSpace( this._siteSetting.WeixinAppSecret ) )
            {
                throw new HimallException( "未配置公众号参数" );
            }
        }


        /// <param name="id">红包id</param>
        public ActionResult Index( long id )
        {
            if( this.PlatformType != Core.PlatformType.WeiXin )
            {
                return Content( "只能在微信端访问" );
            }

            var bonus = this._iBonusService.Get( id );
            if( bonus == null )
            {
                return Redirect( "/m-weixin/Bonus/Invalidtwo" );
            }

            BonusModel model = new BonusModel( bonus );

            string code = HttpContext.Request[ "code" ];
            OAuthAccessTokenResult wxOpenInfo = null;

            if( string.IsNullOrEmpty( code ) )
            {
                string selfAddress = Request.Url.AbsoluteUri;
                string url = OAuthApi.GetAuthorizeUrl( this._siteSetting.WeixinAppId.Trim() , selfAddress , "123321#wechat_redirect" , Senparc.Weixin.MP.OAuthScope.snsapi_base , "code" );
                return Redirect( url );
            }
            else
            {
                try
                {
                    wxOpenInfo = OAuthApi.GetAccessToken( this._siteSetting.WeixinAppId.Trim() , this._siteSetting.WeixinAppSecret.Trim() , code , "authorization_code" );
                }
                catch( ErrorJsonResultException wxe )
                {
                    if( wxe.JsonResult.errcode == Senparc.Weixin.ReturnCode.不合法的oauth_code )
                    {
                        return Redirect( "/m-weixin/Bonus/Index/" + id );
                    }
                    return Content( wxe.JsonResult.errmsg );
                }
                catch( Exception e )
                {
                    Exception innerEx = e.InnerException == null ? e : e.InnerException;
                    return Content( innerEx.Message );
                }
            }

            if( model.Type == BonusInfo.BonusType.Attention )
            {
                throw new Exception( "红包异常" ); //这里只能领取活动红包，不能领取关注红包
            }

            if( model.EndTime <= DateTime.Now || model.IsInvalid )
            {
                return Redirect( "/m-weixin/Bonus/Invalid/" + model.Id );
            }
            else if( model.StartTime > DateTime.Now )
            {
                return Redirect( "/m-weixin/Bonus/NotStart/" + model.Id + "?openId=" + wxOpenInfo.openid );
            }

            ViewBag.OpenId = wxOpenInfo.openid;
            model.ImagePath = HimallIO.GetFilePath(model.ImagePath);
            return View( model );
        }

        //完成
        public ActionResult Completed( long id , string openId = "" , decimal price = 0 )
        {
            ViewBag.Price = price;
            BonusModel model = new BonusModel( this._iBonusService.Get( id ) );
            model.ImagePath = HimallIO.GetFilePath(model.ImagePath);
            ViewBag.OpenId = openId;
            return View( model );
        }

        public ActionResult CanReceiveNotAttention( long id , string openId = "" , decimal price = 0 )
        {
            ViewBag.Price = price;
            BonusModel model = new BonusModel( this._iBonusService.Get( id ) );
            //TODO:改成统一的方式取 Token
            var token = AccessTokenContainer.TryGetToken( this._siteSetting.WeixinAppId , this._siteSetting.WeixinAppSecret );

            SceneHelper helper = new SceneHelper();
            var qrTicket = Senparc.Weixin.MP.AdvancedAPIs.QrCode.QrCodeApi.Create( token , 86400 , 123456789 ).ticket;
			ViewBag.ticket = Application.WXApiApplication.GetTicket(this._siteSetting.WeixinAppId, this._siteSetting.WeixinAppSecret);
            model.ImagePath = HimallIO.GetFilePath(model.ImagePath);
            ViewBag.OpenId = openId;
            ViewBag.QRTicket = qrTicket;
            return View( model );
        }

        //失效
        public ActionResult Invalid( long id )
        {
            BonusModel model = new BonusModel( this._iBonusService.Get( id ) );
            model.ImagePath = HimallIO.GetFilePath(model.ImagePath);
            return View( model );
        }

        public ActionResult Invalidtwo()
        {
            return View();
        }

        //未开始
        public ActionResult NotStart( long id , string openId = "" )
        {
            BonusModel model = new BonusModel( this._iBonusService.Get( id ) );
            model.ImagePath = HimallIO.GetFilePath(model.ImagePath);
            ViewBag.OpenId = openId;
            return View( model );
        }

        //已领取过
        public ActionResult HasReceive( long id )
        {
            BonusModel model = new BonusModel( this._iBonusService.Get( id ) );
            model.ImagePath = HimallIO.GetFilePath(model.ImagePath);
            return View( model );
        }

        //红包已被领取完
        public ActionResult HaveNot( long id )
        {
            BonusModel model = new BonusModel( this._iBonusService.Get( id ) );
            return View( model );
        }

        //未关注
        public ActionResult NotAttention( long id )
        {
            BonusModel model = new BonusModel( this._iBonusService.Get( id ) );
            //TODO:改成统一的方式取 Token
            var token = AccessTokenContainer.TryGetToken( this._siteSetting.WeixinAppId , this._siteSetting.WeixinAppSecret );

            SceneHelper helper = new SceneHelper();
            Himall.Model.SceneModel scene = new SceneModel( QR_SCENE_Type.Bonus , model );
            int sceneId = helper.SetModel( scene );
            var ticket = Senparc.Weixin.MP.AdvancedAPIs.QrCode.QrCodeApi.Create( token , 86400 , sceneId ).ticket;
            ViewBag.ticket = ticket;
            return View( "~/Areas/Mobile/Templates/Default/Views/Bonus/NotAttention.cshtml" , model );
        }

        /// <summary>
        /// 设置为分享状态
        /// </summary>
        [HttpPost]
        public ActionResult SetShare( long id , string openId = "" )
        {
            this._iBonusService.SetShare( id , openId );
            return Json( true );
        }

        /// <summary>
        /// 拆红包
        /// </summary>
        /// <param name="id">红包id</param>
        /// <param name="openId">微信id</param>
        /// <param name="isShare">是否分享，1：已分享，0：没分享</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Receive( long id , string openId = "" )
        {
            ReceiveModel obj = ( ReceiveModel )this._iBonusService.Receive( id , openId );
            var bonus = this._iBonusService.Get( id );
            BonusModel model = new BonusModel( bonus );
            if( obj.State == ReceiveStatus.CanReceive )
            {
                return Redirect( "/m-weixin/Bonus/Completed/" + model.Id + "?openId=" + openId + "&price=" + obj.Price );
            }
            if( obj.State == ReceiveStatus.CanReceiveNotAttention )
            {
                return Redirect( "/m-weixin/Bonus/CanReceiveNotAttention/" + model.Id + "?openId=" + openId + "&price=" + obj.Price );
            }
            else if( obj.State == ReceiveStatus.Receive )
            {
                return Redirect( "/m-weixin/Bonus/HasReceive/" + model.Id );
            }
            else if( obj.State == ReceiveStatus.HaveNot )
            {
                return Redirect( "/m-weixin/Bonus/HaveNot/" + model.Id );
            }
            else if( obj.State == ReceiveStatus.NotAttention )
            {
                return Redirect( "/m-weixin/Bonus/NotAttention/" + model.Id );
            }
            else if( obj.State == ReceiveStatus.Invalid )
            {
                return Redirect( "/m-weixin/Bonus/Invalid/" + model.Id );
            }
            else
            {
                throw new Exception( "领取发生异常" );
            }
        }

    }
}