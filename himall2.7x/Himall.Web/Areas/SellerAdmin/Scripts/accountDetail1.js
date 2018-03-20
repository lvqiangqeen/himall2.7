// JavaScript source code
$(function () {
    $("#List").hiMallDatagrid({
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
        queryParams: { shopId: $("#Mshopid").val(), startDate: $("#Msd").val(), endDate: $("#Med").val() },
        columns:
        [[
            { field: "OrderId", title: "订单", width: 120, align: "center" },
            { field: "OrderAmount", title: "商品实付金额", width: 120, align: "center" },
            { field: "CommissionAmount", title: "佣金", width: 120, align: "center" },
            { field: "RefundTotalAmount", title: "退单金额", width: 120, align: "center" },
            { field: "RefundCommisAmount", title: "退还佣金", width: 120, align: "center" },
            { field: "AccountAmount", title: "最终结算", width: 120, align: "center" },
            { field: "Date", title: "时间", width: 180, align: "center" },
        ]]
    });
});