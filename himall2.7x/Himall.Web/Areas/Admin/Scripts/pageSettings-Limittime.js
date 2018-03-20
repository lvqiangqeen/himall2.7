
$(function () {
    //设置限时购
    $("#btn_save").click(function () {
        var Limittime = false;
        if ($("#Limittime1").is(':checked')) {
            Limittime = true;
        }
        $.post(LimittimeUrl, { Limittime: Limittime }, function (data) {
            if (data.success) {
                $.dialog.succeedTips("设置成功！", function () { window.location.href = "/Admin/PageSettings"; });
            } else {
                $.dialog.errorTips('操作失败,请稍候尝试.');
            }
        }, "json");
    })
})
