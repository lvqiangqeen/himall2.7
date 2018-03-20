// JavaScript source code
$(function () {
    $('#btn').click(function () {
        var items = $('input[formItem]');
        var data = [];
        $.each(items, function (i, item) {
            data.push({ key: $(item).attr('name'), value: $(item).val() });
        });
        var dataString = JSON.stringify(data);
        var id = $('#pluginId').val();
        var loading = showLoading();
        $.post('save', { pluginId: id, values: dataString }, function (result) {
            if (result.success)
                $.dialog.succeedTips('保存成功！', function () { });
            else
                $.dialog.errorTips('保存失败！' + result.msg);
            loading.close();
        });
    });
    $('#btnsend').click(function () {
        var destination = $("#destination").val();
        var id = $('#pluginId').val();
        var loading = showLoading();
        $.post('send', { pluginId: id, destination: destination }, function (result) {
            if (result.success) {
                $.dialog.succeedTips('测试成功！');
                loading.close();
            }
            else {
                $.dialog.errorTips('测试失败！' + result.msg);
                loading.close();
            }
        });
    });
});

