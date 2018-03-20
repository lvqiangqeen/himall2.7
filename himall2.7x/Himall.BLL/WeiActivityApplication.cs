using System;
using System.Collections.Generic;
using System.Linq;
using Himall.IServices;
using Himall.DTO;
using Himall.Model;
using Himall.CommonModel;
using Himall.Core;

namespace Himall.Application
{
    public class WeiActivityApplication
    {
        private static IWeiActivityInfoService _iActivityInfoService = ObjectContainer.Current.Resolve<IWeiActivityInfoService>();
        private static IWeiActivityWinService _iActivityWinService = ObjectContainer.Current.Resolve<IWeiActivityWinService>();
        private static IBonusService _iBonusService = ObjectContainer.Current.Resolve<IBonusService>();
        private static ICouponService _iCouponService = ObjectContainer.Current.Resolve<ICouponService>();
        public static long AddActivitySubmit(WeiActivityModel model)
        {

            WeiActivityInfo weiInfo = new WeiActivityInfo();
            if (model.Id > 0)
            {
                weiInfo.Id = model.Id;
            }
            weiInfo.ActivityTitle = model.activityTitle;
            weiInfo.ActivityType = model.activityType;
            weiInfo.ActivityDetails = model.activityDetails;
            weiInfo.ActivityUrl = TransferImage(model.activityUrl);
            weiInfo.BeginTime = model.beginTime;
            weiInfo.EndTime = model.endTime;
            weiInfo.ParticipationType = model.participationType;
            weiInfo.ParticipationCount = model.participationCount;
            weiInfo.ConsumePoint = model.consumePoint;
            weiInfo.CodeUrl = model.codeUrl;

            List<WeiActivityAwardInfo> listAwardInfo = new List<WeiActivityAwardInfo>();
            foreach (var item in model.awards)
            {
                WeiActivityAwardInfo awardInfo = new WeiActivityAwardInfo();
                awardInfo.ActivityId = item.activityId;
                awardInfo.AwardCount = item.awardCount;
                awardInfo.AwardLevel = item.awardLevel;
                awardInfo.AwardType = item.awardType;
                awardInfo.BonusId = item.bonusId;
                awardInfo.CouponId = item.couponId;
                awardInfo.Integral = item.integral;
                awardInfo.Proportion = item.proportion;
                listAwardInfo.Add(awardInfo);
            }
            weiInfo.Himall_WeiActivityAward = listAwardInfo;
            return _iActivityInfoService.AddActivity(weiInfo);


        }

        private static string TransferImage(string sourceFile)
        {
            if (!string.IsNullOrWhiteSpace(sourceFile) && !sourceFile.Contains("/Storage/Plat/"))
            {
                string newDir = "/Storage/Plat/WeiActivity/";

                string ext = sourceFile.Substring(sourceFile.LastIndexOf('.'));//得到扩展名
                string newName = "WeiActivity_" + System.DateTime.Now.ToString("yyyyMMddHHmmssffffff") + ext;//得到新的文件名

                if (!string.IsNullOrWhiteSpace(sourceFile))
                {
                    if (sourceFile.Contains("/temp/"))
                    {
                        string logoname = sourceFile.Substring(sourceFile.LastIndexOf('/') + 1);
                        string oldlogo = sourceFile.Substring(sourceFile.LastIndexOf("/temp"));
                        string newLogo = newDir + newName;
                        Core.HimallIO.CopyFile(oldlogo, newLogo, true);
                        sourceFile = newLogo;
                        return sourceFile;//返回新的文件路径
                    }
                    else if (sourceFile.Contains("/Storage/"))
                    {
                        sourceFile = sourceFile.Substring(sourceFile.LastIndexOf("/Storage"));
                    }
                }
            }
            else if (sourceFile.Contains("/Storage/"))
            {
                sourceFile = sourceFile.Substring(sourceFile.LastIndexOf("/Storage"));
            }

            return sourceFile;
        }



