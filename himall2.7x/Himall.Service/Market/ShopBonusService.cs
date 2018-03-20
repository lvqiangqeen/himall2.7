using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Entity;
using Himall.Core;
using System.Data.Entity.Infrastructure;
using Himall.Service.Market.Business;
using System.Drawing;
using System.IO;
using Senparc.Weixin.MP.CommonAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.User;
using Himall.IServices.QueryModel;

namespace Himall.Service
{
   public class ShopBonusService : ServiceBase , IShopBonusService
    {
        /// <summary>
        /// 优惠券类型
        /// </summary>
        private WXCardLogInfo.CouponTypeEnum ThisCouponType = WXCardLogInfo.CouponTypeEnum.Bonus;
        /// <summary>
        /// 微信卡券服务
        /// </summary>
        private IWXCardService ser_wxcard;
        public ShopBonusService()
        {
            ser_wxcard = Himall.ServiceProvider.Instance<IWXCardService>.Create;
        }

        public ObsoletePageModel<ShopBonusInfo> Get( long shopid , string name , int state , int pageIndex , int pageSize )
        {
            IQueryable<ShopBonusInfo> query = Context.ShopBonusInfo.Where( p => p.ShopId == shopid );
            if( !string.IsNullOrEmpty( name ) )
            {
                query = query.Where( p => p.Name.Contains( name ) );
            }
            if( state == 1 )
            {
                query = query.Where( p => p.DateEnd > DateTime.Now && !p.IsInvalid );
            }
            else
            {
                query = query.Where( p => p.DateEnd < DateTime.Now || p.IsInvalid );
            }
            if( pageIndex <= 0 )
            {
                pageIndex = 1;
            }
            int total = 0;
            IQueryable<ShopBonusInfo> datas = query.GetPage( out total , p => p.OrderByDescending( o => o.BonusDateStart ) , pageIndex , pageSize );
            ObsoletePageModel<ShopBonusInfo> pageModel = new ObsoletePageModel<ShopBonusInfo>()
            {
                Models = datas.OrderByDescending( p => p.BonusDateStart) ,
                Total = total
            };
            return pageModel;
        }

        public decimal GetUsedPrice( long orderid , long userid )
        {
            var model = Context.ShopBonusReceiveInfo.Where( p => p.UsedOrderId == orderid && p.UserId == userid ).FirstOrDefault();
            if( model != null )
            {
                return (decimal)model.Price;
            }
            return 0;
        }

        public ShopBonusInfo Get( long id )
        {
            return Context.ShopBonusInfo.FindById( id );
        }

        public ShopBonusInfo GetByGrantId( long grantid )
        {
            var model = Context.ShopBonusGrantInfo.Where( p => p.Id == grantid ).FirstOrDefault();
            if( model != null )
            {
                return model.Himall_ShopBonus;
            }
            return null;
        }

        /// <summary>
        /// 获取可用红包
        /// </summary>
        /// <returns></returns>
        public List<ShopBonusReceiveInfo> GetDetailToUse( long shopid , long userid , decimal sumprice )
        {
            var result = Context.ShopBonusReceiveInfo.Where( p =>
                p.UserId == userid &&
                p.State == ShopBonusReceiveInfo.ReceiveState.NotUse &&
                p.Himall_ShopBonusGrant.Himall_ShopBonus.ShopId == shopid &&
                p.Himall_ShopBonusGrant.Himall_ShopBonus.BonusDateEnd > DateTime.Now &&
                p.Himall_ShopBonusGrant.Himall_ShopBonus.BonusDateStart < DateTime.Now ).ToList();

            if( result.Count <= 0 )
            {
                return result;
            }

            var bonus = result[ 0 ].Himall_ShopBonusGrant.Himall_ShopBonus;
            if( bonus.UseState == ShopBonusInfo.UseStateType.FilledSend )
            {//修改为不需要大于优惠券本身价格 && p.Price < sumprice 
                result = result.Where( p => p.Himall_ShopBonusGrant.Himall_ShopBonus.UsrStatePrice <= sumprice).OrderByDescending( p => p.Price).ToList();
            }
            return result.Where( p => p.Price < sumprice ).OrderByDescending( p => p.Price ).ToList();
        }

