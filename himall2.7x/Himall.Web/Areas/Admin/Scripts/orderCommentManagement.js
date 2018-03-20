function query()
{
    //订单表格
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
        pageSize: 15,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams: {},
        columns:
        [[
            { field: "OrderId", title: '订单号', width: 150 },
            { field: "ShopName", title: "店铺", width: 150, align: "center" },
            { field: "UserName", title: "评价会员", width: 80, align: "center" },
            { field: "PackMark", title: "商品包装", width: 100, align: "center" },
            { field: "DeliveryMark", title: "送货速度", width: 100, align: "center" },
            { field: "ServiceMark", title: "配送服务", width: 100, align: "center" },
            { field: "CommentDate", title: "评价日期", width:150, align: "center" },
            {
                field: "operation", operation: true, title: "操作", width: 100,
                formatter: function (value, row, index) {
                    var id = row.OrderId.toString();
                    var html = ["<span class=\"btn-a\">"];
                    html.push("<a onclick=\"deleteOrderComment('" + row.Id + "');\">删除</a>");
                    html.push("</span>");
                    return html.join("");
                }
            }
        ]]
    });
}


$(function () {

    query();

    $('#searchButton').click(function (e) {
        searchClose(e);
        var startDate = $("#inputStartDate").val();
        var endDate = $("#inputEndDate").val();
        var orderId = $.trim($('#txtOrderId').val());
        var shopName = $.trim($('#txtShopName').val());
        var productName = $.trim($('#txtProductName').val());
        var userName = $.trim($('#txtUserName').val());
        $("#list").hiMallDatagrid('reload', { startDate: startDate, endDate: endDate, orderId: orderId, shopName: shopName, productName: productName, userName: userName });
    })
});

function deleteOrderComment(id) {
    $.dialog.confirm('确定删除该评价吗？', function () {
        var loading = showLoading();
        $.post("./Delete", { id: id }, function (data) { loading.close(); $.dialog.tips(data.msg); query(); });
        var pageNo = $("#list").hiMallDatagrid('options').pageNumber;
        $("#list").hiMallDatagrid('reload', { pageNumber: pageNo });
    });
}

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