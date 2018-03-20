// JavaScript source code
function Post() {
    var loading = showLoading();
    $.ajax({
        type: 'post',
        url: 'SaveSetting',
        data: $("form").serialize(),
        success: function (data) {
            loading.close();
            if (data.success)
                $.dialog.tips('保存成功！');
            else
                $.dialog.tips('保存失败！' + data.msg);
        }
    });
}
