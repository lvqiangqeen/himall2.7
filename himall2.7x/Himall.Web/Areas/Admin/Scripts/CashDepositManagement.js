// JavaScript source code
$(function () {
    Query();
    $("#searchBtn").click(function (e) {
        Query();
        searchClose(e);
    });
});

function Query() {

    var shopName = $("#shopName").val();
    var type = $("#searchType").val();

    $("#shopDatagrid").hiMallDatagrid({
        url: "./List",
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
        queryParams: { "shopName": shopName, type: type },
        toolbar: "#shopToolBar",
        operationButtons: "#batchOperate",
        columns:
        [[

            { field: "Id", title: "Id", hidden: true },
            { field: "ShopName", title: "店铺名称", width: 140 },
            { field: "Type", title: "缴纳状态", width: 140 },
            { field: "TotalBalance", title: "缴纳保证金", width: 130 },
            { field: "CurrentBalance", title: "当前余额", width: 130 },
            { field: "NeedPay", title: "应缴金额", width: 130 },
            { field: "Date", title: "最近缴纳时间", width: 140 },
            {
                field: "operation", operation: true, title: "操作",
                formatter: function (value, row, index) {
                    var id = row.Id.toString();
                    var html = ['<span class="btn-a">'];
                    html.push('<a onclick="Deduction(' + id + "," + row.CurrentBalance + ');">扣款</a>');
                    html.push('<a href="./CashDepositDetail/' + id + '">查看明细</a>');
                    if (row.NeedPay > 0 && row.EnableLabels == true)
                        html.push('<a onclick="closeLabels(' + id + ');">取消标识</a>');
                    html.push("</span>");

                    return html.join("");
                }
            }
        ]]
    });

};
function closeLabels(id) {
    $.dialog.confirm('商家消费者保障标识、七天无理由退换标识及及时发货标识将被取消，商家补齐保障金后将自动恢复；确认取消吗？', function () {
        var loading = showLoading();
        $.post("UpdateEnableLabels", { id: id, enableLabels: false }, function (result) {
            loading.close();
            if (result.Success) {
                $.dialog.tips("成功取消");
                Query();
            }
            else
                $.dialog.errorTips("取消失败" + result.msg);
        })
    })

}

function Deduction(id, balance) {
    $.dialog({
        title: '保证金扣款',
        lock: true,
        id: 'deduction',
        content: $("#deduction").html(),
        padding: '0 40px',
        init: function () {
            $("#price").html(balance);
        },
        button: [
        {
            name: '确认',
            callback: function () {
                if ($("#balance").val().length <= 0) {
                    $.dialog.errorTips("请输入扣除金额");
                    return false;
                }
                if (parseFloat($("#balance").val()) <= 0) {
                    $.dialog.errorTips("只能输入正数");
                    return false;
                }
                if ($("#shortDescription").val().length <= 0) {
                    $.dialog.errorTips("请输入扣除说明");
                    return false;
                }
                if ($("#balance").val() > balance) {
                    $.dialog.errorTips("扣除金额不能大于" + balance);
                    return false;
                }
                var loading = showLoading();
                $.post('./Deduction', { id: id, balance: parseFloat($("#balance").val()), description: $("#shortDescription").val() }, function (result) {
                    if (result.Success == true) {
                        $.dialog.tips("扣款成功！");
                        Query();
                    }
                    else
                        $.dialog.tips("扣款失败" + result.msg);
                    loading.close();
                });
            },
            focus: true
        },
            {
                name: '取消',
            }]
    });
}