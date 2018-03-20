// JavaScript source code
$(function () {
    $("#Name").focus();
});
function bindSubmitClickEvent() {
    var form = $('form');
    $('#submit').click(function () {
        if (form.valid()) {
            var loading = showLoading();
            $.post('addMobile', form.serialize(), function (result) {
                loading.close();
                if (result.success) {
                    $.dialog.tips('保存成功', function () {
                        location.href = $("#ACurl").val();
                    });
                }
                else
                    $.dialog.errorTips('保存失败!' + result.msg);
            })
        }
    });
}


$(function () {
    bindSubmitClickEvent();

    $("#btDelete").click(function () {
        var loading = showLoading();
        $.post('deleteMobile', {}, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.tips('删除成功', function () {
                    location.href = $("#ACurl").val();
                });
            }
            else
                $.dialog.errorTips('删除失败!' + result.msg);
        })
    });
})