        public ObsoletePageModel<ShopBonusReceiveInfo> GetDetailByQuery( CouponRecordQuery query )
        {
            if( query.PageNo <= 0 )
            {
                query.PageNo = 1;
            }
            int total = 0;
            var bonusReceiveContext = Context.ShopBonusReceiveInfo.Where( p => p.UserId == query.UserId );
            if( query.Status == 0 )
            {
                bonusReceiveContext = bonusReceiveContext.Where(
                    p => p.Himall_ShopBonusGrant.Himall_ShopBonus.BonusDateEnd > DateTime.Now &&
                    p.State == ShopBonusReceiveInfo.ReceiveState.NotUse );
            }
            else if( query.Status == 1 )
            {
                bonusReceiveContext = bonusReceiveContext.Where( p => p.State == ShopBonusReceiveInfo.ReceiveState.Use );
            }
            if( query.Status == 2 )
            {
                bonusReceiveContext = bonusReceiveContext.Where( p => p.Himall_ShopBonusGrant.Himall_ShopBonus.BonusDateEnd < DateTime.Now );
            }

            IQueryable<ShopBonusReceiveInfo> datas = bonusReceiveContext.GetPage( out total , p => p.OrderByDescending( o => o.ReceiveTime ) , query.PageNo , query.PageSize );
            ObsoletePageModel<ShopBonusReceiveInfo> pageModel = new ObsoletePageModel<ShopBonusReceiveInfo>()
            {
                Models = datas ,
                Total = total
            };
            return pageModel;
        }

        public List<ShopBonusReceiveInfo> GetDetailByUserId( long userid )
        {
            return Context.ShopBonusReceiveInfo.Where( p => p.UserId == userid ).ToList();
        }

        public List<ShopBonusReceiveInfo> GetCanUseDetailByUserId( long userid )
        {
            return Context.ShopBonusReceiveInfo.Where( p => p.UserId == userid && p.State == ShopBonusReceiveInfo.ReceiveState.NotUse && p.Himall_ShopBonusGrant.Himall_ShopBonus.BonusDateEnd > DateTime.Now ).ToList();
        }

        public ShopBonusInfo GetByShopId( long shopid )
        {
            return Context.ShopBonusInfo.Where( p => p.ShopId == shopid && !p.IsInvalid && p.DateEnd > DateTime.Now ).FirstOrDefault();
        }

        public ShopBonusReceiveInfo GetDetailById( long userid , long id ) 
        {
            return Context.ShopBonusReceiveInfo.Where( p => p.Id == id && p.UserId == userid ).FirstOrDefault();
        }

        public ShopBonusGrantInfo GetGrantByUserOrder( long orderid , long userid )
        {
            var result = Context.ShopBonusGrantInfo.Where( p =>
                p.OrderId == orderid &&
                p.UserId == userid &&
                p.Himall_ShopBonusReceive.Any( o => o.UserId == null || o.UserId <= 0 ) &&  //还存在未领取的红包
                p.Himall_ShopBonus.DateEnd > DateTime.Now ).FirstOrDefault();  //未超过红包有效期
            return result;
        }

        public List<ShopBonusReceiveInfo> GetDetailByGrantId( long grantid )
        {
            return Context.ShopBonusReceiveInfo.Where( p => p.BonusGrantId == grantid && !string.IsNullOrEmpty( p.OpenId ) ).ToList();
        }

        public ObsoletePageModel<ShopBonusReceiveInfo> GetDetail( long bonusid , int pageIndex , int pageSize )
        {
            if( pageIndex <= 0 )
            {
                pageIndex = 1;
            }
            int total = 0;
            var bonusReceiveContext = Context.ShopBonusReceiveInfo.Where( p => p.Himall_ShopBonusGrant.ShopBonusId == bonusid );
            IQueryable<ShopBonusReceiveInfo> datas = bonusReceiveContext.GetPage( out total , p => p.OrderByDescending( o => o.ReceiveTime ) , pageIndex , pageSize );
            ObsoletePageModel<ShopBonusReceiveInfo> pageModel = new ObsoletePageModel<ShopBonusReceiveInfo>()
            {
                Models = datas ,
                Total = total
            };
            return pageModel;
        }

