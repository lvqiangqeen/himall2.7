$(function () {

    $("#logo").himallUpload(
    {
        title: '微店logo：<br><em>（长方形）</em>',
        imageDescript: '建议上传60 * 30的图片',
        displayImgSrc: $('#logoSrc').val(),
        imgFieldName: "logo"
    });

    $("#frontCover").himallUpload(
    {
        title: '微店banner：',
        imageDescript: '建议上传640 * 320的图片',
        displayImgSrc: $('#backgroundImageSrc').val(),
        imgFieldName: "backgroundImage"
    });

    $("#wxlogobox").himallUpload(
    {
        title: '微店Logo：<br><em>（正方形）</em>',
        imageDescript: '建议上传100*100的图片,供卡券使用',
        displayImgSrc: $('#wxlogoSrc').val(),
        imgFieldName: "WXLogo"
    });

    initValidation();
});


function initValidation() {
    
    var a = v({
        form: 'form',
        ajaxSubmit: true,
        beforeSubmit: function () {
            
        },
        afterSubmit: submitCallback,
        beforeSubmit: formValid
    });

}


function submitCallback(result) {
    var id = parseInt($('input[name="id"]').val());
    var method = id ? '修改' : '创建';
    loadingobj.close();
    if (result.success)
        $.dialog.tips(method + '微店成功', function () { location.href = 'Management'; });
    else
        $.dialog.errorTips(result.msg);

}

function formValid() {
    var logo = $("#logo").himallUpload('getImgSrc');
    var frontCover = $('#frontCover').himallUpload('getImgSrc');
    var wxlogo = $('#wxlogobox').himallUpload('getImgSrc');
    var str = $('#tags').val();
    if (str != '') {
        var tagArray = str.split(';');
        for (var i = 0; i < tagArray.length; i++) {
            if (tagArray[i].length > 4) {
                $.dialog.tips('每个标签限4个字');
                return false;
            }
        }
    }
    var result = false;
    if (!logo)
        $.dialog.tips('请上传Logo');
    else if (!frontCover)
        $.dialog.tips('请上传封面图片');
    else if (!wxlogo)
        $.dialog.tips('请上传微信Logo');
    else {
        result = true;
        loadingobj = showLoading();
    }
    return result;
}