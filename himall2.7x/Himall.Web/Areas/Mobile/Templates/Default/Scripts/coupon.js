// 个人优惠卷管理
$(function () {
    $('#spanEnable').click(function () {
        displayCoupon('enabled');
    });
    $('#spanDisable').click(function () {
        displayCoupon('disabled');
    });
});

function displayCoupon(type) {
    if (type=='disabled') {
        $('#spanEnable').removeClass('active');
        $('#spanDisable').addClass('active');
        $('.coupon-bd ul[name="disabled"]').show();
        $('.coupon-bd ul[name="enabled"]').hide();
    }
    else {
        $('#spanEnable').addClass('active');
        $('#spanDisable').removeClass('active');
        $('.coupon-bd ul[name="disabled"]').hide();
        $('.coupon-bd ul[name="enabled"]').show();
    }
};

// 优惠券领取
$(function() {
    $('a[name="acceptCoupon"]').click(function() {
        var $thisCoupon = $(this);
        var cpid = $thisCoupon.attr('cpid') || 0;
        var vshopid = $thisCoupon.attr('vshopid') || 0;
        var couponCon = $("#couponCon").val();
        if (parseInt(cpid) > 0) {
            $.post(couponCon, { vshopid: vshopid, couponid: parseInt(cpid) }, function(result) {
                if (result.status == 0) {
                    $.dialog.succeedTips("领取成功！")
                    // window.location.href = '/' + areaName + '/VShop/GetCouponSuccess/' + result.crid;
                }
                else {
                    $.dialog.tips(result.msg, function() { });
                }
                return;
            });
        }
    });
});