        public long GetGrantIdByOrderId( long orderid)
        {
            var model = Context.ShopBonusGrantInfo.Where( p => p.OrderId == orderid ).FirstOrDefault();
            if( model != null )
            {
                return model.Id;
            }
            return 0;
        }

        public bool IsAdd( long shopid)
        {
            var bonus = Context.ShopBonusInfo.Where( p => p.ShopId == shopid && !p.IsInvalid && p.DateEnd > DateTime.Now && p.DateStart < DateTime.Now ).FirstOrDefault();
            if( bonus != null )
            {
                return false;
            }
            return true;
        }

        public void Add( ShopBonusInfo model ,  long shopid )
        {
            var bonus = Context.ShopBonusInfo.Where( p => p.ShopId == shopid ).FirstOrDefault();
            if( bonus != null && !bonus.IsInvalid && bonus.DateEnd > DateTime.Now && bonus.DateStart < DateTime.Now )
            {
                throw new HimallException( "一个时间段只能新增一个随机红包" );
            }
            model.DateEnd = model.DateEnd.AddHours( 23 ).AddMinutes( 59 ).AddSeconds( 59 );
            model.BonusDateEnd = model.BonusDateEnd.AddHours( 23 ).AddMinutes( 59 ).AddSeconds( 59 );
            model.ShopId = shopid;
            model.IsInvalid = false;
            model.ReceiveCount = 0;
            model.QRPath = "";
            model = Context.ShopBonusInfo.Add( model );
            Context.Configuration.ValidateOnSaveEnabled = false;
            Context.SaveChanges();
            Context.Configuration.ValidateOnSaveEnabled = false;
            Context.SaveChanges();
            
            #region 同步微信
            if (model.SynchronizeCard == true)
            {
                WXCardLogInfo wxdata = new WXCardLogInfo();
                wxdata.CardColor =model.CardColor;
                wxdata.CardTitle = model.CardTitle;
                wxdata.CardSubTitle = model.CardSubtitle;
                wxdata.CouponType = ThisCouponType;
                wxdata.CouponId = model.Id;
                wxdata.ShopId = model.ShopId;
                wxdata.Quantity = 0;  //最大库存数
                wxdata.DefaultDetail = model.RandomAmountStart.ToString("F2") + "元" + model.RandomAmountEnd.ToString("F2") + "元随机优惠券1张";
                wxdata.LeastCost = (model.UseState == ShopBonusInfo.UseStateType.None ? 0 : (int)(model.UsrStatePrice * 100));
                wxdata.BeginTime = model.BonusDateStart.Date;
                wxdata.EndTime = model.BonusDateEnd.AddDays(1).AddMinutes(-1);
                ServiceProvider.Instance<IWXCardService>.Create.Add(wxdata);
            }
            #endregion
        }

        public void Update( ShopBonusInfo model )
        {
            ShopBonusInfo info = Context.ShopBonusInfo.FindById( model.Id );
            info.Name = model.Name;
            info.GrantPrice = model.GrantPrice;
            info.DateStart = model.DateStart;
            info.DateEnd = model.DateEnd.AddHours( 23 ).AddMinutes( 59 ).AddSeconds( 59 );
            info.ShareDetail = model.ShareDetail;
            info.ShareImg = model.ShareImg;
            info.ShareTitle = model.ShareTitle;

            Context.Configuration.ValidateOnSaveEnabled = false;
            Context.SaveChanges();
            Context.Configuration.ValidateOnSaveEnabled = true;
        }

        public void Invalid( long id )
        {
            var model = Context.ShopBonusInfo.FirstOrDefault( p => p.Id == id );
            model.IsInvalid = true;
            Context.Configuration.ValidateOnSaveEnabled = false;
            Context.SaveChanges();
            Context.Configuration.ValidateOnSaveEnabled = true;
        }

        public bool IsOverDate( DateTime bonusDateEnd , DateTime dateEnd , long shopid)
        {
            var market = GetShopBonusService( shopid );
            var time = ( ( Himall.Model.ActiveMarketServiceInfo )market ).MarketServiceRecordInfo.Max( a => a.EndTime );
            if( bonusDateEnd > time || dateEnd > time )
            {
                return true;
            }
            return false;
        }

