    var editor;
$(function () {
    (function initRichTextEditor() {
        editor = UE.getEditor('des');
        editor.addListener('contentChange', function () {
            $('#contentError').hide();
        });

    })();

    $("#Save").click(function () {
        var agreementType = $("#hidRole").val();
        var agreementContent = editor.getContent();
        var strLength = editor.getContentTxt().length
        //验证字符长度
        if (strLength > 10000) {
            $.dialog.tips('输入字符过长！');
            return;
        }
        var loading = showLoading();
        $.post("./UpdateAgreement", { agreementType: agreementType, agreementContent: agreementContent },
            function (result) {
                if (result.success) {
                    $.dialog.tips('保存成功');
                }
                else
                    $.dialog.tips('保存失败！' + result.msg);
                loading.close();
            });
    });
});
