// JavaScript source code
$(function () {
    $('button').click(function () {
        var loading = showLoading();
        var appId = $('input[name="WeixinAppId"]').val();
        var appSecret = $('input[name="WeixinAppSecret"]').val();
        $.post('./SaveWeiXinSettings', { weixinAppId: appId, WeixinAppSecret: appSecret }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.tips('保存成功');
                window.location.reload();
            }
            else
                $.dialog.alert('保存失败！' + result.msg);
        });
    });
})