// JavaScript source code
var editor;
var editor2;
function Post() {
    //验证字符长度
    //console.log($.trim(editor.getContentLength()))
    //console.log($.trim(editor.getContent().length))
    if (editor.getContentLength() > 2000) {
        $.dialog.tips('卖家规则输入字符过长！');
        return;
    }
    if (editor2.getContentLength() > 2000) {
        $.dialog.tips('销售员规则输入字符过长！');
        return;
    }
    var loading = showLoading();
    $.ajax({
        type: 'post',
        url: 'SaveSetting',
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

function initRichTextEditor() {
    editor = UE.getEditor('SellerRule', { maximumWords: 2000 });
    editor.addListener('contentChange', function () {
        $('#contentError').hide();
    });
    editor2 = UE.getEditor('PromoterRule', { maximumWords: 2000 });
    editor2.addListener('contentChange', function () {
        $('#contentError').hide();
    });

};

$(function () {
    initRichTextEditor();
});

$(function () {
    var img = $("#MMdbshare").val();
    $('#DisBanner').himallUpload({
        title: '',
        imageDescript: '最佳尺寸:符合微信尺寸(640*186)',
        displayImgSrc: img,
        imgFieldName: "DisBanner",
        dataWidth: 8
    });
})