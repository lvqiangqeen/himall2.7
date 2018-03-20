// JavaScript source code
var btsubmit;
var loading;
var isposting = false;

$(function () {
    btsubmit = $("#btsubmit");
    $("#DistributorShareLogo").hishopUpload({
        title: '分享Logo：',
        imageDescript: '<label style="padding-top:0;">建议上传100*100的图片</label>',
        displayImgSrc: $("#MDSlogo").val(),
        imgFieldName: "DistributorShareLogo",
        defaultImg: '/Images/default_100x100.png',
        imagesCount: 1,
        dataWidth: 6
    });
});

function beginpost() {
    if (isposting) {
        $.dialog.tips("数据提交中...");
        return false;
    }
    isposting = true;
    btsubmit.text("提交中...");    
    loading = showLoading();
}

function successpost(data) {
    isposting = false;
    btsubmit.text("保 存");
    loading.close();
    if (data.success == true) {
        $.dialog.tips("设置分销聚合页推广信息成功！"
            , function () {
                window.location.reload();//数据提交成功页面跳转
            }
        );
    } else {
        $.dialog.errorTips(data.msg);
    }
}