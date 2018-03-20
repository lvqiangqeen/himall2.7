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
        NoDataMsg: '没有找到任何的会员积分信息',
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
            { field: "UserId", hidden: true },
            { field: "UserName", title: '会员名' },
            { field: "AvailableIntegrals", title: '可用积分' },
            { field: "MemberGrade", title: '会员等级' },
              { field: "HistoryIntegrals", title: '历史积分' },
              { field: "RegDate", title: '会员注册时间' },
        {
            field: "operation", operation: true, title: "操作",
            formatter: function (value, row, index) {
                var userID = row.UserId.toString();
                var html = ["<span class=\"btn-a\">"];
                html.push("<a href='./Detail/" + userID + "'>查看</a>");
                html.push("</span>");
                return html.join("");
            }
        }
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