
$(function () {
    $(".nav li").each(function () {
        $(this).click(function () {
            $(this).parent().children(".active").removeClass();
            $(this).addClass("active");
        });
    });
    Query(0);
});


function Query(status) {
    $('.nav-tabs-custom li').each(function () {
        if ($(this).val() == status) {
            $(this).addClass('active').siblings().removeClass('active');
        }
    });

    InitData(status)

    //$('.nav-tabs-custom li').click(function (e) {
    //    searchClose(e);
    //    $(this).addClass('active').siblings().removeClass('active');
    //    if ($(this).attr('type') == 'statusTab') {//状态分类
    //        $("#list").hiMallDatagrid('reload', { status: $(this).attr('value') || null });
    //    }
    //});
}


function InitData(status) {
    //订单表格
    $("#list").hiMallDatagrid({
        url: './list',
        nowrap: true,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: true,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "Id",
        pageSize: 15,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams: { status: status },
        columns:
        [[
            { field: "ShopName", title: "店铺名称", width: 120, align: "center" },
            { field: "TimeSlot", title: "时间段", width: 120, align: "center" },
            { field: "ProductActualPaidAmount", title: "商品实付总额", width: 120, align: "center" },
            { field: "FreightAmount", title: "运费", width: 60, align: "center" },
            { field: "CommissionAmount", title: "佣金", width: 80, align: "center" },
            { field: "RefundAmount", title: "退款金额", width: 80, align: "center" },
            { field: "RefundCommissionAmount", title: "退还佣金", width: 80, align: "center" },
            { field: "AdvancePaymentAmount", title: "营销费用总额", width: 110, align: "center" },
             { field: "BrokerageAmount", title: "分销佣金", width: 80, align: "center" },
              { field: "ReturnBrokerageAmount", title: "退还分销佣金", width: 110, align: "center" },
            { field: "PeriodSettlement", title: "本期应结", width: 80, align: "center" },
            { field: "AccountDate", title: "出账日期", width: 100, align: "center" },
            {
                field: "operation", width: 120, operation: true, title: "操作",
                formatter: function (value, row, index) {
                    var html = ["<span class=\"btn-a\">"];
                    html.push("<a href='./AccountDetail/" + row.Id + "'>明细</a>");
                    html.push("</span>");
                    return html.join("");
                }
            }
        ]]
    });

}

