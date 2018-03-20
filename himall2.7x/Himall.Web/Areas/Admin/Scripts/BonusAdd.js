// JavaScript source code
var self = 0;
$(function () {
  
    $("#subCate").click(function () {
        var type = $('input[name=Type]:checked').val();
        if (type == 2) {
            $.post("/Admin/Bonus/CanAdd", {}, function (result) {
                if (!result) {
                    $.dialog.tips('关注红包一个时段只能添加一个！');
                    return false;
                }

                if (check()) {
                    document.getElementById("BonusForm").submit();
                    $("#subCate").attr("disabled", true);
                }
            })
        }
        else {
            if (check()) {
                document.getElementById("BonusForm").submit();
                $("#subCate").attr("disabled", true);
            }
        }

    })

    $("#IsAttention").change(function () {
        if ($("#IsAttention").is(":checked")) {
            $.dialog.confirm("强制用户关注公众号、引诱用户分享等行为可能会引起微信对活动页面进行拦截，严重时甚至会封号，是否确认开启？",
                function () { },
                function () {
                    $("#IsAttention").removeAttr("checked");
                })
        }
    })

    $("#IsGuideShare").change(function () {
        if ($("#IsGuideShare").is(":checked")) {
            $.dialog.confirm("强制用户关注公众号、引诱用户分享等行为可能会引起微信对活动页面进行拦截，严重时甚至会封号，是否确认开启？",
                function () { },
                function () {
                    $("#IsGuideShare").removeAttr("checked");
                })
        }
    })

    $("input[name=Type]").change(function () {
        
        self = $(this).val();
        if (self == 1) {
            $("#typemsg").addClass("hide");
            $("#Name").parents(".form-group").removeClass("hide");
            $("#labBonusName")[0].innerHTML="活动标题：";
            $("#MerchantsName").parents(".form-group").removeClass("hide");
            $("#Remark").parents(".form-group").removeClass("hide");
            $("#Blessing").parents(".form-group").removeClass("hide");
            $("#Description").parents(".form-group").removeClass("hide");
            $("#upload-img").parents(".form-group").removeClass("hide");
            $("#IsAttention").parents(".form-group").removeClass("hide");
            $("#IsGuideShare").parents(".form-group").removeClass("hide");
            $("#StartTime").parents(".form-group").removeClass("hide");
            $("#EndTime").parents(".form-group").removeClass("hide");
        }
        else if (self == 2) {
            $("#typemsg").removeClass("hide");
            $("#Name").parents(".form-group").addClass("hide");
            $("#MerchantsName").parents(".form-group").addClass("hide");
            $("#Remark").parents(".form-group").addClass("hide");
            $("#Blessing").parents(".form-group").addClass("hide");
            $("#Description").parents(".form-group").addClass("hide");
            $("#upload-img").parents(".form-group").addClass("hide");
            $("#IsAttention").parents(".form-group").addClass("hide");
            $("#IsGuideShare").parents(".form-group").addClass("hide");
            $("#StartTime").parents(".form-group").removeClass("hide");
            $("#EndTime").parents(".form-group").removeClass("hide");
            $(".form-group.bunus-tip").removeClass("hide");
        }
        else if (self == 3) {
            $("#typemsg").addClass("hide");
            $("#Name").parents(".form-group").removeClass("hide");
            $(".col-sm-6").addClass("hide");
            $("#labBonusName")[0].innerHTML = "红包名称：";
            $("#MerchantsName").parents(".form-group").addClass("hide");
            $("#Remark").parents(".form-group").addClass("hide");
            $("#Blessing").parents(".form-group").addClass("hide");
            $("#Description").parents(".form-group").addClass("hide");
            $("#upload-img").parents(".form-group").addClass("hide");
            $("#IsAttention").parents(".form-group").addClass("hide");
            $("#IsGuideShare").parents(".form-group").addClass("hide");
            $("#StartTime").parents(".form-group").addClass("hide");
            $("#EndTime").parents(".form-group").addClass("hide");
            $(".form-group.bunus-tip").addClass("hide");
        }
       
    });
    $("input[name=PriceType]").change(function () {
        var self = $(this).val();
        if (self == 1) {
            $("input[name=FixedAmount]").removeAttr("disabled");
            $("input[name=RandomAmountEnd]").attr('disabled', "true");
            $("input[name=RandomAmountStart]").attr('disabled', "true");
        }
        else if (self == 2) {
            $("input[name=FixedAmount]").attr('disabled', "true");
            $("input[name=RandomAmountEnd]").removeAttr("disabled");
            $("input[name=RandomAmountStart]").removeAttr("disabled");
        }
    })

    $("#upload-img").himallUpload({
        title: '',
        imageDescript: ' 尺寸100*100',
        imgFieldName: "ImagePath",
        dataWidth: 10
    });

    $(".start_datetime").datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd',
        autoclose: true,
        weekStart: 1,
        minView: 2
    });
    $(".end_datetime").datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd',
        autoclose: true,
        weekStart: 1,
        minView: 2
    });

    $('.start_datetime').on('changeDate', function () {
        if ($(".end_datetime").val()) {
            if ($(".start_datetime").val() > $(".end_datetime").val()) {
                $('.end_datetime').val($(".start_datetime").val());
            }
        }

        $('.end_datetime').datetimepicker('setStartDate', $(".start_datetime").val());
    });
})

