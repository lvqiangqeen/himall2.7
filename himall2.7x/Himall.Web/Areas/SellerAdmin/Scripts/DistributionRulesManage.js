// JavaScript source code
$(function () {
    $("#QCodeBtn").click(function () {
        var codedom = $(".qcover");
        codedom.show();

        if ($("#bt_copyurl").data("isinit") != "1") {
            $("#bt_copyurl").zclip({
                path: '/Scripts/ZeroClipboard.swf', //记得把ZeroClipboard.swf引入到项目中
                copy: function () {
                    return $('#qlink').html();
                },
                afterCopy: function () {
                    $.dialog.tips("复制成功");
                }
            });
        }
        $("#bt_copyurl").data("isinit", 1);
    });

    $(".qcover").click(function () {
        var codedom = $(".qcover");
        codedom.hide();
    });
    $(".qcover .zclip").click(function (event) {
        event.stopPropagation();
    });
});