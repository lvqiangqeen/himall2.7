// JavaScript source code
$('#Save').click(function () {
    var loading = showLoading();
    $.post('./SaveExpressSetting', { Kuaidi100Key: $("#Kuaidi100Key").val(), KuaidiApp_key: $("#KuaidiApp_key").val(), KuaidiAppSecret: $("#KuaidiAppSecret").val(), KuaidiType: $("input[name='KuaidiType']:checked").val(), }, function (result) {
        loading.close();
        if (result.success) {
            $.dialog.tips('保存成功');
        }
        else
            $.dialog.errorTips('保存失败！' + result.msg);
    });
});

$("#KuaidiType1").on("click", function () {
    $(".kd1").show();
    $(".kd2").hide();
})
$("#KuaidiType2").on("click", function () {
    $(".kd2").show();
    $(".kd1").hide();
})