        public object Receive( long grantid , string openId , string wxhead , string wxname )
        {
            Log.Info( string.Format( "Receive函数 = gid:{0} , oid:{1}" , grantid , openId ) );
            if( Context.ShopBonusReceiveInfo.Where( p => p.OpenId == openId && p.BonusGrantId == grantid ).Count() > 0 ) //已领取过
            {
                return new ShopReceiveModel { State = ShopReceiveStatus.Receive , Price = 0 };
            }

            var receives = Context.ShopBonusReceiveInfo.Where( p => p.BonusGrantId == grantid && string.IsNullOrEmpty( p.OpenId ) ).FirstOrDefault();
            if( receives == null )  //已被领完
            {
                return new ShopReceiveModel { State = ShopReceiveStatus.HaveNot , Price = 0 };
            }
            else if( receives.Himall_ShopBonusGrant.Himall_ShopBonus.IsInvalid )  //失效
            {
                return new ShopReceiveModel { State = ShopReceiveStatus.Invalid , Price = 0 };
            }

            MemberOpenIdInfo model = model = Context.MemberOpenIdInfo.Where( p => p.OpenId == openId ).FirstOrDefault();
            if( model != null ) //在平台有帐号并且已经绑定openid
            {
                receives.UserId = model.UserId;
            }

            receives.OpenId = openId;
            receives.ReceiveTime = DateTime.Now;
            receives.WXHead = wxhead;
            receives.WXName = wxname;
            Context.SaveChanges();
            if( model != null )
            {
                string username = Context.UserMemberInfo.Where( p => p.Id == model.UserId ).Select( p => p.UserName ).FirstOrDefault();
                return new ShopReceiveModel { State = ShopReceiveStatus.CanReceive , Price = ( decimal )receives.Price , UserName = username, Id=receives.Id };
            }
            return new ShopReceiveModel { State = ShopReceiveStatus.CanReceiveNotUser , Price = ( decimal )receives.Price, Id=receives.Id };
        }

        public void SetBonusToUsed( long userid , List<OrderInfo> orders , long rid )
        {
            var model = Context.ShopBonusReceiveInfo.Where( p => p.UserId == userid && p.Id == rid ).FirstOrDefault();

            model.State = ShopBonusReceiveInfo.ReceiveState.Use;
            model.UsedTime = DateTime.Now;
            model.UsedOrderId = orders.FirstOrDefault( p => p.ShopId == model.Himall_ShopBonusGrant.Himall_ShopBonus.ShopId ).Id;

            Context.SaveChanges();
            //TODO:DZY[150916]同步核销卡券
            ser_wxcard.Consume(model.Id, ThisCouponType);
        }

        public ShopBonusGrantInfo GetByOrderId( long orderid )
        {
            var model = Context.ShopBonusGrantInfo.Where( p => p.OrderId == orderid ).FirstOrDefault();
            return model;
        }

        /// <summary>
        /// 生成红包详情
        /// </summary>
        public long GenerateBonusDetail( ShopBonusInfo model ,  long orderid ,string receiveurl )
        {
            if( model.DateEnd <= DateTime.Now || model.IsInvalid )  //过期、失效
            {
                Log.Info( "此活动已过期 , shopid = " + model.ShopId );
                return 0;
            }
            else if( model.DateStart > DateTime.Now ) // 未开始
            {
                Log.Info( "此活动未开始 , shopid = " + model.ShopId );
                return 0;
            }

            ShopBonusGrantInfo grant = null;
            try
            {
                var order = Context.OrderInfo.Where(r => r.Id == orderid).FirstOrDefault();

                grant = Context.ShopBonusGrantInfo.Where( p => p.OrderId == orderid ).FirstOrDefault();
                if( Context.ShopBonusGrantInfo.Exist( ( p => p.OrderId == orderid ) ) )
                {
                    Log.Info( "此活动已存在防止多次添加 , shopid = " + model.ShopId );
                    return grant.Id;
                }
                grant = new ShopBonusGrantInfo();
                grant.ShopBonusId = model.Id;
                grant.UserId = order.UserId;
                grant.OrderId = orderid;
                grant.BonusQR = "";
                grant = Context.ShopBonusGrantInfo.Add( grant );
                Context.SaveChanges();

                string path = GenerateQR( Path.Combine( receiveurl , grant.Id.ToString() ) );
                grant.BonusQR = path;

                List<ShopBonusReceiveInfo> list = new List<ShopBonusReceiveInfo>();
                for( int i = 0 ; i < model.Count ; i++ )
                {
                    decimal randPrice = GenerateRandomAmountPrice( model.RandomAmountStart , model.RandomAmountEnd );
                    ShopBonusReceiveInfo detail = new ShopBonusReceiveInfo
                    {
                        BonusGrantId = grant.Id ,
                        Price = randPrice ,
                        OpenId = null ,
                        State = ShopBonusReceiveInfo.ReceiveState.NotUse
                    };
                    list.Add( detail );
                }
                Context.ShopBonusReceiveInfo.AddRange( list );
                Context.SaveChanges();
            }
            catch( Exception e )
            {
                Log.Info( "错误：" , e );
                Context.ShopBonusReceiveInfo.Remove( p => p.Himall_ShopBonusGrant.ShopBonusId == model.Id );
                Context.ShopBonusGrantInfo.Remove( p => p.ShopBonusId == model.Id );
                Context.ShopBonusInfo.Remove( p => p.Id == model.Id );
                Context.SaveChanges();
            }
            return grant.Id;
        }

