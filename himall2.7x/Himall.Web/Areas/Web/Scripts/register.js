
$(function () {
    bindCheckCode();

    checkUserName();

    checkPassword();

    checkMobile();

    checkEmail();

    checkCheckCode();

    checksysCheckCode();

    bindSubmit();

    $('#regName').focus();
});


function bindSubmit() {
    $('#registsubmit').click(function () {
        var result = checkValid();
        if (result) {
            var username = $('#regName').val(), password = $('#pwd').val();
            var mobile = $('#cellPhone').val();
            var email = $('#callEmail').val();
            var syscode = $('#syscheckCode').val();
            var introducer = $("#introducer").val();
            var checkCode = $('#checkCode').val();
            var loading = showLoading();
            $.post('/Register/RegisterUser', { username: username, password: password, mobile: mobile, email: email, introducer: introducer,checkCode:checkCode }, function (data) {
                loading.close();
                if (data.success) {
                    var strMessage = "注册成功！";
                    if (data.num > 0) {
                        strMessage = "注册成功，获得" + data.num + "张赠送优惠券！";
                    } else if (getQueryString("type") == "gift") {
                        strMessage = "很抱歉！优惠券已被领完，请期待下次活动！";
                    }
                    $.dialog.succeedTips(strMessage, function () {
                        location.href = '/userCenter/home';
                    }, 3);
                }
                else {
                    $.dialog.errorTips("注册失败！" + data.msg);
                }
            });
        }
    });
}

//获取URL中值
function getQueryString(name) {
    var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)", "i");
    var r = window.location.search.substr(1).match(reg);
    if (r != null) return unescape(r[2]); return null;
}

function checkPassword() {

    $('#pwd').focus(function () {
        $('#pwd_info').show();
        $('#pwd_error').removeClass('error').addClass('focus').hide();
    }).blur(function () {
        $('#pwd_info').hide();
        checkPasswordIsValid();
    });

    $('#pwdRepeat').focus(function () {
        $('#pwdRepeat_info').show();
        $('#pwdRepeat_error').removeClass('error').addClass('focus').hide();

    }).blur(function () {
        $('#pwdRepeat_info').hide();
        checkRepeatPasswordIsValid();
    });

}

function checkUserName() {
    $('#regName').change(function () {
        var regName = $.trim($(this).val());
        if (!regName)
            $('#regName_error').show();
        else
            $('#regName_error').hide();
    }).focus(function () {
        $('#regName_info').show();
        $('#regName_error').hide();
    }).blur(function () {
        $('#regName_info').hide();
        checkUsernameIsValid();
    });
}

function bindCheckCode() {
    $('#checkCodeChangeBtn,#checkCodeImg').click(function () {
        var src = $('#checkCodeImg').attr('src');
        $('#checkCodeImg').attr('src', src);
    });
}


function checkValid() {
    //return checkUsernameIsValid() & checkPasswordIsValid() & checkPasswordIsValid() & checkRepeatPasswordIsValid() & checkCheckCodeIsValid() & checkAgreementIsValid() & checkMobileIsValid();
    var result = checkUsernameIsValid() & checkPasswordIsValid() & checkPasswordIsValid();
    result = result & checkRepeatPasswordIsValid() & checkCheckCodeIsValid();
    result = result & checkMobileIsValid() & checkEmailIsValid();
    result = result & checkAgreementIsValid() & checksysCheckCodeIsValid();
    return result;
}

function checksysCheckCodeIsValid() {
    var checkCode = $('#syscheckCode').val();
    var errorLabel = $('#syscheckCode_error');
    checkCode = $.trim(checkCode);

    var result = true;
    var destination = $("#cellPhone").val();
    var pluginId = "Himall.Plugin.Message.SMS";
    var ischeckemail = ($("#ischeckemail").val() == 1);
    var ischecktel = ($("#ischecktel").val() == 1);
    if (ischeckemail) {
        pluginId = "Himall.Plugin.Message.Email";
        destination = $("#callEmail").val();
    }
    if (ischecktel || ischeckemail) {
        result = false;
    }
    if (!result) {
        if (checkCode && (ischecktel || ischeckemail)) {
            $.ajax({
                type: "post",
                url: "/register/CheckCode",
                data: { pluginId: pluginId, code: checkCode, destination: destination },
                dataType: "json",
                async: false,
                success: function (data) {
                    if (data.success == true) {
                        errorLabel.hide();
                        result = true;
                    }
                    else {
                        errorLabel.html('验证码不正确或者已经超时').show();
                    }
                }
            });
        } else {
            errorLabel.html('请输入验证码').show();
        }
    }
    return result;
}

