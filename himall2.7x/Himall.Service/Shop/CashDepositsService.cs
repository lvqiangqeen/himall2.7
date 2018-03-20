using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Entity;
using Himall.Core;

namespace Himall.Service
{
    public class CashDepositsService : ServiceBase, ICashDepositsService
    {
        public ObsoletePageModel<CashDepositInfo> GetCashDeposits(CashDepositQuery query)
        {
            IQueryable<CashDepositInfo> cashDeposits = Context.CashDepositInfo.AsQueryable();
            if (!string.IsNullOrWhiteSpace(query.ShopName))
            {
                cashDeposits = cashDeposits.Where(item => item.Himall_Shops.ShopName.Contains(query.ShopName));
            }
            var c = cashDeposits.ToList();
            List<long> shopIds = new List<long>();
            foreach (var cashDeposit in c)
            {
                var needPayShop = GetNeedPayCashDepositByShopId(cashDeposit.ShopId);
                if (needPayShop > 0)
                {
                    shopIds.Add(cashDeposit.ShopId);
                }
            }
            if (query.Type.HasValue)
            {
                if (query.Type.Value == false)
                {
                    cashDeposits = cashDeposits.Where(item => shopIds.Contains(item.ShopId));
                }
                else
                    cashDeposits = cashDeposits.Where(item => !shopIds.Contains(item.ShopId));
            }
            int total;
            cashDeposits = cashDeposits.GetPage(out total, query.PageNo, query.PageSize, d => d.OrderByDescending(o => o.Date));
            ObsoletePageModel<CashDepositInfo> pageModel = new ObsoletePageModel<CashDepositInfo>() { Models = cashDeposits, Total = total };
            return pageModel;
        }

        public ObsoletePageModel<CashDepositDetailInfo> GetCashDepositDetails(CashDepositDetailQuery query)
        {
            IQueryable<CashDepositDetailInfo> cashDepositDetails = Context.CashDepositDetailInfo.AsQueryable();
            if (query.StartDate.HasValue)
            {
                cashDepositDetails = cashDepositDetails.Where(item => query.StartDate <= item.AddDate);
            }
            if (query.EndDate.HasValue)
            {
                cashDepositDetails = cashDepositDetails.Where(item => query.EndDate >= item.AddDate);
            }
            if (!string.IsNullOrWhiteSpace(query.Operator))
            {
                cashDepositDetails = cashDepositDetails.Where(item => item.Operator.Contains(query.Operator));
            }
            int total;
            cashDepositDetails = cashDepositDetails.FindBy(item => item.CashDepositId == query.CashDepositId, query.PageNo, query.PageSize, out total, item => item.AddDate, false);
            ObsoletePageModel<CashDepositDetailInfo> pageModel = new ObsoletePageModel<CashDepositDetailInfo>() { Models = cashDepositDetails, Total = total };
            return pageModel;
        }

        public CashDepositInfo GetCashDeposit(long id)
        {
            return Context.CashDepositInfo.FindById(id);
        }

        public void AddCashDepositDetails(CashDepositDetailInfo cashDepositDetail)
        {

            Context.CashDepositDetailInfo.Add(cashDepositDetail);

            CashDepositInfo cashDeposit = Context.CashDepositInfo.FindById(cashDepositDetail.CashDepositId);
            if (cashDepositDetail.Balance < 0 && cashDeposit.CurrentBalance + cashDepositDetail.Balance < 0)
                new HimallException("扣除金额不能多余店铺可用余额");
            cashDeposit.CurrentBalance = cashDeposit.CurrentBalance + cashDepositDetail.Balance;
            if (cashDepositDetail.Balance > 0)
            {
                cashDeposit.EnableLabels = true;
            }
            if (cashDepositDetail.Balance > 0)
            {
                cashDeposit.TotalBalance = cashDeposit.TotalBalance + cashDepositDetail.Balance;
                cashDeposit.Date = DateTime.Now;
            }
            Context.SaveChanges();
        }


        public void AddCashDeposit(CashDepositInfo cashDeposit)
        {
            Context.CashDepositInfo.Add(cashDeposit);
            Context.SaveChanges();
        }
        public CashDepositInfo GetCashDepositByShopId(long shopId)
        {
            return Context.CashDepositInfo.Where(item => item.ShopId == shopId).FirstOrDefault();
        }

        public Decimal GetNeedPayCashDepositByShopId(long shopId)
        {
            decimal needCashDeposit = 0.00M;
            var shopService = ServiceProvider.Instance<IShopService>.Create;
            var shopCategoryService = ServiceProvider.Instance<IShopCategoryService>.Create;

            var categories = shopCategoryService.GetBusinessCategory(shopId).ToList();
            var mainCategories = categories.Where(item => item.ParentCategoryId == 0).Select(item => item.Id);

            var cashDeposits = Context.CategoryCashDepositInfo.FindBy(item => mainCategories.Contains(item.CategoryId));
            decimal needCashDeposits = -1;
            if (cashDeposits.Count() > 0)
            {
                needCashDeposits = Context.CategoryCashDepositInfo.FindBy(item => mainCategories.Contains(item.CategoryId)).Max(item => item.NeedPayCashDeposit);
            }
            var cashDeposit = Context.CashDepositInfo.Where(item => item.ShopId == shopId).FirstOrDefault();
            if (cashDeposit != null && cashDeposit.CurrentBalance < needCashDeposits)
            {
                needCashDeposit = needCashDeposits - cashDeposit.CurrentBalance;
            }
            if (cashDeposit == null)
            {
                needCashDeposit = needCashDeposits;
            }
            return needCashDeposit;
        }

