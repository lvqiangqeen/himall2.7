// JavaScript source code
$(function () {
    $("input[name='mode']").change(function () {
        var mode = $(this).val();
        if (mode == "1") {
            $("#divExist").show();
            $("#divNew").hide();
        }
        else {
            $("#divExist").hide();
            $("#divNew").show();
        }
    });
    InitBrands();
    UploadInit();
    InitBrandAuthPic();
    InitBrandLetter();
    $("#band_name").blur(function () {
        var name = $('#band_name').val();
        var reg = /^[a-zA-Z0-9\u4e00-\u9fa5]+$/;
        if (name.length > 30) {
            $("#brandNameTip").text("请输入小于30个字符！");
            $('#band_name').css({ border: '1px solid #f60' });
            $('#band_name').focus();
            return false;
        }
        if (reg.test(name) == false) {
            $("#brandNameTip").text("品牌名称必须是中文，字母，数字！");
            $('#band_name').css({ border: '1px solid #f60' });
            $('#band_name').focus();
            return false;
        }
        else {
            $("#brandNameTip").text("");
            $('#band_name').css({ border: '1px solid #ccc' });
        }

        $.ajax({
            type: 'post',
            url: './IsExist',
            data: { name: name },
            dataType: "json",
            async: false,
            success: function (data) {
                if (data.success == true) {
                    $("#brandNameTip").text("该品牌已存在，请选择申请已有品牌！");
                    $('#band_name').css({ border: '1px solid #f60' });
                    $('#band_name').focus();
                }
                else {
                    $("#brandNameTip").text("");
                    $('#band_name').css({ border: '1px solid #ccc' });
                }
            }
        });
    });

    $("#band_remark").blur(function () {
        var band_remark = $("#band_remark").val();
        if (band_remark.length > 0) {
            if (band_remark.length > 200) {
                $("#band_remarkTip").text("备注不能超过200个字符！");
                $('#band_remark').css({ border: '1px solid #f60' });
                $('#band_remark').focus();
                return false;
            }
        }
    });

    $("#band_des").blur(function () {
        var band_des = $("#band_des").val();
        if (band_des.length > 0) {
            if (band_des.length > 200) {
                $("#band_desTip").text("简介不能超过200个字符！");
                $('#band_des').css({ border: '1px solid #f60' });
                $('#band_des').focus();
                return false;
            }
        }
    });
});