        public static PageModel<WeiActivityModel> Get(string text, WeiActivityType type, int pageIndex, int pageSize)
        {
            PageModel<WeiActivityInfo> weiInfo = _iActivityInfoService.Get(text, type, pageIndex, pageSize);
            var datas = weiInfo.Models.ToList().Select(m => new WeiActivityModel()
            {
                Id = m.Id,
                activityTitle = m.ActivityTitle,
                activityType = m.ActivityType,
                activityDetails = m.ActivityDetails,
                activityUrl = Himall.Core.HimallIO.GetImagePath(m.ActivityUrl),
                beginTime = m.BeginTime,
                endTime = m.EndTime,
                participationType = m.ParticipationType,
                participationCount = Convert.ToInt32(m.ParticipationCount),
                consumePoint = m.ConsumePoint,
                codeUrl = m.CodeUrl,
                addDate = m.AddDate,
                totalNumber = _iActivityWinService.GetWinNumber(m.Id, "ALL"),
                winNumber = _iActivityWinService.GetWinNumber(m.Id, "True")
            }).ToList();

            PageModel<WeiActivityModel> t = new PageModel<WeiActivityModel>();
            t.Models = datas.AsQueryable();
            t.Total = weiInfo.Total;
            return t;

        }


        public static void DeleteActivity(long id)
        {
            _iActivityInfoService.DeleteActivity(id);
        }

        public static WeiActivityModel GetActivityModel(long id)
        {
            WeiActivityInfo model = _iActivityInfoService.GetActivityModel(id);

            WeiActivityModel viewModel = new WeiActivityModel();
            viewModel.Id = model.Id;
            viewModel.activityTitle = model.ActivityTitle;
            viewModel.activityType = model.ActivityType;
            viewModel.activityDetails = model.ActivityDetails;
            viewModel.activityUrl = Himall.Core.HimallIO.GetImagePath(model.ActivityUrl);
            viewModel.beginTime = model.BeginTime;
            viewModel.endTime = model.EndTime;
            viewModel.participationType = model.ParticipationType;
            viewModel.participationCount = Convert.ToInt32(model.ParticipationCount);
            viewModel.consumePoint = model.ConsumePoint;
            viewModel.codeUrl = model.CodeUrl;
            viewModel.addDate = model.AddDate;

            List<WeiActivityAwardModel> listAwardModel = new List<WeiActivityAwardModel>();
            foreach (var item in model.Himall_WeiActivityAward)
            {
                WeiActivityAwardModel awardModel = new WeiActivityAwardModel();
                awardModel.Id = item.Id;
                awardModel.activityId = item.ActivityId;
                awardModel.awardCount = item.AwardCount;
                awardModel.awardLevel = item.AwardLevel;
                awardModel.awardType = item.AwardType;
                awardModel.bonusId = item.BonusId == null ? 0 : Convert.ToInt32(item.BonusId);
                awardModel.couponId = item.CouponId == null ? 0 : Convert.ToInt32(item.CouponId); ;
                awardModel.integral = item.Integral == null ? 0 : Convert.ToInt32(item.Integral); ;
                awardModel.proportion = item.Proportion;
                listAwardModel.Add(awardModel);
            }
            viewModel.awards = listAwardModel;
            return viewModel;
        }

