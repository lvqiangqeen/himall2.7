var endDate = startDate = Date.toString('yyyy-MM-dd');
var curPageSize = 15;
var defaultDate = new Date();
defaultDate.setDate(defaultDate.getDate() - 1);
$(function () {

    $('#inputStartDate').daterangepicker(
        {
            format: 'YYYY-MM-DD',
            startDate: defaultDate, endDate: defaultDate
        }
    ).bind('apply.daterangepicker', function (event, obj) {
        if ($('#inputStartDate').val() == "" || $('#inputStartDate').val() == "YYYY/MM/DD — YYYY/MM/DD") {
            return false;
        }
        var strArr = $('#inputStartDate').val().split("-");
        var startDate = strArr[0] + "-" + strArr[1] + "-" + strArr[2];
        var endDate = strArr[3] + "-" + strArr[4] + "-" + strArr[5];
        if (startDate != '' && endDate != '')
            LoadData(startDate, endDate);
        else {
            $.dialog.tips('请选择时间范围');
        }
    })

    $('#pageSizeSelect').on('change', function () {
        curPageSize = $('#pageSizeSelect').val();
        LoadData(startDate, endDate);
    });

    $('#defaultBtn').click();
});
function LoadData(date1, date2)
{
    var para = {};
    para.startDate = date1;
    para.endDate = date2;
    //临时存取日期
    endDate = date2;
    startDate = date1;

    $("#list").hiMallDatagrid({
        url: 'GetProductSaleStatisticList',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的记录',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "ProductId",
        pageSize: curPageSize,
        pagePosition: 'bottom',
        operationButtons: "#operationButtons",
        pageNumber: 1,
        queryParams: para,
        columns: [[
             {
                 field: "ProductName", title: '商品名称', width: 'auto'
             },

             { field: "VistiCounts", sort: true, title: "浏览量", width: 'auto', align: "center" },
             { field: "VisitUserCounts", sort: true, title: "浏览人数", width: 'auto', align: "center" },
             { field: "PayUserCounts", sort: true, title: "付款人数", width: 'auto', align: "center" },
             {
                 field: "SinglePercentConversion", sort: true, title: "单品转化率", width: 'auto', align: "center",
                formatter: function (value, row, index) {
                    return value + '%';
                }
             },
             { field: "SaleCounts", sort: true, title: "销售数量", width: 'auto', align: "center" },
             { field: "SaleAmounts", sort: true, title: "销售金额", width: 'auto', align: "center" }
        ]]
    });
}

function ExportExecl() {
    var href = "/Admin/Statistics/ExportProductStatistic?startDate=" + startDate + "&endDate=" + endDate;
    $("#aMonthExport").attr("href", href);
}