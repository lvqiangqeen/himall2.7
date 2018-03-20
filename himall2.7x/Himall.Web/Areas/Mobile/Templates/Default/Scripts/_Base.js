$(function() {
    if ($(document).width()<=640) {
        $("html").niceScroll({cursorwidth: 0, cursorborder:0});
    }
});

function checkLogin(returnHref, callBack, loginshopid) {
   
    var memberId = $.cookie('Himall-User');
    if (memberId) {
        callBack();
    }
    else {
        $.ajax({
            type: 'get',
            url: '/' + areaName + '/login/CheckLogin',
            data: {},
            dataType:'json',
            success: function(result) {
                if (result.success) {
                    callBack();
                }
                else {
                    $.dialog.tips("您尚未登录，请先登录", function () {
                        if (loginshopid && MAppType != '') {
                            location.href = "/" + areaName + "/Redirect?redirectUrl=" + returnHref + '&shop=' + MAppType;
                        }
                        else {
                            location.href = "/" + areaName + "/Redirect?redirectUrl=" + returnHref;
                        }
                    });
                }
            }
        });
        
    }
}
