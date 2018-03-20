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
using Himall.CommonModel;

namespace Himall.Service
{
    public class ActivityInfoService : ServiceBase, IWeiActivityInfoService
    {

        public ObsoletePageModel<WeiActivityInfo> Get(string text, WeiActivityType type, int pageIndex, int pageSize, bool? isIntegralActivity = null, bool isShowAll = true)
        {
            IQueryable<WeiActivityInfo> query = Context.WeiActivityInfo;

            if (!string.IsNullOrEmpty(text))
            {
                query = query.Where(p => p.ActivityTitle.Contains(text));
            }
            if (pageIndex <= 0)
            {
                pageIndex = 1;
            }
            if (isIntegralActivity != null)
            {
                if (isIntegralActivity == true)
                {
                    query = query.Where(d => d.ConsumePoint > 0);
                }
                else
                {
                    query = query.Where(d => d.ConsumePoint == 0);
                }
            }
            if (!isShowAll)
            {
                //query = query.Where(d => d.EndTime>DateTime.Now);
                query = query.Where(d => d.BeginTime < DateTime.Now && d.EndTime > DateTime.Now);//取有效期内活动
            }
            query = query.Where(p => p.ActivityType == type);
            int total = 0;
            IQueryable<WeiActivityInfo> datas = query.GetPage(out total, p => p.OrderByDescending(o => o.AddDate), pageIndex, pageSize);
            ObsoletePageModel<WeiActivityInfo> pageModel = new ObsoletePageModel<WeiActivityInfo>()
            {
                Models = datas,
                Total = total
            };
            return pageModel;
        }
        public WeiActivityInfo GetActivityModel(long id)
        {
            try
            {
                WeiActivityInfo weiInfo = Context.WeiActivityInfo.Where(p => p.Id == id).ToList()[0];
                return weiInfo;
            }
            catch (Exception)
            {

                throw;
            }

        }
        public long AddActivity(WeiActivityInfo model)
        {
            if (model.Id <= 0)
            {

                model.AddDate = DateTime.Now;
                Context.WeiActivityInfo.Add(model);
                Context.SaveChanges();
                model.CodeUrl = GenerateQR(model.CodeUrl + model.Id);
            }
            else
            {
                WeiActivityInfo info = Context.WeiActivityInfo.Find(model.Id);
                info.ParticipationCount = model.ParticipationCount;
                info.ParticipationType = model.ParticipationType;
                info.ActivityDetails = model.ActivityDetails;
                info.ActivityTitle = model.ActivityTitle;
                info.ActivityType = model.ActivityType;
                info.ActivityUrl = model.ActivityUrl;
                info.ConsumePoint = model.ConsumePoint;
                info.EndTime = model.EndTime;

            }
            Context.SaveChanges();
            return model.Id;

        }
        /// <summary>
        /// 生成二维码
        /// </summary>
        private string GenerateQR(string path)
        {
            Bitmap bi = Himall.Core.Helper.QRCodeHelper.Create(path);
            string fileName = Guid.NewGuid().ToString() + ".jpg";
            string fileFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Storage", "Plat", "WeiActivity");
            string fileFullPath = Path.Combine(fileFolderPath, fileName);
            if (!Directory.Exists(fileFolderPath))
            {
                Directory.CreateDirectory(fileFolderPath);
            }
            bi.Save(fileFullPath);

            return "/Storage/Plat/WeiActivity/" + fileName;
        }

        public long UpdateActivity(WeiActivityInfo model)
        {
            return 1;
        }
        public void DeleteActivity(long id)
        {
            //删除主表信息
            var ociobj = Context.WeiActivityInfo.FindById(id);
            if (ociobj != null)
            {
                //删除相关信息
                var procoms = Context.WeiActivityAwardInfo.FindBy(d => ociobj.Id == d.ActivityId).ToList();
                Context.WeiActivityAwardInfo.RemoveRange(procoms);

                var procom = Context.WeiActivityWinInfo.FindBy(d => ociobj.Id == d.ActivityId).ToList();
                Context.WeiActivityWinInfo.RemoveRange(procom);
                //删除微信活动主表
                Context.WeiActivityInfo.Remove(ociobj);
                Context.SaveChanges();
            }


            ////删除从表信息
            //var activityAwardInfo = context.WeiActivityAwardInfo.FirstOrDefault(e => e.ActivityId == id);
            //if (activityAwardInfo!=null)
            //{
            //    context.WeiActivityAwardInfo.Remove(activityAwardInfo);
            //}

            ////删除主表信息
            //var activityInfo = context.WeiActivityInfo.FindById(id);
            //if (activityInfo!=null)
            //{
            //    context.WeiActivityInfo.Remove(activityInfo);
            //}
            //context.SaveChanges();
        }

        public List<WeiActivityInfo> GetAllModel()
        {
            return Context.WeiActivityInfo.ToList();
        }

        public int GetProportion(long id, long awardId)
        {
            var items = Context.WeiActivityInfo.Where(p => p.Id == id).ToList();
            return 1;

        }
    }
}
