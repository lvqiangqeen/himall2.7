// JavaScript source code
var editmsgtype = null;
$(function () {
    $('#btn_industry').click(function () {
        var loading = showLoading();
        $.post('ResetWXIndustry', null, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.succeedTips('重置行业成功！', function () { });
            } else {
                $.dialog.errorTips('重置行业失败！' + result.msg);
            }
        });
    });

    $('.btn_resettmpl').click(function () {
        var _t = $(this);
        editmsgtype = _t.data("msgtype");
        ResetWXTmplate();
    });

});

function ResetWXTmplate() {
    var loading = showLoading();
    $.post('ResetWXTmplate', { type: editmsgtype }, function (result) {
        loading.close();
        if (result.success) {
            $.dialog.succeedTips('初始微信消息模板成功！', function () { });
            window.location.reload();
        } else {
            $.dialog.errorTips('重置微信消息模板失败！' + result.msg);
        }
    });

}