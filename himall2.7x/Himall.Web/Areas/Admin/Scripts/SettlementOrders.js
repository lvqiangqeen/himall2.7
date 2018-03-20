var datacols = [[
    {
        field: "OrderId", title: '订单号', width: 'auto',
        formatter: function (value, row, index) {
            return '<a href="/Admin/order/Detail/' + value + '">' + value + '</a>';
        }
    },
    { field: "ShopName", title: "商家", width: 'auto', align: "center" },
    { field: "SettledTime", title: "结算时间", width: 'auto', align: "center" },
    { field: "RecognizedAmount", title: "入帐金额", width: 'auto', align: "center" },
    {
        field: "SettlementAmount", title: "结算金额", width: 'auto', align: "center",

        formatter: function (value, row, index) {
            return '<a href="/Admin/Billing/SettlementDetail/' + row.OrderId + '">' + value + '</a>';
        }
    },
    { field: "OrderAmount", title: "订单实付", width: 'auto', align: "center" },
    { field: "PlatCommission", title: "平台佣金", width: 'auto', align: "center" },
    { field: "DistributorCommission", title: "分销佣金", width: 'auto', align: "center" },
    { field: "OpenCommission", title: "开团奖励", width: 'auto', align: "center" },
    { field: "JoinCommission", title: "成团返现", width: 'auto', align: "center" },

    { field: "OrderFinshTime", title: "订单完成时间", width: 'auto', align: "center" }

]];

var detailId = GetQueryString('detailId');
var shopId = GetQueryString('shopId');
$(function () {
	//组合显示字段
	//订单表格
	$("#list").hiMallDatagrid({
		url: 'SettlementOrderList',
		nowrap: false,
		rownumbers: true,
		NoDataMsg: '没有找到符合条件的已结算订单记录',
		border: false,
		fit: true,
		fitColumns: true,
		pagination: true,
		idField: "OrderId",
		pageSize: 15,
		pagePosition: 'bottom',
		operationButtons: "#operationButtons",
		pageNumber: 1,
		queryParams: { shopId:shopId,detailId: detailId },
		columns: datacols
	});

	$('#searchButton').click(function (e) {
		searchClose(e);
		var startDate = $("#inputStartDate").val();
		var endDate = $("#inputEndDate").val();
		var BillingstartDate = $("#BillingStartDate").val();
		var BillingendDate = $("#BillingEndDate").val();
		var orderId = $.trim($('#txtOrderId').val());
		var paymentName = $("#payment").val();
		$("#list").hiMallDatagrid('reload', {
			startDate: startDate, endDate: endDate, BillingstartDate: BillingstartDate,
			BillingendDate: BillingendDate, orderId: orderId, paymentName: paymentName,
			shopId: shopId, detailId: detailId
		});
	})
});


$(function () {
	$(".start_datetime").datetimepicker({
		language: 'zh-CN',
		format: 'yyyy-mm-dd',
		autoclose: true,
		weekStart: 1,
		minView: 2
	});
	$(".end_datetime").datetimepicker({
		language: 'zh-CN',
		format: 'yyyy-mm-dd',
		autoclose: true,
		weekStart: 1,
		minView: 2
	});
	//$(".start_datetime").click(function () {
	//    $('.end_datetime').datetimepicker('show');
	//});
	//$(".end_datetime").click(function () {
	//    $('.start_datetime').datetimepicker('show');
	//});
	$('.start_datetime').on('changeDate', function () {
		if ($(".end_datetime").val()) {
			if ($(".start_datetime").val() > $(".end_datetime").val()) {
				$('.end_datetime').val($(".start_datetime").val());
			}
		}

		$('.end_datetime').datetimepicker('setStartDate', $(".start_datetime").val());
	});


});

function ExportExecl() {
	var startDate = $("#inputStartDate").val();
	var endDate = $("#inputEndDate").val();
	var BillingstartDate = $("#BillingStartDate").val();
	var BillingendDate = $("#BillingEndDate").val();
	var orderId = $.trim($('#txtOrderId').val());
	var paymentName = $("#payment").val();
	var href = $(this).attr('href').split('?')[0] + '?startDate={0}&endDate={1}&BillingstartDate={2}&BillingendDate={3}&shopId={4}&paymentName={5}&detailId={6}'.format(startDate, endDate, BillingstartDate, BillingendDate, shopId, paymentName,detailId);
	$(this).attr('href', href);
}