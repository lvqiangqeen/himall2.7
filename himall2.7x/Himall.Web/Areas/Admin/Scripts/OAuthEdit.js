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
            loading.close();
            if (result.success)
                $.dialog.tips('保存成功！', function () { location.href = "management"; });
            else
                $.dialog.errorTips('保存失败！' + result.msg);
        });
    });

});