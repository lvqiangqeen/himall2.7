// JavaScript source code
$(function () {
    $('#btnSendGood').click(function () {
        var orderIds = [];
        var companyNames = [];
        var shipOrderNumbers = [];
        var invaldat = false;

        $('.cssOrderId').each(function () {
        	orderIds.push($(this).data('id'));
        });
        $('.cssCompanyName').each(function (index, item) {
            if ($(item).val() == "") {
                $.dialog.errorTips("你有没有填写的快递公司！");
                invaldat = true;
                return;
            }
            companyNames.push($(this).find("option:selected").text());
        });
        $('.cssShipOrderNumber').each(function (index, item) {
            var valx = $(this).parent().parent().find(".cssCompanyName option:selected").val();
            if ($(item).val() == "" && parseInt(valx) != -1) {
                $.dialog.errorTips("你有没有填写的快递单号！");
                invaldat = true;
                return;
            }
            shipOrderNumbers.push($(this).val());
        });
        if (invaldat) { return; }

        if (orderIds.length == 0) {
            $.dialog.errorTips("没有选择任何需要发货的订单！");
            return;
        }

        var loading = showLoading();
        $.post(location.href, { ids: orderIds.join(','), companyNames: companyNames.join(','), shipOrderNumbers: shipOrderNumbers.join(',') }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.succeedTips("操作成功！", function () { window.location.href = "./Management" });
            }
            else
                $.dialog.errorTips("操作失败," + result.msg);
        });
    });
})