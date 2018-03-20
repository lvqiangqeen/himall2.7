using Himall.Core;
using Himall.Entity;
using Himall.IServices;
using Himall.Model;
using System.Collections.Generic;
using System.Linq;
using Himall.Model.DTO;

namespace Himall.Service
{
    public class ShopHomeModuleService : ServiceBase , IShopHomeModuleService
    {
        #region 老版本
        public void AddShopProductModule( ShopHomeModuleInfo shopProductModuleInfo )
        {
            if( string.IsNullOrWhiteSpace( shopProductModuleInfo.Name ) )
                throw new InvalidPropertyException( "商品模块名称不能为空" );

            Context.ShopHomeModuleInfo.Add( shopProductModuleInfo );
            Context.SaveChanges();
        }

        public void UpdateShopProductModuleName( long shopId , long id , string name )
        {
            var shopProductModule = Context.ShopHomeModuleInfo.FirstOrDefault( item => item.Id == id && item.ShopId == shopId );
            if( shopProductModule == null )
                throw new HimallException( "在本店铺中未找到指定商品模块" );

            shopProductModule.Name = name;
            Context.SaveChanges();
        }

        public void UpdateShopProductModuleProducts( long shopId , long id , IEnumerable<long> productIds )
        {
            var shopProductModule = Context.ShopHomeModuleInfo.FirstOrDefault( item => item.Id == id && item.ShopId == shopId );
            if( shopProductModule == null )
                throw new HimallException( "在本店铺中未找到指定商品模块" );

            var products = shopProductModule.ShopHomeModuleProductInfo.ToArray();

            //找出待删除的id
            var needToDeleteIds = products.Where( item => !productIds.Contains( item.ProductId ) ).Select( item => item.Id );

            //找出待添加的商品id
            var produtIds = products.Select( item => item.ProductId );
            var needToAddProductIds = productIds.Where( item => !produtIds.Contains( item ) );

            //删除
            Context.ShopHomeModuleProductInfo.Remove( item => needToDeleteIds.Contains( item.Id ) );

            //添加待添加的
            foreach( var productId in needToAddProductIds.ToList() )
            {
                var item = new ShopHomeModuleProductInfo()
                {
                    ProductId = productId ,
                    HomeModuleId = shopProductModule.Id ,
                };
                Context.ShopHomeModuleProductInfo.Add( item );
            }
            Context.SaveChanges();
        }

        public void Delete( long shopId , long id )
        {
            Context.ShopHomeModuleInfo.Remove( item => item.Id == id && item.ShopId == shopId );
            Context.SaveChanges();
        }


        public IQueryable<ShopHomeModuleInfo> GetAllShopHomeModuleInfos( long shopId )
        {
            var list = Context.ShopHomeModuleInfo.Where( p => p.DisplaySequence == 0 && p.ShopId == shopId );
            if( list.Count() > 1 )
            {
                int index = 0;
                foreach( var model in list )
                {
                    model.DisplaySequence = index++;
                }
                Context.SaveChanges();
            }
            return Context.ShopHomeModuleInfo.FindBy( item => item.ShopId == shopId ).OrderBy( p => p.DisplaySequence );
        }

        public ShopHomeModuleInfo GetShopHomeModuleInfo( long shopId , long id )
        {
            return Context.ShopHomeModuleInfo.FirstOrDefault( item => item.ShopId == shopId && item.Id == id );
        }
        #endregion

        public string GetFooter( long shopid )
        {
            var model = Context.ShopFooterInfo.FirstOrDefault( p => p.ShopId == shopid );
            if( model == null )
            {
                ShopFooterInfo info = new ShopFooterInfo();
                info.ShopId = shopid;
                info.Footer = "";
                info = Context.ShopFooterInfo.Add(info);
                Context.SaveChanges();
                //补充获取
                model = Context.ShopFooterInfo.FirstOrDefault(p => p.ShopId == shopid);
            }
            
            return model==null?"":model.Footer;
        }

