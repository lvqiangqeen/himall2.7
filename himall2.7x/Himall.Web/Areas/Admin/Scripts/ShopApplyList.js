// JavaScript source code

function query() {
    //订单表格
    $("#list").hiMallDatagrid({
        url: './GetApplyList',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的申请列表',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "Id",
        pageSize: 15,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams: { shopName: $("#shopName").val(), status: $("#status").val() },
        columns:
        [[
            { field: "Id", title: 'Id', hidden: true, width: 150 },

              { field: "ShopName", title: "店铺名称", width: 250, align: "center" },

               { field: "ApplyDate", title: "申请日期", width: 250, align: "center" },
           {
               field: "AuditedStatus", title: "审核状态", width: 250, align: "center"
           },
        {
            field: "operation", operation: true, title: "操作", width: 220,
            formatter: function (value, row, index) {
                var id = row.Id.toString();
                var html = ["<span class=\"btn-a\">"];
                if (row.AuditedStatus == "待审核") {
                    html.push("<a class=\"good-check\" href='./applyDetail/" + id + "'>审核</a>");
                }
                else {
                    html.push("<a class=\"good-check\" href='./applyDetail/" + id + "'>查看详情</a>");
                }
                html.push("</span>");
                return html.join("");
            }
        }
        ]]
    });
}
$(function () {
    query();
    $("#searchBtn").click(function () {
        query();
    });
});