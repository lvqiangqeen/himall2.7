// JavaScript source code
$(function () {
    $("#freightInput").focus();
    $('button').click(function () {
        var freight = $('input[name="freight"]').autoNumeric('get');
        var freeFreight = $('input[name="freeFreight"]').autoNumeric('get');
        if (freeFreight.length < 1) {
            alert("请输入数字");
            return;
        }
        if (freeFreight < 0) {
            alert("请不要输入小于0的数字");
            return;
        }
        var loading = showLoading();

        $.post('SaveFreightSetting', { freight: freight, freeFreight: freeFreight }, function (result) {
            loading.close();
            if (result.success)
                $.dialog.succeedTips('保存成功!');
            else
                $.dialog.errorTips('保存失败!' + result.msg);

        });
    });
    $('.auto').autoNumeric('init');
})