        private Random _random = new Random( ( int )( DateTime.Now.Ticks & 0xffffffffL ) | ( int )( DateTime.Now.Ticks >> 32 ) );
        /// <summary>
        /// 生成随机数
        /// </summary>
        private decimal GenerateRandomAmountPrice( decimal start , decimal end )
        {

            if( start == end )
            {
                return start;
            }

            decimal temp = _random.Next( (int)start , (int)end );
            string startF = String.Format( "{0:N2} " , start );
            string endF = String.Format( "{0:N2} " , end );
            startF = startF.Substring( startF.IndexOf( '.' ) + 1 , 2 );//小数位
            endF = endF.Substring( endF.IndexOf( '.' ) + 1 , 2 );//小数位
            if( int.Parse( startF ) == 0 )
            {
                startF = "1";
            }
            if( int.Parse( endF ) < int.Parse( startF ) )
            {
                endF = "100";
            }
            decimal tempF = ( decimal )_random.Next( int.Parse( startF ) , int.Parse( endF ) ) / 100;

            temp += tempF;
            return temp;
        }

        /// <summary>
        /// 生成二维码
        /// </summary>
        private string GenerateQR( string path )
        {
            Bitmap bi = Himall.Core.Helper.QRCodeHelper.Create( path );
            MemoryStream ms = new MemoryStream();
            bi.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            string fileName = Guid.NewGuid().ToString() + ".jpg";
            var fileFolderPath = "/temp/ShopBonusShareCode" + fileName;
            MemoryStream me = new MemoryStream(ms.ToArray());
            Core.HimallIO.CreateFile(fileFolderPath, me, FileCreateType.Create);
            string newPath = MoveImages(fileFolderPath);
            ms.Dispose();
            me.Dispose();
            return newPath;
        }

        private string MoveImages(string image)
        {
            if (string.IsNullOrWhiteSpace(image))
            {
                return "";
            }
            var ext = image.Substring(image.LastIndexOf("."));
            var name = DateTime.Now.ToString("yyyyMMddhhmmss");
            //转移图片
            string relativeDir = "/Storage/ShopbonusGrant/ShopBonusShareCode/";
            string fileName = "hare" + name + ext;
            if (image.Contains("/temp/"))//只有在临时目录中的图片才需要复制
            {
                string temp = image.Substring(image.LastIndexOf("/temp"));
                Core.HimallIO.CopyFile(temp, relativeDir + fileName, true);
                return relativeDir + fileName;
            }  //目标地址
            else
            {
                var fname = image.Substring(image.LastIndexOf("/") + 1);
                return relativeDir + fname;
            }
        }
        public ActiveMarketServiceInfo GetShopBonusService( long shopId )
        {

            if( shopId <= 0 )
            {
                throw new HimallException( "ShopId不能识别" );
            }
            var market = Context.ActiveMarketServiceInfo.FirstOrDefault( m => m.ShopId == shopId && m.TypeId == MarketType.RandomlyBonus );
            return market;
        }
    }

    
}
