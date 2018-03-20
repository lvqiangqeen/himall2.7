using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Himall.Entity;
using Himall.Core;
using Himall.IServices.QueryModel;
using System.Data.Entity;
using System.Text;
using System.Data.Common;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;
using Himall.CommonModel;
using MySql.Data.MySqlClient;
using Dapper;

namespace Himall.Service
{
    public class DistributionService : ServiceBase, IDistributionService
    {
        public void UpdateRecruitmentSetting(RecruitSettingInfo model)
        {

            var m = Context.RecruitSettingInfo.FirstOrDefault();
            if (m != null)
            {
                m.MustAddress = model.MustAddress;
                m.MustMobile = model.MustMobile;
                m.MustRealName = model.MustRealName;
                m.Enable = model.Enable;
            }
            else
            {
                Context.RecruitSettingInfo.Add(model);
            }
            Context.SaveChanges();
        }

        public RecruitSettingInfo GetRecruitmentSetting()
        {
            var m = Context.RecruitSettingInfo.FirstOrDefault();
            return m;
        }

        public void UpdateRecruitmentPlan(RecruitPlanInfo model)
        {
            var m = Context.RecruitPlanInfo.FirstOrDefault();
            if (m != null)
            {
                m.Content = model.Content;
                m.Title = model.Title;
            }
            else
            {
                Context.RecruitPlanInfo.Add(model);
            }
            Context.SaveChanges();
        }

        public RecruitPlanInfo GetRecruitmentPlan()
        {
            var m = Context.RecruitPlanInfo.FirstOrDefault();
            return m;
        }

        public void UpdateDistributorSetting(DistributorSettingInfo model)
        {
            var m = Context.DistributorSettingInfo.FirstOrDefault();
            if (m != null)
            {
                m.PromoterRule = model.PromoterRule;
                m.SellerRule = model.SellerRule;
                m.Enable = model.Enable;
                m.DisBanner = model.DisBanner;

            }
            else
            {
                Context.DistributorSettingInfo.Add(model);
            }
            if (m != null)
            //转移图片
            {
                if (!string.IsNullOrWhiteSpace(model.DisBanner))
                    m.DisBanner = MoveImages(model.DisBanner);

            }
            else
            {
                if (!string.IsNullOrWhiteSpace(model.DisBanner))
                    model.DisBanner = MoveImages(model.DisBanner);
            }
            Context.SaveChanges();
        }

        public DistributorSettingInfo GetDistributionSetting()
        {
            var m = Context.DistributorSettingInfo.FirstOrDefault();
            return m;
        }


        public ObsoletePageModel<PromoterInfo> GetPromoterList(PromoterQuery query)
        {
            ObsoletePageModel<PromoterInfo> model = new ObsoletePageModel<PromoterInfo>();
            var sql = Context.PromoterInfo.AsQueryable();
            if (!string.IsNullOrWhiteSpace(query.UserName))
            {
                sql = sql.Where(a => a.Himall_Members.UserName.Contains(query.UserName));
            }
            if (query.Status.HasValue)
            {
                sql = sql.Where(a => a.Status == query.Status.Value);
            }
            var orderby = sql.GetOrderBy(a => a.OrderByDescending(b => b.ApplyTime));
            var total = 0;
            var list = sql.GetPage(out total, query.PageNo, query.PageSize, orderby);
            model.Models = list;
            model.Total = total;
            return model;
        }

        /// <summary>
        /// 清退分销员
        /// </summary>
        /// <param name="Id"></param>
        public void DisablePromoter(long Id)
        {
            var promoter = Context.PromoterInfo.Where(a => a.Id == Id).FirstOrDefault();
            if (promoter == null)
            {
                throw new HimallException("不存在此分销员");
            }
            promoter.Status = PromoterInfo.PromoterStatus.NotAvailable;
            promoter.PassTime = DateTime.Now;
            Context.SaveChanges();
        }

        public void AduitPromoter(long Id)
        {
            var promoter = Context.PromoterInfo.Where(a => a.Id == Id).FirstOrDefault();
            if (promoter == null)
            {
                throw new HimallException("不存在此分销员");
            }
            promoter.Status = PromoterInfo.PromoterStatus.Audited;
            promoter.PassTime = DateTime.Now;
            Context.SaveChanges();
        }

        public void RefusePromoter(long Id)
        {
            var promoter = Context.PromoterInfo.Where(a => a.Id == Id).FirstOrDefault();
            if (promoter == null)
            {
                throw new HimallException("不存在此分销员");
            }
            promoter.Status = PromoterInfo.PromoterStatus.Refused;
            Context.SaveChanges();
        }


