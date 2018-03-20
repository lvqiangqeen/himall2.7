// JavaScript source code
$(function () {
    $('input[type="checkbox"]').onoff();

    $('input.CashRules').change(function () {
        var _this = $(this),
            categoryCashDepositId = $(this).attr('categorycashdepositid'),
             enableNoReasonReturn = $(this).attr('enablenoreasonreturn'),
             url = "CloseNoReasonReturn",
            state = _this[0].checked,
            loading = showLoading();
        if (state)
            url = "OpenNoReasonReturn";
        $.post(url, { categoryId: categoryCashDepositId }, function (result) {
            loading.close();
            if (!result.Success) {
                $.dialog.errorTips('操作失败!失败原因：' + result.msg);
            }
        }, "json");

    });
});

function updateNeedpay(id, obj) {
    var cashDeposit = obj.val()
    cashDeposit = parseFloat(cashDeposit);
    if (cashDeposit < 0) {
        $.dialog.errorTips("价格错误，不可为负数！");
        return;
    }
    var loading = showLoading();
    $.post('UpdateNeedPayCashDeposit', { categoryId: id, cashDeposit: cashDeposit }, function (result) {
        loading.close();
        if (result.Success) {
            $.dialog.tips("修改成功");
        }
        else
            $.dialog.errorTips("修改失败：" + result.msg);
    });
}