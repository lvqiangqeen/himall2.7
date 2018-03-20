var isSend = 1;

//发送验证码
$("#sendCode_Phone").on("click", function () {
    if ($("#sendCode_bank").attr("diz") != null) {
        return false;
    }
    $.post('/SellerAdmin/AccountSettings/SendCode', { pluginId: $("#pluginId_Phone").val(), destination: $("#destination_Phone").val() }, function (result) {
        if (result.success) {
            var count = 120;
            sia1 = setInterval(function () { count--; countDown1Phone1(count, "sendCode_Phone"); }, 1000);
        }
        else {
            $.dialog.errorTips('发送验证码失败：' + result.msg);
        }
    });
})

function countDown1Phone1(ss, dv) {
    if (ss > 0) {
        $("#" + dv).html("重新获取（" + ss + "s）");
        $("#" + dv).attr("diz", "1");
    } else {
        $("#" + dv).html("获取验证码");
        $("#" + dv).removeAttr("diz");
        clearInterval(sia1);
    }
}

//绑定新手机验证码
$("#sendCode_newPhone").on("click", function () {
    if ($("#sendCode_bank").attr("diz") != null) {
        return false;
    }
    if ($("#new_phone").val() == "") {
        $.dialog.errorTips('请输入验证手机号码！');
        return false;
    }
    $.post('/SellerAdmin/AccountSettings/SendPhoneCode', { pluginId: $("#pluginId_Phone").val(), destination: $("#new_phone").val() }, function (result) {
        if (result.success) {
            var count = 120;
            sia2 = setInterval(function () { count--; countDown1Phone2(count, "sendCode_newPhone"); }, 1000);
        }
        else {
            $.dialog.errorTips('发送验证码失败：' + result.msg);
        }
    });
})

function countDown1Phone2(ss, dv) {
    if (ss > 0) {
        $("#" + dv).html("重新获取（" + ss + "s）");
        $("#" + dv).attr("diz", "1");
    } else {
        $("#" + dv).html("获取验证码");
        $("#" + dv).removeAttr("diz");
        clearInterval(sia2);
    }
}

//下一步，验证验证码
$('#Phone_next').click(function () {
    $.post('/SellerAdmin/AccountSettings/verificationCode', { pluginId: $("#pluginId_Phone").val(), code: $("#code_Phone").val() }, function (result) {
        if (result.success) {
            $("#sendCode_bank").removeAttr("diz");
            JumpPhone(1);
        }
        else {
            $.dialog.errorTips('验证码错误！');
        }
    });
});

$(function () {

    //手机设置
    $('.PhoneBindBtn').click(function () {
        if ($("#destination_Phone").val() == "") {
            JumpPhone(1);
        }
        $('#PhoneBind').show();
        if ($('.cover').length == 0) {
            $('#PhoneBind').after('<div class="cover"></div>');
        }
        $('.cover').fadeIn();
    });

    $(document).on('click', '.cover,.close', function () {
        $('.cover').fadeOut();
        $('#PhoneBind').hide();
    });

    //邮箱设置
    $('.EmaliBindBtn').click(function () {
        if ($("#destination_Emali").val() == "") {
            JumpEmali(1);
        }
        $('#EmaliBind').show();
        if ($('.cover').length == 0) {
            $('#EmaliBind').after('<div class="cover"></div>');
        }
        $('.cover').fadeIn();
    });

    $(document).on('click', '.cover,.close', function () {
        $('.cover').fadeOut();
        $('#EmaliBind').hide();
    });
})

//绑定手机提交
$("#btn_Phonestep2").on("click", function () {
    $.post('/SellerAdmin/AccountSettings/setPhone', { pluginId: $("#pluginId_Phone").val(), code: $("#new_phoneCode").val(), phone: $("#new_phone").val() }, function (result) {
        if (result.success) {
            JumpPhone(2);
        }
        else {
            $.dialog.errorTips(result.msg);
        }
    });
})

