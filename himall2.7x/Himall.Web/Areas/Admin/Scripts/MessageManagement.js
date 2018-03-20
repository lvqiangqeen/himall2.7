// JavaScript source code

$(function () {

    $('input[type="checkbox"]').not(":disabled").onoff();

    $('input.messagePlugin').change(function () {
        var _this = $(this),
            state = _this[0].checked,
            pluginId = $(this).attr('pluginid'),
            messageType = _this.attr('messageType'),
            loading = showLoading();
        $.post('./Enable', { pluginId: pluginId, messageType: messageType, enable: state }, function (result) {
            loading.close();
            if (!result.success) {
                $.dialog.errorTips('操作失败!失败原因：' + result.msg);
            }
        }, "json");

    });

});