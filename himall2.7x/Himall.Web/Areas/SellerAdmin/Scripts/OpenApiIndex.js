
$(function () {
    $("#btnReg").click(function () {
        SetRegisterState();        
    });
    $("#btnClose").click(function () {
        SetEnableState(false);
    });
    $("#btnOpen").click(function () {
        SetEnableState(true);
    });
});

function SetEnableState(state) {
    var loading = showLoading();
    $.post('/SellerAdmin/OpenApi/SetEnableState', { state: state }, function (result) {
        loading.close();
        if (result.success) {
            $.dialog.succeedTips("操作成功！", function () {
                window.location.reload();
            });
        }
        else
            $.dialog.errorTips("操作失败：" + result.msg);
    });
}
function SetRegisterState() {
    var loading = showLoading();
    $.post('/SellerAdmin/OpenApi/SetRegisterState',null, function (result) {
        loading.close();
        if (result.success) {
            $.dialog.succeedTips("操作成功！", function () {
                window.location.reload();
            });
        }
        else
            $.dialog.errorTips("操作失败：" + result.msg);
    }).error(function (result) {
        loading.close();
        $.dialog.errorTips("操作失败：" + result.msg);
    });
}