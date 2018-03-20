using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Himall.Application;
using Himall.Core;
using Himall.Core.Helper;
using Himall.Model;
using Himall.Web.App_Code.Common;
using Himall.Web.Areas.Web.Models;
using Himall.Web.Framework;

namespace Himall.Web.Areas.Web.Controllers
{
	public class OrderController : BaseMemberController
	{

		/// <summary>
		/// 提交订单页面显示
		/// </summary>
		/// <param name="cartItemIds">提交的购物车物品集合</param>
		/// <param name="regionId">客户送货区域标识</param>
		public ActionResult Submit(string cartItemIds, long? regionId)
		{
			//Logo
			ViewBag.Logo = CurrentSiteSetting.Logo;//获取Logo
			ViewBag.Step = 2;
			//设置会员信息
			ViewBag.Member = CurrentUser;
			string cartInfo = WebHelper.GetCookie(CookieKeysCollection.HIMALL_CART);
            var submitModel = OrderApplication.Submit(cartItemIds, regionId, UserId, cartInfo);

			ViewBag.IsCashOnDelivery = submitModel.IsCashOnDelivery;
			ViewBag.IsLimitBuy = submitModel.IsLimitBuy;

			InitOrderSubmitModel(submitModel);
            #region 是否开启门店授权
            ViewBag.IsOpenStore = SiteSettingApplication.GetSiteSettings() != null && SiteSettingApplication.GetSiteSettings().IsOpenStore;
            #endregion
            return View(submitModel);
		}

		/// <summary>
		/// 点击立即购买调用的GET方法，但是重定向到了Submit页面
		/// </summary>
		/// <param name="skuIds">多个库存Id</param>
		/// <param name="counts">每个库存对应的数据量</param>
		/// <param name="regionId">客户收货地区的id</param>
		/// <param name="collpids">组合购Id集合</param>
		/// <returns>订单提交页面的数据</returns>
		public ActionResult SubmitByProductId(string skuIds, string counts, long? regionId, string collpids = null)
		{
			//Logo
			ViewBag.Logo = CurrentSiteSetting.Logo;//获取Logo
			//设置会员信息
			ViewBag.Member = CurrentUser;
			var submitModel = OrderApplication.SubmitByProductId(UserId, skuIds, counts, regionId, collpids);

			ViewBag.IsCashOnDelivery = submitModel.IsCashOnDelivery;
			ViewBag.IsLimitBuy = submitModel.IsLimitBuy;

			InitOrderSubmitModel(submitModel);
             #region 是否开启门店授权
            ViewBag.IsOpenStore = SiteSettingApplication.GetSiteSettings() != null && SiteSettingApplication.GetSiteSettings().IsOpenStore;
            #endregion
			return View("Submit", submitModel);
		}

		/// <summary>
		/// 获取当前用户收货地址列表
		/// </summary>
		/// <returns>收货地址列表</returns>
		[HttpPost]
		public JsonResult GetUserShippingAddresses()
		{
			var json = OrderApplication.GetUserShippingAddresses(UserId);
			return Json(json);
		}

		/// <summary>
		/// 从购物车中提交订单时调用的POST方法
		/// </summary>
		/// <param name="model"></param>
		/// <returns>返回订单列表和调转地址</returns>
		[HttpPost]
		public JsonResult SubmitOrder(CommonModel.OrderPostModel model)
		{
			model.CurrentUser = CurrentUser;
            model.DistributionUserLinkId = GetDistributionUserLinkId();
			model.PlatformType = (int)PlatformType.PC;

			var result = OrderApplication.SubmitOrder(model);
           
			ClearDistributionUserLinkId();   //清理分销cookie
			return Json(new { success = result.Success, orderIds = result.OrderIds, redirect = result.OrderTotal > 0 });
		}

