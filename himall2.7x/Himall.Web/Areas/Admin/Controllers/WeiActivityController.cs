using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Himall.Model;
using Himall.DTO;
using Himall.Web.Models;
using Himall.IServices;
using Himall.Web.Framework;
using Himall.CommonModel;
using System.IO;
using Himall.Core.Helper;
using Himall.Application;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class WeiActivityController : Controller
    {
        public ActionResult Management()
        {
            return View();
        }
        /// <summary>
        /// 分页查询刮刮卡
        /// </summary>
        /// <param name="name">活动名称</param>
        /// <param name="page">页数</param>
        /// <param name="rows">行数</param>
        [HttpPost]
        public JsonResult List(string name, int page = 1, int rows = 20)
        {
            var result = WeiActivityApplication.Get(name.Trim(),WeiActivityType.ScratchCard, page, rows);
            var datas = result.Models.ToList();
            DataGridModel<WeiActivityModel> weiActivityModel = new DataGridModel<WeiActivityModel>
            {
                rows = datas,
                total = result.Total
            };
            return Json(weiActivityModel);
        }



        [HttpPost]
        public JsonResult Deleteid(long id)
        {
            WeiActivityApplication.DeleteActivity(id);
            return Json(true);
        }

        public ActionResult Detail(long id)
        {
            WeiActivityModel WeiModel = WeiActivityApplication.GetActivityModel(id);
            return View(WeiModel);
        }


        public ActionResult WinManagement()
        {
          
            return View();
        }
        /// <summary>
        /// 分页查询中奖信息
        /// </summary>
        /// <param name="text">刮刮卡中奖状态</param>
        /// <param name="id">活动Id</param>
        /// <param name="page">页数</param>
        /// <param name="rows">行数</param>
        [HttpPost]
        public JsonResult WinList(string text,long id, int page = 1, int rows = 20)
        {
            var result = WeiActivityWinApplication.GetActivityWin(text, id, page, rows);
            var datas = result.Models.ToList();
            DataGridModel<WeiActivityWinModel> weiActivityWinModel = new DataGridModel<WeiActivityWinModel>
            {
                rows = datas,
                total = result.Total
            };
            return Json(weiActivityWinModel);

        }
        [UnAuthorize]
        public ActionResult Add()
        {
            ViewBag.BrandDrop = BindDrop(false);

            return View();
        }


        [HttpPost]
        public ActionResult Add(FormCollection fc)
        {
            WeiActivityModel model = new WeiActivityModel();
            if (!string.IsNullOrWhiteSpace(fc["activityTitle"]))
            {
                model.activityTitle = fc["activityTitle"].ToString();
            }
            if (!string.IsNullOrWhiteSpace(fc["activityDetails"]))
            {
                model.activityDetails = fc["activityDetails"].ToString();
            }
            if (!string.IsNullOrWhiteSpace(fc["beginTime"]))
            {
                model.beginTime = Convert.ToDateTime(fc["beginTime"].ToString());
            }
            if (!string.IsNullOrWhiteSpace(fc["endTime"]))
            {
                model.endTime = Convert.ToDateTime(fc["endTime"].ToString());
            }
            if (!string.IsNullOrWhiteSpace(fc["participationType"]))
            {
                model.participationType = (WeiParticipateType)Enum.Parse(typeof(WeiParticipateType), fc["participationType"].ToString());
            }
            if (!string.IsNullOrWhiteSpace(fc["participationCount"]))
            {
                model.participationCount = Convert.ToInt32(fc["participationCount"].ToString().Replace(",", " ").Trim());
            }
            if (!string.IsNullOrWhiteSpace(fc["isPoint"]))
            {
                if (Convert.ToInt32(fc["isPoint"]) > 0)
                {
                    model.consumePoint = Convert.ToInt32(fc["consumePoint"].ToString());
                }
                else
                {
                    model.consumePoint = 0;
                }
            }


       
            //临时图片地址
            model.activityUrl = fc["activityUrl"];
            string url = "http://" + Request.Url.Host.ToString() + "/m-Mobile/ScratchCard/index/";
            model.codeUrl = url;

            model.activityType = WeiActivityType.ScratchCard;
            model.addDate = DateTime.Now;

            if (!string.IsNullOrWhiteSpace(fc["harward"]))
            {
                List<WeiActivityAwardModel> listAwardModel = new List<WeiActivityAwardModel>();
                for (int i = 1; i <= Convert.ToInt32(fc["harward"]); i++)
                {
                    WeiActivityAwardModel awardModel = new WeiActivityAwardModel();
                    awardModel.activityId = model.Id;
                    if (!string.IsNullOrWhiteSpace(fc["integral" + i]))
                    {
                        awardModel.integral = Convert.ToInt32(fc["integral" + i].ToString());
                    }
                    awardModel.awardType = (WeiActivityAwardType)Enum.Parse(typeof(WeiActivityAwardType), fc["ReceiveType" + i].ToString());
                    if (!string.IsNullOrWhiteSpace(fc["brand" + i]))
                    {
                        awardModel.bonusId = Convert.ToInt32(fc["brand" + i].ToString());
                    }
                    if (!string.IsNullOrWhiteSpace(fc["coupon" + i]))
                    {
                        awardModel.couponId = Convert.ToInt32(fc["coupon" + i].ToString());
                    }
                    if (!string.IsNullOrWhiteSpace(fc["awardCount" + i]))
                    {
                        awardModel.awardCount = Convert.ToInt32(fc["awardCount" + i].ToString());
                    }
                    if (!string.IsNullOrWhiteSpace(fc["proportion" + i]))
                    {
                        awardModel.proportion = Convert.ToInt32(fc["proportion" + i].ToString());
                    }
                    awardModel.awardLevel = i;
                    if (awardModel.awardCount > 0 && awardModel.proportion > 0)
                    {
                        listAwardModel.Add(awardModel);
                    }
                }
                model.awards = listAwardModel;
            }

            long id = WeiActivityApplication.AddActivitySubmit(model);
            return RedirectToAction("Detail/" + id);
        }
        /// <summary>
        /// 用于显示 查询红包剩余数量
        /// </summary>
        /// <param name="id">红包ID</param>
        /// <returns></returns>
        private string GetBonusSurplus(long id)
        {
            return WeiActivityWinApplication.GetBonusSurplus(id);
        }

        private List<SelectListItem> BindDrop(bool Edit)
        {
            List<SelectListItem> bonusList = new List<SelectListItem> { new SelectListItem
                {
                    Selected = true,
                    Text ="请选择红包...",
                    Value = "0"
                }};
            var bonus = WeiActivityWinApplication.GetBonusByType(BonusInfo.BonusType.Prize);
            if (Edit)//是否是修改
            {
                foreach (var item in bonus)
                {
                        bonusList.Add(new SelectListItem
                        {
                            Selected = false,
                            Text = item.Name ,
                            Value = item.Id.ToString()
                        });
                }
            }
            else
            { 
                foreach (var item in bonus)
                {
                    var count =int.Parse(GetBonusSurplus(item.Id));
                    if (count>0)
                    {
                        bonusList.Add(new SelectListItem
                        {
                            Selected = false,
                            Text = item.Name + " 剩余：" + count + " 个",
                            Value = item.Id.ToString()
                        });
                    }
                }
            }
            return bonusList;
        }

        /// <summary>
        /// 获取优惠券
        /// </summary>
        /// <param name="text"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="endtime"></param>
        /// <param name="ReceiveType"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetCouponByName(string text, int page, int pageSize, string endtime = "", int ReceiveType = -1)
        {
            DateTime dtime = DateTime.Parse("1900-01-01");
            if (!endtime.Equals(""))
                dtime = Convert.ToDateTime(endtime);
            var result = CouponApplication.GetCouponByName(text, dtime, 2, page, pageSize);//主动发放
            var datas = result.Models.ToList();
            DataGridModel<CouponModel> CouponModel = new DataGridModel<CouponModel>
            {
                rows = datas,
                total = result.Total
            };
            return Json(CouponModel);
        }

        public ActionResult Edit(long id)
        {
            ViewBag.BrandDrop = BindDrop(true);
            //查询当前刮刮卡信息
            WeiActivityModel weiAwardModel=WeiActivityApplication.GetActivityModel(id);
            return View(weiAwardModel);
        }

        [HttpPost]
        public ActionResult Edit(FormCollection fc)
        {
            WeiActivityModel model = WeiActivityApplication.GetActivityModel(Convert.ToInt32(fc["activityId"]));
            
            if (!string.IsNullOrWhiteSpace(fc["activityTitle"]))
            {
                model.activityTitle = fc["activityTitle"].ToString();
            }
            if (!string.IsNullOrWhiteSpace(fc["activityDetails"]))
            {
                model.activityDetails = fc["activityDetails"].ToString();
            }
           
            if (!string.IsNullOrWhiteSpace(fc["endTime"]))
            {
                model.endTime = Convert.ToDateTime(fc["endTime"].ToString());
            }
            if (!string.IsNullOrWhiteSpace(fc["participationType"]))
            {
                model.participationType = (WeiParticipateType)Enum.Parse(typeof(WeiParticipateType), fc["participationType"].ToString());
            }
            if (!string.IsNullOrWhiteSpace(fc["participationCount"]))
            {
                model.participationCount = Convert.ToInt32(fc["participationCount"].ToString().Replace(",", " ").Trim());
            }
            if (!string.IsNullOrWhiteSpace(fc["isPoint"]))
            {
                if (Convert.ToInt32(fc["isPoint"]) > 0)
                {
                    model.consumePoint = Convert.ToInt32(fc["consumePoint"].ToString());
                }
                else
                {
                    model.consumePoint = 0;
                }
            }

            //临时图片地址
            model.activityUrl = fc["activityUrl"];
            string url = "http://" + Request.Url.Host.ToString() + "/m-Mobile/ScratchCard/index/";
            model.codeUrl = url;

            model.activityType = WeiActivityType.ScratchCard;
        

            long id = WeiActivityApplication.AddActivitySubmit(model);
            return RedirectToAction("Detail/" + id);
        }

    }
}