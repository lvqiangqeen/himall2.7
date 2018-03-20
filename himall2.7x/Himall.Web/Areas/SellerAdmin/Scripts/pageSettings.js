$(function () {

    imageAdsClickEventBind();
    InitUpload();
    SetLogo();
});

function InitUpload() {
    $("#uploadImg").himallUpload(
   {
       displayImgSrc: logo,
       imgFieldName: "Logo",
       title: 'LOGO:',
       imageDescript: '160*160',
       dataWidth: 8
   });
}

//设置LOGO
function SetLogo() {
    $('.logo-area').click(function () {
        $.dialog({
            title: 'LOGO设置',
            lock: true,
            width: 350,
            id: 'logoArea',
            content: document.getElementById("logosetting"),
            padding: '0 30px',
            okVal: '保存',
            ok: function () {
                var logosrc = $("input[name='Logo']").val();
                if (logosrc == "") {
                    $.dialog.tips("请上传一张LOGO图片！");
                    return false;
                }
                var loading = showLoading();
                $.post(setlogoUrl, { logo: logosrc },
                    function (data) {
                        loading.close();
                        if (data.success) {
                            $.dialog.succeedTips("LOGO修改成功！");
                            $("input[name='Logo']").val(data.logo);
                            logo = data.logo;
                        }
                        else { $.dialog.errorTips("LOGO修改失败！") }
                    });
            }
        });
    });
}

function imageAdsClickEventBind() {

    $('a[type="imageAd"]').click(function () {
        var that = this;
        var thisPic = $(this).attr('pic');
        var thisUrl = $(this).attr('url');
        $.dialog({
            title: 'banner设置',
            lock: true,
            width: 420,
            padding: '0 30px',
            id: 'goodsArea',
            content: ['<div class="dialog-form">',
                '<div id="HandSlidePic" class="form-group upload-img clearfix">',
                '</div>',
                '<div class="form-group">',
                    '<label class="label-inline fl" for="" style="margin-right:18px!important">跳转链接:</label>',
                    '<input class="form-control input-sm" type="text" id="url">',
                '</div>',
            '</div>'].join(''),
            okVal: '保存',
            init: function () {
                $("#HandSlidePic").himallUpload(
                {
                    title: '请上传图片',
                    imageDescript: $(that).attr("imageDescript"),
                    displayImgSrc: thisPic,
                    imgFieldName: "HandSlidePic",
					dataWidth: 8
                });
                $("#url").val(thisUrl);
            },
            ok: function () {
                var valida = false;
                var id = parseInt($(that).attr('value'));
                var url = $("#url").val();
                var pic = $("#HandSlidePic").himallUpload('getImgSrc');
                if (url.length === 0) { $("#url").focus(); $.dialog.errorTips('链接地址不能为空.'); return valida; }
                if (pic.length === 0) { $.dialog.errorTips('图片不能为空.'); return valida; }
                var loading = showLoading();
                $.ajax({
                    type: "POST",
                    url: "UpdateImageAd",
                    data: { url: url, pic: pic, id: id },
                    dataType: "json",
                    async:false,
                    success: function (data) {
                        loading.close();
                        if (data.success) {
                            $(that).attr('pic', data.imageUrl);
                            $(that).attr('url', url);
                            $.dialog.tips('保存成功!');
                        }
                        else {
                            $.dialog.errorTips('保存失败！' + data.msg);
                            return false;
                        }
                    },
                    error: function (data) {
                        loading.close(); $.dialog.errorTips('操作失败,请稍候尝试.'); }
                });
            }
        });
    });
}

