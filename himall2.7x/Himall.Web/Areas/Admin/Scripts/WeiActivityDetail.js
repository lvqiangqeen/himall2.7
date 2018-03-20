$(function () {
    $("#copylink").zclip({
        path: '/Scripts/ZeroClipboard.swf', //记得把ZeroClipboard.swf引入到项目中
        copy: function () {
            return $('#signlink').html();
        },
        afterCopy: function () {
            $.dialog.tips("复制成功");
        }
    })
})