        public static WeiActivityModel GetActivityModelByBigWheel(long id)
        {
            WeiActivityInfo model = _iActivityInfoService.GetActivityModel(id);

            WeiActivityModel viewModel = new WeiActivityModel();
            viewModel.Id = model.Id;
            viewModel.activityTitle = model.ActivityTitle;
            viewModel.activityType = model.ActivityType;
            viewModel.activityDetails = model.ActivityDetails;
            viewModel.activityUrl = model.ActivityUrl;
            viewModel.beginTime = model.BeginTime;
            viewModel.endTime = model.EndTime;
            viewModel.participationType = model.ParticipationType;
            viewModel.participationCount = Convert.ToInt32(model.ParticipationCount);
            viewModel.consumePoint = model.ConsumePoint;
            viewModel.codeUrl = model.CodeUrl;
            viewModel.addDate = model.AddDate;

            List<WeiActivityAwardModel> listAwardModel = new List<WeiActivityAwardModel>();
            var item = model.Himall_WeiActivityAward.ToList();
            int awardNum = 0;//获取奖等序号
            int falg = 9 - item.Count();
            for (int i = 0; i < 9; i++)//创建9宫格实体
            {


                //放空值 ，未中奖
                if (i % 2 == 0)
                {
                    //最高奖等6 ，4以内奖等能均匀分布
                    if (item.Count() <= 4)
                    {
                        //获取奖项
                        WeiActivityAwardModel awardModel = new WeiActivityAwardModel();
                        listAwardModel.Add(awardModel);
                    }
                    else //大于4组奖项  未中奖显示不能均匀分布 
                    {
                        if (falg > 0)
                        {
                            //获取奖项
                            WeiActivityAwardModel awardModel = new WeiActivityAwardModel();
                            listAwardModel.Add(awardModel);
                            falg--;

                        }
                        else
                        {
                            //获取奖项
                            WeiActivityAwardModel awardModel = new WeiActivityAwardModel();
                            awardModel.Id = item[awardNum].Id;
                            awardModel.activityId = item[awardNum].ActivityId;
                            awardModel.awardCount = item[awardNum].AwardCount;
                            awardModel.awardLevel = item[awardNum].AwardLevel;
                            awardModel.awardType = item[awardNum].AwardType;
                            awardModel.bonusId = item[awardNum].BonusId == null ? 0 : Convert.ToInt32(item[awardNum].BonusId);
                            awardModel.couponId = item[awardNum].CouponId == null ? 0 : Convert.ToInt32(item[awardNum].CouponId);
                            awardModel.integral = item[awardNum].Integral == null ? 0 : Convert.ToInt32(item[awardNum].Integral);
                            awardModel.couponId = item[awardNum].CouponId == null ? 0 : Convert.ToInt32(item[awardNum].CouponId);
                            awardModel.integral = item[awardNum].Integral == null ? 0 : Convert.ToInt32(item[awardNum].Integral);
                            if (awardModel.couponId != 0)
                            {
                                awardModel.couponName = _iCouponService.GetCouponInfo(long.Parse(item[awardNum].CouponId.ToString())).CouponName;
                            }
                            awardModel.proportion = item[awardNum].Proportion;
                            listAwardModel.Add(awardModel);
                            awardNum++;
                        }
                    }
                }
                else
                {
                    //奖等未取完
                    if (item.Count() > awardNum)
                    {
                        //获取奖项
                        WeiActivityAwardModel awardModel = new WeiActivityAwardModel();
                        awardModel.Id = item[awardNum].Id;
                        awardModel.activityId = item[awardNum].ActivityId;
                        awardModel.awardCount = item[awardNum].AwardCount;
                        awardModel.awardLevel = item[awardNum].AwardLevel;
                        awardModel.awardType = item[awardNum].AwardType;
                        awardModel.bonusId = item[awardNum].BonusId == null ? 0 : Convert.ToInt32(item[awardNum].BonusId);

                        awardModel.couponId = item[awardNum].CouponId == null ? 0 : Convert.ToInt32(item[awardNum].CouponId);
                        awardModel.integral = item[awardNum].Integral == null ? 0 : Convert.ToInt32(item[awardNum].Integral);
                        if (awardModel.couponId != 0)
                        {
                            awardModel.couponName = _iCouponService.GetCouponInfo(long.Parse(item[awardNum].CouponId.ToString())).CouponName;
                        }

                        awardModel.proportion = item[awardNum].Proportion;
                        listAwardModel.Add(awardModel);
                        awardNum++;
                    }
                    else
                    {
                        //获取奖项
                        WeiActivityAwardModel awardModel = new WeiActivityAwardModel();
                        listAwardModel.Add(awardModel);
                    }

                }




            }
            viewModel.awards = listAwardModel;
            return viewModel;
        }


    }
}
