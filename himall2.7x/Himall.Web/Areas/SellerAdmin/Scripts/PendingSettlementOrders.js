var datacols = [[
    {
        field: "OrderId", title: '订单号', width: 120,
        formatter: function (value, row, index) {
            return '<a title="' + row.SettlementCycle + '" href="/SellerAdmin/order/Detail/' + value + '" target="_blank">' + value + '</a>';
        }
    },
    { field: "Status", title: "订单状态", width: 200, align: "center" },
    { field: "OrderAmount", title: "订单金额", width: 80, align: "center" },
    { field: "PlatCommission", title: "平台佣金", width: 80, align: "center" },
    { field: "DistributorCommission", title: "分销佣金", width: 80, align: "center" },
    { field: "RefundAmount", title: "退款金额", width: 80, align: "center" },
    { field: "OpenCommission", title: "开团奖励", width: 80, align: "center" },
    { field: "JoinCommission", title: "成团返现", width: 80, align: "center" },
    { field: "SettlementAmount", title: "结算金额", width: 80, align: "center" },
    { field: "OrderFinshTimeStr", title: "订单完成时间", width: 160, align: "center" },
    {
        field: "operation", title: '查看详情', width: 120,
        formatter: function (value, row, index) {
            return '<a href="/SellerAdmin/Billing/PendingSettlementDetail/' + row.OrderId + '">查看详情</a>';
        }
    }
]];

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
        idField: "Id",
        pageSize: 15,
        pagePosition: 'bottom',
        operationButtons: "#operationButtons",
        pageNumber: 1,
        queryParams: {},
        columns: datacols
    });

    $('#searchButton').click(function (e) {
        searchClose(e);
        var startDate = $("#inputStartDate").val();
        var endDate = $("#inputEndDate").val();
        var orderId = $.trim($('#txtOrderId').val());
        $("#list").hiMallDatagrid('reload', { startDate: startDate, endDate: endDate, orderId: orderId });
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

function ExportExecl()
{
	var inputStartDate = $('#inputStartDate').val();
	var inputEndDate = $('#inputEndDate').val();
	var txtOrderId = $('#txtOrderId').val();
	var href = $(this).attr('href').split('?')[0] + '?startDate={0}&endDate={1}&orderId={2}'.format(inputStartDate, inputEndDate, txtOrderId);
	$(this).attr('href', href);
}

