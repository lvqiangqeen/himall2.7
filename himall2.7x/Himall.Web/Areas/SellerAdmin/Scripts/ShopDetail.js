// JavaScript source code
var companyRegionIds = $("#VCRID").val();

$(function () {
    $('#Save').click(function () {
        var form = $('#from_Save');
        if (form.valid()) {

            $('#CompanyRegisteredCapital').val($('#CompanyRegisteredCapital').val().replace(/,/g, ''));
            //if (isSelectAddr($("#companyLocationProvince"), $("#companyLocationCity"), $("#companyLocationDistrict"))) {
            //    var region = $("#companyLocationDistrict").val() == 0 ? $("#companyLocationCity").val() : $("#companyLocationDistrict").val();
            //    $('#NewCompanyRegionId').val(region);
            //}
            var data = form.serialize();

            //data=data+'&CompanyRegionId='
            var loading = showLoading();
            $.post('EditProfile1', data, function (result) {
                loading.close();
                if (result.success) {
                    $.dialog.tips('保存成功！');
                    window.location.reload();
                }
                else
                    $.dialog.errorTips('保存失败！' + result.msg);
            });
        } else {
            $(".input-validation-error")[0].focus();
        }
    });

    var businessLicenseCerts = $("#HRJVB").val();
    var productCerts = $("#HRJVP").val();
    var otherCerts = $("#HRJVO").val();

    $("#BusinessLicenseCert").hishopUpload({
        title: '经营许可类证书:(最多三张)',
        imageDescript: '<label class="pic-upload-suggest">(图片大小不能超过1M)</label>',
        displayImgSrc: businessLicenseCerts,
        imgFieldName: "BusinessLicenseCert",
        defaultImg: '/Images/default_100x100.png',
        imagesCount: 3,
        dataWidth: 6
    });
    $("#ProductCert").hishopUpload({
        title: '产品类证书:(最多三张)',
        imageDescript: '<label class="pic-upload-suggest">(图片大小不能超过1M)</label>',
        displayImgSrc: productCerts,
        imgFieldName: "ProductCert",
        defaultImg: '/Images/default_100x100.png',
        imagesCount: 3,
        dataWidth: 6
    });
    $("#OtherCert").hishopUpload({
        title: '其它证书:(最多三张)',
        imageDescript: '<label class="pic-upload-suggest">(图片大小不能超过1M)</label>',
        displayImgSrc: otherCerts,
        imgFieldName: "OtherCert",
        defaultImg: '/Images/default_100x100.png',
        //pictureSize: '100*100',
        imagesCount: 3,
        dataWidth: 6
    });

    $('.dEdit_2.shide .upload-img').append('<div class="col-sm-3"><span class="pic-upload-suggest">(图片大小不能超过1M)</span></div>');

    $("#span_area").RegionSelector({
        selectClass: "form-control input-sm select-sort",
        valueHidden: "#NewCompanyRegionId"
    });
});

function bindCity(provinceControl, cityControl, countyControl, regionId) {
    var regions = regionId.split(',');
    var shengfen = regions.length > 0 ? regions[0] : 0;
    var city = regions.length > 1 ? regions[1] : 0;
    var county = regions.length > 2 ? regions[2] : 0;
    $('#' + provinceControl + ',#' + cityControl + ',#' + countyControl).cityLink(shengfen, city, county);
}

//点修改
$("a[id^='edit_']").on("click", function () {
    var id = $(this).attr("id").split('_')[1];
    $(".dView_" + id).hide();
    $(".dEdit_" + id).show();
})

//点击取消
$("a[id^='cancel_']").on("click", function () {
    var id = $(this).attr("id").split('_')[1];
    $(".dView_" + id).show();
    $(".dEdit_" + id).hide();
})

//公司信息保存
$("#save_1").on("click", function () {
    var form = $('#from_Save1');
    if (form.valid()) {
        var data = form.serialize();

        var loading = showLoading();
        $.post('Edit1', data, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.tips('保存成功！');
                window.location.reload();
            }
            else
                $.dialog.errorTips('保存失败！' + result.msg);
        });
    } else {
        $(".input-validation-error")[0].focus();
    }
})

//个人保存
$("#save_Personal1").on("click", function () {
    var form = $('#from_SavePersonal1');
    if (form.valid()) {
        var data = form.serialize();

        var loading = showLoading();
        $.post('EditPersonal1', data, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.tips('保存成功！');
                window.location.reload();
            }
            else
                $.dialog.errorTips('保存失败！' + result.msg);
        });
    } else {
        $(".input-validation-error")[0].focus();
    }
})

//营业执照信息
$("#save_2").on("click", function () {
    var form = $('#from_Save2');
    if (form.valid()) {
        var data = form.serialize();

        var loading = showLoading();
        $.post('Edit2', data, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.tips('保存成功！');
                window.location.reload();
            }
            else
                $.dialog.errorTips('保存失败！' + result.msg);
        });
    } else {
        $(".input-validation-error")[0].focus();
    }
})


//保存真实姓名
$("#save_5").on("click", function () {
    if ($("#RealName").val().length < 2) {
        $.dialog.errorTips('真实姓名长度不得小于2！');
        return false;
    }
    if ($("#RealName").val().length >10) {
        $.dialog.errorTips('真实姓名长度不得大于10！');
        return false;
    }
    var loading = showLoading();
    $.post('Edit5', { RealName: $("#RealName").val() }, function (result) {
        loading.close();
        if (result.success) {
            $.dialog.tips('保存成功！');
            window.location.reload();
        }
        else
            $.dialog.errorTips('保存失败！' + result.msg);
    },"json");
})

$("#btn_winxinfi").on("click", function () {
    window.location.reload();
})