function checkCheckCodeIsValid() {
    var checkCode = $('#checkCode').val();
    var errorLabel = $('#checkCode_error');
    checkCode = $.trim(checkCode);

    var result = false;
    //if (checkCode && $('#cellPhone').length == 0) {
    if (checkCode) {
        $.ajax({
            type: "post",
            url: "/register/CheckCheckCode",
            data: { checkCode: checkCode },
            dataType: "json",
            async: false,
            success: function (data) {
                if (data.success) {
                    if (data.result) {
                        errorLabel.hide();
                        result = true;
                    }
                    else {
                        errorLabel.html('验证码错误').show();
                    }
                }
                else {
                    $.dialog.errorTips("验证码校验出错", '', 1);
                }
            }
        });
    } else {
        errorLabel.html('请输入验证码').show();
    }
    //}
    //else if ($('#cellPhone').length > 0&&checkCode) {
    //    $.ajax({
    //        type: "post",
    //        url: "/register/CheckCode",
    //        data: { pluginId: "Himall.Plugin.Message.SMS", code: checkCode, destination: $("#cellPhone").val() },
    //        dataType: "json",
    //        async: false,
    //        success: function (data) {
    //            if (data.success == true) {
    //                errorLabel.hide();
    //                result = true;
    //            }
    //            else {
    //                errorLabel.html('验证码不正确或者已经超时').show();
    //            }
    //        }
    //    });
    //}
    //else {
    //    errorLabel.html('请输入验证码').show();
    //}
    return result;
}

function checkCheckCode() {
    var errorLabel = $('#checkCode_error');
    $('#checkCode').focus(function () {
        errorLabel.hide();
    }).blur(function () {
        checkCheckCodeIsValid();
    });
}

function checksysCheckCode() {
    var errorLabel = $('#syscheckCode_error');
    $('#syscheckCode').focus(function () {
        errorLabel.hide();
    }).blur(function () {
        checksysCheckCodeIsValid();
    });
}

function checkUsernameIsValid() {
    var result = false;
    var username = $('#regName').val();
    var regtype = $("#regType").val();
    var errorLabel = $('#regName_error');
    var normalreg = /^([\u4E00-\u9FA5]|[A-Za-z0-9])[\u4E00-\u9FA5\A-Za-z0-9\_\-]{3,11}$/;
    var telreg = /^[\u4E00-\u9FA5\A-Za-z0-9\_\-]{4,20}$/;
    var reg = normalreg;
    if (regtype == 1) {
        reg = telreg;
    }
    if (!username || username == '用户名') {
        errorLabel.html('请输入用户名').show();
        return result;
    }
    if (regtype != 1) {
        if ((/^\d+$/.test(username))) {
            errorLabel.html('不可以使用纯数字用户名').show();
            return result;
        }
    }

    if (!reg.test(username)) {
        errorLabel.html('4-12位字符，支持中英文、数字及"-"、"_"的组合').show();
        return result;
    }
    else {
        $.ajax({
            type: "post",
            url: "/register/CheckUserName",
            data: { username: username },
            dataType: "json",
            async: false,
            success: function (data) {
                if (data.success) {
                    if (!data.result) {
                        errorLabel.hide();
                        result = true;
                    }
                    else {
                        errorLabel.html('用户名 ' + username + ' 已经被占用').show();
                    }
                }
                else {
                    $.dialog.errorTips("用户名校验出错", '', 1);
                }
            }
        });
    }
    return result;
}

function checkPasswordIsValid() {
    var result = false;

    //var reg = /^[\@A-Za-z0-9\!\#\$\%\^\&\*\.\~]{6,22}$/;
    var pwdTextBox = $('#pwd');
    var password = pwdTextBox.val();
    var reg = /^[^\s]{6,20}$/;
    var result = reg.test(password);
    //   var result = password.length >= 6 && password.length <= 20;

    if (!result) {
        $('#pwd_error').addClass('error').removeClass('focus').show();
    }
    else {
        $('#pwd_error').removeClass('error').addClass('focus').hide();
        result = true;
    }
    return result;
}

function checkRepeatPasswordIsValid() {
    var result = false;

    //var reg = /^[\@A-Za-z0-9\!\#\$\%\^\&\*\.\~]{6,22}$/;
    var pwdRepeatTextBox = $('#pwdRepeat');
    var repeatPassword = pwdRepeatTextBox.val(), password = $('#pwd').val();
    //var result = reg.test(password);

    var result = repeatPassword == password;

    if (!result) {
        $('#pwdRepeat_error').addClass('error').removeClass('focus').show();
    }
    else {
        $('#pwdRepeat_error').removeClass('error').addClass('focus').hide();
        result = true;
    }
    return result;
}

