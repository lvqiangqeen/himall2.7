var count = 120;
$('#btnAuthCode').click(function () {
    var destination = $("#destination").text();
    var id = $('#pluginId').val();
    $('#btnAuthCode').attr("disabled", true);
    $.post('/UserInfo/SendCode', { pluginId: id, destination: destination }, function (result) {
        if (result.success) {
            setTimeout(countDown1('timeDiv1', ''), 1000);
        }
        else {
            $.dialog.errorTips('发送验证码失败：' + result.msg);
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
$('#id_btn').click(function () {
    var destination = $("#destination").text();
    var id = $('#pluginId').val();
    var code = $('#code').val();
    $.post('/UserInfo/CheckCode', { pluginId: id, code: code, destination: destination }, function (result) {
        if (result.success) {
            $.dialog.succeedTips('验证成功！', function () { location.href = "/UserInfo/ReBindStep2?pluginId=" + id + "&key=" + result.key; });

        }
        else {
            $.dialog.errorTips(result.msg);
        }
    });
});