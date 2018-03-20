// JavaScript source code
$(function () {
    $('input[type="checkbox"]').onoff();

    $('input.pluginCheck').change(function () {
        var _this = $(this),
            state = _this[0].checked,
            pluginId = $(this).attr('pluginid'),
            loading = showLoading();
        $.post('./Enable', { pluginId: pluginId, enable: state }, function (result) {
            loading.close();
            if (!result.success) {
                $.dialog.errorTips('操作失败!失败原因：' + result.msg);
            }
        }, "json");
    })

    $('input.payWayCheck').change(function () {
        var _this = $(this),
            state = _this[0].checked,
            loading = showLoading();
        $.post('./ChangeReceivingAddressState', { enable: state }, function (result) {
            loading.close();
            if (!result.success) {
                $.dialog.errorTips('操作失败!失败原因：' + result.msg);
            }
        }, "json");
    })
});
