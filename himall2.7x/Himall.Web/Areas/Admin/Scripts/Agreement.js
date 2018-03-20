    var editor;
$(function () {
    (function initRichTextEditor() {
        editor = UE.getEditor('des');
        editor.addListener('contentChange', function () {
            $('#contentError').hide();
        });

    })();
    //默认选中用户注册协议
    $('#navul li').each(function () {
        var xs = "0";
        if (window.location.href.indexOf("Seller")>=0) {
            xs = "1";
        }
        if ($(this).val() == xs) {
            $(this).addClass('active').siblings().removeClass('active');
            $("#hidRole").val($(this).attr('value'));
        }
    });
    //更新选中状态
    $('#navul li').click(function (e) {
        $(this).addClass('active').siblings().removeClass('active');
        $("#hidRole").val($(this).attr('value'));
        var loading = showLoading();
        $.ajax({
            type: 'post',
            url: "./GetManagement",
            cache: false,
            async: true,
            data: { agreementType: $("#hidRole").val() },
            dataType: "json",
            success: function (data) {
                editor.setContent(data.AgreementContent);
                loading.close();
            }
        });

    });

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
