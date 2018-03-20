// JavaScript source code
$(function () {

    $('#btnSave').click(function () {
        var month = $("#range").val();
        var price = $("#range").data("price");
        if (month <= 12) {
            $.dialog.confirm('您确定花费' + month * price + '元购买' + month + '个月限时购服务吗？', function () {
                $('#submit').click();
            });
        }
    })



    $("#range").focus();
    var a = v({
        form: 'form1',
        ajaxSubmit: true,
        beforeSubmit: function () {
            loadingobj = showLoading();
        },
        afterSubmit: function (data) {// 表单提交成功回调
            loadingobj.close();
            if (data.success) {
                $.dialog.succeedTips("提交成功！", function () {
                    window.location.reload();
                }, 0.3);
            } else {
                $.dialog.alert(data.msg, '', 3);
            }
        }
    });
    a.add(
        {
            target: 'range',
            empty: true,
            ruleType: 'uint&&(value>0)&&(value<=12)',// v.js规则验证
            error: '只能为数字，且只能是1到12之间的整数!'
        });
});
