//判断是否登陆来进行跳转
$("#lqgift").on("click", function () {
    $.ajax({
        type: 'get',
        url: '/' + areaName + '/login/CheckLogin',
        data: {},
        dataType: 'json',
        success: function (result) {
            if (result.success) {
                location.href = "/" + areaName + "/RegisterActivity/Share"
            }
            else {
                location.href = "/" + areaName + "/Register?type=gift"
            }
        }
    });
})