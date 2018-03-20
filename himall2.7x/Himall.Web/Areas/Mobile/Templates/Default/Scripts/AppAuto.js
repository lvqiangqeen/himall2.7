document.addEventListener('plusready', function () {
	$(".members_con .add-buy-btn").hide();
	$.cookie('Himall-User',plus.storage.getItem('appuserkey'));
    $("footer").hide();
    $(".container").css("padding-bottom", "0");
    var ms = plus.webview.currentWebview();
    var login = plus.storage.getItem('appuserkey');
    var url = ms.url,weburl=plus.storage.getItem('weburl');
    $(".members_con").on("click", "a", function (e) {
        login = plus.storage.getItem('appuserkey');
        e.preventDefault();
        var href = $(this).attr("href");
        if (href.indexOf('m-wap/Product/Detail') != -1) {  //商品详情
            var id = href.split("/");
            id = id[id.length - 1];
            showProduct(id);
        }else if(href.indexOf('m-Wap/topic/detail') != -1){//专题页面
        	var id = href.split("/");
            id = id[id.length - 1];
			mui.fire(plus.webview.getWebviewById('special.html'),'updateData',{topicId:id});
        }else
            if (href.indexOf('/m-Wap/FightGroup') != -1) {  //拼团列表
                mui.openWindow({
                    id: 'merge-list.html',
                    url: url + 'merge-list.html',
                });
            } else
                if (href.indexOf('/m-wap/limittimebuy/detail') != -1) { //限时购详情   
                    var id = href.split("/");
                    id = id[id.length - 1];
                    showProduct(id);
                } else
                    if (href.indexOf('/m-Wap/LimitTimeBuy/Home') != -1) {  //限时购列表   
                        mui.openWindow({
                            id: 'limitbuy-list.html',
                            url: url + 'limitbuy-list.html',
                        });
                    } else
                        if (href.indexOf('/m-Wap/SignIn') != -1) {  //签到 
                            if (isLogin(login, "1")) {
                                mui.openWindow({
                                    id: 'wx-page.html',
                                    url: url + 'wx-page.html',
                                    extras: { title: "签到", url: "/m-Wap/SignIn", fullPath: url }
                                });
                            }
                        } else
                            if (href.indexOf('/m-wap/shopregister/step1') != -1) {  //商家入驻 
                                if (isLogin(login, "1")) {
                                    mui.openWindow({
                                        id: 'wx-page.html',
                                        url: url + 'wx-page.html',
                                        extras: { title: "商家入驻", url: "/m-wap/shopregister/step1", fullPath: url }
                                    });
                                }
                            } else
                                if (href.indexOf('/m-wap/shopbranch/storelist') != -1) {  //周边门店 
                                    mui.openWindow({
                                        id: 'around-stores.html',
                                        url: url + 'around-stores.html',
                                    });
                                } else
                                    if (href.indexOf('/m-wap/vshop/CouponInfo') != -1) {  //优惠券详情页 
                                        var id = href.split("/");
                                        id = id[id.length - 1];
                                        if (isLogin(login, "1")) {
                                            mui.openWindow({
                                                id: 'wx-page.html',
                                                url: url + 'wx-page.html',
                                                extras: { title: "优惠券详情页", url: "/m-wap/vshop/CouponInfo/" + id, fullPath: url }
                                            });
                                        }
                                    } else
                                        if (href.indexOf('/m-weixin/bonus/index') != -1) {  //现金红包 
                                            var id = href.split("/");
                                            id = id[id.length - 1];
                                            mui.openWindow({
                                                id: 'wx-page.html',
                                                url: url + 'wx-page.html',
                                                extras: { title: "现金红包", url: "/m-weixin/bonus/index/" + id, fullPath: url }
                                            });
                                        }
    });
    function isLogin(login, type) {
        if (login == "" || login == null) {
            mui.openWindow({
                id: 'login.html',
                url: url + 'login.html'
            });
            return;
        } else {
            return true;
        }
    }

    function showProduct(id) {
        mui.fire(plus.webview.getWebviewById('product-detail.html'), 'updateData', { productId: id });
        mui.openWindow({
            id: 'product-detail.html',
            url: url + 'product-detail.html',
            show: {
                autoShow: true,
                aniShow: 'pop-in',
                duration: 300
            },
            waiting: {
                autoShow: false
            }
        });
    }
});