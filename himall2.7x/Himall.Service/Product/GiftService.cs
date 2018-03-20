using Himall.Core;
using Himall.Core.Helper;
using Himall.Entity;
using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Transactions;
using Himall.IServices.QueryModel;
using System.Data;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;
using System.IO;

namespace Himall.Service
{
    /// <summary>
    /// 积分礼品
    /// </summary>
    public partial class GiftService : ServiceBase, IGiftService
    {
        /// <summary>
        /// 添加礼品
        /// </summary>
        public void AddGift(GiftInfo model)
        {
            Context.GiftInfo.Add(model);
            Context.SaveChanges();
            //保存图片地址
            if (string.IsNullOrWhiteSpace(model.ImagePath)) model.ImagePath = string.Format(@"/Storage/Gift/{0}", model.Id);
            Context.SaveChanges();
            model.Description = HTMLProcess(model.Description, model.ImagePath);
            Context.SaveChanges();
        }
        /// <summary>
        /// 修改礼品
        /// </summary>
        /// <param name="model"></param>
        public void UpdateGift(GiftInfo model)
        {
            //保存图片地址
            if (string.IsNullOrWhiteSpace(model.ImagePath)) model.ImagePath = string.Format(@"/Storage/Gift/{0}", model.Id);

            model.Description = HTMLProcess(model.Description, model.ImagePath);
            //context.Set<GiftInfo>().Attach(model);
            var dbentry = Context.Entry<GiftInfo>(model);
            dbentry.State = EntityState.Modified;
            Context.SaveChanges();
        }
        /// <summary>
        /// 调整排序
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sequence"></param>
        public void UpdateSequence(long id, int sequence)
        {
            var gift = Context.GiftInfo.FindById(id);
            if (gift == null)
                throw new HimallException("未找到id为" + id + "的礼品");
            gift.Sequence = sequence;
            Context.SaveChanges();
        }
        /// <summary>
        /// 调整排序
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sequence"></param>
        public void ChangeStatus(long id, bool status)
        {
            var gift = Context.GiftInfo.FindById(id);
            if (gift == null)
                throw new HimallException("未找到id为" + id + "的礼品");
            if (status)
            {
                gift.SalesStatus = GiftInfo.GiftSalesStatus.Normal;
                //已过期礼品增加一个月兑换截止时间
                if (gift.EndDate.Date < DateTime.Now.Date)
                {
                    gift.EndDate = DateTime.Now.AddMonths(1);
                }
            }
            else
            {
                gift.SalesStatus = GiftInfo.GiftSalesStatus.OffShelves;
            }
            Context.SaveChanges();
        }
        /// <summary>
        /// 获取礼品
        /// </summary>
        /// <param name="id"></param>
        public GiftInfo GetById(long id)
        {
            GiftInfo result = null;
            result = Context.GiftInfo.FirstOrDefault(d => d.Id == id);
            if (result != null)
            {
                if (result.NeedGrade != 0)
                {
                    MemberGrade grade = Context.MemberGrade.FirstOrDefault(d => d.Id == result.NeedGrade);
                    if (grade != null)
                    {
                        result.GradeIntegral = grade.Integral;
                        result.NeedGradeName = grade.GradeName;
                    }
                    else
                    {
                        result.GradeIntegral = -1;
                        result.NeedGradeName = "等级已删除";
                    }
                }
                else
                {
                    result.GradeIntegral = 0;
                    result.NeedGradeName = "不限等级";
                }
            }
            return result;
        }
        /// <summary>
        /// 获取礼品
        /// <para>无追踪实体，UpdateGift前调用</para>
        /// </summary>
        /// <param name="id"></param>
        public GiftInfo GetByIdAsNoTracking(long id)
        {
            GiftInfo result = null;
            result = Context.GiftInfo.AsNoTracking().FirstOrDefault(d => d.Id == id);
            return result;
        }
        /// <summary>
        /// 查询礼品
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public ObsoletePageModel<GiftModel> GetGifts(GiftQuery query)
        {
            ObsoletePageModel<GiftModel> result = new ObsoletePageModel<GiftModel>();
            result.Total = 0;

            var datasql = (from d in Context.GiftInfo
                           join b in Context.MemberGrade on d.NeedGrade equals b.Id into j1
                           from jd in j1.DefaultIfEmpty()
                           select new GiftModel
                           {
                               Id = d.Id,
                               GiftName = d.GiftName,
                               NeedIntegral = d.NeedIntegral,
                               LimtQuantity = d.LimtQuantity,
                               StockQuantity = d.StockQuantity,
                               EndDate = d.EndDate,
                               NeedGrade = d.NeedGrade,
                               VirtualSales = d.VirtualSales,
                               RealSales = d.RealSales,
                               SalesStatus = d.SalesStatus,
                               ImagePath = d.ImagePath,
                               Sequence = d.Sequence,
                               GiftValue = d.GiftValue,
                               AddDate = d.AddDate,
                               GradeIntegral = jd == null ? 0 : jd.Integral,
                               NeedGradeName = jd == null ? "不限等级" : jd.GradeName
                           }).AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.skey))
            {
                datasql = datasql.Where(d => d.GiftName.Contains(query.skey));
            }

            if (query.status != null)
            {
                DateTime CurDay = DateTime.Now;
                DateTime CurAddOneDay = CurDay.AddDays(1).Date;
                switch (query.status)
                {
                    case GiftInfo.GiftSalesStatus.HasExpired:
                        datasql = datasql.Where(d => d.SalesStatus == GiftInfo.GiftSalesStatus.Normal && d.EndDate < CurDay);
                        break;
                    case GiftInfo.GiftSalesStatus.Normal:
                        datasql = datasql.Where(d => d.SalesStatus == GiftInfo.GiftSalesStatus.Normal && d.EndDate >= CurDay);
                        break;
                    default:
                        datasql = datasql.Where(d => d.SalesStatus == query.status);
                        break;
                }
            }

