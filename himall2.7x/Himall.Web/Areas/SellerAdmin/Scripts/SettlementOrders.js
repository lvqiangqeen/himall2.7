var datacols = [[
    {
        field: "OrderId", title: '订单号', width: 120,
        formatter: function (value, row, index) {
            return '<a href="/SellerAdmin/order/Detail/' + value + '">' + value + '</a>';
        }
    },
    { field: "SettledTime", title: "结算时间", width: 200, align: "center" },
    {
        field: "SettlementAmount", title: "结算金额", width: 80, align: "center",

        formatter: function (value, row, index) {
            return '<a href="/SellerAdmin/Billing/SettlementDetail/' + row.OrderId + '">' + value + '</a>';
        }

    },
    { field: "OrderAmount", title: "订单金额", width: 80, align: "center" },
    { field: "PlatCommission", title: "平台佣金", width: 80, align: "center" },
    { field: "DistributorCommission", title: "分销佣金", width: 80, align: "center" },
    { field: "RefundAmount", title: "退款金额", width: 80, align: "center" },
    { field: "OpenCommission", title: "开团奖励", width: 80, align: "center" },
    { field: "JoinCommission", title: "成团返现", width: 80, align: "center" },
    { field: "OrderFinshTime", title: "订单完成时间", width: 160, align: "center" }

]];

var detailId = GetQueryString('detailId');
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
        queryParams: {detailId: detailId },
        columns: datacols
    });

    $('#searchButton').click(function (e) {
        searchClose(e);
        var startDate = $("#inputStartDate").val();
        var endDate = $("#inputEndDate").val();
        var BillingstartDate = $("#BillingStartDate").val();
        var BillingendDate = $("#BillingEndDate").val();
        var orderId = $.trim($('#txtOrderId').val());
        $("#list").hiMallDatagrid('reload', { startDate: startDate, endDate: endDate, BillingstartDate: BillingstartDate, BillingendDate: BillingendDate, orderId: orderId,detailId:detailId });
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
	var inputStartDate = $('#inputStartDate').val();
	var inputEndDate = $('#inputEndDate').val();
	var txtOrderId = $('#txtOrderId').val();
	var billingstartDate = $("#BillingStartDate").val();
	var billingendDate = $("#BillingEndDate").val();
	var orderId = $.trim($('#txtOrderId').val());
	var href = $(this).attr('href').split('?')[0] + '?startDate={0}&endDate={1}&billingstartDate={2}&billingendDate={3}&orderId={4}&detailId={5}'.format(inputStartDate, inputEndDate, billingstartDate, billingendDate, txtOrderId,detailId);
	$(this).attr('href', href);
}