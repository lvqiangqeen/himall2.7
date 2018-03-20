using System.Collections.Generic;
using System.Linq;
using Himall.Core;
using Himall.DTO;
using Himall.IServices;

namespace Himall.Application
{
    public class DistributionApplication
    {
        private static IDistributionService _iDistributionService = ObjectContainer.Current.Resolve<IDistributionService>();
        private static IBrandService _iBrandService = ObjectContainer.Current.Resolve<IBrandService>();
        private static ICategoryService _iCategoryService = ObjectContainer.Current.Resolve<ICategoryService>();


        #region 首页商品相关
        /// <summary>
        /// 获取已绑定的分销首页商品
        /// </summary>
        /// <returns></returns>
        public static object GetAllHomeProductIds()
        {
            return _iDistributionService.GetDistributionProducts().Select(item => item.Himall_ProductBrokerage.ProductId);
        }

        /// <summary>
        /// 设置分销首页商品
        /// </summary>
        /// <param name="productIds"></param>
        public static void SetDistributionProducts(string productIds)
        {
            List<Himall.Model.ProductBrokerageInfo> lProductBrokerageInfo = new List<Model.ProductBrokerageInfo>();
            var lbId = _iDistributionService.GetDistributionProducts().Select(item => item.ProductbrokerageId).ToList();
            if (!string.IsNullOrEmpty(productIds))
            {
                var productIdsArr = productIds.Split(',').Select(item => long.Parse(item)).ToList();
                lProductBrokerageInfo = _iDistributionService.GetDistributionProductInfo(productIdsArr);
                foreach (Himall.Model.ProductBrokerageInfo item in lProductBrokerageInfo)
                {
                    //添加没有的项
                    if (!lbId.Contains(item.Id))
                    {
                        Himall.Model.DistributionProductsInfo mDistributionProductsInfo = new Model.DistributionProductsInfo()
                        {
                            ProductbrokerageId = item.Id,
                            Sequence = 0
                        };
                        _iDistributionService.AddDistributionProducts(mDistributionProductsInfo);
                    }
                }
            }

            //移除不包含的项
            if (lProductBrokerageInfo.Count > 0)
            {
                var delProductIds = lbId.Where(e => !lProductBrokerageInfo.Select(item => item.Id).Contains(e));
                if (delProductIds.Count() > 0)
                    _iDistributionService.RemoveDistributionProducts(delProductIds);
            }
            else
            {
                _iDistributionService.RemoveDistributionProducts(lbId);
            }
        }

        /// <summary>
        /// 查询已绑定的分销首页商品信息
        /// </summary>
        /// <param name="page">分页页码</param>
        /// <param name="rows">每页行数</param>
        /// <param name="keyWords">搜索关键字</param>
        /// <param name="categoryId">3级分类</param>
        /// <returns></returns>
        public static Himall.CommonModel.QueryPageModel<Himall.DTO.DistributionProducts> GetDistributionProducts(int page, int rows, string keyWords, long? categoryId = null)
        {
            var model = _iDistributionService.GetDistributionProducts(page, rows, keyWords, categoryId);

            Himall.CommonModel.QueryPageModel<Himall.DTO.DistributionProducts> query = new CommonModel.QueryPageModel<DistributionProducts>()
            {
                Models = ChangeDistributionProductsModels(model.Models),
                Total = model.Total
            };
            return query;
        }

        /// <summary>
        /// 获取所有分销首页数据，不包含已停止商品
        /// </summary>
        /// <param name="userId">会员ID</param>
        /// <returns></returns>
        public static List<Himall.DTO.DistributionProducts> GetDistributionProducts(long userId)
        {
            var model = _iDistributionService.GetDistributionProducts(Himall.Model.ProductBrokerageInfo.ProductBrokerageStatus.Normal);

            return ChangeDistributionProductsModels(model, userId);
        }

        /// <summary>
        /// 分销首页商品实体转换
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        private static List<Himall.DTO.DistributionProducts> ChangeDistributionProductsModels(List<Himall.Model.DistributionProductsInfo> models, long? userId = 0)
        {
            List<long> canAgentIds = new List<long>();
            if (userId.HasValue)
            {
                List<long> proids = _iDistributionService.GetDistributionProducts().Select(item => item.Himall_ProductBrokerage.ProductId.Value).ToList();
                canAgentIds = _iDistributionService.GetCanAgentProductId(proids, userId.Value).ToList();
            }

            IQueryable<Himall.DTO.DistributionProducts> returnsql = models.Select(a =>
            {
                //品牌
                var brand = _iBrandService.GetBrand(a.Himall_ProductBrokerage.Himall_Products.BrandId);
                //分销价格
                decimal Commission = 0;
                decimal rate = a.Himall_ProductBrokerage.rate;
                if (rate > 0)
                {
                    Commission = (a.Himall_ProductBrokerage.Himall_Products.MinSalePrice * rate / 100);
                    int _tmp = (int)(Commission * 100);
                    //保留两位小数，但不四舍五入
                    Commission = (decimal)(((decimal)_tmp) / (decimal)100);
                }
                //成交数
                long SaleNum = 0;
                SaleNum = a.Himall_ProductBrokerage.Himall_Products.Himall_ProductVistis.Sum(item => item.SaleCounts);

                //是否代理
                bool isHasAgent = false;
                if (userId.HasValue)
                {
                    isHasAgent = !canAgentIds.Contains(a.Himall_ProductBrokerage.ProductId.Value);
                }

                return new Himall.DTO.DistributionProducts
                {
                    Id = a.ID,
                    Brand = brand == null ? "" : brand.Name,
                    CategoryName = _iCategoryService.GetCategory(long.Parse(_iCategoryService.GetCategory(a.Himall_ProductBrokerage.Himall_Products.CategoryId).Path.Split('|').Last())).Name,
                    Image = Himall.Core.HimallIO.GetProductSizeImage(a.Himall_ProductBrokerage.Himall_Products.RelativePath, 1, 100),
                    Price = a.Himall_ProductBrokerage.Himall_Products.MinSalePrice,
                    ProductbrokerageId = a.Himall_ProductBrokerage.Id,
                    ProductId = a.Himall_ProductBrokerage.ProductId.Value,
                    ProductName = a.Himall_ProductBrokerage.Himall_Products.ProductName,
                    Sequence = a.Sequence,
                    Status = a.Himall_ProductBrokerage.Status,
                    Commission = Commission,
                    AgentNum = a.Himall_ProductBrokerage.AgentNum.HasValue ? a.Himall_ProductBrokerage.AgentNum.Value : 0,
                    SaleNum = SaleNum,
                    isHasAgent = isHasAgent
                };
            }
            ).AsQueryable();
            return returnsql.ToList();
        }

        /// <summary>
        /// 删除设置
        /// </summary>
        /// <param name="Id"></param>
        public static void DelDistributionProducts(long Id)
        {
            _iDistributionService.DelDistributionProducts(Id);
        }

        /// <summary>
        /// 修改排序
        /// </summary>
        /// <param name="Id">主键ID</param>
        /// <param name="sequence">排序数</param>
        public static void UpdateSequence(long Id, short sequence)
        {
            var model = _iDistributionService.GetDistributionProductsInfo(Id);
            model.Sequence = sequence;
            _iDistributionService.UpdateDistributionProducts(model);
        }
        #endregion
    }
}