            if (query.isShowAll != true)
            {
                datasql = datasql.Where(d => d.SalesStatus != GiftInfo.GiftSalesStatus.IsDelete);
            }
            var orderby = datasql.GetOrderBy(d=>d.OrderBy(o=>o.Sequence).ThenByDescending(o=>o.Id));
            //排序
            switch (query.Sort)
            {
                case GiftQuery.GiftSortEnum.SalesNumber:
                    if (query.IsAsc)
                    {
                        orderby = datasql.GetOrderBy(o=>o.OrderBy(d => d.RealSales).ThenByDescending(d => d.Id));
                    }
                    else
                    {
                        orderby = datasql.GetOrderBy(o=>o.OrderByDescending(d => d.RealSales).ThenByDescending(d => d.Id));
                    }
                    break;
                case GiftQuery.GiftSortEnum.RealSalesNumber:
                    if (query.IsAsc)
                    {
                        orderby = datasql.GetOrderBy(o=>o.OrderBy(d => d.RealSales).ThenByDescending(d => d.Id));
                    }
                    else
                    {
                        orderby = datasql.GetOrderBy(o=>o.OrderByDescending(d => d.RealSales).ThenByDescending(d => d.Id));
                    }
                    break;
                default:
                        orderby = datasql.GetOrderBy(o=>o.OrderBy(d => d.Sequence).ThenByDescending(d => d.Id));
                    break;
            }

            int total = 0;
            datasql = datasql.GetPage(out total, query.PageNo, query.PageSize, orderby);

            //数据转换
            result.Models = datasql;
            result.Total = total;

            return result;
        }

        #region 广告配置
        /// <summary>
        /// 获取广告配置
        /// </summary>
        /// <param name="adtype">活动类型</param>
        /// <param name="adplatform">显示平台</param>
        /// <returns></returns>
        public IntegralMallAdInfo GetAdInfo(IntegralMallAdInfo.AdActivityType adtype, IntegralMallAdInfo.AdShowPlatform adplatform)
        {
            int sadtype = adtype.GetHashCode();
            int sadplatform = adplatform.GetHashCode();
            int sstatus = IntegralMallAdInfo.AdShowStatus.Show.GetHashCode();
            IntegralMallAdInfo result =Context.IntegralMallAdInfo.FirstOrDefault(d => d.ActivityType == sadtype && d.ShowPlatform == sadplatform && d.ShowStatus == sstatus);
            return result;
        }
        /// <summary>
        /// 更新广告信息
        /// </summary>
        /// <param name="ActivityType">活动类型</param>
        /// <param name="ActivityId">活动编号</param>
        /// <param name="Cover">广告图片</param>
        /// <param name="ShowStatus">显示状态</param>
        /// <param name="ShowPlatform">显示平台</param>
        /// <returns></returns>
        public IntegralMallAdInfo UpdateAdInfo(IntegralMallAdInfo.AdActivityType ActivityType, long ActivityId, string Cover, IntegralMallAdInfo.AdShowStatus? ShowStatus, IntegralMallAdInfo.AdShowPlatform? ShowPlatform)
        {
            IntegralMallAdInfo data = new IntegralMallAdInfo();
            int adtype = ActivityType.GetHashCode();
            data = Context.IntegralMallAdInfo.FirstOrDefault(d => d.ActivityType == adtype);
            bool isadd = false;
            if (data == null)
            {
                data = new IntegralMallAdInfo();
                isadd = true;
                data.ShowAdStatus = IntegralMallAdInfo.AdShowStatus.Show;
            }
            else
            {
                if (data.ActivityId == ActivityId)
                {
                    if (data.ShowAdStatus == IntegralMallAdInfo.AdShowStatus.Hide)
                    {
                        data.ShowAdStatus = IntegralMallAdInfo.AdShowStatus.Show;
                    }
                    else
                    {
                        data.ShowAdStatus = IntegralMallAdInfo.AdShowStatus.Hide;
                    }
                }
                else
                {
                    data.ShowAdStatus = IntegralMallAdInfo.AdShowStatus.Show;
                }
            }
            data.ActivityType = adtype;
            data.ActivityId = ActivityId;
            data.Cover = Cover;
            data.ShowAdPlatform = ShowPlatform;
            if (isadd)
            {
                Context.IntegralMallAdInfo.Add(data);
            }
            Context.SaveChanges();
            return data;
        }
        #endregion


        /// <summary>
        /// 转移外站图片，去除script脚本
        /// </summary>
        /// <param name="content">html内容</param>
        /// <param name="id"></param>
        /// <returns></returns>
        private string HTMLProcess(string content, string path)
        {
            var details = Path.Combine(path, "Details").Replace("\\", "/");
            var rename = Path.Combine(path, "Temp").Replace("\\", "/");
            var urlTemp = Core.Helper.IOHelper.GetMapPath(rename);
            try
            {
               
                //TD需要修改
                string imageRealtivePath = details;       
                content = Core.Helper.HtmlContentHelper.TransferToLocalImage(content, "/", imageRealtivePath, Core.HimallIO.GetImagePath(imageRealtivePath) + "/");
                content = Core.Helper.HtmlContentHelper.RemoveScriptsAndStyles(content);
            }
            catch
            {
               
            }
            return content;
        }

    }
}
