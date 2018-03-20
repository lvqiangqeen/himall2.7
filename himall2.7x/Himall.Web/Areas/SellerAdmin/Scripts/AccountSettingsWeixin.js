var isSend = 1;
var opid = '';
var sceneid = '';


//发送验证码
$("#wsendCode").on("click", function () {
    if ($("#wsendCode").attr("diz") != null) {
        return false;
    }
    $.post('/SellerAdmin/AccountSettings/SendCode', { pluginId: $("#wpluginId").val(), destination: $("#wdestination").val() }, function (result) {
        if (result.success) {
            var count = 120;
            siAwei = setInterval(function () { count--; countDown1Swei(count, "wsendCode"); }, 1000);
        }
        else {
            $.dialog.errorTips('发送验证码失败：' + result.msg);
        }
    });
})


function countDown1Swei(ss, dv) {
    if (ss > 0) {
        $("#" + dv).html("重新获取（" + ss + "s）");
        $("#" + dv).attr("diz", "1");
    } else {
        $("#" + dv).html("获取验证码");
        $("#" + dv).removeAttr("diz");
        clearInterval(siAwei);
    }
}

//下一步，验证验证码
$('#wnext').click(function () {
    $.post('/SellerAdmin/AccountSettings/verificationCode', { pluginId: $("#wpluginId").val(), code: $("#wcode").val() }, function (result) {
        if (result.success) {
            $.post('/SellerAdmin/AccountSettings/getWinxin', { }, function (result) {
                if (result.success) {
                    sceneid = result.Sceneid;
                    var url = "https://mp.weixin.qq.com/cgi-bin/showqrcode?ticket=";
                    url = url + result.ticket;
                    $(".wximg").attr("src", url);
                    checkScanState();
                    JumpStep(1);
                }
            });
        }
        else {
            $.dialog.errorTips('验证码错误！');
        }
    });
});

$(function () {

    //绑定微信设置
    $('.weixinBindBtn').click(function () {
        $('#weixinBind').show();
        if ($('.cover').length == 0) {
            $('#weixinBind').after('<div class="cover"></div>');
        }
        $('.cover').fadeIn();
    });

    $(document).on('click', '.cover,.close', function () {
        $('.cover').fadeOut();
        $('#weixinBind').hide();
    });

})
function checkScanState() {
    console.log(sceneid);
    $.getJSON('/SellerAdmin/ScanShopState/GetState', { sceneid: sceneid }, function (result) {
        if (result.success) {
            opid = result.data.OpenId;
            $("#opid").val(opid);
            $('#nikename').html(result.data.NickName); 
            $('#sex').html(result.data.sex);
            $('#address').html(result.data.province + result.data.city);
            $.dialog.succeedTips('扫码成功!', function () {
                JumpStep(2);
            });
        }
        else {
            setTimeout(checkScanState, 0);
        }
    });
}

//微信绑定提交
$("#btn_weixstep3").on("click", function () {
    var nikename = $("#nikename").text();
    var trueName = $("#trueName").val();
    var sex = $("#sex").text();
    var address = $("#address").text();
    if (trueName == "") {
        $.dialog.errorTips("真实姓名不能为空！");
        return false;
    }
    else if (trueName.length < 2) {
        $.dialog.errorTips("真实姓名长度不得小于两位！");
        return false;
    }
    else if (trueName.length > 10) {
        $.dialog.errorTips("真实姓名长度不得大于10位！");
        return false;
    }
    if (opid == "") {
        $.dialog.errorTips("微信绑定失败，请重新操作！");
        return false;
    }
    $.post("/SellerAdmin/AccountSettings/setShopWeiXin", { sceneid: sceneid, trueName: trueName }, function (data) {
        if (data.success) {
            $("#dWeiXinNickName").html(nikename);
            $("#WeiXinNickName").val(nikename);
            $("#WeiXinSex").val(sex);
            $("#dWeiXinSex").html(sex);
            $("#WeiXinAddress").val(address);
            $("#dWeiXinAddress").html(address);
            $("#WeiXinTrueName").val(trueName);
            $("#dWeiXinTrueName").html(trueName);
            JumpStep(3);
        } else {
            $.dialog.errorTips("绑定失败!");
        }
    }, "json");
})

//绑定完成
$("#btn_winxinfi").on("click", function () {
    $('.cover').fadeOut();
    $('#weixinBind').hide();
    JumpStep(0);
})

//微信绑定跳转
function JumpStep(step) {
    //$('div[name="stepname"]').hide();
    switch (step) {
        case 0:
            $('.weixn-step-1').show().siblings().hide();
            $('#weixinBind ul.choose-step li').eq(0).addClass('active').siblings().removeClass();
            break;
        case 1:
            $('.weixn-step-2').show().siblings().hide();
            $('#weixinBind ul.choose-step li').eq(1).addClass('active').siblings().removeClass();
            break;
        case 2:
            $('.weixn-step-3').show().siblings().hide();
            $('#weixinBind ul.choose-step li').eq(2).addClass('active').siblings().removeClass();
            break;
        case 3:
            $('.weixn-step-4').show().siblings().hide();
            $('#weixinBind ul.choose-step li').eq(3).addClass('active').siblings().removeClass();
            break;
    }
}
