
$(function () {
    query();
    $("#searchBtn").click(function () { query(); });
    AutoComplete();
})

function query() {
    $("#list").hiMallDatagrid({
        url: './list',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "Id",
        pageSize: 10,
        pageNumber: 1,
        queryParams: { userName: $("#autoTextBox").val(), startDate: $("#inputStartDate").val(), endDate: $("#inputEndDate").val() },
        toolbar: /*"#goods-datagrid-toolbar",*/'',
        operationButtons: "",
        columns:
        [[
            { field: "Id", hidden: true },
            { field: "UserName", title: '操作人' },
            { field: "PageUrl", title: '页面', 
				formatter: function (value, row, index) {
					return '<span title="'+value+'" class="overflow-ellipsis" style="width:300px; text-align:center">'+value+'</span>';
				}
			},
            { field: "Description", title: '行为' },
            { field: "Date", title: '操作日期' },
        { field: "IPAddress", title: 'IP' }
        ]]
    });
}

function AutoComplete() {
    //autocomplete
    $('#autoTextBox').autocomplete({
        source: function (query, process) {
            var matchCount = this.options.items;//返回结果集最大数量
            $.post("./GetManagers", { "keyWords": $('#autoTextBox').val() }, function (respData) {
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
};
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