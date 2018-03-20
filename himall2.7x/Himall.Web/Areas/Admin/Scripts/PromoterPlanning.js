// JavaScript source code
function initRichTextEditor() {
    editor = UE.getEditor('Content');
    editor.addListener('contentChange', function () {
        $('#contentError').hide();
    });
}
$(function () {
    initRichTextEditor();
});

function Post() {
    //验证字符长度

    if ($("#Title").val().length == 0) {
        $.dialog.errorTips('推广标题必填！');
        return;
    }
    if (editor.getContentLength() > 2000) {
        $.dialog.errorTips('推广内容输入字符过长！');
        return;
    } else if (editor.getContentLength() == 0) {
        $.dialog.errorTips('推广内容必须填写！');
        return;
    }
    var loading = showLoading();
    $.ajax({
        type: 'post',
        url: 'SavePlanning',
        data: $("form").serialize(),
        success: function (data) {
            loading.close();
            if (data.success)
                $.dialog.tips('保存成功！');
            else
                $.dialog.tips('保存失败！' + data.msg);
        }
    });
}

    $(function () {
        $("#copyurlbt").zclip({
            path: '/Scripts/ZeroClipboard.swf', //记得把ZeroClipboard.swf引入到项目中
            copy: function () {
                return $('#copyurlbt').data("url");
            },
            afterCopy: function () {
                $.dialog.succeedTips('复制链接成功！');
            }
        });
    });