        public ObsoletePageModel<ProductsDistributionModel> GetDistributionlist(DistributionQuery query)
        {
            var sql = Context.ProductBrokerageInfo.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.ShopName))
            {
                sql = sql.Where(a => a.Himall_Shops.ShopName.Contains(query.ShopName));
            }
            if (!string.IsNullOrWhiteSpace(query.ProductName))
            {
                sql = sql.Where(a => a.Himall_Products.ProductName.Contains(query.ProductName));
            }
            if (query.Status.HasValue)
            {
                sql = sql.Where(a => a.Status == query.Status.Value);
            }
            if (query.ShopId.HasValue)
            {
                sql = sql.Where(a => a.ShopId == query.ShopId.Value);
            }
            var brokeragesql = Context.BrokerageIncomeInfo.GroupBy(d => d.ProductID)
                .Select(d => new
                {
                    ProductId = d.Key,
                    NoSettledBrokerage = d.Sum(s => s.Brokerage - (s.Himall_BrokerageRefund.Sum(si => si.Brokerage) == null ? 0 : s.Himall_BrokerageRefund.Sum(si => si.Brokerage)))
                });
            var provistisql = Context.ProductVistiInfo.GroupBy(d => d.ProductId).Select(d => new
            {
                ProductId = d.Key,
                SaleAmount = d.Sum(t => t.SaleAmounts),
                SaleNum = d.Sum(t => t.SaleCounts)
            });
            var sqlquery = (from a in sql
                            join b in provistisql on a.ProductId equals b.ProductId into pvj
                            from pvjd in pvj.DefaultIfEmpty()
                            join pbbi in brokeragesql on a.ProductId equals pbbi.ProductId into pbbij
                            from pbbid in pbbij.DefaultIfEmpty()
                            select new ProductsDistributionModel()
                            {
                                Id = a.ProductId.Value,
                                AgentNum = a.AgentNum.HasValue ? a.AgentNum.Value : 0,
                                Brokerage = a.BrokerageAmount,
                                DistributionSaleAmount = a.saleAmount,
                                DistributionSaleNum = a.SaleNum.HasValue ? a.SaleNum.Value : 0,
                                SaleAmount = (pvjd == null ? 0 : pvjd.SaleAmount),
                                SaleNum = (pvjd == null ? 0 : pvjd.SaleNum),
                                ForwardNum = a.ForwardNum.HasValue ? a.ForwardNum.Value : 0,
                                ShopName = a.Himall_Shops.ShopName,
                                ProductName = a.Himall_Products.ProductName,
                                Sort = a.Sort.HasValue ? a.Sort.Value : 0,
                                NoSettledBrokerage = (pbbid == null ? 0 : pbbid.NoSettledBrokerage),
                                Status = a.Status
                            });
            var total = 0;
            sqlquery = sqlquery.GetPage(out total, d => d.OrderBy(item => item.Sort).ThenByDescending(a => a.Id), query.PageNo, query.PageSize);
            ObsoletePageModel<ProductsDistributionModel> model = new ObsoletePageModel<ProductsDistributionModel>() { Models = sqlquery, Total = total };
            return model;
        }


        /// <summary>
        /// 更新分销商品排序
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="sort"></param>
        public void UpdateProductsDistributionOrder(long productId, int sort)
        {
            var model = Context.ProductBrokerageInfo.Where(a => a.ProductId == productId).FirstOrDefault();
            if (model == null)
            {
                throw new HimallException("不存在该分销商品");
            }
            model.Sort = sort;

            Context.SaveChanges();
        }

        public ObsoletePageModel<ProformanceModel> GetPerformanceList(ProformanceQuery query)
        {
            ObsoletePageModel<ProformanceModel> model = new ObsoletePageModel<ProformanceModel>();
            var timeQuery = "";
            if (query.startTime.HasValue)
            {
                timeQuery += (" and CreateTime>=@StartTime");
            };
            if (query.endTime.HasValue)
            {
                timeQuery += (" and CreateTime<=@Endtime");
            };
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT userName,m.Id,IFNULL(gtbl1.unpaid,0) as UnPaid,IFNULL(gtbl2.paid,0) as Paid,IFNULL(gtbl3.bcount,0) as TotalNumber,IFNULL(gtbl3.bsum,0) as TotalTurnover");
            sb.Append(" FROM Himall_Promoter as mp join Himall_Members as m on m.Id=mp.UserId");
            sb.Append(" LEFT JOIN(select UserId,sum(Brokerage) as unpaid from   Himall_BrokerageIncome where `Status`=0");
            sb.Append(timeQuery);
            sb.Append(" GROUP BY UserId) as gtbl1 on m.Id=gtbl1.UserId");
            sb.Append(" LEFT JOIN(select UserId,sum(Brokerage) as paid from Himall_BrokerageIncome where `Status`=1");
            sb.Append(timeQuery);
            sb.Append(" GROUP BY UserId) as gtbl2 on m.Id=gtbl2.UserId");
            sb.Append(" LEFT JOIN(SELECT userId,sum(TotalPrice) bsum,COUNT(id) bcount FROM Himall_BrokerageIncome where 1=1");
            sb.Append(timeQuery);
            sb.Append("  GROUP BY UserId) as gtbl3 on m.Id=gtbl3.UserId");
            if (query.UserId.HasValue && query.UserId.Value != 0)
            {
                sb.Append(" WHERE m.id=@UserId");
            }
            sb.Append(" order by TotalTurnover  desc");
            var start = (query.PageNo - 1) * query.PageSize;
            var end = query.PageNo * query.PageSize;
            sb.Append(" limit " + start + "," + query.PageSize);
            var args = new DbParameter[] { 
                       new MySqlParameter("UserId", query.UserId),
                      new MySqlParameter("StartTime", query.startTime),
                      new MySqlParameter("Endtime", query.endTime),
               };

            var list = Context.Database.SqlQuery<ProformanceModel>(sb.ToString(), args).ToList();

            model.Models = list.AsQueryable();

            var count = 0;
            if (query.UserId.HasValue && query.UserId.Value != 0)
                count = Context.PromoterInfo.Where(a => a.UserId == query.UserId.Value).Count();
            else
                count = Context.PromoterInfo.Count();
            model.Total = count;
            return model;
        }

        public ObsoletePageModel<UserProformanceModel> GetPerformanceDetail(UserProformanceQuery query)
        {
            var sql = Context.BrokerageIncomeInfo.Where(a => a.UserId == query.UserId.Value);
            if (query.OrderId.HasValue && query.OrderId.Value != 0)
            {
                sql = sql.Where(a => a.OrderId == query.OrderId.Value);
            }
            if (query.startTime.HasValue)
            {
                sql = sql.Where(a => a.OrderTime >= query.startTime.Value);
            }
            if (query.endTime.HasValue)
            {
                sql = sql.Where(a => a.OrderTime <= query.endTime.Value);
            }
            var model = sql.Join(Context.OrderItemInfo, a => a.OrderItemId, b => b.Id, (a, b) => new
                UserProformanceModel
                {
                    Brokerage = a.Brokerage,
                    RealTotalPrice = b.RealTotalPrice,
                    OrderStatus = b.OrderInfo.OrderStatus,
                    ProductName = a.ProductName,
                    UserId = a.UserId,
                    UserName = b.OrderInfo.UserName,
                    Id = b.OrderId,
                    FinshedTime = b.OrderInfo.FinishDate,
                    OrderTime = b.OrderInfo.OrderDate
                }
                );
            var total = 0;
            model = model.GetPage(out total, query.PageNo, query.PageSize, d => d.OrderByDescending(item => item.OrderTime));
            ObsoletePageModel<UserProformanceModel> pageModel = new ObsoletePageModel<UserProformanceModel>() { Models = model, Total = total };
            return pageModel;
        }

        #region 商家分销设置
        /// <summary>
        /// 获取商家分销设置
        /// </summary>
        /// <returns></returns>
        public ShopDistributorSettingInfo getShopDistributorSettingInfo(long shopid)
        {
            ShopDistributorSettingInfo result = new ShopDistributorSettingInfo();
            result = Context.ShopDistributorSettingInfo.FirstOrDefault(d => d.ShopId == shopid);
            if (result == null)
            {
                result = new ShopDistributorSettingInfo();
                result.ShopId = shopid;
                result.DistributorDefaultRate = 0;
                Context.ShopDistributorSettingInfo.Add(result);
                Context.SaveChanges();
            }
            else
            {
                if (result.DistributorDefaultRate > 99)
                {
                    result.DistributorDefaultRate = 0;    //错误佣金比置零
                    Context.SaveChanges();
                }
            }
            return result;
        }
        /// <summary>
        /// 商家聚合页推广设置
        /// </summary>
        /// <param name="model"></param>
        public void UpdateShopDistributor(ShopDistributorSettingInfo model)
        {
            ShopDistributorSettingInfo config = getShopDistributorSettingInfo(model.ShopId);
            config.DistributorShareLogo = model.DistributorShareLogo;
            config.DistributorShareName = model.DistributorShareName;
            config.DistributorShareContent = model.DistributorShareContent;
            Context.SaveChanges();
        }
        /// <summary>
        /// 设置商家默认分佣比例
        /// </summary>
        /// <param name="rate"></param>
        public void UpdateDefaultBrokerage(decimal rate, long shopid)
        {
            string _str = rate.ToString();
            //验证格式
            if (!System.Text.RegularExpressions.Regex.IsMatch(_str, @"^\d{1,2}(\.\d)?$"))
            {
                throw new HimallException("错误的数据格式，只可以保留一位小数");
            }
            ShopDistributorSettingInfo config = getShopDistributorSettingInfo(shopid);
            config.DistributorDefaultRate = rate;
            Context.SaveChanges();
        }
        #endregion

		/// <summary>
		/// 是否为分销员
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public bool IsPromoter(long userId)
		{
			return this.Context.PromoterInfo.Exist(p => p.UserId == userId && (p.Status == PromoterInfo.PromoterStatus.Audited || p.Status == PromoterInfo.PromoterStatus.NotAvailable));
		}

        /// <summary>
        /// 获取当前所有分销商品编号
        /// </summary>
        /// <param name="shopid"></param>
        /// <returns></returns>
        public List<long> GetAllDistributionProductIds(long shopid)
        {
            List<long> result = new List<long>();
            result = Context.ProductBrokerageInfo
                .Where(d => d.ShopId == shopid && d.Status == ProductBrokerageInfo.ProductBrokerageStatus.Normal)
                .Select(d => d.ProductId.Value)
                .ToList();
            return result;
        }
        /// <summary>
        /// 批量添加分销商品
        /// </summary>
        /// <param name="productids"></param>
        /// <param name="shopid"></param>
        /// <param name="rate"></param>
        public void BatAddDistributionProducts(IEnumerable<long> productids, long shopid, decimal rate)
        {
            if (productids.Count() < 1)
            {
                throw new HimallException("错误的商品编号");
            }
            if (shopid < 0)
            {
                throw new HimallException("错误的店铺编号");
            }
            if (rate < 0.1m)
            {
                throw new HimallException("错误的佣金比");
            }
            var _ser_pro = ServiceProvider.Instance<IProductService>.Create;
            List<ProductInfo> prodata = _ser_pro.GetProductByIds(productids)
                .Where(d => d.AuditStatus == ProductInfo.ProductAuditStatus.Audited && d.SaleStatus == ProductInfo.ProductSaleStatus.OnSale)
                .ToList();
            List<ProductBrokerageInfo> datalist = new List<ProductBrokerageInfo>();
            bool isNeedCommit = false;
            foreach (var item in prodata)
            {
                //仅添加与修改
                ProductBrokerageInfo data = Context.ProductBrokerageInfo.FirstOrDefault(d => d.ProductId == item.Id && d.ShopId == shopid);
                if (data == null)
                {
                    data = new ProductBrokerageInfo();
                    data.ProductId = item.Id;
                    data.ShopId = shopid;
                    data.rate = rate;
                    data.AgentNum = 0;
                    data.SaleNum = 0;
                    data.Sort = 0;
                    data.ForwardNum = 0;
                    data.Status = ProductBrokerageInfo.ProductBrokerageStatus.Normal;
                    data.CreateTime = DateTime.Now;
                    datalist.Add(data);
                    isNeedCommit = true;
                }
                else
                {
                    data.Status = ProductBrokerageInfo.ProductBrokerageStatus.Normal;
                    isNeedCommit = true;
                }
            }
            if (isNeedCommit)
            {
                if (datalist.Count > 0)
                {
                    Context.ProductBrokerageInfo.AddRange(datalist);
                }
                Context.SaveChanges();
            }
        }

        public void AddDistributionProducts(ProductBrokerageInfo model)
        {
            throw new NotImplementedException();
        }

        public ObsoletePageModel<object> GetDistributionOrders(dynamic query)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// (批量)取消商品分销推广
        /// </summary>
        /// <param name="ProductIds"></param>
        /// <param name="shopId">店铺编号,null表示所有店铺</param>
        public void CancelDistributionProduct(IEnumerable<long> ProductIds, long? shopId = null)
        {
            var datalist = Context.ProductBrokerageInfo.Where(a => ProductIds.Contains(a.ProductId.Value));
            if (shopId != null)
            {
                datalist = datalist.Where(d => d.ShopId == shopId);
            }
            foreach (var item in datalist)
            {
                item.Status = ProductBrokerageInfo.ProductBrokerageStatus.Removed;
            }
            Context.SaveChanges();
        }

        /// <summary>
        /// （批量）设置商品的分佣比例
        /// </summary>
        /// <param name="percent"></param>
        /// <param name="ProductIds"></param>
        /// <param name="shopId"></param>
        public void SetProductBrokerage(decimal percent, IEnumerable<long> ProductIds, long? shopId = null)
        {
            string _str = percent.ToString();
            //验证格式
            if (!System.Text.RegularExpressions.Regex.IsMatch(_str, @"^\d{1,2}(\.\d)?$"))
            {
                throw new HimallException("错误的数据格式，只可以保留一位小数");
            }

            var datalist = Context.ProductBrokerageInfo.Where(a => ProductIds.Contains(a.ProductId.Value));
            if (shopId != null)
            {
                datalist = datalist.Where(d => d.ShopId == shopId);
            }
            foreach (var item in datalist)
            {
                item.rate = percent;
            }
            Context.SaveChanges();
        }

        public ObsoletePageModel<object> GetDistributionProductsDetail(dynamic query)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 获取分销用户业绩
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public DistributionUserPerformanceSetModel GetUserPerformance(long userId)
        {
            DistributionUserPerformanceSetModel result = new DistributionUserPerformanceSetModel();

            decimal? _tmpdec = 0;
            int? _tmpnum = 0;
            string _tmpsql = "";
            string _exesql = "";
            StringBuilder _tmpsb = new StringBuilder();
            DateTime _tmptime = DateTime.Now.AddDays(-7).Date;

            #region 收益数据
            _tmpsb.Clear();
            _tmpsb.Append(" SELECT SUM((t0.Brokerage-IFNULL(t1.Brokerage,0))) AS sumnum ");
            _tmpsb.Append(" FROM Himall_BrokerageIncome AS t0 ");
            _tmpsb.Append(" left JOIN Himall_BrokerageRefund as t1 on t0.Id=t1.IncomeId ");
            _tmpsb.Append(" WHERE (t0.UserId = @UserId {otherwhere}) ");
            _tmpsb.Append(" GROUP BY t0.UserId ");
            //初始条件
            DbParameter[] args = new DbParameter[] { 
                       new MySqlParameter("UserId", userId)
               };

            #region 七天内数据
            _tmpsql = _tmpsb.ToString();   //初始sql
            _tmpsql = _tmpsql.Replace("{otherwhere}", " and t0.OrderTime>=@OrderTime {otherwhere}");  //公共条件代入
            _tmptime = DateTime.Now.AddDays(-7).Date;
            DbParameter[] args1 = new DbParameter[] { 
                      new MySqlParameter("OrderTime", _tmptime)
               };
            args1 = args.Concat(args1).ToArray();

            #region 七天总收益
            _exesql = _tmpsql.Replace("{otherwhere}", "");  //其他条件代入
            _tmpdec = Context.Database.SqlQuery<decimal>(_exesql, args1).FirstOrDefault();
            if (_tmpdec.HasValue)
            {
                result.Day7SumIncome = _tmpdec.Value;
            }
            #endregion

            #region 七天已结收益
            _exesql = _tmpsql.Replace("{otherwhere}", " and Status=" + (((int)BrokerageIncomeInfo.BrokerageStatus.Settled).ToString()) + " ");  //其他条件代入
            _tmpdec = Context.Database.SqlQuery<decimal>(_exesql, args1).FirstOrDefault();
            if (_tmpdec.HasValue)
            {
                result.Day7Settled = _tmpdec.Value;
            }
            #endregion

            #region 七天未结收益
            _exesql = _tmpsql.Replace("{otherwhere}", " and Status!=" + (((int)BrokerageIncomeInfo.BrokerageStatus.Settled).ToString()) + " ");  //其他条件代入
            _tmpdec = Context.Database.SqlQuery<decimal>(_exesql, args1).FirstOrDefault();
            if (_tmpdec.HasValue)
            {
                result.Day7NoSettled = _tmpdec.Value;
            }
            #endregion

            #endregion

            #region 一个月数据
            _tmpsql = _tmpsb.ToString();   //初始sql
            _tmpsql = _tmpsql.Replace("{otherwhere}", " and t0.OrderTime>=@OrderTime {otherwhere}");  //公共条件代入
            _tmptime = DateTime.Now.AddDays(-(DateTime.Now.Day - 1)).Date;
            DbParameter[] args2 = new DbParameter[] { 
                      new MySqlParameter("OrderTime", _tmptime)
               };
            args2 = args.Concat(args2).ToArray();

            #region 一个月总收益
            _exesql = _tmpsql.Replace("{otherwhere}", "");  //其他条件代入
            _tmpdec = Context.Database.SqlQuery<decimal>(_exesql, args2).FirstOrDefault();
            if (_tmpdec.HasValue)
            {
                result.MonthSumIncome = _tmpdec.Value;
            }
            #endregion

            #region 一个月已结收益
            _exesql = _tmpsql.Replace("{otherwhere}", " and Status=" + (((int)BrokerageIncomeInfo.BrokerageStatus.Settled).ToString()) + " ");  //其他条件代入
            _tmpdec = Context.Database.SqlQuery<decimal>(_exesql, args2).FirstOrDefault();
            if (_tmpdec.HasValue)
            {
                result.MonthSumSettled = _tmpdec.Value;
            }
            #endregion

            #region 一个月未结收益
            _exesql = _tmpsql.Replace("{otherwhere}", " and Status!=" + (((int)BrokerageIncomeInfo.BrokerageStatus.Settled).ToString()) + " ");  //其他条件代入
            _tmpdec = Context.Database.SqlQuery<decimal>(_exesql, args2).FirstOrDefault();
            if (_tmpdec.HasValue)
            {
                result.MonthSumNoSettled = _tmpdec.Value;
            }
            #endregion

            #endregion

            #region 累积总数
            _tmpsql = _tmpsb.ToString();   //初始sql

            #region 累积总收益
            _exesql = _tmpsql.Replace("{otherwhere}", "");  //其他条件代入
            _tmpdec = Context.Database.SqlQuery<decimal>(_exesql, args).FirstOrDefault();
            if (_tmpdec.HasValue)
            {
                result.SumIncome = _tmpdec.Value;
            }
            #endregion

            #region 累积已结收益
            _exesql = _tmpsql.Replace("{otherwhere}", " and Status=" + (((int)BrokerageIncomeInfo.BrokerageStatus.Settled).ToString()) + " ");  //其他条件代入
            _tmpdec = Context.Database.SqlQuery<decimal>(_exesql, args).FirstOrDefault();
            if (_tmpdec.HasValue)
            {
                result.SumSettled = _tmpdec.Value;
            }
            #endregion

            #region 累积未结收益
            _exesql = _tmpsql.Replace("{otherwhere}", " and Status!=" + (((int)BrokerageIncomeInfo.BrokerageStatus.Settled).ToString()) + " ");  //其他条件代入
            _tmpdec = Context.Database.SqlQuery<decimal>(_exesql, args).FirstOrDefault();
            if (_tmpdec.HasValue)
            {
                result.SumNoSettled = _tmpdec.Value;
            }
            #endregion

            #endregion
            #endregion

            //成交数
            _tmpnum = Context.BrokerageIncomeInfo.Where(d => d.UserId == userId).Count();
            if (_tmpnum.HasValue)
            {
                result.SumOrderCount = _tmpnum.Value;
            }

            //月成交数
            _tmptime = DateTime.Now.AddDays(-(DateTime.Now.Day - 1)).Date;
            _tmpnum = Context.BrokerageIncomeInfo.Where(d => d.UserId == userId && d.CreateTime >= _tmptime).Count();
            if (_tmpnum.HasValue)
            {
                result.MonthSumOrderCount = _tmpnum.Value;
            }

            //去重求累积客户数
            _tmpnum = Context.BrokerageIncomeInfo.Where(d => d.UserId == userId).Select(d => new { d.BuyerUserId }).Distinct().Count();
            if (_tmpnum.HasValue)
            {
                result.SumCustomer = _tmpnum.Value;
            }

            if (result.SumCustomer > 0)
            {
                //月新增客户数
                _tmptime = DateTime.Now.AddDays(-(DateTime.Now.Day - 1)).Date;
                _tmpnum = Context.BrokerageIncomeInfo.Where(d => d.UserId == userId && d.OrderTime < _tmptime).Select(d => new { d.BuyerUserId }).Distinct().Count();
                if (!_tmpnum.HasValue)
                {
                    _tmpnum = 0;
                }
                result.MonthNewCustomer = result.SumCustomer - _tmpnum.Value;
            }

            return result;
        }

        public ObsoletePageModel<DistributionFeatModel> GetUserBillList(DistributionUserBillQuery query)
        {
            if (query == null)
            {
                throw new HimallException("错误的搜索条件");
            }
            ObsoletePageModel<DistributionFeatModel> result = new ObsoletePageModel<DistributionFeatModel>();
            List<DistributionFeatModel> datalist = new List<DistributionFeatModel>();
            var sql = (from din in Context.BrokerageIncomeInfo
                       join dr in Context.OrderItemInfo.Include(_td => _td.OrderInfo) on din.OrderItemId equals dr.Id
                       join du in Context.UserMemberInfo on din.UserId equals du.Id
                       join dref in Context.BrokerageRefundInfo on din.Id equals dref.IncomeId into dirj
                       from dj in dirj.DefaultIfEmpty()
                       select new DistributionFeatModel
                       {
                           Id = din.Id,
                           Brokerage = din.Brokerage,
                           OrderId = din.OrderId,
                           OrderItemId = din.OrderItemId,
                           ShopId = din.ShopId,
                           BuyUserId = din.BuyerUserId,
                           ProductId = din.ProductID,
                           ProductName = din.ProductName,
                           SkuId = din.SkuID,
                           SkuInfo = din.SkuInfo,
                           SettleState = din.Status,
                           SettleTime = din.SettlementTime,
                           SalesUserId = din.UserId,
                           OrderTime = din.OrderTime,
                           CreateTime = din.CreateTime,
                           SalesName = du.UserName,
                           OrderState = dr.OrderInfo.OrderStatus,
                           LastRightsTime = dr.OrderInfo.FinishDate,
                           ShippingDate = dr.OrderInfo.ShippingDate,
                           OrderItemPrice = dr.RealTotalPrice,
                           RefundPrice = (dj != null ? dj.RefundAmount : 0),
                           RefundBrokerage = (dj != null ? dj.Brokerage : 0),
                           RefundTime = (dj != null ? dj.RefundTime : null)
                       });

            if (query.UserId.HasValue)
            {
                sql = sql.Where(d => d.SalesUserId == query.UserId.Value);
            }

            if (query.ShopId.HasValue)
            {
                sql = sql.Where(d => d.ShopId == query.ShopId.Value);
            }

            if (query.OrderState.HasValue)
            {
                sql = sql.Where(d => d.OrderState == query.OrderState.Value);
            }
            if (query.OrderId.HasValue)
            {
                sql = sql.Where(d => d.OrderId == query.OrderId.Value);
            }

            if (query.StartTime.HasValue)
            {
                sql = sql.Where(d => d.OrderTime >= query.StartTime.Value);
            }
            if (query.EndTime.HasValue)
            {
                query.EndTime = query.EndTime.Value.AddDays(1).Date.AddMinutes(-1);
                sql = sql.Where(d => d.OrderTime <= query.EndTime.Value);
            }

            if (query.SettleState.Count() > 0)
            {
                sql = sql.Where(d => query.SettleState.Contains(d.SettleState));
            }

            int total = 0;
            var datasql = sql.GetPage(out total, d => d.OrderByDescending(o => o.CreateTime), query.PageNo, query.PageSize);
            datalist = datasql.ToList();
            if (datalist.Count() > 0)
            {
                //补充产品图片
                List<long> proids = datalist.Select(d => d.ProductId).ToList();
                List<ProductInfo> prolist = Context.ProductInfo.Where(d => proids.Contains(d.Id)).ToList();
                foreach (var item in datalist)
                {
                    ProductInfo _prodata = prolist.Find(d => d.Id == item.ProductId);
                    if (_prodata != null)
                    {
                        item.ProductImage = _prodata.GetImage(ImageSize.Size_350);
                    }
                }
            }
            datasql = datalist.AsQueryable();
            result.Models = datasql;
            result.Total = total;
            return result;
        }

        public object GetUserBillDetail(long billId)
        {
            throw new NotImplementedException();
        }

        public ObsoletePageModel<object> GetUserAgentProducts(long userId)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 获取productIds中可以代理的商品编号
        /// </summary>
        /// <param name="productIds">商品编号范围</param>
        /// <param name="userId"></param>
        /// <returns>可以代理的商品编号</returns>
        public IEnumerable<long> GetCanAgentProductId(IEnumerable<long> productIds, long userId)
        {
            List<long> result = new List<long>();
            if (productIds != null)
            {
                if (productIds.Count() > 0)
                {
                    List<long> hasAgentIds = Context.AgentProductsInfo.Where(d => productIds.Contains(d.ProductId) && d.UserId == userId).Select(d => d.ProductId).ToList();
                    if (hasAgentIds.Count > 0)
                    {
                        result = productIds.Where(d => !hasAgentIds.Contains(d)).ToList();
                    }
                    else
                    {
                        result = productIds.ToList();
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// （批量）移除用户代理的商品
        /// </summary>
        /// <param name="ProductIds"></param>
        /// <param name="userId"></param>
        public void RemoveAgentProducts(IEnumerable<long> productIds, long userId)
        {
            var list = Context.AgentProductsInfo.Where(a => a.UserId == userId && productIds.Contains(a.ProductId));
            Context.AgentProductsInfo.RemoveRange(list);
            Context.SaveChanges();
            //处理代理量
            string ids = "";
            int _n = 0;
            foreach (var item in productIds)
            {
                if (_n > 0)
                {
                    ids += ",";
                }
                ids += item.ToString();
                _n++;
            }
            Context.Database.ExecuteSqlCommand("update Himall_ProductBrokerage set AgentNum=AgentNum-1 where ProductId in(" + ids + ")");
        }

        public void AddAgentProducts(IEnumerable<long> productIds, long userId)
        {
            var products = Context.ProductBrokerageInfo.Where(a => productIds.Contains(a.ProductId.Value)).ToList();

            foreach (var t in products)
            {
                if (!Context.AgentProductsInfo.Any(d => d.ProductId == t.ProductId && d.UserId == userId))
                {
                    AgentProductsInfo agent = new AgentProductsInfo();
                    agent.ProductId = t.ProductId.Value;
                    agent.ShopId = t.ShopId.Value;
                    agent.UserId = userId;
                    agent.AddTime = DateTime.Now;
                    Context.AgentProductsInfo.Add(agent);

                    //增加代理数
                    t.AgentNum += 1;
                }
            }
            Context.SaveChanges();
        }

        public void AddUserDistribution(BrokerageIncomeInfo model)
        {
            model.CreateTime = DateTime.Now;
            model.SettlementTime = null;
            model.Status = BrokerageIncomeInfo.BrokerageStatus.NotSettled;
            Context.BrokerageIncomeInfo.Add(model);
            Context.SaveChanges();
        }
        /// <summary>
        /// 添加分销退款
        /// </summary>
        /// <param name="model"></param>
        public void AddDistributionRefund(long OrderItemId, decimal RefundPrice, decimal Brokerage, long RefundId)
        {
            OrderItemInfo orderitemdata = Context.OrderItemInfo.FirstOrDefault(d => d.Id == OrderItemId);
            BrokerageIncomeInfo info = Context.BrokerageIncomeInfo.FirstOrDefault(d => d.OrderItemId == OrderItemId);
            if (info != null && orderitemdata != null)
            {
                if (info.Status != BrokerageIncomeInfo.BrokerageStatus.Settled)
                {
                    if (info.Himall_BrokerageRefund.Count < 1)
                    {
                        BrokerageRefundInfo data = new BrokerageRefundInfo();
                        data.IncomeId = info.Id;
                        data.RefundAmount = RefundPrice;
                        data.RefundTime = DateTime.Now;
                        data.RefundId = RefundId;
                        data.Brokerage = Brokerage;
                        Context.BrokerageRefundInfo.Add(data);
                        info.Status = BrokerageIncomeInfo.BrokerageStatus.NotAvailable;
                        Context.SaveChanges();
                    }
                }
            }
        }
        /// <summary>
        /// 修改分销退款
        /// </summary>
        /// <param name="model"></param>
        public void UpdateDistributionRefund(long OrderItemId, decimal RefundPrice, decimal Brokerage, long RefundId)
        {
            OrderItemInfo orderitemdata = Context.OrderItemInfo.FirstOrDefault(d => d.Id == OrderItemId);
            BrokerageIncomeInfo info = Context.BrokerageIncomeInfo.FirstOrDefault(d => d.OrderItemId == OrderItemId);
            if (info != null && orderitemdata != null)
            {
                if (info.Status != BrokerageIncomeInfo.BrokerageStatus.Settled)
                {
                    if (info.Himall_BrokerageRefund.Count > 0)
                    {
                        BrokerageRefundInfo data = info.Himall_BrokerageRefund.FirstOrDefault(d => d.RefundId == RefundId);
                        data.IncomeId = info.Id;
                        data.RefundAmount = RefundPrice;
                        data.RefundTime = DateTime.Now;
                        data.RefundId = RefundId;
                        data.Brokerage = Brokerage;
                        info.Status = BrokerageIncomeInfo.BrokerageStatus.NotAvailable;
                        Context.SaveChanges();
                    }
                    else
                    {
                        AddDistributionRefund(OrderItemId, RefundPrice, Brokerage, RefundId);
                    }
                }
            }
        }
        /// <summary>
        /// 关闭分销退款
        /// </summary>
        /// <param name="OrderItemId"></param>
        public void CloseDistributionRefund(long OrderItemId)
        {
            BrokerageIncomeInfo info = Context.BrokerageIncomeInfo.FirstOrDefault(d => d.OrderItemId == OrderItemId);
            if (info != null)
            {
                if (info.Status != BrokerageIncomeInfo.BrokerageStatus.Settled)
                {
                    info.Status = BrokerageIncomeInfo.BrokerageStatus.NotSettled;
                    var datalist = info.Himall_BrokerageRefund.ToList();
                    Context.BrokerageRefundInfo.RemoveRange(datalist);
                    Context.SaveChanges();
                }
            }
        }
        /// <summary>
        /// 完成分销退款
        /// </summary>
        /// <param name="OrderItemId"></param>
        public void OverDistributionRefund(long OrderItemId, decimal RefundAmount, long RefundQuantity)
        {
            BrokerageIncomeInfo info = Context.BrokerageIncomeInfo.FirstOrDefault(d => d.OrderItemId == OrderItemId);
            if (info != null)
            {
                if (info.Status != BrokerageIncomeInfo.BrokerageStatus.Settled)
                {
                    info.Status = BrokerageIncomeInfo.BrokerageStatus.NotSettled;

                    //处理销量
                    var pro = Context.ProductBrokerageInfo.FirstOrDefault(d => d.ProductId == info.ProductID);
                    if (pro != null)
                    {
                        pro.SaleNum -= (int)RefundQuantity;
                        pro.saleAmount -= RefundAmount;
                        var rbnum = info.Himall_BrokerageRefund.Sum(d => d.Brokerage);
                        pro.BrokerageTotal -= (rbnum > 0 ? rbnum : 0);
                    }
                    Context.SaveChanges();
                }
            }
        }
        /// <summary>
        /// 发生退款退货行为时，改变结算状态为不可结算
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="orderItemId">订单条目ID</param>
        public void DisabledSettlement(long userId, long orderItemId)
        {
            var model = Context.BrokerageIncomeInfo.Where(a => a.UserId == userId && a.OrderItemId == orderItemId).FirstOrDefault();
            if (model != null)
                model.Status = BrokerageIncomeInfo.BrokerageStatus.NotAvailable;
            Context.SaveChanges();
        }
        /// <summary>
        /// 退款处理完成变为可结算状态
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="orderItemId"></param>
        public void EnableSettlement(long userId, long orderItemId)
        {
            var model = Context.BrokerageIncomeInfo.Where(a => a.UserId == userId && a.OrderItemId == orderItemId).FirstOrDefault();
            if (model != null)
                model.Status = BrokerageIncomeInfo.BrokerageStatus.NotSettled;
            Context.SaveChanges();
        }

        public void UserBrokerageSettlement(long userId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取所有的分销商品
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public ObsoletePageModel<ProductBrokerageInfo> GetDistributionProducts(ProductBrokerageQuery query)
        {
            ObsoletePageModel<ProductBrokerageInfo> result = new ObsoletePageModel<ProductBrokerageInfo>();

            DateTime dateMonth = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01"));
            DateTime dateWeek = DateTime.Parse(DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek).ToString("yyyy-MM-dd"));

            var monthsql = Context.BrokerageIncomeInfo.GroupBy(d => d.ProductID).Select(d => new
            {
                ProductId = d.Key,
                MonthAgent = d.Count(t => t.CreateTime > dateMonth)
            });

            var weeksql = Context.BrokerageIncomeInfo.GroupBy(d => d.ProductID).Select(d => new
            {
                ProductId = d.Key,
                WeekAgent = d.Count(t => t.CreateTime > dateWeek)
            });
            IQueryable<ProductBrokerageInfoDto> sql = (
                from p in Context.ProductInfo
                join d in Context.ProductBrokerageInfo on p.Id equals d.ProductId
                join g in monthsql on p.Id equals g.ProductId into join1
                from x in join1.DefaultIfEmpty()
                join w in weeksql on p.Id equals w.ProductId into join2
                from z in join2.DefaultIfEmpty()
                where p.IsDeleted == false && p.SaleStatus == ProductInfo.ProductSaleStatus.OnSale
                    && p.AuditStatus == ProductInfo.ProductAuditStatus.Audited
                select new ProductBrokerageInfoDto
                {
                    Id = d.Id,
                    ProductId = d.ProductId,
                    AgentNum = d.AgentNum,
                    CreateTime = d.CreateTime,
                    ForwardNum = d.ForwardNum,
                    rate = d.rate,
                    SaleNum = d.SaleNum,
                    ShopId = d.ShopId,
                    Sort = d.Sort,
                    Status = d.Status,
                    Product = p,
                    CategoryName = p.Himall_Categories.Name,
                    MonthAgent = x.MonthAgent,
                    WeekAgent = z.WeekAgent
                });

            //搜索条件
            if (query.ProductBrokerageState.HasValue)
            {
                sql = sql.Where(d => d.Status == query.ProductBrokerageState.Value);
            }

            if (query.ShopId.HasValue)
            {
                sql = sql.Where(d => d.ShopId == query.ShopId.Value);
            }

            if (query.OnlyShowNormal.HasValue)
            {
                sql = sql.Where(d => d.Product.AuditStatus == ProductInfo.ProductAuditStatus.Audited && d.Product.SaleStatus == ProductInfo.ProductSaleStatus.OnSale);
            }

            if (!string.IsNullOrWhiteSpace(query.skey))
            {
                sql = sql.Where(d => d.Product.ProductName.Contains(query.skey));
            }


            if (!string.IsNullOrEmpty(query.ProductIds))
            {
                IEnumerable<long> productIds = query.ProductIds.Split(',').Select(e => long.Parse(e));
                sql = sql.Where(d => productIds.Contains(d.ProductId.Value));
            }

            if (!string.IsNullOrEmpty(query.CategoryPathA) && query.CategoryPathA != "0")
            {
                string searchCategoryPath = "|" + query.CategoryPathA + "|";
                if (!string.IsNullOrEmpty(query.CategoryPathB)&&query.CategoryPathB!="0")
                {
                    searchCategoryPath += query.CategoryPathB + "|";
                    if (!string.IsNullOrEmpty(query.CategoryPathC)&& query.CategoryPathC != "0")
                    {
                        searchCategoryPath += query.CategoryPathC + "|";
                    }
                }
                sql = sql.Where(d => ("|" + d.Product.CategoryPath + "|").Contains(searchCategoryPath));
            }

            if (query.CategoryId.HasValue&&query.CategoryId!=0)
            {
                sql = sql.Where(d => ("|" + d.Product.CategoryPath + "|").Contains("|" + query.CategoryId.Value + "|"));
            }

            //处理代理用户
            if (query.AgentUserId.HasValue)
            {
                //TODO:DZY[151123] 需要改成join  in方式只支持2000左右个id
                List<long> proids = Context.AgentProductsInfo.Where(d => d.UserId == query.AgentUserId.Value).Select(d => d.ProductId).ToList();
                sql = sql.Where(d => proids.Contains(d.ProductId.Value));
            }

            var orderby = sql.GetOrderBy(d => d.OrderBy(o => o.Sort).ThenByDescending(o => o.CreateTime));
            switch (query.Sort)
            {
                case ProductBrokerageQuery.EnumProductSort.AgentNum:
                    orderby = sql.GetOrderBy(d => d.OrderByDescending(o => o.AgentNum).ThenBy(o => o.Sort).ThenByDescending(o => o.CreateTime));
                    break;
                case ProductBrokerageQuery.EnumProductSort.Brokerage:
                    orderby = sql.GetOrderBy(d => d.OrderByDescending(o => (o.rate * o.Product.MinSalePrice)).ThenBy(o => o.Sort).ThenByDescending(o => o.CreateTime));
                    break;
                case ProductBrokerageQuery.EnumProductSort.SalesNumber:
                    orderby = sql.GetOrderBy(d => d.OrderByDescending(o => o.SaleNum).ThenBy(o => o.Sort).ThenByDescending(o => o.CreateTime));
                    break;
                case ProductBrokerageQuery.EnumProductSort.PriceAsc:
                    orderby = sql.GetOrderBy(d => d.OrderBy(o => o.Product.MinSalePrice).ThenBy(o => o.Sort).ThenByDescending(o => o.CreateTime));
                    break;
                case ProductBrokerageQuery.EnumProductSort.PriceDesc:
                    orderby = sql.GetOrderBy(d => d.OrderByDescending(o => o.Product.MinSalePrice).ThenBy(o => o.Sort).ThenByDescending(o => o.CreateTime));
                    break;
                case ProductBrokerageQuery.EnumProductSort.MonthDesc:
                    orderby = sql.GetOrderBy(d => d.OrderByDescending(o => o.MonthAgent).ThenBy(o => o.Sort).ThenByDescending(o => o.CreateTime));
                    break;
                case ProductBrokerageQuery.EnumProductSort.WeekDesc:
                    orderby = sql.GetOrderBy(d => d.OrderByDescending(o => o.WeekAgent).ThenBy(o => o.Sort).ThenByDescending(o => o.CreateTime));
                    break;
            }

            int total = 0;
            sql = sql.GetPage(out total, query.PageNo, query.PageSize, orderby);
            List<ProductBrokerageInfoDto> datalist = sql.ToList();
            IQueryable<ProductBrokerageInfo> returnsql = datalist.Select(d => new ProductBrokerageInfo
                {
                    Id = d.Id,
                    ProductId = d.ProductId,
                    AgentNum = d.AgentNum,
                    CreateTime = d.CreateTime,
                    ForwardNum = d.ForwardNum,
                    rate = d.rate,
                    SaleNum = d.SaleNum,
                    ShopId = d.ShopId,
                    Sort = d.Sort,
                    Status = d.Status,
                    Product = d.Product,
                    CategoryName = d.CategoryName
                }).AsQueryable();
            result.Models = returnsql;
            result.Total = total;
            return result;
        }

        /// <summary>
        /// 获取分销商品信息
        /// </summary>productId
        /// <param name="id"></param>
        /// <returns></returns>
        public ProductBrokerageInfo GetDistributionProductInfo(long productId)
        {
            return Context.ProductBrokerageInfo.FirstOrDefault(d => d.ProductId == productId);
        }

        /// <summary>
        /// 批量获取分销商品信息
        /// </summary>
        /// <param name="productIds"></param>
        /// <returns></returns>
        public List<ProductBrokerageInfo> GetDistributionProductInfo(IEnumerable<long> productIds)
        {
            return Context.ProductBrokerageInfo.Where(d => productIds.Contains(d.ProductId.Value)).ToList();
        }

        /// <summary>
        /// 获取店铺列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public ObsoletePageModel<DistributionShopModel> GetShopDistributionList(DistributionShopQuery query)
        {
            ObsoletePageModel<DistributionShopModel> result = new ObsoletePageModel<DistributionShopModel>() { Total = 0 };
            var sql = Context.ProductBrokerageInfo.Include(d => d.Himall_Products)
                .Where(d => d.Himall_Products.SaleStatus == ProductInfo.ProductSaleStatus.OnSale
                    && d.Himall_Products.AuditStatus == ProductInfo.ProductAuditStatus.Audited
                    && d.Status == ProductBrokerageInfo.ProductBrokerageStatus.Normal
                    )
                    .GroupBy(d => d.ShopId).Where(d => d.Count() > 0)
                   .Select(d => new { ShopId = d.Key, SaleSum = d.Sum(item => item.SaleNum), ProductCount = d.Count() })
                   .Join(Context.ShopInfo, d => d.ShopId, s => s.Id, (d, s) => new DistributionShopModel
                   {
                       Id = s.Id,
                       ShopId = s.Id,
                       GradeId = s.GradeId,
                       ShopName = s.ShopName,
                       ShopStatus = s.ShopStatus,
                       SaleSum = d.SaleSum,
                       ProductCount = d.ProductCount
                   });
            if (!string.IsNullOrWhiteSpace(query.skey))
            {
                sql = sql.Where(d => d.ShopName.Contains(query.skey));
            }
            var orderby = sql.GetOrderBy(d => d.OrderByDescending(o => o.SaleSum));
            switch (query.Sort)
            {
                case DistributionShopQuery.EnumShopSort.SalesNumber:
                    orderby = sql.GetOrderBy(d => d.OrderByDescending(o => o.SaleSum));
                    break;
                case DistributionShopQuery.EnumShopSort.ProductNum:
                    orderby = sql.GetOrderBy(d => d.OrderByDescending(o => o.ProductCount));
                    break;
            }

            int total = 0;
            sql = sql.GetPage(out total, orderby, query.PageNo, query.PageSize);
            List<DistributionShopModel> datalist = sql.ToList();
            //数据补充
            foreach (var item in datalist)
            {
                var prolist = Context.ProductBrokerageInfo.Where(d => d.ShopId == item.Id && d.Himall_Products.SaleStatus == ProductInfo.ProductSaleStatus.OnSale
                    && d.Himall_Products.AuditStatus == ProductInfo.ProductAuditStatus.Audited
                    && d.Status == ProductBrokerageInfo.ProductBrokerageStatus.Normal).OrderByDescending(d => d.SaleNum).Take(4)
                    .ToList()
                    .Select(d =>
                    {
                        var prodata = d.Himall_Products;
                        return new DistributionProductModel
                    {
                        Id = d.Id,
                        ProductId = d.ProductId,
                        ProductName = prodata.ProductName,
                        Status = d.Status,
                        Image = prodata.GetImage(ImageSize.Size_350),
                        MinSalePrice = prodata.MinSalePrice,
                        rate = d.rate,
                    };
                    }).ToList();
                item.ProductList = prolist;
            }

            var datasql = datalist.AsQueryable();
            result.Models = datasql;
            result.Total = total;
            return result;
        }
        /// <summary>
        /// 获取店铺分销商品数量
        /// <para>仅统计可以正常购买的</para>
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public long GetShopDistributionProductCount(long shopId)
        {
            long result = 0;
            var data = Context.ProductBrokerageInfo.Include(d => d.Himall_Products)
                .Where(d => d.Himall_Products.SaleStatus == ProductInfo.ProductSaleStatus.OnSale
                    && d.Himall_Products.AuditStatus == ProductInfo.ProductAuditStatus.Audited
                    && d.Status == ProductBrokerageInfo.ProductBrokerageStatus.Normal
                    && d.ShopId == shopId).Count();
            if (data > 0)
            {
                result = data;
            }
            return result;
        }


        public void ApplyForDistributor(PromoterModel model)
        {
            PromoterInfo info = new PromoterInfo();
            //获取平台配置
            var platsetting = Context.DistributorSettingInfo.FirstOrDefault();
            var setting = Context.RecruitSettingInfo.FirstOrDefault();
            if (platsetting == null || !platsetting.Enable)
                throw new HimallException("平台未开启分销！");
            if (setting == null)
                throw new HimallException("平台未设置招募信息");
            if (setting.Enable)
                info.Status = PromoterInfo.PromoterStatus.UnAudit;
            else
            {
                info.Status = PromoterInfo.PromoterStatus.Audited;
                info.PassTime = DateTime.Now;
            }
            info.ApplyTime = DateTime.Now;
            info.UserId = model.UserId;
            var member = Context.UserMemberInfo.Where(a => a.Id == model.UserId).FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(model.Address))
                member.Address = model.Address;
            if (!string.IsNullOrWhiteSpace(model.RealName))
                member.RealName = model.RealName;
            if (model.RegionId != 0)
                member.RegionId = model.RegionId;
            if (model.TopRegionId != 0)
                member.TopRegionId = model.TopRegionId;
            string nickname = member.Nick;
            if (string.IsNullOrWhiteSpace(nickname))
            {
                nickname = member.UserName;
            }
            info.ShopName = nickname + " 小店";
            var p = member.Himall_Promoter.FirstOrDefault();
            if (p == null)
                member.Himall_Promoter.Add(info);
            else
            {
                p.ShopName = info.ShopName;
                p.Status = info.Status;
            }
            if (!string.IsNullOrEmpty(model.Mobile))
            {
                var memberContactInfo = Context.MemberContactsInfo.FirstOrDefault(item => item.Contact == model.Mobile && item.UserId == model.UserId && item.UserType == MemberContactsInfo.UserTypes.General);
                if (memberContactInfo == null)
                {
                    member.CellPhone = model.Mobile;
                    Context.MemberContactsInfo.Add(new MemberContactsInfo()
                    {
                        ServiceProvider = "Himall.Plugin.Message.SMS",
                        UserId = member.Id,
                        UserType = MemberContactsInfo.UserTypes.General,
                        Contact = model.Mobile
                    });
                }
            }
            Context.SaveChanges();
        }


        public PromoterInfo GetPromoter(long id)
        {
            return Context.PromoterInfo.Find(id);
        }


        public PromoterInfo GetPromoterByUserId(long userId)
        {
            return Context.PromoterInfo.Where(a => a.UserId == userId).FirstOrDefault();
        }



        PromoterStatistics IDistributionService.GetPromoterStatistics()
        {
            var t = DateTime.Now;
            var threeDays = t.AddDays(-3);
            var week = t.AddDays(-7);
            var p = Context.PromoterInfo.Where(a => a.Status == PromoterInfo.PromoterStatus.Audited);
            var PromoterCount = p.Count();
            var ThreeDaysIncrease = p.Where(a => a.ApplyTime >= threeDays).Count();
            var SevenDaysIncrease = p.Where(a => a.ApplyTime >= week).Count();
            PromoterStatistics model = new PromoterStatistics();
            model.PromoterCount = PromoterCount;
            model.ThreeDaysIncrease = ThreeDaysIncrease;
            model.SevenDaysIncrease = SevenDaysIncrease;
            return model;
        }


        public void UpdateDistributionShare(DistributionShareSetting model)
        {
            var m = Context.DistributionShareSetting.FirstOrDefault();
            if (m != null)
            {
                m.ProShareLogo = model.ProShareLogo;
                m.ProShareTitle = model.ProShareTitle;
                m.ProShareDesc = model.ProShareDesc;
                m.ShopShareLogo = model.ShopShareLogo;
                m.ShopShareTitle = model.ShopShareTitle;
                m.ShopShareDesc = model.ShopShareDesc;
                m.DisShareLogo = model.DisShareLogo;
                m.DisShareTitle = model.DisShareTitle;
                m.DisShareDesc = model.DisShareDesc;
                m.RecruitShareLogo = model.RecruitShareLogo;
                m.RecruitShareTitle = model.RecruitShareTitle;
                m.RecruitShareDesc = model.RecruitShareDesc;
            }
            else
            {
                Context.DistributionShareSetting.Add(model);
            }
            Context.SaveChanges();
            if (m != null)
            //转移图片
            {
                if (!string.IsNullOrWhiteSpace(model.ProShareLogo))
                    m.ProShareLogo = MoveImages(model.ProShareLogo);
                if (!string.IsNullOrWhiteSpace(model.ShopShareLogo))
                    m.ShopShareLogo = MoveImages(model.ShopShareLogo);
                if (!string.IsNullOrWhiteSpace(model.DisShareLogo))
                    m.DisShareLogo = MoveImages(model.DisShareLogo);
                if (!string.IsNullOrWhiteSpace(model.RecruitShareLogo))
                    m.RecruitShareLogo = MoveImages(model.RecruitShareLogo);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(model.ProShareLogo))
                    model.ProShareLogo = MoveImages(model.ProShareLogo);
                if (!string.IsNullOrWhiteSpace(model.ShopShareLogo))
                    model.ShopShareLogo = MoveImages(model.ShopShareLogo);
                if (!string.IsNullOrWhiteSpace(model.DisShareLogo))
                    model.DisShareLogo = MoveImages(model.DisShareLogo);
                if (!string.IsNullOrWhiteSpace(model.RecruitShareLogo))
                    model.RecruitShareLogo = MoveImages(model.RecruitShareLogo);
            }
            Context.SaveChanges();
        }


        string MoveImages(string image)
        {
            if (string.IsNullOrWhiteSpace(image))
            {
                return "";
            }
            var ext = image.Substring(image.LastIndexOf("."));
            var name = DateTime.Now.ToString("yyyyMMddhhmmss");
            //转移图片
            string relativeDir = "/Storage/Plat/DistributionShare/";
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

        public DistributionShareSetting GetDistributionShare()
        {
            var m = Context.DistributionShareSetting.FirstOrDefault();
            return m;
        }


        public void UpdateProductShareNum(long productId)
        {
            var m = Context.ProductBrokerageInfo.Where(a => a.ProductId == productId).FirstOrDefault();
            if (m.ForwardNum.HasValue)
                m.ForwardNum += 1;
            else
            {
                m.ForwardNum = 1;
            }
            Context.SaveChanges();
        }

        /// <summary>
        /// 获取所有分销首页商品(过滤下架和删除的商品)
        /// </summary>
        /// <returns></returns>
        public List<DistributionProductsInfo> GetDistributionProducts()
        {
            //List<DistributionProductsInfo> list = null;
            //using (MySqlConnection conn = new MySqlConnection(Connection.ConnectionString))
            //{
            //    string sql = "select ps.* from himall_distributionproducts ps left join himall_productbrokerage pg on ps.ProductbrokerageId=pg.Id " +
            //                " left join himall_products p on pg.ProductId=p.Id where p.IsDeleted=0 and AuditStatus=2 and SaleStatus=1";
            //    list = conn.Query<DistributionProductsInfo>(sql).ToList();
            //}
            //return list;
            return Context.DistributionProductsInfo.Where(p => p.Himall_ProductBrokerage.Himall_Products.IsDeleted == false && p.Himall_ProductBrokerage.Himall_Products.SaleStatus == ProductInfo.ProductSaleStatus.OnSale
                    && p.Himall_ProductBrokerage.Himall_Products.AuditStatus == ProductInfo.ProductAuditStatus.Audited).ToList();
        }

        /// <summary>
        /// 添加首页商品设置
        /// </summary>
        /// <param name="mDistributionProductsInfo"></param>
        public void AddDistributionProducts(DistributionProductsInfo mDistributionProductsInfo)
        {
            Context.DistributionProductsInfo.Add(mDistributionProductsInfo);
            Context.SaveChanges();
        }
        /// <summary>
        /// 移除首页分销商品
        /// </summary>
        /// <param name="Ids"></param>
        public void RemoveDistributionProducts(IEnumerable<long> Ids)
        {
            Context.DistributionProductsInfo.Remove(item => Ids.Contains(item.ProductbrokerageId));
            Context.SaveChanges();
        }


        /// <summary>
        /// 获取分销首页所有商品
        /// </summary>
        /// <param name="page">分页页码</param>
        /// <param name="rows">每页行数</param>
        /// <param name="keyWords">搜索关键字</param>
        /// <param name="categoryId">3级分类</param>
        public Himall.CommonModel.QueryPageModel<Himall.Model.DistributionProductsInfo> GetDistributionProducts(int page, int rows, string keyWords, long? categoryId = null)
        {
            Himall.CommonModel.QueryPageModel<Himall.Model.DistributionProductsInfo> result = new Himall.CommonModel.QueryPageModel<Himall.Model.DistributionProductsInfo>();

            IQueryable<DistributionProductsInfo> sql = Context.DistributionProductsInfo;

            //搜索条件
            if (!string.IsNullOrEmpty(keyWords))
            {
                keyWords = keyWords.Trim();
                var brandIds = Context.BrandInfo.FindBy(item => item.Name.Contains(keyWords) && item.IsDeleted == false).Select(item => item.Id).ToArray();
                sql = sql.FindBy(d => d.Himall_ProductBrokerage.Himall_Products.ProductName.Contains(keyWords) || brandIds.Contains(d.Himall_ProductBrokerage.Himall_Products.BrandId));
            }

            if (categoryId.HasValue)
            {
                sql = sql.Where(d => ("|" + d.Himall_ProductBrokerage.Himall_Products.CategoryPath + "|").Contains("|" + categoryId.Value + "|"));
            }
            sql = sql.Where(p => p.Himall_ProductBrokerage.Himall_Products.IsDeleted == false && p.Himall_ProductBrokerage.Himall_Products.SaleStatus == ProductInfo.ProductSaleStatus.OnSale
                    && p.Himall_ProductBrokerage.Himall_Products.AuditStatus == ProductInfo.ProductAuditStatus.Audited);

            var orderby = sql.GetOrderBy(d => d.OrderBy(o => o.Sequence));

            int total = 0;
            sql = sql.GetPage(out total, page, rows, orderby);
            List<DistributionProductsInfo> datalist = sql.ToList();

            result.Models = datalist;
            result.Total = total;
            return result;
        }

        /// <summary>
        /// 获取所有分销首页商品
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public List<Himall.Model.DistributionProductsInfo> GetDistributionProducts(ProductBrokerageInfo.ProductBrokerageStatus? status = null)
        {
            IQueryable<DistributionProductsInfo> sql = Context.DistributionProductsInfo;

            if (status.HasValue)
            {
                sql = sql.Where(item => item.Himall_ProductBrokerage.Status == status.Value);
            }

            sql = sql.Where(item => item.Himall_ProductBrokerage.Himall_Products.IsDeleted == false && item.Himall_ProductBrokerage.Himall_Products.SaleStatus == ProductInfo.ProductSaleStatus.OnSale
                    && item.Himall_ProductBrokerage.Himall_Products.AuditStatus == ProductInfo.ProductAuditStatus.Audited);

            sql = sql.OrderBy(p => p.Sequence);
            return sql.ToList();
        }

        /// <summary>
        /// 删除分销设置
        /// </summary>
        /// <param name="Id"></param>
        public void DelDistributionProducts(long Id)
        {
            Context.DistributionProductsInfo.Remove(item => item.ID == Id);
            Context.SaveChanges();
        }


        /// <summary>
        /// 获取分销设置对象
        /// </summary>
        /// <param name="Id">主键ID</param>
        /// <returns></returns>
        public DistributionProductsInfo GetDistributionProductsInfo(long Id)
        {
            return Context.DistributionProductsInfo.Where(item => item.ID == Id).First();
        }

        /// <summary>
        /// 修改分销首页设置
        /// </summary>
        /// <param name="model">分销实体</param>
        public void UpdateDistributionProducts(DistributionProductsInfo model)
        {
            Context.SaveChanges();
        }
    }
    /// <summary>
    /// 分销商品中间引用类
    /// <para>仅service使用</para>
    /// </summary>
    public class ProductBrokerageInfoDto : ProductBrokerageInfo
    {
        /// <summary>
        /// 每月销量
        /// </summary>
        public int? MonthAgent { get; set; }
        /// <summary>
        /// 每星期销量
        /// </summary>
        public int? WeekAgent { get; set; }
    }
}
