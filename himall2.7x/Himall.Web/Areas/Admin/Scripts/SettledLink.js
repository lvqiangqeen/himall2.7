$(function () {
    $("#copylink").zclip({
        path: '/Scripts/ZeroClipboard.swf', //记得把ZeroClipboard.swf引入到项目中 
        copy: function () {
            var _curdom = $(this);
            return _curdom.data("url");
        },
        afterCopy: function () {
            $.dialog.succeedTips('复制链接成功！');
        }
    });
});