
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
    $('#autoTextBox').autocomplete({
        source: function (query, process) {
            var matchCount = this.options.items;//返回结果集最大数量
            $.post("./getMembers", { "keyWords": $('#autoTextBox').val() }, function (respData) {
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
    $("#list").hiMallDatagrid('reload', { skey: $.trim($('#skey').val()), pageNumber: pageNo });
}

function LoadData() {
    $("#list").html('');

    //商品表格
    $("#list").hiMallDatagrid({
        url: './GetProductDataList',
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
            {
                field: "ProductName", title: '商品名称', width: 350, align: 'left',
                formatter: function (value, row, index) {
                    var html = '<span class="overflow-ellipsis" style="width:300px"><a title="' + value + '" target="_blank" href="/product/detail/' + row.Id + '">' + value + '</a></span>';
                    return html;
                }
            },
            { field: "AgentNum", title: '代理次数', width: 100, align: 'center' },
            { field: "ForwardNum", title: '转发次数', width: 100, align: 'center' },
            { field: "DistributionSaleNum", title: '推广成交数', width: 100, align: 'center' },
            { field: "DistributionSaleAmount", title: '推广成交额', width: 100, align: 'center' },
            {
                field: "SaleAmount", title: '总交易额', width: 100, align: 'center',
                formatter: function (value, row, index) {
                    var html = "￥" + value.toFixed(2);
                    return html;
                }
            },
                { field: "SaleNum", title: '总交易数', width: 100, align: 'center' },
            {
                field: "Brokerage", title: '已结佣金', width: 100, align: 'center',
                formatter: function (value, row, index) {
                    var html = "￥" + value.toFixed(2);
                    return html;
                }
            },
            {
                field: "NoSettledBrokerage", title: '未结佣金', width: 100, align: 'center',
                formatter: function (value, row, index) {
                    var html = "￥" + value.toFixed(2);
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