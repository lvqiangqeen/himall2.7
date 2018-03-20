var sUserAgent = navigator.userAgent.toLowerCase();
var bIsIpad = sUserAgent.match(/ipad/i) == "ipad";
var bIsIphoneOs = sUserAgent.match(/iphone os/i) == "iphone os";
var bIsMidp = sUserAgent.match(/midp/i) == "midp";
var bIsUc7 = sUserAgent.match(/rv:1.2.3.4/i) == "rv:1.2.3.4";
var bIsUc = sUserAgent.match(/ucweb/i) == "ucweb";
var bIsAndroid = sUserAgent.match(/android/i) == "android";
var bIsCE = sUserAgent.match(/windows ce/i) == "windows ce";
var bIsWM = sUserAgent.match(/windows mobile/i) == "windows mobile";
var JumpKey = {
    Key: [
      { PC: '/userorder', WAP: '/member/orders', WX: '/member/orders' }
    , { PC: '/usercenter', WAP: '/member/center', WX: '/member/center' }
    , { PC: '/login', WAP: '/login/entrance', WX: '/login/entrance' }
    , { PC: '/register', WAP: '/register', WX: '/register' }
    , { PC: '/shop', WAP: '/vshop/detail', WX: '/vshop/detail' }
    ]
};

SetPlatForm();

function SetPlatForm() {
    //debugger;
    var ua = window.navigator.userAgent.toLowerCase();
    //bIsAndroid = true;
    if ((bIsIpad || bIsIphoneOs || bIsMidp || bIsUc7 || bIsUc || bIsAndroid || bIsCE || bIsWM)) {
        if (ua.match(/MicroMessenger/i) == 'micromessenger' && location.href.toLowerCase().indexOf('/m-wap') == -1 && location.href.toLowerCase().indexOf('/m-weixin') == -1) {
            //if (location.href.toLowerCase().indexOf('/m-wap') == -1 && location.href.toLowerCase().indexOf('/m-weixin') == -1) {
            //wx-pc
            if (location.href.toLowerCase().indexOf('/shop/home') >= 0) {//shop vshop/detail
                isErrorURL(location.href.toLowerCase(), 'm-weixin', function () {
                    var url = '';
                    var arr = location.href.toLowerCase().split('/');
                    for (var i = 0; i < arr.length; i++) {
                        url = url + arr[i] + '/';
                        if (i == 2)
                            url = url + 'm-weixin/'
                    }
                    getVShopIdByShopId(arr[arr.length - 1], url, 'm-weixin');
                });
            }
            else {
                url = ConvertUrl('pc', 'wx');
                isErrorURL(url, 'm-weixin', function (url) {
                    location.href = url;
                });
            }
        }
        else if (ua.match(/MicroMessenger/i) != 'micromessenger' && location.href.toLowerCase().indexOf('/m-wap') == -1 && location.href.toLowerCase().indexOf('/m-weixin') == -1) {
            if (location.href.toLowerCase().indexOf('/shop/home') >= 0) {
                isErrorURL(location.href.toLowerCase(), 'm-wap', function () {
                    var url = '';
                    var arr = location.href.toLowerCase().split('/');
                    for (var i = 0; i < arr.length; i++) {
                        url = url + arr[i] + '/';
                        if (i == 2)
                            url = url + 'm-wap/'
                    }
                    getVShopIdByShopId(arr[arr.length - 1], url, 'm-wap');
                });
            }
            else {
                url = ConvertUrl('pc', 'wap');
                isErrorURL(url, 'm-wap', function (url) {
                    location.href = url;
                });
            }
        }
    }
    else {//pc jump to wap、weixin
        if (location.href.toLowerCase().indexOf('/m-wap') > -1) {
            url = ConvertUrl('wap', 'pc');
            isErrorURL(url, '', function (url) {
                location.href = url;
            });
        }
        else if (location.href.toLowerCase().indexOf('/m-weixin') > -1) {
            url = ConvertUrl('wx', 'pc');
            isErrorURL(url, '', function (url) {
                location.href = url;
            });
        }
        $('body').show();
    }

}

function ConvertUrl(sourceType, deType) {
    var deValue = '';
    var sValue = '';
    var url = location.href.toLowerCase();
    for (var i = 0; i < JumpKey.Key.length; i++)
    {
        deValue = getJumpKeyValue(i, deType).toLowerCase();
        sValue = getJumpKeyValue(i, sourceType).toLowerCase();
        if (url.indexOf(sValue) > -1) {
            url = url.replace(sValue, deValue);
            break;
        }
    }
    if (deType == 'wx')
    {
        url = url.replace(location.host.toLowerCase(), location.host + '/' + 'm-weixin');
    }
    else if (deType == 'wap')
    {
        url = url.replace(location.host.toLowerCase(), location.host + '/' + 'm-wap');
    }
    return url;
}

function getJumpKeyValue(idx, name) {
    switch(name.toLowerCase())
    {
        case 'pc':
            return JumpKey.Key[idx].PC;
            break;
        case 'wap':
            return JumpKey.Key[idx].WAP;
            break;
        case 'wx':
            return JumpKey.Key[idx].WX;
            break;
    }
}

function getVShopIdByShopId(shopId, url, platform) {
    $.post('/' + platform + '/vshop/GetVShopIdByShopId', { shopId: shopId }, function (result) {
        if (result.success == true) {
            var urlStr = '';
            var arr = url.split('/');
            for (var i = 0; i < arr.length - 3; i++) {
                urlStr = urlStr + arr[i] + '/'
            }
            location.href = urlStr.replace('/shop', '/vshop') + 'detail/' + result.msg;
        }
    })
}

function isErrorURL(url, platform,fncallback) {
    $.ajax({
        type: "GET",
        cache: false,
        url: "" + url + "",
        data: "",
        complete: function (data) {
            if (data.status == 404 || data.status==500) {
                location.href = '//' + location.host + '/' + platform;
            }
            else {
                fncallback(url);
            }
        }
    });
}