        public void UpdateEnableLabels(long id, bool enableLabels)
        {
            var cashDeposits = Context.CashDepositInfo.FindById(id);
            cashDeposits.EnableLabels = enableLabels;
            Context.SaveChanges();
        }

        #region 类目保证金
        public IEnumerable<CategoryCashDepositInfo> GetCategoryCashDeposits()
        {
            IEnumerable<CategoryCashDepositInfo> result = null;
            result = Context.CategoryCashDepositInfo.Include("CategoriesInfo").DefaultIfEmpty();
            return result;
        }

        public void AddCategoryCashDeposits(CategoryCashDepositInfo model)
        {
            Context.CategoryCashDepositInfo.Add(model);
            Context.SaveChanges();
        }
        public void DeleteCategoryCashDeposits(long categoryId)
        {
            var categoryCashDeposits = Context.CategoryCashDepositInfo.Where(item => item.CategoryId == categoryId).FirstOrDefault();
            if (categoryCashDeposits != null)
            {
                Context.CategoryCashDepositInfo.Remove(categoryCashDeposits);
                Context.SaveChanges();
            }
        }

        public void UpdateNeedPayCashDeposit(long categoryId, decimal cashDeposit)
        {
            var categoryCashDeposit = Context.CategoryCashDepositInfo.Where(item => item.CategoryId == categoryId).FirstOrDefault();
            categoryCashDeposit.NeedPayCashDeposit = cashDeposit;
            Context.SaveChanges();
        }
        public void OpenNoReasonReturn(long categoryId)
        {
            var categoryCashDeposit = Context.CategoryCashDepositInfo.Where(item => item.CategoryId == categoryId).FirstOrDefault();
            categoryCashDeposit.EnableNoReasonReturn = true;
            Context.SaveChanges();
        }
        public void CloseNoReasonReturn(long categoryId)
        {
            var categoryCashDeposit = Context.CategoryCashDepositInfo.Where(item => item.CategoryId == categoryId).FirstOrDefault();
            categoryCashDeposit.EnableNoReasonReturn = false;
            Context.SaveChanges();
        }

        public CashDepositsObligation GetCashDepositsObligation(long productId)
        {
            CashDepositsObligation cashDepositsObligation = new CashDepositsObligation()
            {
                IsCustomerSecurity = false,
                IsSevenDayNoReasonReturn = false,
                IsTimelyShip = false
            };
            var productService = ServiceProvider.Instance<IProductService>.Create;
            var shopService = ServiceProvider.Instance<IShopService>.Create;
            var shopCategoryService = ServiceProvider.Instance<IShopCategoryService>.Create;
            var categoryService = ServiceProvider.Instance<ICategoryService>.Create;


            var product = productService.GetProduct(productId);
            var shop = shopService.GetShop(product.ShopId);
            var cashDeposit = Context.CashDepositInfo.Where(item => item.ShopId == shop.Id).FirstOrDefault();
            var categories = shopCategoryService.GetBusinessCategory(shop.Id).ToList();
            var mainCategories = categories.Where(item => item.ParentCategoryId == 0).Select(item => item.Id);
            var cateCashDeposit = Context.CategoryCashDepositInfo.FindBy(item => mainCategories.Contains(item.CategoryId)).ToList();
            if (cateCashDeposit.Count==0)
            {
                return cashDepositsObligation;
            }
            var needCashDeposit = cateCashDeposit.Max(item => item.NeedPayCashDeposit);

            //平台自营，商家缴纳足够保证金或者平台未取消其资质资格
            if (shop.IsSelf || (cashDeposit != null && cashDeposit.CurrentBalance >= needCashDeposit) || (cashDeposit != null && cashDeposit.CurrentBalance < needCashDeposit && cashDeposit.EnableLabels == true))
            {
                List<long> categoryIds = new List<long>();
                categoryIds.Add(product.CategoryId);
                var mainCategory = categoryService.GetTopLevelCategories(categoryIds).FirstOrDefault();
                var categoryCashDepositInfo = Context.CategoryCashDepositInfo.Where(item => item.CategoryId == mainCategory.Id).FirstOrDefault();
                cashDepositsObligation.IsSevenDayNoReasonReturn = categoryCashDepositInfo.EnableNoReasonReturn;
                cashDepositsObligation.IsCustomerSecurity = true;
                //设置了运费模板
                if (product.Himall_FreightTemplate != null)
                {
                    if (!string.IsNullOrEmpty(product.Himall_FreightTemplate.SendTime))
                        cashDepositsObligation.IsTimelyShip = true;
                }

            }
            return cashDepositsObligation;
        }
        #endregion
    }
}
