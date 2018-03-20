
$(function () {
    initDatePicker();

    LoadData();

    AutoComplete();

    $("#searchBtn").click(function () {
        reload(1);
    });
});

function AutoComplete() {
    //autocomplete
    $('#sales').autocomplete({
        source: function (query, process) {
            var matchCount = this.options.items;//返回结果集最大数量
            $.post("./getMembers", { "keyWords": $('#sales').val() }, function (respData) {
                return process(respData);
            });
        },
        formatItem: function (item) {
            return item.value;
        },
        setValue: function (item) {
            return { 'data-value': item.value, 'real-value': item.key };
        }
    });
}

function reload(pageNo) {
    //var pageNo = $("#list").hiMallDatagrid('options').pageNumber;    //取当前页
    //整个理搜索参数
    var stime = $("#stime").val();
    var etime = $("#etime").val();
    var orderid = $("#orderid").val();
    var ordstate = $("#ordstate").val();
    var sales = $("#sales").attr("real-value");
    var ststate = $("#ststate").val();
    $("#list").hiMallDatagrid('reload', { orderid: orderid, ordstate: ordstate, sstate: ststate, salesid: sales, stime: stime, etime: etime, pageNumber: pageNo });
}

function LoadData() {
    $("#list").html('');

    //商品表格
    $("#list").hiMallDatagrid({
        url: './GetOrderList',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        pageSize: 9,
        pagePosition: 'bottom',
        pageNumber: 1,
        columns:
        [[
            { field: "OrderId", title: '订单编号', width: 150, align: 'left' },
            {
                field: "ProductName", title: '商品', width: 450, align: 'left',
                formatter: function (value, row, index) {
                    var html = '<span class="overflow-ellipsis" style="width:300px"><a title="' + value + '" target="_blank" href="/product/detail/' + row.ProductId + '">' + value + '</a></span>';
                    return html;
                }
            },
            { field: "ShowSettleState", title: '结算状态', width: 120, align: 'center' },
            { field: "SalesName", title: '销售员', width: 120, align: 'center' },
            {
                field: "CanBrokerage", title: '佣金', width: 120, align: 'center',
                formatter: function (value, row, index) {
                    var html = "￥" + value.toFixed(2);
                    return html;
                }
            },
            { field: "ShowOrderState", title: '订单状态', width: 120, align: 'center' },
            {
                field: "SettleTime", title: '结算时间', width: 120, align: 'left',
                formatter: function (value, row, index) {
                    var html = time_string(value);
                    return html;
                }
            },
            {
                field: "OrderTime", title: '下单时间', width: 120, align: 'left',
                formatter: function (value, row, index) {
                    var html = time_string(value);
                    return html;
                }
            }
        ]]
    });
}


function initDatePicker() {

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

}