//完成
$(".btn_fi").on("click", function () {
    window.location.reload();
})

//跳转
function JumpPhone(step) {
    $('.Phone-step-' + (step+1)).show().siblings().hide();
    $('#PhoneBind .choose-step li').eq(step).addClass('active').siblings().removeClass();
}


//邮件相关
var isSend1 = 0;
//发送验证码
$("#sendCode_Emali").on("click", function () {
    if ($("#sendCode_Emali").attr("diz") != null) {
        return false;
    }
    $.post('/SellerAdmin/AccountSettings/SendCode', { pluginId: $("#pluginId_Emali").val(), destination: $("#destination_Emali").val() }, function (result) {
        if (result.success) {
            var count = 120;
            sie1 = setInterval(function () { count--; countDown1Email1(count, "sendCode_Emali"); }, 1000);
        }
        else {
            $.dialog.errorTips('发送验证码失败：' + result.msg);
        }
    });
})
function countDown1Email1(ss, dv) {
    if (ss > 0) {
        $("#" + dv).html("重新获取（" + ss + "s）");
        $("#" + dv).attr("diz", "1");
    } else {
        $("#" + dv).html("获取验证码");
        $("#" + dv).removeAttr("diz");
        clearInterval(sie1);
    }
}

//绑定新邮箱验证码
$("#sendCode_newEmali").on("click", function () {
    if ($("#sendCode_newEmali").attr("diz") != null) {
        return false;
    }
    if ($("#new_Emali").val() == "") {
        $.dialog.errorTips('请输入验证邮箱！');
        return false;
    }
    $.post('/SellerAdmin/AccountSettings/SendPhoneCode', { pluginId: $("#pluginId_Emali").val(), destination: $("#new_Emali").val() }, function (result) {
        if (result.success) {
            var count = 120;
            sie2 = setInterval(function () { count--; countDown1Email2(count, "sendCode_newEmali"); }, 1000);
        }
        else {
            $.dialog.errorTips('发送验证码失败：' + result.msg);
        }
    });
})

function countDown1Email2(ss, dv) {
    if (ss > 0) {
        $("#" + dv).html("重新获取（" + ss + "s）");
        $("#" + dv).attr("diz", "1");
    } else {
        $("#" + dv).html("获取验证码");
        $("#" + dv).removeAttr("diz");
        clearInterval(sie2);
    }
}

//下一步，验证验证码
$('#Emali_next').click(function () {
    $.post('/SellerAdmin/AccountSettings/verificationCode', { pluginId: $("#pluginId_Emali").val(), code: $("#code_Emali").val() }, function (result) {
        if (result.success) {
            $("#Emali_next").removeAttr("diz");
            JumpEmali(1);
        }
        else {
            $.dialog.errorTips('验证码错误！');
        }
    });
});


//绑定邮箱提交
$("#btn_Emalistep2").on("click", function () {
    $.post('/SellerAdmin/AccountSettings/setPhone', { pluginId: $("#pluginId_Emali").val(), code: $("#new_EmaliCode").val(), phone: $("#new_Emali").val() }, function (result) {
        if (result.success) {
            JumpEmali(2);
        }
        else {
            $.dialog.errorTips(result.msg);
        }
    });
})

$(".btn_fi").on("click", function () {
    window.location.reload();
})

function JumpEmali(step) {
    switch (step) {
        case 0:
            $('.Emalistep-1').show().siblings().hide();
            $('#EmaliBind .choose-step li').eq(0).addClass('active').siblings().removeClass();
            break;
        case 1:
            $('.Emali-step-2').show().siblings().hide();
            $('#EmaliBind .choose-step li').eq(1).addClass('active').siblings().removeClass();
            break;
        case 2:
            $('.Emali-step-3').show().siblings().hide();
            $('#EmaliBind .choose-step li').eq(2).addClass('active').siblings().removeClass();
            break;
    }
}

