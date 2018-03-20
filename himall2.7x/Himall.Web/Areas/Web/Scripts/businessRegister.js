
$(function () {
    bindCheckCode();

    checkUserName();

    checkPassword();

    checkCheckCode();

    bindSubmit();

    $('#regName').focus();
});


function bindSubmit() {
    $('#registsubmit').click(function () {
        var result = checkValid();
        if (result) {
            var username = $('#regName').val(), password = $('#pwd').val();
            var loading = showLoading();
            $.post('/Register/RegBusiness', { username: username, password: password }, function (data) {
                loading.close();
                if (data.success) {
                    $.dialog.succeedTips("注册成功！", function () {
                        location.href = '/selleradmin/login';
                    }, 2);
                }
                else {
                    $.dialog.errorTips("注册失败！" + data.msg, '', 1);
                }
            });
        }
    });
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
    return checkUsernameIsValid() & checkPasswordIsValid() & checkPasswordIsValid() & checkRepeatPasswordIsValid() & checkCheckCodeIsValid();
}


function checkCheckCodeIsValid() {
    var checkCode = $('#checkCode').val();
    var errorLabel = $('#checkCode_error');
    checkCode = $.trim(checkCode);

    var result = false;
    if (checkCode && checkCode.length == 4) {
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
    }
    else {
        errorLabel.html('请输入验证码').show();
    }
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

function checkUsernameIsValid() {
    var result = false;
    var username = $('#regName').val();
    var errorLabel=$('#regName_error');
    var reg = /^[\u4E00-\u9FA5\@A-Za-z0-9\_\-]{4,20}$/;

    if (!username || username == '用户名') {
        errorLabel.html('请输入用户名').show();
    }
    else if (!reg.test(username)) {
        errorLabel.html('4-20位字符，支持中英文、数字及"-"、"_"的组合').show();
    }
    else{
        $.ajax({
            type: "post",
            url: "/register/CheckManagerUser",
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
                    $.dialog.errorTips("验证码校验出错", '', 1);
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
    //var result = reg.test(password);

    var result = password.length >= 6 && password.length <= 20;

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

function reloadImg() {
    $("#checkCodeImg").attr("src", "/Register/GetCheckCode?_t=" + Math.round(Math.random() * 10000));
}