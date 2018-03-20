// JavaScript source code
$(function () {
    $('button').click(function () {
        var loading = showLoading();
        var appId = $('input[name="AppId"]').val();
        var appSecret = $('input[name="AppSecret"]').val();
        var followUrl = $('input[name="FollowUrl"]').val();
        var weixinToken = $('#weixinToken').val();
        $.post('./SaveVShopSettings', { weixinAppId: appId, WeixinAppSecret: appSecret, weixinfollowUrl: followUrl, weixiToken: weixinToken }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.tips('保存成功', function () { window.location.reload(); });
            }
            else
                $.dialog.alert('保存失败！' + result.msg);
        });
    });
})