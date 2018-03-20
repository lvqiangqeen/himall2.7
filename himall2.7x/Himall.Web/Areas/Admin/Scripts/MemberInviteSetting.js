// JavaScript source code
$(function () {
    var isposting = false;
    function beginPost() {
        var shareIcon = $('input[name="ShareIcon"]').val();
        if (shareIcon == "" || null == shareIcon) {
            $.dialog.errorTips("分享图标不能为空");
            return false;
        }
        if (isposting) {
            $.dialog.tips("数据提交中...");
            return false;
        }
        isposting = true;
        $("#btsubmit").text("提交中...");
        loading = showLoading();
    }

    function successPost(data) {
        isposting = false;
        $("#btsubmit").text("保 存");
        loading.close();
        if (data.success == true) {
            $.dialog.tips("保存成功", function () {
                //数据提交成功页面跳转
            });
        } else {
            $.dialog.errorTips(data.msg);
        }
    };
});
//$('#Save').click(function () {
//    var loading = showLoading();
//    $.post('./Edit', $('form').serialize(), function (result) {
//        loading.close();
//        if (result.success) {
//            $.dialog.tips('保存成功');
//        }
//        else
//            $.dialog.errorTips('保存失败！' + result.msg);
//    });
//});

$(function () {
    var img = $("#MMsi").val();
    $('#ShareIcon').himallUpload({
        title: '分享图标：',
        imageDescript: '最佳尺寸:符合微信尺寸(100*100)',
        displayImgSrc: img,
        imgFieldName: "ShareIcon",
        dataWidth: 8
    });
})