function checkAgreementIsValid() {
    var result = false;
    var errorLabel = $('#checkAgreement_error');
    if ($("#readme").is(":checked")) {
        errorLabel.hide();
        result = true;
    } else {
        errorLabel.html('请仔细阅读并同意以上协议').show();
    }
    return result;
}

function reloadImg() {
    $("#checkCodeImg").attr("src", "/Register/GetCheckCode?_t=" + Math.round(Math.random() * 10000));
}

function checkMobile() {
    $('#cellPhone').change(function () {
        var cellPhone = $.trim($(this).val());
        if (!cellPhone)
            $('#cellPhone_error').show();
        else
            $('#cellPhone_error').hide();
    }).focus(function () {
        $('#cellPhone_info').show();
        $('#cellPhone_error').hide();
    }).blur(function () {
        var un = $(this).val();
        $("#regName").val(un);
        $('#cellPhone_info').hide();
        checkMobileIsValid();
    });
}

function checkMobileIsValid() {

    if ($('#cellPhone').length == 0) {
        return true;
    }
    var result = false;
    var cellPhone = $('#cellPhone').val();
    var errorLabel = $('#cellPhone_error');
    var reg = /^0?(13|15|18|14|17)[0-9]{9}$/;

    if (!cellPhone || cellPhone == '手机号码') {
        errorLabel.html('请输入手机号码').show();
    }
    else if (!reg.test(cellPhone)) {
        errorLabel.html('请输入正确的手机号码').show();
    }
    else {
        $.ajax({
            type: "post",
            url: "/register/CheckMobile",
            data: { mobile: cellPhone },
            dataType: "json",
            async: false,
            success: function (data) {
                if (data.result == false) {
                    errorLabel.hide();
                    result = true;
                }
                else {
                    errorLabel.html('手机号码 ' + cellPhone + ' 已经被占用').show();
                }
            }
        });
    }
    return result;
}

var delayTime = 120;
var delayFlag = true;
function countDown() {
    delayTime--;
    $("#sendMobileCode").attr("disabled", "disabled");
    $("#dyMobileButton").html(delayTime + '秒后重新获取');
    if (delayTime == 1) {
        delayTime = 120;
        $("#mobileCodeSucMessage").removeClass().empty();
        $("#dyMobileButton").html("获取短信验证码");
        $("#cellPhone_error").addClass("hide");
        $("#sendMobileCode").removeClass().addClass("btn").removeAttr("disabled");
        delayFlag = true;
    } else {
        delayFlag = false;
        setTimeout(countDown, 1000);
    }
}

function sendMobileCode() {

    $('#cellPhone_error').hide();
    if ($("#sendMobileCode").attr("disabled")) {
        return;
    }
    var errorLabel = $('#cellPhone_error');
    var mobile = $("#cellPhone").val();
    var reg = /^0?(13|15|18|14|17)[0-9]{9}$/;
    if (!mobile) {
        $("#cellPhone_error").removeClass().addClass("error").html("请输入手机号");
        $("#cellPhone_error").show();
        return;
    }
    if (!reg.test(mobile)) {
        $("#cellPhone_error").removeClass().addClass("error").html("手机号码格式有误，请输入正确的手机号");
        $("#cellPhone_error").show();
        return;
    }

    if (!checkCheckCodeIsValid()) {
        return;
    }
    //$('#checkCode').removeClass("highlight2");
    // 检测手机号码是否存在
    $.post('/Register/CheckMobile', { mobile: mobile }, function (data) {
        if (data.result == false) {
            errorLabel.hide();
            sendmCode();
        }
        else {
            errorLabel.html('手机号码 ' + mobile + ' 已经被占用').show();
        }
    });

}
// 手机注册发送验证码target
function sendmCode() {
    if ($("#sendMobileCode").attr("disabled") || delayFlag == false) {
        return;
    }

    $("#sendMobileCode").attr("disabled", "disabled");
    jQuery.ajax({
        type: "post",
        url: "/Register/SendCode?pluginId=Himall.Plugin.Message.SMS&destination=" + $("#cellPhone").val(),
        success: function (result) {
            if (result.success == true) {
                $("#cellPhone_error").hide();
                $("#dyMobileButton").html("120秒后重新获取");
                //if (obj.remain) {
                //    $("#mobileCodeSucMessage").empty().html(obj.remain);
                //} else {
                //    $("#cellPhone_error").removeClass().empty().html("验证码已发送，请查收短信。");
                //    $("#cellPhone_error").show();
                //}
                setTimeout(countDown, 1000);
                $("#sendMobileCode").removeClass().addClass("btn").attr("disabled", "disabled");
                $("#syscheckCode").removeAttr("disabled");
            }
        }
    });
}

