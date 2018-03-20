$(function () {
    query();
    $("#searchBtn").click(function () { query(); });
    $(".daySpan").bind("click",function () {
        var userId = 0;
        if ($("#autoTextBox").val() != '') {
            userId = $("#autoTextBox").attr("real-value");
        }
        $("#list").hiMallDatagrid('reload', { userId: userId, days:$(this).data("day")});
        $(this).addClass("active").siblings().removeClass("active");
    });
    AutoComplete();
});

function query() {
    var userId = 0;
    if ($("#autoTextBox").val() != '')
    {
        userId=$("#autoTextBox").attr("real-value");
    }
    $("#list").hiMallDatagrid({
        url: './List',
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
        queryParams: { userId:userId , days: 0 },
        toolbar: /*"#goods-datagrid-toolbar",*/'',
        columns:
        [[
            { field: "Id", hidden: true },
            { field: "UserName", title: '销售员帐号' },
            { field: "Paid", title: '已结佣金' },
            { field: "UnPaid", title: '未结佣金' },
            { field: "TotalTurnover", title: '成交总金额' },
            { field: "TotalNumber", title: '成交总数', width: 100, },
           
        {
            field: "operation", operation: true, title: "操作", width: 90,
            formatter: function (value, row, index) {
                 var id = row.Id;
              
                var html = ["<span class=\"btn-a\">"];      
                    html.push("<a  href='detail/"+id+"'>查看详情</a>");       
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
            return { 'data-value': item.value, 'real-value': item.key};
        }
    });
}

