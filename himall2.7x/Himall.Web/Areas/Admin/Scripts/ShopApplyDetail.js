// JavaScript source code
$(function () {
    function AgreeOrNot(type, Id) {
        var loading = showLoading();
        $.post("../AgreeOrNot", { type: type, Id: Id },
            function (result) {
                if (result.success) {
                    $.dialog.succeedTips('审核成功！', function () { window.location.href = '../applylist'; });
                }
                else
                    $.dialog.tips('审核失败！' + result.msg);
                loading.close();
            });
    }
})