//邮箱有关
var delayEmailTime = 120;
var delayEmailFlag = true;
function checkEmail() {
    $('#callEmail').change(function () {
        var cellPhone = $.trim($(this).val());
        if (!cellPhone)
            $('#callEmail_error').show();
        else
            $('#callEmail_error').hide();
    }).focus(function () {
        $('#callEmail_info').show();
        $('#callEmail_error').hide();
    }).blur(function () {
        $('#callEmail_info').hide();
        checkEmailIsValid();
    });
}

function checkEmailIsValid() {

    if ($('#callEmail').length == 0) {
        return true;
    }
    var result = false;
    var callEmail = $('#callEmail').val();
    var errorLabel = $('#callEmail_error');
    var reg = /^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$/;

    if (!callEmail || callEmail == '电子邮箱') {
        errorLabel.html('请输入电子邮箱').show();
    }
    else if (!reg.test(callEmail)) {
        errorLabel.html('请输入正确的电子邮箱').show();
    }
    else {
        $.ajax({
            type: "post",
            url: "/register/CheckEmail",
            data: { email: callEmail },
            dataType: "json",
            async: false,
            success: function (data) {
                if (data.result == false) {
                    errorLabel.hide();
                    result = true;
                }
                else {
                    errorLabel.html('电子邮箱 ' + cellPhone + ' 已经被占用').show();
                }
            }
        });
    }
    return result;
}
function countEmailDown() {
    delayEmailTime--;
    $("#sendEmailCode").attr("disabled", "disabled");
    $("#dyEmailButton").html(delayEmailTime + '秒后重新获取');
    if (delayEmailTime == 1) {
        delayEmailTime = 120;
        $("#emailCodeSucMessage").removeClass().empty();
        $("#dyEmailButton").html("获取邮箱验证码");
        $("#callEmail_error").addClass("hide");
        $("#sendEmailCode").removeClass().addClass("btn").removeAttr("disabled");
        delayEmailFlag = true;
    } else {
        delayEmailFlag = false;
        setTimeout(countEmailDown, 1000);
    }
}

function sendEmailCode() {
    $('#callEmail_error').hide();
    if ($("#sendEmailCode").attr("disabled")) {
        return;
    }
    var errorLabel = $('#callEmail_error');
    var email = $("#callEmail").val();
    var reg = /^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$/;
    if (!email) {
        $("#callEmail_error").removeClass().addClass("error").html("请输入电子邮箱");
        $("#callEmail_error").show();
        return;
    }
    if (!reg.test(email)) {
        $("#callEmail_error").removeClass().addClass("error").html("电子邮箱格式有误，请输入正确的电子邮箱");
        $("#callEmail_error").show();
        return;
    }
    if (!checkCheckCodeIsValid()) {
        return;
    }
    //$('#checkCode').removeClass("highlight2");
    // 检测邮箱是否存在
    $.post('/Register/CheckEmail', { email: email }, function (data) {
        if (data.result == false) {
            errorLabel.hide();
            sendemCode();
        }
        else {
            errorLabel.html('电子邮箱 ' + email + ' 已经被占用').show();
        }
    });

}

// 手机注册发送验证码target
function sendemCode() {
    if ($("#sendEmailCode").attr("disabled") || delayEmailFlag == false) {
        return;
    }

    $("#sendEmailCode").attr("disabled", "disabled");
    jQuery.ajax({
        type: "post",
        url: "/Register/SendCode?pluginId=Himall.Plugin.Message.Email&destination=" + $("#callEmail").val(),
        success: function (result) {
            if (result.success == true) {
                $("#callEmail_error").hide();
                $("#dyEmailButton").html("120秒后重新获取");
                //if (obj.remain) {
                //    $("#mobileCodeSucMessage").empty().html(obj.remain);
                //} else {
                //    $("#cellPhone_error").removeClass().empty().html("验证码已发送，请查收短信。");
                //    $("#cellPhone_error").show();
                //}
                setTimeout(countEmailDown, 1000);
                $("#sendEmailCode").removeClass().addClass("btn").attr("disabled", "disabled");
                $("#syscheckCode").removeAttr("disabled");
            }
        }
    });
}