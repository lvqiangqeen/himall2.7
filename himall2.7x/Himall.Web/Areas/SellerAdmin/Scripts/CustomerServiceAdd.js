// JavaScript source code
$(function () {
    $("#Name").focus();
});
function bindSubmitClickEvent() {
    var form = $('form');
    $('#submit').click(function () {
        if (form.valid()) {
            var loading = showLoading();
            $.post('add', form.serialize(), function (result) {
                loading.close();
                if (result.success) {
                    $.dialog.tips('保存成功', function () {
                        location.href = $("#MCurl").val();
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


})
