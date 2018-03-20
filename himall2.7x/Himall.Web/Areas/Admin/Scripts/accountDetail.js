
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
    
    searchOrder();


});

function exportExcel() {
    var href = "/Admin/Account/DetailListExportExcel?accountId=" + accountId;
    var enumOrderTypeId = parseInt($("#selEnumOrderType").val());
    switch (enumOrderTypeId) {
        case 1:
            href += "&startDate=" + $("#inputStartDate").val() + "&endDate=" + $("#inputEndDate").val() + "&enumOrderTypeId=" + $("#selEnumOrderType").val()
            break;
        case 0:
            href += "&startDate=" + $("#inputStartDate").val() + "&endDate=" + $("#inputEndDate").val() + "&enumOrderTypeId=" + $("#selEnumOrderType").val()
            break;
        case 2:
            href = "/Admin/Account/AgreementDetailListExportExcel?accountId=" + accountId + "&startDate=" + $("#inputStartDate").val() + "&endDate=" + $("#inputEndDate").val() + "&enumOrderTypeId=" + $("#selEnumOrderType").val();
            break;
    }
    $("#exceptExcelA").attr("href", href);
}

function search() {
    var enumOrderTypeId = parseInt($("#selEnumOrderType").val());
    switch (enumOrderTypeId) {
        case 1:
            $("#ListAgreement").hide();
            $("#ListReturnOrder").hide();
            $("#ListBrokerage").hide();
            $("#ListOrder").show();
            searchOrder();
            break;
        case 0:
            $("#ListAgreement").hide();
            $("#ListOrder").hide();
            $("#ListReturnOrder").show();
            $("#ListBrokerage").hide();
            searchReturnOrder();
            break;
        case 2:
            $("#ListReturnOrder").hide();
            $("#ListOrder").hide();
            $("#ListAgreement").show();
            $("#ListBrokerage").hide();
            searchPurchaseAgreement();
            break;

        case 3:
            $("#ListReturnOrder").hide();
            $("#ListOrder").hide();
            $("#ListAgreement").hide();
            $("#ListBrokerage").show();
            searchBrokerage();
            break;
    }
}

function searchOrder() {

    $("#ListOrder").hiMallDatagrid({
        url: '../DetailList',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "Id",
        pageSize: 15,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams: { accountId: accountId, startDate: $("#inputStartDate").val(), endDate: $("#inputEndDate").val(), enumOrderTypeId: $("#selEnumOrderType").val() },
        columns:
        [[
            { field: "OrderTypeDescription", title: "类型", width: 120, align: "center" },
            { field: "OrderId", title: "订单编号", width: 120, align: "center" },
            { field: "ProductActualPaidAmount", title: "商品实付金额", width: 80, align: "center" },
            { field: "FreightAmount", title: "运费", width: 80, align: "center" },
            { field: "CommissionAmount", title: "佣金", width: 80, align: "center" },
            { field: "OrderDate", title: "下单日期", width: 180, align: "center" },
            { field: "Date", title: "成交日期", width: 180, align: "center" },
        ]]
    });
}

function searchBrokerage()
{
    $("#ListBrokerage").hiMallDatagrid({
        url: '../BrokerageDetailList',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "Id",
        pageSize: 15,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams: { accountId: accountId, startDate: $("#inputStartDate").val(), endDate: $("#inputEndDate").val(), enumOrderTypeId: $("#selEnumOrderType").val() },
        columns:
        [[
            { field: "TypeName", title: "类型", width: 120, align: "center" },
            { field: "OrderId", title: "订单编号", width: 120, align: "center" },
            { field: "ProductName", title: "商品名称", width: 80, align: "center" },
            { field: "RealTotal", title: "实付金额", width: 80, align: "center" },
            { field: "Brokerage", title: "分销佣金", width: 80, align: "center" },
            { field: "UserName", title: "销售员", width: 80, align: "center" },
            { field: "SettlementTimeString", title: "结算时间", width: 180, align: "center" },
        ]]
    });

}



function searchReturnOrder() {

    $("#ListReturnOrder").hiMallDatagrid({
        url: '../DetailList',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "Id",
        pageSize: 15,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams: { accountId: accountId, startDate: $("#inputStartDate").val(), endDate: $("#inputEndDate").val(), enumOrderTypeId: $("#selEnumOrderType").val() },
        columns:
        [[
            { field: "OrderTypeDescription", title: "类型", width: 120, align: "center" },
            { field: "OrderId", title: "订单编号", width: 120, align: "center" },
            { field: "ProductActualPaidAmount", title: "商品实付金额", width: 80, align: "center" },
            { field: "FreightAmount", title: "运费", width: 80, align: "center" },
            { field: "RefundTotalAmount", title: "退款金额", width: 80, align: "center" },
            { field: "RefundCommisAmount", title: "退还佣金", width: 80, align: "center" },
             { field: "ReturnBrokerageAmount", title: "退还分销佣金", width: 80, align: "center" },
            { field: "OrderRefundsDates", title: "退单日期", width: 180, align: "center" }
        ]]
    });
}

function searchPurchaseAgreement() {
    $("#ListAgreement").hiMallDatagrid({
        url: '../MetaDetailList',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "Id",
        pageSize: 15,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams: { accountId: accountId, startDate: $("#inputStartDate").val(), endDate: $("#inputEndDate").val(), enumOrderTypeId: $("#selEnumOrderType").val() },
        columns:
        [[
            {
                field: "OrderTypeDescription", title: "类型", width: 120, align: "center",
                formatter: function (value, row, index) {
                    return '营销服务费';
                }
            },
            { field: "MetaKey", title: "营销类型", width: 120, align: "center" },
            { field: "MetaValue", title: "费用", width: 120, align: "center" },
            {
                field: "DateRange", title: "服务周期", width: 120, align: "center"
            }
        ]]
    });
}
