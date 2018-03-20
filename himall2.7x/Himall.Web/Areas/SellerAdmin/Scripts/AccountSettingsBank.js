var isSend = 1;
var opid = '';
var sceneid = '';
var url = "https://mp.weixin.qq.com/cgi-bin/showqrcode?ticket=";

//发送验证码
$("#sendCode_bank").on("click", function () {
    if ($("#sendCode_bank").attr("diz") != null) {
        return false;
    }
    $.post('/SellerAdmin/AccountSettings/SendCode', { pluginId: $("#pluginId_bank").val(), destination: $("#destination_bank").val() }, function (result) {
        if (result.success) {
            var count = 120;
            siABank = setInterval(function () { count--; countDown1Bank(count, "sendCode_bank"); }, 1000);
        }
        else {
            $.dialog.errorTips('发送验证码失败：' + result.msg);
        }
    });
})


function countDown1Bank(ss, dv) {
    if (ss > 0) {
        $("#" + dv).html("重新获取（" + ss + "s）");
        $("#" + dv).attr("diz", "1");
    } else {
        $("#" + dv).html("获取验证码");
        $("#" + dv).removeAttr("diz");
        clearInterval(siABank);
    }
}

//下一步，验证验证码
$('#bank_next').click(function () {
    $.post('/SellerAdmin/AccountSettings/verificationCode', { pluginId: $("#pluginId_bank").val(), code: $("#code_bank").val() }, function (result) {
        if (result.success) {
            JumpBank(1);
        }
        else {
            $.dialog.errorTips('验证码错误！');
        }
    });
});

$(function () {

    //银行设置
    $('.bankBindBtn').click(function () {
        $('#BankBind').show();
        if ($('.cover').length == 0) {
            $('#BankBind').after('<div class="cover"></div>');
        }
        $('.cover').fadeIn();
    });

    $(document).on('click', '.cover,.close', function () {
        $('.cover').fadeOut();
        $('#BankBind').hide();
    });
})

//银行提交
$("#btn_bankstep3").on("click", function () {
    var BankAccountName = $("#BankAccountName").val();
    var BankAccountNumber = $("#BankAccountNumber").val();
    var BankName = $("#BankName").val();
    var BankCode = $("#BankCode").val();
    var BankRegionId = $("#BankRegionId").val();
    if (BankAccountName == "") {
        $.dialog.errorTips('请输入开户行！');
        return false;
    }
    if (BankAccountName.length > 100) {
        $.dialog.errorTips('开户行名称最大长度不能超过100！');
        return false;
    }
    if (BankAccountNumber == "") {
        $.dialog.errorTips('请输入银行账号！');
        return false;
    }
    if (BankAccountNumber.length > 100) {
        $.dialog.errorTips('银行账号最大长度不能超过100！');
        return false;
    }
    if (BankName == "") {
        $.dialog.errorTips('请输入支行名称！');
        return false;
    }
    if (BankName.length > 100) {
        $.dialog.errorTips('支行名称最大长度不能超过100！');
        return false;
    }
    if (BankRegionId == "" || $("#BankRegionId").attr("isfinal") == "false") {
        $.dialog.errorTips('请选择开户银行所在地！');
        return false;
    }
    $.post("/SellerAdmin/AccountSettings/setBank", { BankAccountName: BankAccountName, BankAccountNumber: BankAccountNumber, BankName: BankName, BankCode: BankCode, BankRegionId: BankRegionId }, function (data) {
        if (data.success) {
            JumpBank(2);
        } else {
            $.dialog.errorTips("绑定失败!");
        }
    }, "json");
})

//银行完成
$("#btn_bankfi").on("click", function () {
    window.location.reload();
})

//银行跳转
function JumpBank(step) {
    //$('div[name="stepname"]').hide();
    switch (step) {
        case 0:
            $('.bank-step-1').show().siblings().hide();
            $('#BankBind .choose-step li').eq(0).addClass('active').siblings().removeClass();
            break;
        case 1:
            $('.bank-step-2').show().siblings().hide();
            $('#BankBind .choose-step li').eq(1).addClass('active').siblings().removeClass();
            break;
        case 2:
            $('.bank-step-3').show().siblings().hide();
            $('#BankBind .choose-step li').eq(2).addClass('active').siblings().removeClass();
            break;
    }
}
