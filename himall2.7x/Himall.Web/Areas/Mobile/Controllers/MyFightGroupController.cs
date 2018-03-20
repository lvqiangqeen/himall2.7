using System;
using System.Linq;
using System.Web.Mvc;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Himall.Web.App_Code.Common;
using Himall.Web.Areas.Mobile.Models;
using Himall.Web.Areas.Web.Models;
using Himall.Core;
using Himall.Core.Helper;
using Himall.Core.Plugins.Payment;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Framework;
using Senparc.Weixin.MP.Helpers;

using Himall.CommonModel;
using Himall.DTO;
using Himall.Application;

namespace Himall.Web.Areas.Mobile.Controllers
{
    /// <summary>
    /// 用户拼团
    /// </summary>
    public class MyFightGroupController : BaseMobileMemberController
    {
        private IProductService _iProductService;
        private long curUserId = 0;
        public MyFightGroupController( IProductService iProductService
            )
        {
            _iProductService = iProductService;

            curUserId = UserId;
        }

        #region 我的拼团
        /// <summary>
        /// 我的拼团
        /// </summary>
        /// <returns></returns>
        public ActionResult MyGroups()
        {
            return View();
        }
        [HttpPost]
        public JsonResult PostJoinGroups(int page)
        {
            int pagesize = 5;
            List<FightGroupOrderJoinStatus> seastatus = new List<FightGroupOrderJoinStatus>();
            //seastatus.Add(FightGroupOrderJoinStatus.Ongoing);
            seastatus.Add(FightGroupOrderJoinStatus.JoinSuccess);
            seastatus.Add(FightGroupOrderJoinStatus.BuildFailed);
            seastatus.Add(FightGroupOrderJoinStatus.BuildSuccess);
            var data = FightGroupApplication.GetJoinGroups(curUserId, seastatus, page, pagesize);
            var datalist = data.Models.ToList();
            List<MyFightGroupPostJoinGroupsModel> resultlist = new List<MyFightGroupPostJoinGroupsModel>();
            foreach (var item in datalist)
            {
                MyFightGroupPostJoinGroupsModel _tmp = new MyFightGroupPostJoinGroupsModel();
                _tmp.Id = item.Id;
                _tmp.ActiveId = item.ActiveId;
                _tmp.ProductName = item.ProductName;
                _tmp.ProductImgPath = item.ProductImgPath;
                _tmp.ProductDefaultImage =HimallIO.GetProductSizeImage(_tmp.ProductImgPath, 1, (int)ImageSize.Size_350);
                _tmp.GroupEndTime = item.OverTime.HasValue ? item.OverTime.Value : item.GroupEndTime;
                _tmp.BuildStatus = item.BuildStatus;
                _tmp.NeedNumber =item.LimitedNumber - item.JoinedNumber;
                _tmp.UserIcons = new List<string>();
                foreach (var sitem in item.GroupOrders)
                {
                    _tmp.UserIcons.Add(sitem.Photo);
                    if (sitem.OrderUserId == curUserId)
                    {
                        _tmp.OrderId = sitem.OrderId;
                        _tmp.GroupPrice = sitem.SalePrice;
                    }
                }
                resultlist.Add(_tmp);
            }
            return Json(new { success = true, rows = resultlist, total = data.Total });
        }
        #endregion

        #region 拼团详情
        /// <summary>
        /// 我的拼团详情
        /// </summary>
        /// <param name="id"></param>
        /// <param name="aid"></param>
        /// <returns></returns>
        public ActionResult GroupDetail(long id, long aid)
		{
			FightGroupActiveModel gpact = FightGroupApplication.GetActive(aid, false);
			if (gpact == null)
			{
				throw new HimallException("错误的活动信息");
			}
			FightGroupsModel groupsdata = FightGroupApplication.GetGroup(aid, id);
			if (groupsdata == null)
			{
				throw new HimallException("错误的拼团信息");
			}
			if (groupsdata.BuildStatus == FightGroupBuildStatus.Opening)
			{
				//throw new HimallException("开团未成功，等待团长付款中");
				return Redirect(string.Format("/m-{0}/Member/Center/", PlatformType.ToString()));
			}
			MyFightGroupDetailModel model = new MyFightGroupDetailModel();
			model.ActiveData = gpact;
			model.GroupsData = groupsdata;

			model.ShareUrl = string.Format("{0}/m-{1}/FightGroup/GroupDetail/{2}?aid={3}", Request.Url.Scheme + "://" + Request.Url.Authority, "WeiXin", groupsdata.Id, groupsdata.ActiveId);
			model.ShareTitle = "我参加了(" + groupsdata.ProductName + ")的拼团";
			model.ShareImage = gpact.ProductDefaultImage;
			if (!string.IsNullOrWhiteSpace(model.ShareImage))
			{
				if (model.ShareImage.Substring(0, 4) != "http")
				{
					model.ShareImage = HimallIO.GetRomoteImagePath(model.ShareImage);
				}
			}

			int neednum = groupsdata.LimitedNumber - groupsdata.JoinedNumber;
			neednum = neednum < 0 ? 0 : neednum;
            if (neednum > 0)
            {
                model.ShareDesc = "还差" + neednum + "人即可成团";
            }
			if (!string.IsNullOrWhiteSpace(gpact.ProductShortDescription))
			{
                if (!string.IsNullOrWhiteSpace(model.ShareDesc))
                {
                    model.ShareDesc += "，(" + gpact.ProductShortDescription + ")";
                }
                else
                {
                    model.ShareDesc += gpact.ProductShortDescription;
                }
			}
			return View(model);
		}
        /// <summary>
        /// 分享参数二次加载
        /// <para>ios分享错误临时解决方案</para>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="aid"></param>
        /// <returns></returns>
        public JsonResult GetGroupDetailWXShare(long id, long aid)
        {
            FightGroupActiveModel gpact = FightGroupApplication.GetActive(aid, false);
            if (gpact == null)
            {
                throw new HimallException("错误的活动信息");
            }
            FightGroupsModel groupsdata = FightGroupApplication.GetGroup(aid, id);
            if (groupsdata == null)
            {
                throw new HimallException("错误的拼团信息");
            }
            if (groupsdata.BuildStatus == FightGroupBuildStatus.Opening)
            {
                throw new HimallException("开团未成功，等待团长付款中");
            }
            MyFightGroupDetailModel model = new MyFightGroupDetailModel();
            model.ShareUrl = string.Format("{0}/m-{1}/FightGroup/GroupDetail/{2}?aid={3}", Request.Url.Authority, "WeiXin", groupsdata.Id, groupsdata.ActiveId);
            model.ShareTitle = "我参加了(" + groupsdata.ProductName + ")的拼团";
            model.ShareImage = gpact.ProductDefaultImage;
            if (!string.IsNullOrWhiteSpace(model.ShareImage))
            {
                if (model.ShareImage.Substring(0, 4) != "http")
                {
                    model.ShareImage = HimallIO.GetRomoteImagePath(model.ShareImage);
                }
            }

            int neednum = groupsdata.LimitedNumber - groupsdata.JoinedNumber;
            neednum = neednum < 0 ? 0 : neednum;

            model.ShareDesc = "还差" + neednum + "人即可成团";
            if (!string.IsNullOrWhiteSpace(gpact.ProductShortDescription))
            {
                model.ShareDesc += "，(" + gpact.ProductShortDescription + ")";
            }

            //model.WeiXinShareArgs = Application.WXApiApplication.GetWeiXinShareArgs(this.HttpContext.Request.UrlReferrer.AbsoluteUri);

            return Json(model);
        }
        #endregion
    }
}