		/// <summary>
		/// 限时购订单提交
		/// </summary>
		/// <param name="skuIds">库存ID</param>
		/// <param name="counts">购买数量</param>
		/// <param name="recieveAddressId">客户收货区域ID</param
        /// 
        /// 
		/// <param name="couponIds">优惠卷</param>
		/// <param name="invoiceType">发票类型0不要发票1增值税发票2普通发票</param>
		/// <param name="invoiceTitle">发票抬头</param>
		/// <param name="invoiceContext">发票内容</param>
		/// <param name="integral">使用积分</param>
		/// <param name="collpIds">组合构ID</param>
		/// <param name="isCashOnDelivery">是否货到付款</param>
		/// <returns>redis方式返回虚拟订单ID，数据库方式返回实际订单ID</returns>
		[HttpPost]
		public JsonResult SubmitLimitOrder(CommonModel.OrderPostModel model)
		{
			model.CurrentUser = CurrentUser;
			model.PlatformType = (int)PlatformType.PC;
			var result = OrderApplication.GetLimitOrder(model);
			if (LimitOrderHelper.IsRedisCache())
			{
				string id = "";
				SubmitOrderResult r = LimitOrderHelper.SubmitOrder(result, out id);
				if (r == SubmitOrderResult.SoldOut)
					throw new HimallException("已售空");
				else if (r == SubmitOrderResult.NoSkuId)
					throw new InvalidPropertyException("创建订单的时候，SKU为空，或者数量为0");
				else if (r == SubmitOrderResult.NoData)
					throw new InvalidPropertyException("参数错误");
				else if (r == SubmitOrderResult.NoLimit)
					throw new InvalidPropertyException("没有限时购活动");
				else if (string.IsNullOrEmpty(id))
					throw new InvalidPropertyException("参数错误");
				else
				{
					OrderApplication.UpdateDistributionUserLink(GetDistributionUserLinkId().ToArray(), UserId);
					return Json(new { success = true, Id = id });
				}
			}
			else
			{
				var orderIds = OrderApplication.OrderSubmit(result);
				return Json(new { success = true, orderIds = orderIds });
			}
		}

		/// <summary>
		/// 确认零元订单
		/// </summary
		/// <param name="orderIds">订单ID集合</param>
		/// <returns>返回付款成功</returns>
		[HttpPost]
		public ActionResult PayConfirm(string orderIds)
		{
			if (string.IsNullOrEmpty(orderIds))
			{
				return RedirectToAction("index", "userCenter", new { url = "/userOrder", tar = "userOrder" });
			}
			OrderApplication.PayConfirm(UserId, orderIds);
			return RedirectToAction("ReturnSuccess", "pay", new { id = orderIds });
		}

		/// <summary>
		/// 进入支付页面
		/// </summary>
		/// <param name="orderIds">订单Id集合</param>
		/// <returns></returns>
		public ActionResult Pay(string orderIds)
		{
			//网站根目录
			string webRoot = Request.Url.Scheme + "://" + HttpContext.Request.Url.Host + (HttpContext.Request.Url.Port == 80 ? "" : (":" + HttpContext.Request.Url.Port.ToString()));
			var result = OrderApplication.GetPay(UserId, orderIds, webRoot);
			if (!result.IsSuccess)
			{
				throw new HimallException(result.Msg);
			}
			else
			{
				ViewBag.Orders = result.Orders;
				ViewBag.OrderIds = orderIds;
				ViewBag.TotalAmount = result.TotalAmount;
				ViewBag.Logo = CurrentSiteSetting.Logo;//获取Logo
				if (result.TotalAmount == 0)
				{
					return View("PayConfirm");
				}
				else
				{
					ViewBag.HaveNoSalePro = result.HaveNoSalePro;
					ViewBag.Step = 1;//支付第一步
					ViewBag.UnpaidTimeout = CurrentSiteSetting.UnpaidTimeout;
					ViewBag.Capital = result.Capital;
					return View(result.Models);
				}
			}
		}

		//TODO:【2015-09-07】是否设置支付密码
		/// <summary>
		/// 判断是否设置支付密码
		/// </summary>
		public JsonResult GetPayPwd()
		{
			bool result = false;
			result = OrderApplication.GetPayPwd(UserId);
			return Json(new { success = result });
		}

		//TODO:【2015-09-01】预付款支付
		/// <summary>
		/// 预付款支付
		/// </summary>
		/// <param name="orderIds">订单Id集合</param>
		/// <param name="pwd">密码</param>
		/// <returns>支付结果</returns>
		public JsonResult PayByCapital(string orderIds, string pwd)
		{
			OrderApplication.PayByCapital(UserId, orderIds, pwd, Request.Url.Host.ToString());
			return Json(new { success = true, msg = "支付成功" });
		}

		//TODO:增加资产充值
		/// <summary>
		/// 增加资产充值
		/// </summary>
		/// <param name="orderIds">订单id集合</param>
		/// <returns></returns>
		public ActionResult ChargePay(string orderIds)
		{
			string webRoot = Request.Url.Scheme + "://" + HttpContext.Request.Url.Host + (HttpContext.Request.Url.Port == 80 ? "" : (":" + HttpContext.Request.Url.Port.ToString()));
			if (string.IsNullOrEmpty(orderIds))
			{
				return RedirectToAction("index", "userCenter", new { url = "/UserCapital", tar = "UserCapital" });
			}
			var viewmodel = OrderApplication.ChargePay(UserId, orderIds, webRoot);
			return View(viewmodel);
		}

