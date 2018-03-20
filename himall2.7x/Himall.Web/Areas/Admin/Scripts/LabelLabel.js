// JavaScript source code
var isposting = false;
function successpost(data) {
    isposting = false;
    if (data.Success == true) {
        $.dialog.tips("保存成功", function () {
            window.location.href = "Management";//数据提交成功页面跳转
        });
    } else {
        $.dialog.errorTips(data.msg);
    }
}
