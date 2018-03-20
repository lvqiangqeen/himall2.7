using Himall.Core;
using Himall.DTO;
using Himall.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Application
{
    public class WXSmallProgramApplication
    {
        #region 字段
        private static IWXSmallProgramService _iWXSmallProgramService = ObjectContainer.Current.Resolve<IWXSmallProgramService>();
        private static IProductService _iProductService = ObjectContainer.Current.Resolve<IProductService>();
        #endregion

        public static void SetWXSmallProducts(string productIds)
        {
            List<Himall.Model.ProductInfo> lProductInfo = new List<Model.ProductInfo>();
            var lbId = _iWXSmallProgramService.GetWXSmallProducts().Select(item => item.ProductId).ToList();
            if (!string.IsNullOrEmpty(productIds))
            {
                var productIdsArr = productIds.Split(',').Select(item => long.Parse(item)).ToList();
                lProductInfo = _iProductService.GetAllProductByIds(productIdsArr);
                foreach (Himall.Model.ProductInfo item in lProductInfo)
                {
                    if (!lbId.Contains(Convert.ToInt32(item.Id)))
                    {
                        Himall.Model.WXSmallChoiceProductsInfo mProductsInfo = new Model.WXSmallChoiceProductsInfo()
                        {
                            ProductId=Convert.ToInt32(item.Id)
                        };
                        _iWXSmallProgramService.AddWXSmallProducts(mProductsInfo);
                    }
                }
            }
        }
    }
}