		/// <summary>
		/// 获取运费
		/// </summary>
		/// <param name="addressId">地址ID</param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		[HttpPost]
		public JsonResult CalcFreight(int addressId, CalcFreightparameter[] parameters)
		{
			var result = OrderApplication.CalcFreight(addressId, parameters.GroupBy(p=>p.ShopId).ToDictionary(p=>p.Key,p=>p.GroupBy(pp=>pp.ProductId).ToDictionary(pp=>pp.Key,pp=>pp.Sum(ppp=>ppp.Count))));
			if (result.Count == 0)
				return Json(new { success = false, msg = "计算运费失败" });
			else
				return Json(new { success = true, freights = result.Select(p => new { shopId = p.Key, freight = p.Value }).ToArray() });
		}

		/// <summary>
		/// 设置发票抬头
		/// </summary>
		/// <param name="name">抬头名称</param>
		/// <returns>返回抬头ID</returns>
		[HttpPost]
		public JsonResult SaveInvoiceTitle(string name)
		{
			return Json(OrderApplication.SaveInvoiceTitle(UserId, name));
		}

		/// <summary>
		/// 删除发票抬头
		/// </summary>
		/// <param name="id">抬头ID</param>
		/// <returns>是否完成</returns>
		[HttpPost]
		public JsonResult DeleteInvoiceTitle(long id)
		{
			OrderApplication.DeleteInvoiceTitle(id);
			return Json(true);
		}

		/// <summary>
		/// 是否支持货到付款
		/// </summary>
		/// <param name="addreddId"></param>
		/// <returns></returns>
		public JsonResult IsCashOnDelivery(long addreddId)
		{
			var result= PaymentConfigApplication.IsCashOnDelivery(addreddId);

			return Json(result, JsonRequestBehavior.AllowGet);
		}

		#region 私有方法
		private void InitOrderSubmitModel(DTO.OrderSubmitModel model)
		{
			if (model.address != null)
			{
				var query = new CommonModel.ShopBranchQuery();
				query.Status = CommonModel.ShopBranchStatus.Normal;

				var region = RegionApplication.GetRegion(model.address.RegionId, CommonModel.Region.RegionLevel.City);
				query.AddressPath = region.GetIdPath();

				foreach (var item in model.products)
				{
					query.ShopId = item.shopId;
					query.ProductIds = item.freightProductGroup.Select(p => p.ProductId).ToArray();
					item.ExistShopBranch = ShopBranchApplication.Exists(query);
				}
			}
		}
		#endregion
	}

	/// <summary>
	/// 服务器异步处理限时购订单
	/// </summary>
	public class OrderStateController : BaseAsyncController
	{
		public void CheckAsync(string id)
		{
			AsyncManager.OutstandingOperations.Increment();
			int interval = 3000;//定义刷新间隔为200ms
			int maxWaitingTime = 15 * 1000;//定义最大等待时间为15s
			long[] orderIds;
			string message = "";
			Task.Factory.StartNew(() =>
			{
				int time = 0;
				while (true)
				{
					time += interval;
					System.Threading.Thread.Sleep(interval);
					OrderState state = LimitOrderHelper.GetOrderState(id, out message, out orderIds);
					if (state == OrderState.Processed)//已处理
					{
						AsyncManager.Parameters["state"] = state.ToString();
						AsyncManager.Parameters["message"] = message;
						AsyncManager.Parameters["ids"] = orderIds;
						break;
					}
					else if (state == OrderState.Untreated)//未处理
					{
						if (time > maxWaitingTime)
						{//大于最大等待时间
							AsyncManager.Parameters["state"] = state.ToString();
							AsyncManager.Parameters["message"] = message;
							AsyncManager.Parameters["ids"] = null;
							break;
						}
						else
							continue;
					}
					else//出错
					{
						AsyncManager.Parameters["state"] = state.ToString();
						AsyncManager.Parameters["message"] = message;
						AsyncManager.Parameters["ids"] = null;
						break;
					}
				}
				AsyncManager.OutstandingOperations.Decrement();
			});
		}


		public JsonResult CheckCompleted(string state, string message, long[] ids)
		{
			return Json(new { state = state, message = message, orderIds = ids }, JsonRequestBehavior.AllowGet);
		}
	}
}


