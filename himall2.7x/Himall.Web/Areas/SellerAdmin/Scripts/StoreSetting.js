$(function () {
    //开启订单自动分配到门店复选框更改时
    $('#ckbSpecifications').change(function () {
        var autoAllotOrder = this.checked == true;
        $.post('Setting', { autoAllotOrder: autoAllotOrder }, function (result) {
            if (result.success) {
                $.dialog.tips('操作成功!', function () {
                    location.reload();
                });
            }
            else
                $.dialog.alert('操作失败!' + result.msg);
        });
    });
});