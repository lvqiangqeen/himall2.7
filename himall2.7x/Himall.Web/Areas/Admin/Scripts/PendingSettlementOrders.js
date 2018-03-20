var datacols = [[
    {
        field: "OrderId", title: '订单号', width: 'auto',
        formatter: function (value, row, index) {
            return '<a title="' + row.SettlementCycle + '" href="/Admin/order/Detail/' + value + '">' + value + '</a>';
        }
    },

    { field: "Status", title: "订单状态", width: 'auto', align: "center" },
    { field: "ShopName", title: "店铺名称", width: 'auto', align: "center" },
    { field: "OrderAmount", title: "订单金额", width: 'auto', align: "center" },
    { field: "PlatCommission", title: "平台佣金", width: 'auto', align: "center" },
    { field: "DistributorCommission", title: "分销佣金", width: 'auto', align: "center" },
    { field: "RefundAmount", title: "退款金额", width: 'auto', align: "center" },
    { field: "OpenCommission", title: "开团奖励", width: 80, align: "center" },
    { field: "JoinCommission", title: "成团返现", width: 80, align: "center" },
    { field: "SettlementAmount", title: "结算金额", width: 'auto', align: "center" },
    { field: "OrderFinshTimeStr", title: "订单完成时间", width: 'auto', align: "center" },
    { field: "PaymentTypeName", title: "支付方式", width: 'auto', align: "center" },
    {
        field: "operation", title: "查看详情", width: 'auto', align: "center",

        formatter: function (value, row, index) {
            return '<a href="/Admin/Billing/PendingSettlementDetail/' + row.OrderId + '">查看详情</a>';
        }
    }


]];

var shopId = GetQueryString('shopId');
$(function () {
	//组合显示字段
	//订单表格
	$("#list").hiMallDatagrid({
		url: 'PendingSettlementOrderList',
		nowrap: false,
		rownumbers: true,
		NoDataMsg: '没有找到符合条件的待结算订单记录',
		border: false,
		fit: true,
		fitColumns: true,
		pagination: true,
		idField: "OrderId",
		pageSize: 15,
		pagePosition: 'bottom',
		operationButtons: "#operationButtons",
		pageNumber: 1,
		queryParams: {shopId:shopId},
		columns: datacols
	});

	$('#searchButton').click(function (e) {
		searchClose(e);
		var startDate = $("#inputStartDate").val();
		var endDate = $("#inputEndDate").val();
		var orderId = $.trim($('#txtOrderId').val());
		var paymentName = $("#payment").val();
		$("#list").hiMallDatagrid('reload', { startDate: startDate, endDate: endDate, orderId: orderId, paymentName: paymentName, shopId: shopId });
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
	var orderId = $.trim($('#txtOrderId').val());
	var paymentName = $("#payment").val();
	var href = $(this).attr('href').split('?')[0] + '?startDate={0}&endDate={1}&shopId={2}&paymentName={3}&orderId={4}'.format(startDate, endDate,shopId, paymentName, orderId);
	$(this).attr('href', href);
}