// JavaScript source code
$(function () {
    $(".form-horizontal .upload-img label").css("display", "none");
});
function Post() {
    if ($("#ProShareTitle").val().length > 150) {
        $.dialog.tips('商品分享标题过长');
        return;
    }
    if ($("#ShopShareTitle").val().length > 150) {
        $.dialog.tips('商品分享标题过长');
        return;
    }
    if ($("#DisShareTitle").val().length > 150) {
        $.dialog.tips('分销市场分享标题过长');
        return;
    }
    if ($("#RecruitShareTitle").val().length > 150) {
        $.dialog.tips('招募分享标题过长');
        return;
    }
    if ($("#ProShareDesc").val().length > 1000) {
        $.dialog.tips('商品分享标题过长');
        return;
    }
    if ($("#ShopShareDesc").val().length > 1000) {
        $.dialog.tips('商品分享标题过长');
        return;
    }
    if ($("#DisShareDesc").val().length > 1000) {
        $.dialog.tips('分销市场分享描述过长');
        return;
    }
    if ($("#RecruitShareDesc").val().length > 1000) {
        $.dialog.tips('招募分享描述过长');
        return;
    }

    //验证字符长度
    var loading = showLoading();
    $.ajax({
        type: 'post',
        url: 'SaveShareSetting',
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
    var img = $("#MMpshare").val();
    $('#ProShareLogo').himallUpload({
        title: '',
        imageDescript: '最佳尺寸:符合微信尺寸(100*100)',
        displayImgSrc: img,
        imgFieldName: "ProShareLogo",
        dataWidth: 8
    });


    var img2 = $("#MMsshare").val();
    $('#ShopShareLogo').himallUpload({
        title: '',
        imageDescript: '最佳尺寸:符合微信尺寸(100*100)',
        displayImgSrc: img2,
        imgFieldName: "ShopShareLogo",
        dataWidth: 8
    });
    var img3 = $("#MMdshare").val();
    $('#DisShareLogo').himallUpload({
        title: '',
        imageDescript: '最佳尺寸:符合微信尺寸(100*100)',
        displayImgSrc: img3,
        imgFieldName: "DisShareLogo",
        dataWidth: 8
    });
    var img4 = $("#MMrshare").val();
    $('#RecruitShareLogo').himallUpload({
        title: '',
        imageDescript: '最佳尺寸:符合微信尺寸(100*100)',
        displayImgSrc: img4,
        imgFieldName: "RecruitShareLogo",
        dataWidth: 8
    });
})