        public void EditFooter( long shopid , string footer )
        {
            var model = Context.ShopFooterInfo.FirstOrDefault( p => p.ShopId == shopid );
            model.Footer = footer;
            Context.SaveChanges();
        }

        public void SaveFloor( AddShopHomeModuleModel model )
        {
            if( model.Id <= 0 ) //新增
            {
                var rows = Context.ShopHomeModuleInfo.Count();
                int count = rows > 0 ? Context.ShopHomeModuleInfo.Max(p => p.DisplaySequence) : 0;
                ShopHomeModuleInfo main = new ShopHomeModuleInfo();
                main.Name = model.Name;
                main.Url = model.Url;
                main.ShopId = model.ShopId;
                main.IsEnable = true;
                main.DisplaySequence = count + 1;
                main = Context.ShopHomeModuleInfo.Add( main );
                Context.SaveChanges();

                foreach( var p in model.Products )
                {
                    ShopHomeModuleProductInfo product = new ShopHomeModuleProductInfo();
                    product.HomeModuleId = main.Id;
                    product.DisplaySequence = p.DisplaySequence;
                    product.ProductId = p.ProductId;
                    Context.ShopHomeModuleProductInfo.Add( product );
                }

                foreach( var t in model.TopImgs )
                {
                    ShopHomeModulesTopImgInfo top = new ShopHomeModulesTopImgInfo();
                    top.HomeModuleId = main.Id;
                    top.ImgPath = t.ImgPath;
                    top.Url = t.Url;
                    top.DisplaySequence = top.DisplaySequence;
                    Context.ShopHomeModulesTopImgInfo.Add( top );
                }
                Context.SaveChanges();
            }
            else //修改
            {
                Context.ShopHomeModuleProductInfo.Remove( p => p.HomeModuleId == model.Id );
                Context.ShopHomeModulesTopImgInfo.Remove( p => p.HomeModuleId == model.Id );

                ShopHomeModuleInfo main = Context.ShopHomeModuleInfo.FirstOrDefault( p => p.Id == model.Id );
                main.Name = model.Name;
                main.Url = model.Url;

                foreach( var p in model.Products )
                {
                    ShopHomeModuleProductInfo product = new ShopHomeModuleProductInfo();
                    product.HomeModuleId = main.Id;
                    product.DisplaySequence = p.DisplaySequence;
                    product.ProductId = p.ProductId;
                    Context.ShopHomeModuleProductInfo.Add( product );
                }

                foreach( var t in model.TopImgs )
                {
                    ShopHomeModulesTopImgInfo top = new ShopHomeModulesTopImgInfo();
                    top.HomeModuleId = main.Id;
                    top.ImgPath = t.ImgPath;
                    top.Url = t.Url;
                    top.DisplaySequence = top.DisplaySequence;
                    Context.ShopHomeModulesTopImgInfo.Add( top );
                }
                Context.SaveChanges();
            }
        }

        public void Enable( long id , bool enable )
        {
            var model = Context.ShopHomeModuleInfo.FindById( id );
            model.IsEnable = enable;
            Context.SaveChanges();
        }

        public void UpdateFloorSequence( long shopId , int oriRowNumber , int newRowNumber , string direction )
        {
            var source = Context.ShopHomeModuleInfo.FirstOrDefault( item => item.ShopId == shopId && item.DisplaySequence == oriRowNumber );
            var destination = Context.ShopHomeModuleInfo.FirstOrDefault( item => item.ShopId == shopId && item.DisplaySequence == newRowNumber );

            source.DisplaySequence = newRowNumber;
            destination.DisplaySequence = oriRowNumber;
            Context.SaveChanges();
        }

        public void DelFloor( long id )
        {
            Context.ShopHomeModuleProductInfo.Remove( p => p.HomeModuleId == id );
            Context.ShopHomeModulesTopImgInfo.Remove( p => p.HomeModuleId == id );
            Context.ShopHomeModuleInfo.Remove( id );
            Context.SaveChanges();
        }
    }
}