function check() {
    var bonusType = $('input[name=Type]:checked').val();
    if (bonusType == 1) {
        if ($.trim($("#Name").val()) == '') {
            $.dialog.tips('活动名称必填！');
            return false;
        }
        if ($.trim($("#MerchantsName").val()) == '') {
            $.dialog.tips('商户名称必填！');
            return false;
        }
        if ($.trim($("input[name=ImagePath]").val()) == '') {
            $.dialog.tips('请上传图片！');
            return false;
        }
        if ($.trim($("#Description").val()) == '') {
            $.dialog.tips('备注必填！');
            return false;
        }
        if ($.trim($("#Blessing").val()) == '') {
            $.dialog.tips('祝福语必填！');
            return false;
        }
    }
    if (bonusType==3) {
        if ($.trim($("#Name").val()) == '') {
            $.dialog.tips('红包名称必填！');
            return false;
        }
    }

    var type = $('input[name=PriceType]:checked').val();
    if ($.trim($("#TotalPrice").val()) == '') {
        $.dialog.tips('总金额必填！');
        return false;
    }
    var totalPrice = parseFloat($("#TotalPrice").val());

    if (self<3) {
        if ($.trim($("#StartTime").val()) == '') {
            $.dialog.tips('开始日期必填！');
            return false;
        }
        if ($.trim($("#EndTime").val()) == '') {
            $.dialog.tips('结束日期必填！');
            return false;
        }

        if ($.trim($("#StartTime").val()) > $.trim($("#EndTime").val())) {
            $.dialog.tips('结束日期必须大于开始日期！');
            return false;
        }
    }
   
    if (totalPrice > 10000) {
        $.dialog.tips('总面额不能大于1万！');
        return false;
    }

    if (type == 1)  //固定
    {
        if ($.trim($("#FixedAmount").val()) == '') {
            $.dialog.tips('固定金额必填！');
            return false;
        }
        var fixedAmount = parseFloat($("#FixedAmount").val());
        if (fixedAmount > totalPrice) {
            $.dialog.tips('单个面额不能大于总面额！');
            return false;
        }
        if (fixedAmount > 200 || fixedAmount < 0.01) {
            $.dialog.tips('面额错误，取值为0.01 - 200');
            return false;
        }
        if ((Math.round(totalPrice * 100) % Math.round(fixedAmount * 100)) != 0) {
            $.dialog.tips('总金额必须是固定金额的倍数！');
            return false;
        }
    }
    else if (type == 2)  //随机
    {
        if ($.trim($("#RandomAmountEnd").val()) == '') {
            $.dialog.tips('随机金额必填！');
            return false;
        }
        if ($.trim($("#RandomAmountStart").val()) == '') {
            $.dialog.tips('随机金额必填！');
            return false;
        }
        var randomAmountEnd = parseFloat($("#RandomAmountEnd").val());
        var randomAmountStart = parseFloat($("#RandomAmountStart").val());
        if (randomAmountEnd > totalPrice) {
            $.dialog.tips('单个面额不能大于总面额！');
            return false;
        }
        if (randomAmountStart > randomAmountEnd) {
            $.dialog.tips('随机金额填写错误！');
            return false;
        }
        if (isNaN(randomAmountStart) || isNaN(randomAmountEnd) || randomAmountStart > 200 || randomAmountEnd > 200 || randomAmountStart < 0.01 || randomAmountEnd < 0.01) {
            $.dialog.tips('面额不能大于200小于0.01！');
            return false;
        }
    }
    else {
        return false;
    }
    return true;
}