using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using Himall.Application;
using Himall.Core;
using Himall.Core.Plugins;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Framework;
using Himall.Web.Models;
using Himall.Core.Helper;
using Himall.CommonModel;

namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    public class OrderController : BaseSellerController
    {

        private IOrderService _iOrderService;
        private IExpressService _iExpressService;
        private IRegionService _iRegionService;
        private IShopService _iShopService;
        private IProductService _iProductService;
        private IRefundService _iRefundService;
        private IDistributionService _iDistributionService;
        private IShopOpenApiService _iShopOpenApiService;
        private IFightGroupService _iFightGroupService;
        private IPaymentConfigService _iPaymentConfigService;
        public OrderController(IOrderService iOrderService,
            IExpressService iExpressService,
            IRegionService iRegionService,
            IShopService iShopService,
            IProductService iProductService,
            IRefundService iRefundService,
             IDistributionService iDistributionService,
             IShopOpenApiService iShopOpenApiService,
             IFightGroupService iFightGroupService,
            IPaymentConfigService iPaymentConfigService
            )
        {
            _iOrderService = iOrderService;
            _iExpressService = iExpressService;
            _iRegionService = iRegionService;
            _iShopService = iShopService;
            _iProductService = iProductService;
            _iRefundService = iRefundService;
            _iDistributionService = iDistributionService;
            _iShopOpenApiService = iShopOpenApiService;
            _iFightGroupService = iFightGroupService;
            _iPaymentConfigService = iPaymentConfigService;
        }

        public class SendGoodMode
        {
            public List<Himall.DTO.Order> Orders { get; set; }
            public IEnumerable<IExpress> LogisticsCompanies { get; set; }
        }
        public ActionResult Management()
        {
            var shopopen = _iShopOpenApiService.Get(CurrentSellerManager.ShopId);
            ViewBag.CanGoBills = false;
            if (shopopen != null)
            {
                if (!string.IsNullOrWhiteSpace(shopopen.AppKey))
                {
                    if (shopopen.IsEnable && shopopen.IsRegistered)
                    {
                        ViewBag.CanGoBills = true;
                    }
                }
            }
            #region 是否开启门店授权
            bool isOpenStore = SiteSettingApplication.GetSiteSettings() != null && SiteSettingApplication.GetSiteSettings().IsOpenStore;
            if (isOpenStore)
            {
                #region 商家下所有门店
                var data = ShopBranchApplication.GetShopBranchsAll(new ShopBranchQuery()
                {
                    ShopId = CurrentSellerManager.ShopId
                });
                ViewBag.StoreList = data.Models;
                #endregion
            }
            ViewBag.IsOpenStore = isOpenStore;
            #endregion
            var model = _iPaymentConfigService.GetPaymentTypes();
            return View(model);
        }


        public ActionResult Detail(long id, bool updatePrice = false)
        {
            OrderInfo order = _iOrderService.GetOrder(id);
            if (order == null || order.ShopId != CurrentSellerManager.ShopId)
            {
                throw new HimallException("订单已被删除，或者不属于该店铺！");
            }


            if (order.OrderType == OrderInfo.OrderTypes.FightGroup)
            {
                var fgord = _iFightGroupService.GetFightGroupOrderStatusByOrderId(order.Id);
                order.FightGroupOrderJoinStatus = fgord.GetJoinStatus;
                order.FightGroupCanRefund = fgord.CanRefund;
            }
            //if (order.ShopBranchId.HasValue && order.ShopBranchId.Value != 0)
            //{//补充数据
            //    var branch = ShopBranchApplication.GetShopBranchById(order.ShopBranchId.Value);
            //    if (branch != null)
            //        ViewBag.ShopBranchContactUser = branch.UserName;
            //}
            ViewBag.UpdatePrice = updatePrice;
            //ViewBag.Coupon = 0;
            //var coupon = ServiceHelper.Create<ICouponService>().GetCouponRecordInfo(order.UserId, order.Id);
            //if (coupon != null)
            //{
            //    ViewBag.Coupon = coupon.Himall_Coupon.Price;
            //}
            #region 门店信息
            if (order.ShopBranchId.HasValue && order.ShopBranchId.Value > 0)
            {
                var shopBranchInfo = ShopBranchApplication.GetShopBranchById(order.ShopBranchId.Value);
                if (shopBranchInfo != null)
                {
                    ViewBag.ShopBranchInfo = shopBranchInfo;
                    if (order.OrderStatus == OrderInfo.OrderOperateStatus.Finish) ViewBag.ShopBranchContactUser = shopBranchInfo.UserName;
                }
            }
            #endregion
            return View(order);

        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult List(OrderQuery query, int page, int rows)
        {
            query.ShopId = CurrentSellerManager.ShopId;
            query.PageNo = page;
            query.PageSize = rows;
            //var orders = OrderApplication.GetOrders(query);
            var fullOrders = OrderApplication.GetFullOrders(query);
            var models = fullOrders.Models.ToList();

            var shops = Application.ShopApplication.GetShops(models.Select(p => p.ShopId).ToArray());

            IEnumerable<OrderModel> orderModels = models.Select(item =>
            {
                var shop = shops.FirstOrDefault(sp => sp.Id == item.ShopId);
                return new OrderModel()
                {
                    OrderId = item.Id,
                    DeliveryType = item.DeliveryType,
                    OrderStatus = item.OrderStatus.ToDescription(),
                    OrderDate = item.OrderDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    ShopId = item.ShopId,
                    ShopName = item.ShopName,
                    ShopBranchName = item.ShopBranchName,
                    UserId = item.UserId,
                    UserName = item.UserName,
                    TotalPrice = item.OrderTotalAmount,
                    PaymentTypeName = item.PaymentTypeName,
                    IconSrc = GetIconSrc(item.Platform),
                    PlatForm = (int)item.Platform,
                    PlatformText = item.Platform.ToDescription(),
                    PaymentType = item.PaymentType,
                    PaymentTypeStr = item.PaymentType.ToDescription(),
                    OrderType = item.OrderType,
                    SellerRemark = item.SellerRemark,
                    SellerRemarkFlag = item.SellerRemarkFlag,
                    GatewayOrderId = item.GatewayOrderId,
                    Payee = shop.ContactsName,
                    CellPhone = item.CellPhone,
                    RegionFullName = item.RegionFullName,
                    Address = item.Address,
                    UserRemark = item.UserRemark,
                    ShipOrderNumber = item.ShipOrderNumber,
                    OrderItems = item.OrderItems,
                    ShopBranchId = item.ShopBranchId.HasValue ? item.ShopBranchId.Value : 0,
                    RegionId = item.RegionId
                };
            });
            orderModels = orderModels.ToList();

            #region 数据补偿
            //EDIT DZY [150624]
            List<long> ordidl = orderModels.Select(d => d.OrderId).ToList();
            if (ordidl.Count > 0)
            {
                RefundQuery refquery = new RefundQuery();
                refquery.OrderId = ordidl[0];
                refquery.MoreOrderId = ordidl;
                refquery.PageNo = 1;
                refquery.PageSize = orderModels.Count();
                var reflist = _iRefundService.GetOrderRefunds(refquery).Models.Where(d => d.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund && d.SellerAuditStatus != OrderRefundInfo.OrderRefundAuditStatus.UnAudit).ToList();


                foreach (var item in orderModels)
                {
                    //退款状态补偿
                    if (reflist.Count > 0)
                    {
                        var _tmpobj = reflist.FirstOrDefault(d => d.OrderId == item.OrderId);
                        if (_tmpobj != null && item.OrderStatus != OrderInfo.OrderOperateStatus.Close.ToDescription())
                        {
                            item.RefundStatusText = ((_tmpobj.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.Audited)
                                ? _tmpobj.ManagerConfirmStatus.ToDescription()
                                : ((item.DeliveryType == CommonModel.Enum.DeliveryType.SelfTake || (item.ShopBranchId.HasValue && item.ShopBranchId.Value > 0)) ? ((CommonModel.Enum.OrderRefundShopAuditStatus)_tmpobj.SellerAuditStatus).ToDescription() : _tmpobj.SellerAuditStatus.ToDescription()));
                        }
                    }

                    item.FightGroupCanRefund = true;   //非拼团订单默认可退
                    item.CanSendGood = true;   //非拼团订单默认可以发货

                    if (item.OrderStatus != OrderInfo.OrderOperateStatus.WaitDelivery.ToDescription())
                    {
                        item.CanSendGood = false;
                    }

                    //拼团状态补偿
                    if (item.OrderType == OrderInfo.OrderTypes.FightGroup)
                    {
                        var fgord = _iFightGroupService.GetFightGroupOrderStatusByOrderId(item.OrderId);
                        if (fgord != null)
                        {
                            item.FightGroupJoinStatus = fgord.GetJoinStatus;
                            item.FightGroupCanRefund = fgord.CanRefund;
                            item.CanSendGood = item.CanSendGood && fgord.CanSendGood;
                        }
                        else
                        {
                            item.FightGroupJoinStatus = CommonModel.FightGroupOrderJoinStatus.JoinFailed;
                            item.FightGroupCanRefund = false;
                            item.CanSendGood = false;
                        }
                    }

                }
            }
            #endregion

            DataGridModel<OrderModel> dataGrid = new DataGridModel<OrderModel>() { rows = orderModels, total = fullOrders.Total };
            return Json(dataGrid);

        }

        public JsonResult GoExpressBills()
        {
            Result result = new Result();
            result.success = false;
            result.status = -1;
            result.msg = "未开启开放平台";

            string gourl = "http://hierp.kuaidiantong.cn/ExpressBill/Allot?app_key={0}&timestamp={1}&sign={2}";
            var shopopen = _iShopOpenApiService.Get(CurrentSellerManager.ShopId);
            ViewBag.CanGoBills = false;
            if (shopopen != null)
            {
                if (!string.IsNullOrWhiteSpace(shopopen.AppKey) && !string.IsNullOrWhiteSpace(shopopen.AppSecreat))
                {
                    if (shopopen.IsEnable && shopopen.IsRegistered)
                    {
                        SortedDictionary<string, string> data = new SortedDictionary<string, string>();
                        data.Add("app_key", shopopen.AppKey);
                        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        data.Add("timestamp", timestamp);
                        string sign = Hishop.Open.Api.OpenApiSign.GetSign(data, shopopen.AppSecreat);
                        gourl = string.Format(gourl, shopopen.AppKey, timestamp, sign);

                        result.success = true;
                        result.status = 1;
                        result.msg = gourl;
                    }
                    if (!shopopen.IsRegistered)
                    {
                        result.msg = "未完成ERP电子面单系统注册";
                    }
                }
            }

            return Json(result);
        }

        public ActionResult ExportToExcel(OrderQuery query)
        {
            query.ShopId = CurrentSellerManager.ShopId;
            var orders = OrderApplication.GetFullOrdersNoPage(query);

            return ExcelView("ExportOrderinfo", "店铺订单信息", orders);
        }

        /// <summary>
        /// 获取订单来源图标地址
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        string GetIconSrc(PlatformType platform)
        {
            if (platform == PlatformType.IOS || platform == PlatformType.Android)
                return "/images/app.png";
            return string.Format("/images/{0}.png", platform.ToString());
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="payRemark">收款备注</param>
        /// <returns></returns>
        [ShopOperationLog(Message = "商家取消订单")]
        [HttpPost]
        public JsonResult CloseOrder(long orderId)
        {
            Result result = new Result();
            try
            {
                _iOrderService.SellerCloseOrder(orderId, CurrentSellerManager.UserName);
                result.success = true;
            }
            catch (Exception ex)
            {
                result.msg = ex.Message;
            }
            return Json(result);
        }

        public void DownloadProductList(string ids)
        {
            IEnumerable<long> idList = ids.Split(',').Select(item => long.Parse(item));

            var orderItems = _iOrderService.GetOrderItemsByOrderId(idList);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<html><head><meta http-equiv=Content-Type content=\"text/html; charset=gb2312\"></head><body>");
            sb.AppendLine("<table cellspacing=\"0\" cellpadding=\"5\" rules=\"all\" border=\"1\">");
            sb.AppendLine("<caption style='text-align:center;'>配货单(仓库拣货表)</caption>");
            sb.AppendLine("<tr style=\"font-weight: bold; white-space: nowrap;\">");
            sb.AppendLine("<td>商品名称</td>");
            sb.AppendLine("<td>货号</td>");
            sb.AppendLine("<td>规格</td>");
            sb.AppendLine("<td>拣货数量</td>");
            sb.AppendLine("<td>现库存数</td>");
            sb.AppendLine("</tr>");

            long stock = 0;
            SKUInfo sku;
            foreach (OrderItemInfo orderItem in orderItems)
            {
                sku = _iProductService.GetSku(orderItem.SkuId);
                if (sku != null)
                    stock = sku.Stock;

                sb.AppendLine("<tr>");
                sb.AppendLine("<td>" + orderItem.ProductName + "</td>");
                sb.AppendLine("<td style=\"vnd.ms-excel.numberformat:@\">" + orderItem.SKU + "</td>");
                sb.AppendLine("<td>" + orderItem.Color + orderItem.Size + orderItem.Version + "</td>");
                sb.AppendLine("<td>" + orderItem.Quantity + "</td>");
                sb.AppendLine("<td>" + (stock + orderItem.Quantity).ToString() + "</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</table>");
            sb.AppendLine("</body></html>");

            Response.Clear();
            Response.Buffer = false;
            Response.Charset = "GB2312";
            Response.AppendHeader("Content-Disposition", "attachment;filename=" + "productgoods_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls");
            Response.ContentEncoding = System.Text.Encoding.GetEncoding("GB2312");
            Response.ContentType = "application/ms-excel";
            //EnableViewState = false;
            Response.Write(sb.ToString());
            Response.End();
        }
        /// <summary>
        /// 商品配货表
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public FileResult DownloadOrderProductList(string ids)
        {
            HttpContext.Response.BufferOutput = true;
            string fileName = "goods_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";

            string UserAgent = Request.ServerVariables["http_user_agent"].ToLower();
            // Firfox和IE下输出中文名显示正常 
            if (UserAgent.IndexOf("firefox") == -1)
            {
                fileName = System.Web.HttpUtility.UrlEncode(fileName, System.Text.Encoding.UTF8);
            }
            //写入内容到Excel 
            NPOI.HSSF.UserModel.HSSFWorkbook hssfworkbook = writeProductToExcel(ids);
            //将Excel内容写入到流中 
            MemoryStream file = new MemoryStream();
            hssfworkbook.Write(file);
            file.Seek(0, SeekOrigin.Begin);
            return File(file, "application/vnd.ms-excel", fileName);

        }
        /// <summary>
        /// 订单配货表
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [UnAuthorize]
        public FileResult DownloadOrderList(string ids)
        {
            //IEnumerable<long> idList = ids.Split(',').Select(item => long.Parse(item));

            //IEnumerable<OrderInfo> orders = _iOrderService.GetOrders(idList);

            //StringBuilder sb = new StringBuilder();
            //sb.AppendLine("<html><head><meta http-equiv=Content-Type content=\"text/html; charset=gb2312\"></head><body>");
            //sb.AppendLine("<table cellspacing=\"0\" cellpadding=\"5\" rules=\"all\" border=\"1\">");
            //sb.AppendLine("<caption style='text-align:center;'>配货单(仓库拣货表)</caption>");
            //sb.AppendLine("<tr style=\"font-weight: bold; white-space: nowrap;\">");
            //sb.AppendLine("<td>订单单号</td>");
            //sb.AppendLine("<td>商品名称</td>");
            //sb.AppendLine("<td>货号</td>");
            //sb.AppendLine("<td>规格</td>");
            //sb.AppendLine("<td>拣货数量</td>");
            //sb.AppendLine("<td>现库存数</td>");
            //sb.AppendLine("<td>备注</td>");
            //sb.AppendLine("</tr>");

            //long stock = 0;
            //SKUInfo sku;

            //foreach (OrderInfo order in orders.ToList())
            //{
            //    foreach (OrderItemInfo orderItem in order.OrderItemInfo.ToList())
            //    {
            //        sku = _iProductService.GetSku(orderItem.SkuId);
            //        if (sku != null)
            //            stock = sku.Stock;

            //        sb.AppendLine("<tr>");
            //        sb.AppendLine("<td style=\"vnd.ms-excel.numberformat:@\">" + order.Id + "</td>");
            //        sb.AppendLine("<td>" + orderItem.ProductName + "</td>");
            //        sb.AppendLine("<td style=\"vnd.ms-excel.numberformat:@\">" + orderItem.SKU + "</td>");
            //        sb.AppendLine("<td>" + orderItem.Color + orderItem.Size + orderItem.Version + "</td>");
            //        sb.AppendLine("<td>" + orderItem.Quantity + "</td>");
            //        sb.AppendLine("<td>" + (stock + orderItem.Quantity).ToString() + "</td>");
            //        sb.AppendLine("<td>" + order.UserRemark + "</td>");
            //        sb.AppendLine("</tr>");
            //    }
            //}
            //sb.AppendLine("</table>");
            //sb.AppendLine("</body></html>");
            //// 输出
            ////
            //Response.Clear();
            //Response.Buffer = false;
            //Response.Charset = "GB2312";
            //Response.AppendHeader("Content-Disposition", "attachment;filename=" + "ordergoods_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls");
            //Response.ContentEncoding = System.Text.Encoding.GetEncoding("GB2312");
            //Response.ContentType = "application/ms-excel";
            //Response.Write(sb.ToString());
            //Response.End();


            HttpContext.Response.BufferOutput = true;
            string fileName = "ordergoods_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";

            string UserAgent = Request.ServerVariables["http_user_agent"].ToLower();
            // Firfox和IE下输出中文名显示正常 
            if (UserAgent.IndexOf("firefox") == -1)
            {
                fileName = System.Web.HttpUtility.UrlEncode(fileName, System.Text.Encoding.UTF8);
            }
            //写入内容到Excel 
            NPOI.HSSF.UserModel.HSSFWorkbook hssfworkbook = writeToExcel(ids);
            //将Excel内容写入到流中 
            MemoryStream file = new MemoryStream();
            hssfworkbook.Write(file);
            file.Seek(0, SeekOrigin.Begin);
            return File(file, "application/vnd.ms-excel", fileName);

        }
        /// <summary>
        /// 导出商品配货单
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        private NPOI.HSSF.UserModel.HSSFWorkbook writeProductToExcel(string ids)
        {
            SKUInfo sku;
            long stock = 0;
            string productCode = string.Empty;
            IEnumerable<long> idList = ids.Split(',').Select(item => long.Parse(item));
            IEnumerable<OrderInfo> orders = _iOrderService.GetOrders(idList);

            var items = new List<PrepareGoodsModel> { };
            foreach (OrderInfo order in orders.ToList())
            {
                foreach (OrderItemInfo item in order.OrderItemInfo.ToList())
                {
                    sku = _iProductService.GetSku(item.SkuId);
                    if (sku != null)
                    {
                        stock = sku.Stock + item.Quantity;
                        productCode = sku.ProductInfo.ProductCode;
                    }
                    else
                    {
                        stock = 0;
                        productCode = string.Empty;
                    }

                    items.Add(new PrepareGoodsModel
                    {
                        SkuId = item.SkuId,
                        ProductName = item.ProductName,
                        ProductCode = productCode,
                        Standard = item.Color + item.Size + item.Version,
                        Quantity = item.Quantity,
                        Stock = stock
                    });
                }
            }
            var prepares = items.GroupBy(e => e.SkuId).Select(e => new PrepareGoodsModel
            {
                SkuId = e.Key,
                ProductName = e.FirstOrDefault().ProductName,
                ProductCode = e.FirstOrDefault().ProductCode,
                Standard = e.FirstOrDefault().Standard,
                Quantity = e.Sum(item => item.Quantity),
                Stock = e.FirstOrDefault().Stock
            });

            NPOI.HSSF.UserModel.HSSFWorkbook hssfworkbook = new NPOI.HSSF.UserModel.HSSFWorkbook();

            NPOI.HSSF.UserModel.HSSFSheet excelSheet = (NPOI.HSSF.UserModel.HSSFSheet)hssfworkbook.CreateSheet("sheet1");

            NPOI.SS.UserModel.Row row0 = excelSheet.CreateRow(0);
            string[] columns ={
                                  "商品名称","货号","规格","拣货数量","现库存数"
                             };
            for (int i = 0; i < columns.Length; i++)
            {
                NPOI.SS.UserModel.Cell cell = CreateCell(i, row0);
                cell.SetCellValue(columns[i]);
            }

            foreach (var orderItem in prepares)
            {
                int rows = excelSheet.PhysicalNumberOfRows;
                NPOI.SS.UserModel.Row row = CreateRow(rows, excelSheet);
                NPOI.SS.UserModel.Cell cell0 = CreateCell(0, row);
                NPOI.SS.UserModel.CellStyle style = hssfworkbook.CreateCellStyle();
                NPOI.SS.UserModel.DataFormat format = hssfworkbook.CreateDataFormat();
                style.DataFormat = format.GetFormat("####################");
                cell0.CellStyle = style;
                cell0.SetCellValue(orderItem.ProductName);
                cell0 = CreateCell(1, row);
                cell0.SetCellValue(orderItem.ProductCode);
                cell0 = CreateCell(2, row);
                cell0.SetCellValue(orderItem.Standard);
                cell0 = CreateCell(3, row);
                cell0.SetCellValue(orderItem.Quantity);
                cell0 = CreateCell(4, row);
                cell0.SetCellValue(orderItem.Stock);
            }
            return hssfworkbook;
        }
        private NPOI.HSSF.UserModel.HSSFWorkbook writeToExcel(string ids)
        {
            SKUInfo sku;
            long stock = 0;
            string productCode = string.Empty;
            IEnumerable<long> idList = ids.Split(',').Select(item => long.Parse(item));
            IEnumerable<OrderInfo> orders = _iOrderService.GetOrders(idList);

            NPOI.HSSF.UserModel.HSSFWorkbook hssfworkbook = new NPOI.HSSF.UserModel.HSSFWorkbook();

            NPOI.HSSF.UserModel.HSSFSheet excelSheet = (NPOI.HSSF.UserModel.HSSFSheet)hssfworkbook.CreateSheet("sheet1");

            NPOI.SS.UserModel.Row row0 = excelSheet.CreateRow(0);
            string[] columns ={
                                  "订单单号","商品名称","货号","规格","拣货数量","现库存数","备注"
                             };
            for (int i = 0; i < columns.Length; i++)
            {
                NPOI.SS.UserModel.Cell cell = CreateCell(i, row0);
                cell.SetCellValue(columns[i]);
            }

            foreach (OrderInfo order in orders.ToList())
            {
                foreach (OrderItemInfo orderItem in order.OrderItemInfo.ToList())
                {
                    sku = _iProductService.GetSku(orderItem.SkuId);
                    if (sku != null)
                    {
                        stock = sku.Stock;
                        productCode = sku.ProductInfo.ProductCode;
                    }
                    else
                    {
                        stock = 0;
                        productCode = string.Empty;
                    }

                    int rows = excelSheet.PhysicalNumberOfRows;
                    NPOI.SS.UserModel.Row row = CreateRow(rows, excelSheet);
                    NPOI.SS.UserModel.Cell cell0 = CreateCell(0, row);
                    NPOI.SS.UserModel.CellStyle style = hssfworkbook.CreateCellStyle();
                    NPOI.SS.UserModel.DataFormat format = hssfworkbook.CreateDataFormat();
                    style.DataFormat = format.GetFormat("####################");
                    cell0.CellStyle = style;
                    cell0.SetCellValue(order.Id);
                    cell0 = CreateCell(1, row);
                    cell0.SetCellValue(orderItem.ProductName);
                    cell0 = CreateCell(2, row);
                    cell0.SetCellValue(productCode);
                    cell0 = CreateCell(3, row);
                    cell0.SetCellValue(orderItem.Color + orderItem.Size + orderItem.Version);
                    cell0 = CreateCell(4, row);
                    cell0.SetCellValue(orderItem.Quantity);
                    cell0 = CreateCell(5, row);
                    cell0.SetCellValue((stock + orderItem.Quantity).ToString());
                    cell0 = CreateCell(6, row);
                    cell0.SetCellValue(order.UserRemark);

                }
            }


            return hssfworkbook;
        }

        private NPOI.SS.UserModel.Row CreateRow(int rowID, NPOI.HSSF.UserModel.HSSFSheet excelSheet)
        {
            NPOI.SS.UserModel.Row row = excelSheet.GetRow(rowID);
            if (row == null)
            {
                row = excelSheet.CreateRow(rowID);
            }
            return row;
        }

        private NPOI.SS.UserModel.Cell CreateCell(int cellID, NPOI.SS.UserModel.Row row)
        {
            NPOI.SS.UserModel.Cell cell = row.GetCell(cellID);
            if (cell == null)
            {
                cell = row.CreateCell(cellID);
            }
            return cell;
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult GetOrderPrint(string ids)
        {
            Result result = new Result();
            try
            {
                IEnumerable<long> idList = ids.Split(',').Select(item => long.Parse(item));

                IEnumerable<OrderInfo> orders = _iOrderService.GetOrders(idList);
                string siteName = CurrentSiteSetting.SiteName;
                StringBuilder sb = new StringBuilder();
                foreach (OrderInfo order in orders.ToList())
                {
                    sb.AppendFormat("<h3 class=\"print-order\"><strong>{0}店发货清单</strong></h3>", order.ShopName);
                    sb.Append("<div class=\"print-detail\">");
                    sb.AppendFormat("<span>订单号：{0}</span><span>下单时间：{1}</span>", order.Id, order.OrderDate.ToString("yyyy-MM-dd hh:mm:ss"));
                    sb.AppendFormat("<span>姓名：{0}</span><span>联系方式：{1}</span>", order.ShipTo, order.CellPhone);
                    sb.AppendFormat("<span>地址：{0}</span>", order.RegionFullName + " " + order.Address);
                    sb.Append("</div>");


                    sb.Append("<table class=\"table table-bordered print-tab\"><thead><tr><th>商品名称</th><th>规格</th><th>数量</th><th>单价</th><th>总价</th></tr></thead><tbody>");
                    foreach (OrderItemInfo orderItem in order.OrderItemInfo.ToList())
                    {
                        sb.Append("<tr>");
                        sb.AppendFormat("<td style=\"text-align:left\">{0}</td>", orderItem.ProductName);
                        sb.AppendFormat("<td>{0} {1} {2}</td>", orderItem.Color, orderItem.Size, orderItem.Version);
                        sb.AppendFormat("<td>{0}</td>", orderItem.Quantity);
                        sb.AppendFormat("<td>￥{0}</td>", orderItem.SalePrice);
                        sb.AppendFormat("<td>￥{0}</td>", orderItem.RealTotalPrice);
                        sb.Append("</tr>");
                    }
                    sb.AppendFormat("<tr><td style=\"text-align:right\" colspan=\"6\"><span>商品总价：￥{0} &nbsp; 运费：￥{1}</span> &nbsp; <b>实付金额：￥{2}</b></td></tr>",
                        order.ProductTotalAmount, order.Freight, order.OrderTotalAmount);
                    sb.AppendLine("</tbody></table>");
                    sb.Append("<div class=\"print-tags\">");
                    sb.AppendFormat("<p><strong>备注：</strong></p>");
                    sb.AppendFormat("<p class=\"tag-content\">{0}</p>", order.OrderRemarks);
                    sb.Append("</div>");

                }
                result.success = true;
                result.msg = sb.ToString();
            }
            catch (Exception)
            {
                result.success = false;
            }

            return Json(result);
        }


        [HttpPost]
        [UnAuthorize]
        public JsonResult UpdateAddress(long orderId, string shipTo, string cellPhone, int topRegionId, int regionId, string address)
        {
            Result result = new Result();
            try
            {
                string regionFullName = RegionApplication.GetFullName(regionId);
                _iOrderService.SellerUpdateAddress(orderId, CurrentSellerManager.UserName, shipTo, cellPhone, topRegionId, regionId, regionFullName, address);
                result.success = true;
            }
            catch (Exception ex)
            {
                result.msg = ex.Message;
            }
            return Json(result);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult UpdateItemDiscountAmount(long orderItemId, decimal discountAmount)
        {
            Result result = new Result();
            try
            {
                _iOrderService.SellerUpdateItemDiscountAmount(orderItemId, discountAmount, CurrentSellerManager.UserName);
                result.success = true;
            }
            catch (Exception ex)
            {
                result.msg = ex.Message;
            }
            return Json(result);
        }

        [HttpPost]
        public JsonResult UpdateOrderFrieght(long orderId, decimal frieght)
        {
            _iOrderService.SellerUpdateOrderFreight(orderId, frieght);
            return Json(new { success = true });
        }

        public ActionResult SendGood(string ids)
        {
            IEnumerable<long> idList = ids.Split(',').Select(item => long.Parse(item));

            var sendGoodMode = new SendGoodMode();
            var orders = OrderApplication.GetOrders(idList).Where(a => a.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery && a.ShopId == CurrentSellerManager.ShopId && (!a.ShopBranchId.HasValue || (a.ShopBranchId.HasValue && a.ShopBranchId.Value <= 0))).OrderByDescending(a => a.OrderDate);//商家后台发货应排除门店订单
            if (orders == null)
            {
                throw new HimallException("没有找到相关的订单" + ids);
            }
            sendGoodMode.Orders = orders.ToList();
            sendGoodMode.LogisticsCompanies = _iExpressService.GetAllExpress();
            return View(sendGoodMode);
        }

        [HttpPost]
        public JsonResult SendGood(string ids, string companyNames, string shipOrderNumbers)
        {
            var result = new Result();
            var returnurl = String.Format("http://{0}/Common/ExpressData/SaveExpressData", Request.Url.Authority);

            try
            {
                IEnumerable<long> idList = ids.Split(',').Select(item => long.Parse(item));
                string[] companyNameList = companyNames.Split(',');
                string[] shipOrderNumberList = shipOrderNumbers.Split(',');

                int index = 0;
                foreach (long id in idList)
                {
                    Application.OrderApplication.SellerSendGood(id, CurrentSellerManager.UserName, companyNameList[index], shipOrderNumberList[index], returnurl);
                    index++;
                }
                result.success = true;
            }
            catch (Exception ex)
            {
                Log.Error("商家发货操作失败", ex);
                result.msg = ex.Message;
            }

            return Json(result);
        }

        public ActionResult UpdateExpress(long id)
        {
            var sendGoodMode = new SendGoodMode();
            var order = OrderApplication.GetOrder(id);
            if (order == null || order.ShopId != CurrentSellerManager.ShopId)
            {
                throw new HimallException("没有找到相关的订单" + id);
            }
            sendGoodMode.Orders = new List<Himall.DTO.Order>() { order };
            sendGoodMode.LogisticsCompanies = _iExpressService.GetAllExpress();
            return View("SendGood", sendGoodMode);
        }

        [HttpPost]
        public JsonResult UpdateExpress(long ids, string companyNames, string shipOrderNumbers)
        {
            var result = new Result();
            var returnurl = String.Format("http://{0}/Common/ExpressData/SaveExpressData", Request.Url.Authority);

            try
            {
                Application.OrderApplication.UpdateExpress(ids, companyNames, shipOrderNumbers, returnurl);
                result.success = true;
            }
            catch (Exception ex)
            {
                result.msg = ex.Message;
            }

            return Json(result);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult GetExpressData(string expressCompanyName, string shipOrderNumber)
        {
            string kuaidi100Code = _iExpressService.GetExpress(expressCompanyName).Kuaidi100Code;
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(string.Format("http://www.kuaidi100.com/query?type={0}&postid={1}", kuaidi100Code, shipOrderNumber));
            request.Timeout = 8000;

            string content = "暂时没有此快递单号的信息";
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream stream = response.GetResponseStream();
                    System.IO.StreamReader streamReader = new StreamReader(stream, System.Text.Encoding.GetEncoding("UTF-8"));

                    // 读取流字符串内容
                    content = streamReader.ReadToEnd();
                    content = content.Replace("&amp;", "");
                    content = content.Replace("&nbsp;", "");
                    content = content.Replace("&", "");
                }
            }
            catch
            {
            }
            return Json(content);
        }

        public JsonResult GetRegionIdPath(long regionId)
        {
            return Json(_iRegionService.GetRegionPath(regionId));
        }


        [HttpPost]
        public JsonResult GetRegion(long? key = null, int? level = -1)
        {
            if (level == -1)
                key = 0;

            if (key.HasValue)
            {
                var regions = _iRegionService.GetRegion(key.Value);
                return Json(regions);
            }
            else
                return Json(new object[] { });
        }


        public JsonResult SellerRemark(long id, string remark, int? flag)
        {
            var shopId = CurrentShop.Id;
            if (!flag.HasValue)
            {
                flag = 1;
            }
            if (remark.Length == 0)
            {
                return Json(new Result() { success = false, msg = "备注不能为空！" });
            }
            OrderApplication.UpdateSellerRemark(id, shopId, remark, flag.Value);
            return Json(new Result() { success = true, msg = "添加备注成功！" });
        }


        public ActionResult Print(string orderIds)
        {
            Models.OrderPrintViewModel model = new Models.OrderPrintViewModel();
            var orderIds_long = orderIds.Split(',').Select(item => long.Parse(item));
            model.OrdersCount = orderIds_long.Count();
            var shop = _iShopService.GetShop(CurrentSellerManager.ShopId);
            model.SenderName = shop.SenderName;
            model.SenderAddress = shop.SenderAddress;
            model.SenderPhone = shop.SenderPhone;

            if (shop.SenderRegionId.HasValue)
            {
                var region = _iRegionService.GetRegionPath(shop.SenderRegionId.Value);
                model.SenderRegionId = shop.SenderRegionId.Value.ToString();
                model.FullRegionPath = region;
            }
            else
            {
                model.SenderRegionId = "";
                model.FullRegionPath = "";
            }


            var expresses = _iExpressService.GetRecentExpress(CurrentSellerManager.ShopId, int.MaxValue);
            model.Expresses = expresses;
            return View(model);
        }


        [HttpPost]
        public JsonResult Print(string orderIds, string expressName, string startNo, int regionId, string address, string senderName, string senderPhone)
        {
            var expressService = _iExpressService;
            var express = expressService.GetExpress(expressName);

            bool valid = express.CheckExpressCodeIsValid(startNo);
            if (!valid)
            {
                return Json(new { success = false, msg = "起始快递单号无效" });
            }
            else
            {

                //保存发货人信息
                _iShopService.UpdateShopSenderInfo(CurrentSellerManager.ShopId, regionId, address, senderName, senderPhone);


                var printElementIndexes = express.Elements.Select(item => item.PrintElementIndex);
                List<PrintModel> printModels = new List<PrintModel>();

                IEnumerable<long> orderIds_long = orderIds.Split(',').Select(item => long.Parse(item));

                foreach (long orderId in orderIds_long)
                {//为每个订单建立打印数据模型

                    //设置基本打印信息
                    PrintModel printModel = new PrintModel()
                    {
                        Width = express.Width,
                        Height = express.Height,
                        FontSize = 11

                    };

                    //获取打印元素对应订单中的实际内容
                    var dic = expressService.GetPrintElementIndexAndOrderValue(CurrentSellerManager.ShopId, orderId, printElementIndexes);

                    //获取打印元素
                    printModel.Elements = dic.Select(item =>
                    {
                        var expressElement = express.Elements.FirstOrDefault(t => t.PrintElementIndex == item.Key);

                        return new PrintModel.PrintElement()
                        {
                            X = expressElement.LeftTopPoint.X,
                            Y = expressElement.LeftTopPoint.Y,
                            Height = expressElement.RightBottomPoint.Y - expressElement.LeftTopPoint.Y,
                            Width = expressElement.RightBottomPoint.X - expressElement.LeftTopPoint.X,
                            Value = item.Value
                        };

                    });
                    printModels.Add(printModel);
                }

                //保存订单信息
                _iOrderService.SetOrderExpressInfo(CurrentSellerManager.ShopId, expressName, startNo, orderIds_long);

                return Json(new { success = true, data = printModels });

            }

        }

        /// <summary>
        /// 查询同一区域配送范围内的商家下门店
        /// </summary>
        /// <param name="areaId"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetArealShopBranch(int areaId, int shopId)
        {
            if (areaId <= 0)
                return Json(new { Success = false, Message = "请先选择区域！" }, JsonRequestBehavior.AllowGet);
            if (shopId <= 0)
                return Json(new { Success = false, Message = "无法确定商家！" }, JsonRequestBehavior.AllowGet);

            var shopBranchs = ShopBranchApplication.GetArealShopBranchsAll(areaId, shopId);
            return Json(new { Success = true, Models = shopBranchs.Models }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 商家手动给订单分配门店
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <param name="shopBranchId">要分配的门店ID</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DistributionStore(long orderId, long shopBranchId, long shopId)
        {
            var orderInfo = OrderApplication.GetOrder(orderId);//这里要查一次,靠传值不准确
            if (orderInfo == null)
                return Json(new { Success = false, Message = "获取订单错误！" });

            if (orderInfo.OrderStatus == Himall.Model.OrderInfo.OrderOperateStatus.WaitDelivery || orderInfo.OrderStatus == Himall.Model.OrderInfo.OrderOperateStatus.WaitPay)
            {
                var refunds = RefundApplication.GetOrderRefunds(new RefundQuery()
                {
                    OrderId = orderId,
                    ShopId = shopId,
                    PageSize = 1,
                    PageNo = 1
                });
                if (refunds != null && refunds.Total > 0) return Json(new { Success = false, Message = "订单正在申请售后，请先处理售后申请！" });

                var orderItems = OrderApplication.GetOrderItemsByOrderId(orderId);
                List<string> skuIds = orderItems.Where(p => p.ShopId == shopId && p.OrderId == orderId).Select(p => p.SkuId).ToList();//原则上一个订单只属于一个商家,所以此处可不判断shopId
                List<int> counts = orderItems.Where(p => p.ShopId == shopId && p.OrderId == orderId).Select(p => TypeHelper.ObjectToInt(p.Quantity)).ToList();

                if (shopBranchId > 0)//分配给门店
                {
                    var resultStock = InventoryThrough(shopBranchId, shopId, skuIds, counts);
                    if (!resultStock)
                        return Json(new { Success = false, Message = "分配失败，该门店库存不足！" });

                    if (orderInfo != null && orderInfo.ShopBranchId.HasValue && orderInfo.ShopBranchId.Value > 0)
                    {
                        if (orderInfo.ShopBranchId.Value != shopBranchId)
                        {
                            OrderApplication.DistributionStoreUpdateStockToNewShopBranch(skuIds, counts, shopBranchId, orderInfo.ShopBranchId.Value);
                            OrderApplication.UpdateOrderShopBranch(orderId, shopBranchId);// 更新订单所属门店为新分配的门店
                            orderInfo.ShopBranchId = shopBranchId;
                            if (orderInfo.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery) AddAppMessages(orderInfo);
                        }
                    }
                    else
                    {
                        OrderApplication.DistributionStoreUpdateStock(skuIds, counts, shopBranchId);
                        OrderApplication.UpdateOrderShopBranch(orderId, shopBranchId);
                        orderInfo.ShopBranchId = shopBranchId;
                        if (orderInfo.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery) AddAppMessages(orderInfo);
                    }
                }
                else//分配给商家
                {
                    if (orderInfo != null && orderInfo.ShopBranchId.HasValue && orderInfo.ShopBranchId.Value > 0)
                    {
                        OrderApplication.DistributionStoreUpdateStockToShop(skuIds, counts, orderInfo.ShopBranchId.Value);
                        OrderApplication.UpdateOrderShopBranch(orderId, 0);// 更新订单所属门店为商家总店，门店ID为0
                        orderInfo.ShopBranchId = 0;
                        if (orderInfo.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery) AddAppMessages(orderInfo);
                    }
                }
                return Json(new { Success = true, Message = "分配成功！" });
            }
            else
            {
                return Json(new { Success = false, Message = "订单只能在待付款、待发货状态下分配门店！" });
            }
        }

        /// <summary>
        /// 判断门店是否具有订单中所有商品的库存
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="skuIds"></param>
        /// <param name="counts"></param>
        /// <returns></returns>
        private static bool InventoryThrough(long shopBranchId, long shopId, List<string> skuIds, List<int> counts)
        {
            var skuInfos = ProductManagerApplication.GetSKUs(skuIds);
            ShopBranchQuery query = new ShopBranchQuery();
            query.ProductIds = skuInfos.Select(p => p.ProductId).ToArray();
            query.Status = CommonModel.ShopBranchStatus.Normal;
            query.Id = shopBranchId;
            query.PageSize = 1;
            query.PageNo = 1;
            query.ShopId = shopId;

            var data = ShopBranchApplication.GetShopBranchsAll(query);//这里用来判断是否当前要分配的门店是否具有该订单内所有的商品
            var shopBranchSkus = ShopBranchApplication.GetSkus(query.ShopId, data.Models.Select(p => p.Id));//获取商家下门店的SKU
            return data.Models.Any(p => skuInfos.All(skuInfo => shopBranchSkus.Any(sbSku => sbSku.ShopBranchId == p.Id && sbSku.Stock >= counts[skuInfos.IndexOf(skuInfo)] && sbSku.SkuId == skuInfo.Id)));
        }

        [HttpGet]
        public JsonResult GetShopBranchStock(long orderId, long shopBranchId, long shopId)
        {
            if (orderId <= 0)
                return Json(new { Success = false, Message = "获取订单错误" }, JsonRequestBehavior.AllowGet);

            if (shopBranchId <= 0)//如果传过来的门店ID为0，则分配给商家
                return Json(new { Success = true }, JsonRequestBehavior.AllowGet);

            var orderItems = OrderApplication.GetOrderItemsByOrderId(orderId);
            List<string> skuIds = orderItems.Where(p => p.ShopId == shopId && p.OrderId == orderId).Select(p => p.SkuId).ToList();
            List<int> counts = orderItems.Where(p => p.ShopId == shopId && p.OrderId == orderId).Select(p => TypeHelper.ObjectToInt(p.Quantity)).ToList();

            var resultStock = InventoryThrough(shopBranchId, shopId, skuIds, counts);
            if (resultStock)
                return Json(new { Success = true }, JsonRequestBehavior.AllowGet);

            return Json(new { Success = false, Message = "当前门店库存不足" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 新增门店/商家APP发货通知
        /// </summary>
        /// <param name="appMessages"></param>
        public static void AddAppMessages(DTO.Order orderInfo)
        {
            var app = new DTO.AppMessages()
            {
                Content = string.Format("{0} 等待您发货", orderInfo.Id),
                IsRead = false,
                SendTime = DateTime.Now,
                SourceId = orderInfo.Id,
                Title = "您有新的订单",
                TypeId = (int)AppMessagesType.Order,
                OrderPayDate = TypeHelper.ObjectToDateTime(orderInfo.PayDate),
                ShopId = 0,
                ShopBranchId = 0
            };
            if (orderInfo.ShopBranchId.HasValue && orderInfo.ShopBranchId.Value > 0)
            {
                app.ShopBranchId = orderInfo.ShopBranchId.Value;
            }
            else
                app.ShopId = orderInfo.ShopId;
            AppMessageApplication.AddAppMessages(app);
        }

        /// <summary>
        /// 区分售后状态
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="shopBranchId"></param>
        /// <param name="shopId"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetRrefundType(long orderId, long shopId)
        {
            var queryModel = new RefundQuery()
            {
                OrderId = orderId,
                ShopId = shopId,
                PageSize = 1,
                PageNo = 1
            };
            var refunds = Application.RefundApplication.GetOrderRefunds(queryModel);
            if (refunds != null && refunds.Models.Count > 0)
            {
                var refundInfo = refunds.Models.FirstOrDefault();
                int showType = (refundInfo.RefundMode == OrderRefundInfo.OrderRefundMode.OrderItemRefund || refundInfo.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund) ? 2 : (refundInfo.RefundMode == OrderRefundInfo.OrderRefundMode.ReturnGoodsRefund ? 3 : 0);
                if (showType == 0)
                    return Json(new { Success = false, Message = "判断售后状态错误" }, JsonRequestBehavior.AllowGet);

                return Json(new { Success = true, showtype = showType }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Success = false, Message = "无法判断售后状态" }, JsonRequestBehavior.AllowGet);
        }
    }
}