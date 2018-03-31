
//隐藏右上角菜单
document.addEventListener('WeixinJSBridgeReady', function onBridgeReady() {
    WeixinJSBridge.call('showOptionMenu');
});

$(document).ready(function () {
    $("footer .glyphicon-refresh").click(function () {
        location.reload();
    })

    $("footer .glyphicon-arrow-left").click(function () {
        history.go(-1);
    })

});

function goUrl(url) {
    window.location.href = url;
}

function myConfirm(title, content, ensureText, ensuredCallback) {
    var myConfirmCode = '<div class="modal fade" id="myConfirm" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">\
                  <div class="modal-dialog">\
                    <div class="modal-content">\
                      <div class="modal-header">\
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>\
                        <h4 class="modal-title" id="myModalLabel">' + title + '</h4>\
                      </div>\
                      <div class="modal-body">\
                        ' + content + '\
                      </div>\
                      <div class="modal-footer">\
                        <button type="button" class="btn btn-default" data-dismiss="modal">取消</button>\
                        <button type="button" class="btn btn-danger">' + ensureText + '</button>\
                      </div>\
                    </div>\
                  </div>\
                </div>';
    if ($("#myConfirm").length == 0) {
        $("body").append(myConfirmCode);
    }
    $('#myConfirm').modal();
    $('#myConfirm button.btn-danger').unbind("click", "");
    $('#myConfirm button.btn-danger').click(function (event) {
        if (ensuredCallback)
            ensuredCallback();
        $('#myConfirm').modal('hide')
    });
}



function alert_h(content, ensuredCallback) {
    var myConfirmCode = '<div class="modal fade" id="alert_h" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">\
                  <div class="modal-dialog">\
                    <div class="modal-content">\
                      <div class="modal-header">\
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>\
                        <h4 class="modal-title" id="myModalLabel">操作提示</h4>\
                      </div>\
                      <div class="modal-body">\
                        ' + content + '\
                      </div>\
                      <div class="modal-footer">\
                        <button type="button" class="btn btn-primary" data-dismiss="modal">确认</button>\
                      </div>\
                    </div>\
                  </div>\
                </div>';

    if ($("#alert_h").length != 0) {
        $('#alert_h').remove();
    }
    $("body").append(myConfirmCode);
    $('#alert_h').modal();

    $('#alert_h').off('hide.bs.modal').on('hide.bs.modal', function (e) {
        if (ensuredCallback)
            ensuredCallback();
    });
}














