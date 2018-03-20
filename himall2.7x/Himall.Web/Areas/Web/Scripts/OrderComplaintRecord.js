function HtmlDecode(str) {
    var t = document.createElement("div");
    t.innerHTML = str;
    return t.innerText || t.textContent
}
var currentId = 0;
$(function () {
    $('.complain-btn').click(function () {
        currentId = $(this).attr("cid");
        var type = $(this).attr("deal");
        var replyContent = HtmlDecode($(this).parent().find("span").html());
        switch (type) {
            case "cancel":
                $.dialog.confirm("取消此次投诉！", function () { DealComplaint() });
                break;
            case "ok":
                $.dialog({
                    title: '对投诉结果满意',
                    lock: true,
                    width: 400,
                    id: 'Agree',
                    content: '<p class="ftx03">商家回复：' + replyContent + '</p><br>是否满意商家的回复？',
                    padding: '20px',
                    cancelVal: '取消',
                    ok: function () {
                        DealComplaint();
                    },
                    cancel: true
                });
                break;
            case "bad":
                $.dialog({
                    title: '申请仲裁',
                    lock: true,
                    width: 400,
                    id: 'goodCheck',
                    content: '<p class="ftx03">商家回复：' + replyContent + '</p><br>是否不满意商家的回复并且进行投诉？',
                    padding: '20px',
                    cancelVal: '取消',
                    ok: function () {
                        ApplyArbitration();
                    },
                    cancel: true
                });
                break;
        }
    });
});
function DealComplaint() {
    var loading = showLoading();
    $.ajax({
        type: 'post',
        url: '/OrderComplaint/DealComplaint',
        cache: false,
        data: { id: currentId },
        dataType: 'json',
        success: function (data) {
            loading.close();
            if (data.success) {
                $.dialog.succeedTips("处理成功！", function () {
                    window.location.href = window.location.href;
                }, 1);
            }
        },
        error: function () { }
    });
}
function ApplyArbitration() {
    var loading = showLoading();
    $.ajax({
        type: 'post',
        url: '/OrderComplaint/ApplyArbitration',
        cache: false,
        data: { id: currentId },
        dataType: 'json',
        success: function (data) {
            loading.close();
            if (data.success) {
                $.dialog.succeedTips("申请成功！", function () {
                    window.location.href = window.location.href;
                }, 1);
            }
        },
        error: function () { }
    });
}