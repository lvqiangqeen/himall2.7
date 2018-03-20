// JavaScript source code
$(function () {
    $("#AddSlide").click(function () {
        ShowReasonDialog(0);
    });
});

function DeleteReason(n) {
    var id = n || 0;
    var Delet_id = $("#Delet_id").val();
    curval = $("#rea_" + id).html();
    $.dialog.confirm('确认删除“' + curval + '”这条售后原因吗？',
        function () {
            var loading = showLoading();
            ajaxRequest({
                type: 'POST',
                url: $("#Delet_id").val(),
                cache: false,
                param: { id: id },
                dataType: 'json',
                success: function (data) {
                    loading.close();
                    if (data.success == true) {
                        $.dialog.succeedTips('删除成功.', function () {
                            window.location.reload();
                        });

                    }
                },
                error: function (data) {
                    loading.close();
                    $.dialog.errorTips('操作失败,请稍候尝试.');
                }
            });
        }
        );
}

function ShowReasonDialog(n) {
    var id = n || 0;
    var title = (id > 0 ? "修改" : "添加") + "售后原因";
    var curval = "";
    if (id > 0) {
        curval = $("#rea_" + id).html();
    }
    $.dialog({
        title: title,
        lock: true,
        id: 'dlg_oprefreason',
        padding: '10px',
        content: ['<div class="dialog-form clearfix">',
            '<div class="form-group">',
            '<label class="label-inline fl for="">售后原因</label>',
            '<p class="only-text"><input class="form-control input-sm text-box single-line" id="reason" name="reason" type="text" value="' + curval + '"></p>',
            '<p class="only-text">限20字符</p>',
            '</div>',
        '</div>'].join(''),
        okVal: '保存',
        ok: function () {
            var reason = $("#reason").val();
            if (reason.length < 1) {
                $.dialog.errorTips('请填写售后原因内容');
                return false;
            }
            if (reason.length > 20) {
                $.dialog.errorTips('售后原因限20字符');
                return false;
            }
            var loading = showLoading();
            ajaxRequest({
                type: 'POST',
                url: $("#Save_reason").val(),
                cache: false,
                param: { id: id, reason: reason },
                dataType: 'json',
                success: function (data) {
                    loading.close();
                    if (data.success == true) {
                        $.dialog.succeedTips('操作成功.', function () {
                            window.location.reload();
                        });

                    }
                },
                error: function (data) {
                    loading.close();
                    $.dialog.errorTips('操作失败,请稍候尝试.');
                }
            });
        }
    });
}