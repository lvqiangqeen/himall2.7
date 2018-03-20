// JavaScript source code
$(function () {
    InitData();
});

function InitData() {
    //订单表格
    $("#list").hiMallDatagrid({
        url: './Recordlist',
        nowrap: true,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: true,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "Id",
        pageSize: 15,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams: {},
        columns:
        [[
            { field: "OperateDate", title: "缴费时间", width: 120, align: "center" },
            { field: "OperateType", title: "类型", width: 120, align: "center" },
            { field: "Amount", title: "支付金额", width: 120, align: "center" },
            { field: "Content", title: "明细", width: 180, align: "center" },
            { field: "Operate", title: "操作人", width: 120, align: "center" }
        ]]
    });

}