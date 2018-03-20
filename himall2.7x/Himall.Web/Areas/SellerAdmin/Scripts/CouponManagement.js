// JavaScript source code
function query() {
    //订单表格
    $("#list").hiMallDatagrid({
        url: './GetItemList',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的优惠券',
        border: false,
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
            { field: "Id", title: 'Id', hidden: true, width: 150 },
            {
                field: "CouponName", title: "优惠名称", width: 200, align: "center",
            },
           { field: "Price", title: "价值", width: 80, align: "center" },
            { field: "PerMax", title: "领取限制", width: 70, align: "center" },
            { field: "OrderAmount", title: "使用条件", align: "center", width: 90 },
            { field: "Num", title: "库存", width: 90, align: "center" },
            {
                field: "Date", title: "有效期", align: "center",
                formatter: function (value, row, index) {
                    var html = '<span>' + row.StartTime + "至" + row.EndTime + '</span>';
                    return html;
                }
            },

            {
                field: "Receive", title: "领取人/次", width: 100, align: "center",
                formatter: function (value, row, index) {
                    var html = '<span>' + row.RecevicePeople + "/" + row.ReceviceNum + '</span>';
                    return html;
                }
            },
        {
            field: "Used", title: "已使用", width: 50,
        },
        {
            field: "operation", operation: true, title: "操作", width: 220,
            formatter: function (value, row, index) {
                var id = row.Id.toString();
                var now = $("#DTND").val().replace(/-/g, '/');
                var html = ["<span class=\"btn-a\">"];
                switch (row.WXAuditStatus) {
                    case 1:
                        html.push("<a href='./Receivers/" + id + "'>领取详情</a>");
                        html.push("<a href='./Detail/" + id + "'>查看</a>");
                        if (new Date(row.EndTime) > new Date(now)) {
                            html.push("<a class=\"good-check\" href=\"./Edit/" + id + "\">编辑</a>");
                            html.push("<a class=\"good-check\" onclick=\"Cancel(" + id + ")\">使失效</a>");
                        }
                        break;
                    case 0:
                        html.push("同步审核中...");
                        break;
                    case -1:
                        html.push("同步审核失败");
                        break;
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
    $("#AddItem").click(function () {
        location.href = "./Add";
    });

    $('#searchBtn').click(function (e) {
        searchClose(e);
        var CouponName = $.trim($('#CouponName').val());
        $("#list").hiMallDatagrid('reload',
            {
                CouponName: CouponName,
            });
    });
});

function Cancel(id) {
    $.dialog.confirm('确定使该优惠券失效吗？', function () {
        var loading = showLoading();
        $.post("./Cancel", { couponid: id }, function (data) { loading.close(); $.dialog.tips(data.msg); query(); });
    });
}