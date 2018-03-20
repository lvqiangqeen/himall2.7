// JavaScript source code
var editor;

$(function () {
    editor = UE.getEditor('pageFoot');

});


$('button').click(function () {
    var content = editor.getContent();
    var loading = showLoading();
    $.post('SetPageFoot', { content: content },
        function (data) {
            loading.close();
            if (data.success) {
                $.dialog.succeedTips("页脚修改成功！", function () { location.href = "./index"; });
            }
            else { $.dialog.errorTips("页脚修改失败！" + data.msg); }
        });
});

