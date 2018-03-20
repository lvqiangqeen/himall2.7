// JavaScript source code
$(function () {
    $("#SiteName").focus();

    $('#Save').click(function () {
        var loading = showLoading();
        $.post('./Edit', $('form').serialize(), function (result) {
            loading.close();
            if (result.success) {
                $.dialog.tips('保存成功');
            }
            else
                $.dialog.errorTips('保存失败！' + result.msg);
        });
    });

});
