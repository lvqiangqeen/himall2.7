// JavaScript source code
$(function () {
    $('.refuse-shop').click(function () {
        $.dialog({
            title: '拒绝理由',
            lock: true,
            id: 'refuseShop',
            content: ['<div class="dialog-form">',
                '<div class="form-group">',
                    '<textarea id="refuseComment" class="form-control" cols="40" rows="3"></textarea>',
                '</div>',
            '</div>'].join(''),
            padding: '0 40px',
            okVal: '确认',
            ok: function () {
                var comment = $("#refuseComment").val();
                if (comment.length < 5) {
                    $.dialog.errorTips('必须填写拒绝理由，且拒绝理由不能小于五个字符！');
                    return false;
                }
                var loading = showLoading();
                ajaxRequest({
                    type: 'POST',
                    url: "./Auditing",
                    param: { shopId: $("#shopId").val(), status: 4, comment: comment },
                    dataType: "json",
                    success: function (data) {
                        if (data.Successful == true) {
                            location.href = "./Management";
                        }
                        loading.close();
                    }, error: function () { loading.close(); }
                });
            }
        });
    });

    $(".statusBtn").click(function () {
        var status = 1;
        if ($(this).attr('status') == 'passShop')
            status = 7;
        else if ($(this).attr('status') == 'confrimCollect')
            status = 7;
        var loading = showLoading();
        ajaxRequest({
            type: 'POST',
            url: "./Auditing",
            param: { shopId: $("#shopId").val(), status: status },
            dataType: "json",
            success: function (data) {
                if (data.Successful == true) {
                    location.href = "./Management";
                }
                loading.close();
            }, error: function () { loading.close(); }
        });
    });
});