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
        queryParams: { keyWords: $("#autoTextBox").val() },
        toolbar: /*"#goods-datagrid-toolbar",*/'',
        operationButtons: '',
        columns:
        [[
            //{ checkbox: true, width: 39 },
            { field: "Id", hidden: true },
            { field: "UserName", title: '会员帐号' },
            { field: "RegName", title: '发展会员帐号' },
           { field: "RegTime", title: '新会员注册时间' },
           { field: "InviteIntegral", title: '老会员奖励' },
           { field: "RegIntegral", title: '新会员奖励' }
        ]]
    });
}

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