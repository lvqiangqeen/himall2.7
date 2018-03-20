
$(function () {
    $(".nav li").each(function () {
        $(this).click(function () {            
            $(this).parent().children(".active").removeClass();
            $(this).addClass("active");
        });
    });
    $("#searchButton").click(function () {
        var status = $(".active").attr("value");
        Query(status);
    });   
    Query(0);
    $('#settlement').click(function () {
        $.post('ExecutSettlement', {}, function (result) {
            $.dialog.alert('结算完成');
        });
    });
});

function exportExcel(ele)
{
    var href = "/Admin/Account/ExportExcel?status=" + $(".active").attr("value") + "&shopName=" + $("#txtShopName").val();    
    $("#exceptExcelA").attr("href", href);
}

function OpenConfirmAccount(id) {
    $.dialog({
        title: '确认结算',
        lock: true,
        id: 'goodCheck',
        content: ['<div class="dialog-form">',
            '<div class="form-group">',
                '<p class="help-esp">请填写您的结算备注（可不填）</p>',
                '<textarea id="txtRefundRemark" class="form-control" cols="40" rows="2"  style="margin-bottom: 20px;"></textarea>\
                 <span class="field-validation-error" id="orderRefundCotentTip"></span> ',
            '</div>',
        '</div>'].join(''),
        padding: '0 40px',
        button: [
        {
            name: '确认结算',
            callback: function () {
                var replycontent = $("#txtRefundRemark").val();
                if (replycontent.length > 200) {
                    $("#orderRefundCotentTip").text("备注内容在200个字符以内");
                    $("#txtRefundRemark").css({ border: '1px solid #f60' });
                    return false;
                }
                ConfirmRefund(id, $('#txtRefundRemark').val());
            },
            focus: true
        }]
    });
}

function ConfirmRefund(id, remark) {
    var loading = showLoading();
    $.post('./ConfirmAccount', { id: id, remark: remark }, function (result) {
        loading.close();
        if (result.success) {
            $.dialog.succeedTips("操作成功！");
            Query();
        }
        else
            $.dialog.errorTips("操作失败");
    });
}

function Query(status) {

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
        queryParams: { status: status, shopName: $("#txtShopName").val() },
        columns:
        [[
            { field: "ShopName", title: "店铺名称", width: 120, align: "center" },
            { field: "TimeSlot", title: "时间段", width: 120, align: "center" },
            { field: "ProductActualPaidAmount", title: "商品实付总额", width: 110, align: "center" },
            { field: "FreightAmount", title: "运费", width: 60, align: "center" },
            { field: "CommissionAmount", title: "佣金", width: 80, align: "center" },
            { field: "RefundAmount", title: "退款金额", width: 80, align: "center" },
            { field: "RefundCommissionAmount", title: "退还佣金", width: 80, align: "center" },
            { field: "AdvancePaymentAmount", title: "营销费用总额", width: 120, align: "center" },
             { field: "BrokerageAmount", title: "分销佣金", width: 80, align: "center" },
              { field: "ReturnBrokerageAmount", title: "退还分销佣金", width: 120, align: "center" },
            { field: "PeriodSettlement", title: "本期应结", width: 80, align: "center" },
            { field: "AccountDate", title: "出账日期", width: 100, align: "center" },
            {
                field: "operation", width: 150, operation: true, title: "操作",
                formatter: function (value, row, index) {
                    var html = ["<span class=\"btn-a\">"];
                    html.push("<a href='./Detail/" + row.Id + "'>明细</a>");
                    if (row.Status == 0) {
                        html.push("<a class=\"good-check\" onclick=\"OpenConfirmAccount('" + row.Id + "')\">确认结算</a>");
                    }
                    html.push("</span>");
                    return html.join("");
                }
            }
        ]]    
    });
}

