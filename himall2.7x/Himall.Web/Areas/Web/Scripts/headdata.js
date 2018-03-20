function loadUserCoupons() {
    if (!curshopid) return;
    $.ajax({
        type: 'post',
        url: '/product/UserCoupons',
        dataType: 'text',
        cache: true,// 开启ajax缓存
        success: function (data) {
            $("#userCouponList").html(data);
            var couponsUrl = "/Shop/PostShopCoupons/" + curshopid;
            var canCouponsUrl = "/Shop/PostCanUseShopCoupons/" + curshopid;

            $.ajax(canCouponsUrl, {
                type: 'post',
                dataType: 'json',
                cache: false,
                async: false,
                success: function (data) {
                    if (data.length > 0) {
                        var str = '',
                            cls = '';
                        str += '<div class="side-asset-list" id="curShopUse"><p>当前店铺可用优惠券</p><ul>';
                        for (var i = 0; i < data.length; i++) {
                            cls = '';
                            if (data[i].Price >= 100) {
                                cls = "coupon-lg";
                            } else if (data[i].Price <= 50) {
                                cls = "coupon-sm";
                            }
                            str += '<li class="' + cls + '">' +
                                '<h5><a>' + data[i].ShopName + '</a></h5>' +
                                '<h3><span>￥' + data[i].Price + '</span>满' + data[i].OrderAmount + '可用</h3>' +
                                '<p>截止' + time_string(data[i].EndTime, "yyyy.MM.dd") + '</p>' +
                            '</li>';
                        }
                        str += '</ul></div>';

                        $('.side-asset-total').after(str);

                    }
                }
            });
            $.ajax(couponsUrl, {
                type: 'post',
                dataType: 'json',
                cache: false,
                async: false,
                success: function (data) {
                    if (data.length > 0) {
                        $('#right_asset .side-point').show();
                        var pointTime = setTimeout(function () { $('.side-point').hide(); }, 15000);
                        $('#right_asset').click(function () {
                            $('.side-point').hide();
                            clearTimeout(pointTime);
                        });
                        var str = '',
                            cls = '';
                        str += '<div class="side-asset-list"><p>当前店铺有可领优惠券哦</p><ul>';
                        for (var i = 0; i < data.length; i++) {
                            cls = '';
                            if (data[i].Price >= 100) {
                                cls = "coupon-lg";
                            } else if (data[i].Price <= 50) {
                                cls = "coupon-sm";
                            }
                            str += '<li class="getCurCoupon ' + cls + '" data-id="' + data[i].Id + '">' +
                                '<h5><a>' + data[i].ShopName + '</a></h5>' +
                                '<h3><span>￥' + data[i].Price + '</span>满' + data[i].OrderAmount + '可用</h3>' +
                                '<p>截止' + time_string(data[i].EndTime, "yyyy.MM.dd") + '</p>' +
                            '</li>';
                        }
                        str += '</ul></div>';

                        $('#myCouponList').before(str);

                        $('.side-bd').on('click', '.getCurCoupon', function () {
                            var _this = $(this);
                            $.post('/shop/ReceiveCoupons?couponId=' + _this.data('id') + '&shopId=' + curshopid, function (data) {
                                if (data.success) {
                                    $.dialog.succeedTips('领取成功!', '', 3);
                                    if ($('#curShopUse').length == 0) {
                                        $('.side-asset-total').after('<div class="side-asset-list" id="curShopUse"><p>当前店铺可用优惠券</p><ul>');
                                    }
                                    $('#curShopUse').append('<li class="' + _this[0].className.replace("getCurCoupon ", "") + '">' + _this.html() + '</li>');
                                    var mycouponcount = $("#mycouponcount");
                                    var mycounum = mycouponcount.html();
                                    try{
                                        mycounum = parseInt(mycounum);
                                        if (isNaN(mycounum)) {
                                            mycounum = 0;
                                        }
                                    }catch(ex){
                                        mycounum=0;
                                    }
                                    mycounum++;
                                    mycouponcount.html(mycounum);
                                    _this.animate({ height: 0 }, 300, function () {
                                        if (_this.siblings().length == 0) {
                                            _this.parent().parent().remove();
                                        } else {
                                            _this.remove();
                                        }
                                    });
                                } else {
                                    $.dialog.errorTips(data.msg, 3);
                                }
                            });
                        })

                    }
                }
            });
        },
        error: function (e) {
            //alert(e);
        }
    });
}

function loadLoginUserInfo() {
    $.ajax({
        type: 'post',
        url: '/product/GetLoginUserInfo',
        dataType: 'json',
        success: function (data) {
            $("#isLogin").val(data.isLogin);
            var userHtml = "";
            if (data.isLogin) {
                userHtml = "<li> <a href=\"/userCenter/home\">" + data.currentUserName + "</a> &nbsp; <a href=\"javascript:logout()\">[退出]</a></li>";
            } else {
                userHtml = "<li class=\"L-line\"> <a href=\"/Login\">请登录</a></li>" +
               "<li> <a href=\"/Register\">免费注册</a></li>";
            }
            $("#loginUser").html(userHtml);
            loadUserConcern(data.concern);
            loadUserBrowsingProducts(data.browsingProducts);
        },
        error: function () { }
    })
}

function loadUserConcern(data) {
    var concernHtml = "";
    $(data).each(function () {
        concernHtml += "<li>" +
                    "<a href=\"/Product/Detail/" + this.Id + "\" target=\"_blank\"><img src=\"" + this.ImagePath + "\" /></a>" +
                    "<p><a href=\"/Product/Detail/" + this.Id + "\" target=\"_blank\">" + this.ProductName + "</a></p>" +
                "</li>";
    });
    $("#memberConcern").html(concernHtml);
}

function loadUserBrowsingProducts(data) {
    var browsingProductHtml = "";
    $(data).each(function () {
        browsingProductHtml += "<li>" +
                                "<a href=\"/Product/Detail/" + this.Id + "\" target=\"_blank\"><img src=\"" + this.ImagePath + "\" /></a>" +
                                "<p><a href=\"/Product/Detail/" + this.Id + "\" target=\"_blank\">" + this.ProductName + "</a></p>" +
                            "</li>";
    });
    $("#userBrowsingProducts").html(browsingProductHtml);
}