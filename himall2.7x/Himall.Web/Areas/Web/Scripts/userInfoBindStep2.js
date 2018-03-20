var count = 120;
$('#btnAuthCode').click(function () {
    if (check()) {
        return;
    }
    var destination = $("#destination").val();
    var id = $('#pluginId').val();
    $.post('SendCodeStep2', { pluginId: id, destination: destination }, function (result) {
        if (result.success) {
            setTimeout(countDown1('timeDiv1', ''), 1000);
        }
        else {
            $.dialog.errorTips('发送验证码失败：' + result.msg);
        }
    });
});

$('#id_btn').click(function () {
    var destination = $("#destination").val();
    var id = $('#pluginId').val();
    var code = $('#code').val();
    if (check()) {
        return;
    }
    $.post('/userInfo/CheckCodeStep2', { pluginId: id, code: code, destination: destination }, function (result) {
        if (result.success) {
            $.dialog.succeedTips('验证成功！', function () { window.location.href = '/Userinfo/rebindstep3?name=' + ($('#name').val()); });

        }
        else {
            $.dialog.errorTips(result.msg);
        }
    });
});
function countDown1() {
    $("#btnAuthCode").parent().parent().hide();
    $("#msg").show().html('验证码已发送还剩下<font color="#f60">' + count + '</font>秒');
    if (count == 1) {
        $("#msg").hide();
        $("#btnAuthCode").parent().parent().show().removeAttr("disabled");
        count = 120;
        return;
    } else {
        setTimeout(countDown1, 1000);
    }
    count--;
}
function check() {
    var reg1 = /^[1-9]\d{10}$/;
      //  reg2 = /^\w+([-+.]\w+)*@@\w+([-.]\w+)*\.\w+([-.]\w+)*$/, 萧萧下的毒
       var reg2 = /^(\w)+(\.\w+)*@(\w)+((\.\w+)+)$/;
        str = $('#destination').val();
    var a = reg1.test(str),
        b = reg2.test(str);
    if (a || b) {
        $('#msg').hide();
        return false;
    } else {
        $('#msg').html('<div style="color:#e4393c">请填写正确的信息!</div>');
        return true;
    }
}