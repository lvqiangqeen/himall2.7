var reg = /^0*(1)\d{10}$/;var emailReg = /^([a-zA-Z0-9_-])+@([a-zA-Z0-9_-])+(.[a-zA-Z0-9_-])+/;var canSendCode=true;

//更换手机
$("#phoneCh").on("click", function () {
    $(this).parent().hide();
    $("#codebtn").show();
    $("#MemberPhone").removeAttr("readonly");
})

//更换邮箱
$("#emailCh").on("click", function () {
    $(this).parent().hide();
    $("#getEmailCode").show();
    $("#MemberEmail").removeAttr("readonly");
})

//获取手机验证码
$("#codebtn").on("click", function () {
    if(canSendCode){		sendPhoneCode();	}
})

$("#codebtnT").on("click", function () {	if(canSendCode){		sendPhoneCode();	}
})

function sendPhoneCode() {
    var Phone = $("#MemberPhone").val();
    if (Phone == "") {
        $.dialog.errorTips('请输入手机号码');
        return false;
    }
    if (!reg.test($("#MemberPhone").val())) {
        $.dialog.errorTips('请输入正确的手机号码');
        $("#MemberPhone").focus();
        return false;
    }
    if ($("#codebtnT").attr("disabled") == "disabled") {
        console.log(1);
        return false;
    }
    $.post('SendCode', { pluginId: "Himall.Plugin.Message.SMS", destination: Phone }, function (result) {
        if (result.success) {
            var count = 120;
            $("#pcv").show();
            si = setInterval(function () { count--; countDown1(count, "codebtn"); }, 1000);
        }
        else {
            $.dialog.errorTips('发送验证码失败：' + result.msg);
        }
    });
}

function countDown1(ss, dv) {
    if (ss > 0) {
        $("#" + dv).html("重新获取(" + ss + "s)");
//      $("#" + dv).attr("disabled", "disabled");        canSendCode = false;
    } else {
        $("#" + dv).html("获取验证码");
//      $("#" + dv).removeAttr("disabled");        canSendCode = true;
        clearInterval(si);
    }
}

//获取邮箱验证码
$("#getEmailCode").on("click", function () {
    sendEmailCode();
})
$("#getEmailCodeT").on("click", function () {
    sendEmailCode();
})

function sendEmailCode() {
    var Email = $("#MemberEmail").val();
    if (Email == "") {
        $.dialog.errorTips('请输入邮箱');
        return false;
    }
    if (!emailReg.test($("#MemberEmail").val())) {
        $.dialog.errorTips('请输入正确的邮箱');
        $("#MemberEmail").focus();
        return false;
    }
    if ($("#getEmailCodeT").attr("disabled") == "disabled") {
        console.log(1);
        return false;
    }
    $.post('SendCode', { pluginId: "Himall.Plugin.Message.Email", destination: Email }, function (result) {
        if (result.success) {
            var count = 120;
            $("#ecv").show();
            sie = setInterval(function () { count--; countDown1Emali(count, "getEmailCodeT"); }, 1000);
        }
        else {
            $.dialog.errorTips('发送验证码失败：' + result.msg);
        }
    });
}

function countDown1Emali(ss, dv) {
    if (ss > 0) {
        $("#" + dv).val("重新获取（" + ss + "s）");
        $("#" + dv).attr("disabled", "disabled");
    } else {
        $("#" + dv).val("获取验证码");
        $("#" + dv).removeAttr("disabled");
        clearInterval(sie);
    }
}