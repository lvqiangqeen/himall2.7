var datacols = [[
             {
                 field: "CreateTime", title: '时间', width: 120,
             },
             { field: "AccountType", title: "类型", width: 280, align: "center" },
             {
                 field: "Income", title: "收入", width: 80, align: "center",
                 formatter: function (value, row, index) {
                     if (value) {
                         return '<span style=" color:#f00">+' + value + '</span>';
                     }
                 }
             },
             {
                 field: "Expenditure", title: "支出", width: 80, align: "center",
                 formatter: function (value, row, index) {
                     if(value){
                         return '<span style=" color:#00cc66">-' + value + '</span>';
                     }
                 }
             },
             { field: "Balance", title: "余额", width: 80, align: "center" },
             { field: "AccountNo", title: "收支流水号", width: 80, align: "center" },
             {
                 field: "DetailLink", title: "操作", width: 80, align: "center",

                 formatter: function (value, row, index) {
                     if(value!=''&&value!=null)
                     return '<a href=\"'+value+'\"">查看详情</a>';
                 }
             }

]];

$(function () {
    //组合显示字段
    //订单表格
    $("#list").hiMallDatagrid({
        url: 'GetShopAccountItemlist',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的收支明细记录',
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
        var type = $.trim($('#type').val());
        $("#list").hiMallDatagrid('reload', { startDate: startDate, endDate: endDate, type: type });
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
	var type = $.trim($('#type').val());
	var href = $(this).attr('href').split('?')[0] + '?startDate={0}&endDate={1}&type={2}'.format(startDate, endDate, type);
	$(this).attr('href', href);
}