// JavaScript source code
function del(id, name) {
    $.dialog.confirm('确定要删除客服 ' + name + ' 吗？', function () {
        var loading = showLoading();
        $.post('delete', { id: id }, function (result) {
            loading.close();
            if (result.success)
                $.dialog.succeedTips('删除成功!', function () { location.href = location.href });
            else
                $.dialog.errorTips('删除失败！' + result.msg);
        });
    });
}