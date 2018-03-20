// JavaScript source code
$(function () {
    $("#uploadImg").himallUpload(
  {
      imageDescript: '建议上传100px * 50px的图片',
      displayImgSrc: "",
      imgFieldName: "BrandLogo"
  });
});
$(function () {
    $("#BrandName").focus();
});

$(function () {
    function Check() {
        var name = $("#BrandName").val();
        if (name == "") {
            $.dialog.tips("请填写品牌名称！");
        }
        else {

            var img = $("input[name='BrandLogo']").val();
            if (img == "") {
                $.dialog.tips("请上传一张品牌图片！");
            } else {
                var loading = showLoading();
                $.ajax({
                    type: 'post',
                    url: $("#UAie").val(),
                    data: { name: name },
                    dataType: "json",
                    async: true,
                    success: function (data) {
                        if (data.success == true) {
                            $.dialog.tips("该品牌已存在，请不要重复添加！");
                            loading.close();
                        } else {
                            if (data.msg) {
                                $.dialog.tips(data.msg);
                                loading.close();
                            }
                            else {
                                $("#brandSubmit").submit();
                            }
                        }
                    },
                    error: function () {
                        loading.close();
                    }
                });
            }
        }
        return false;
    }
})
