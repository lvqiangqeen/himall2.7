 $(function () {
        var sid = getQueryString("shopId");//获取商家ID参数
        if (sid != null) {
            shopId = sid;
        }
        //fromLatLng = "28.189419,112.997681";
        //loadStoresData(1);   //测试用
        var mapkey = $("#hdQQMapKey").val();
        var geolocation = new qq.maps.Geolocation(mapkey, "myapp");
        if (geolocation) {
            geolocation.getLocation(getPositionSuccess, ShowError)
        }
        else {
            $.dialog.tips("请在系统设置中打开“定位服务“允许Himall商城获取您的位置");
        }
		/*if (navigator.geolocation){ 
		    navigator.geolocation.getCurrentPosition(function(position){
		    	var lat = position.coords.latitude; //纬度 
				var lag = position.coords.longitude; //经度 
				fromLatLng = lat+','+lag;
    			loadStoresData();
		    },function(error){
		    	switch(error.code) { 
				    case error.PERMISSION_DENIED: 
				      $.dialog.tips("定位失败,用户拒绝请求地理定位"); 
				      break; 
				    case error.POSITION_UNAVAILABLE: 
				      $.dialog.tips("定位失败,位置信息是不可用"); 
				      break; 
				    case error.TIMEOUT: 
				      $.dialog.tips("定位失败,请求获取用户位置超时"); 
				      break; 
				    case error.UNKNOWN_ERROR: 
				      $.dialog.tips("定位失败,定位系统失效"); 
				      break; 
				} 
		    });
	    }else{
	    	$.dialog.tips("该浏览器不支持地理定位");
	    }*/
    });
//获取定位
function getPositionSuccess(position) {
    var lat = position.lat;
    var lng = position.lng;
    fromLatLng = lat + ',' + lng;
    console.log(fromLatLng);
    loadStoresData();
}
//定位错误
function ShowError(error) {
    switch (error.code) {
        case error.PERMISSION_DENIED:
            break;
        case error.POSITION_UNAVAILABLE:
            break;
        case error.TIMEOUT:
            break;
        case error.UNKNOWN_ERROR:
            break;
    }
}
//加载周边门店数据
function loadStoresData() {
    if (lodeEnd)
        return;
    var queryData = {
        pageNo: curpageindex, pageSize: curpagesize, shopId: shopId, fromLatLng: fromLatLng, url: "List"
    }
    if (fromLatLng == "" || fromLatLng == undefined) {
        $.dialog.tips('无法获取您的当前位置，请确认是否开启定位服务');
        return;
    }
    $.ajax({
        type: "GET",
        url: queryData.url,
        data: queryData,
        async: false,
        dataType: "json",
        success: function (data) {
            if (data.Success == false) {
                $.dialog.tips(data.Message);
            } else {
                var databox = $("#store_list");
                if (curpageindex == 1)
                    databox.empty();
                if (data) {
                    total = data.Total;
                    if (data.Models && data.Models.length > 0) {
                        $.each(data.Models, function (i, model) {
                            var userhtm = getStoreHtml(model);
                            databox.append(userhtm);
                        });
                        curpageindex += 1;
                        $(".no_sotre").hide();
                        $("#sansearchstroe").hide();
                        $("#imgnonstore").hide();
                        if (total == data.Models.length)
                            lodeEnd = true;
                    } else {
                        $("#sansearchstroe").hide();
                        if (curpageindex == 1) {
                            lodeEnd = true;
                            $.dialog.tips("未匹配到任何门店");
                            $(".no_sotre").show();
                            $("#imgnonstore").show();
                        } else {
                            lodeEnd = true;
                        }
                    }
                    loadEndProcess();
                }
            }
        },
        error: function () {
            $.dialog.tips("系统繁忙，请刷新重试");
        }
    });
}
//获取地址参数，当从详细页过来的时候，需要商家IDshopid
function getQueryString(name) {
    var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)", "i");
    var r = window.location.search.substr(1).match(reg);
    if (r != null) return unescape(r[2]); return null;
}
//组合门店数据
function getStoreHtml(obj) {
    var arr = new Array();
    arr.push('<li>');
    arr.push('<h4 onclick="window.location.href=\'index/' + obj.Id + '\'">' + obj.ShopBranchName + '</h4>');
    arr.push('<span class="store_list_address" onclick="window.location.href=\'index/' + obj.Id + '\'">' + obj.AddressDetail + '</span>');
    arr.push('<div class="store_list_tel" onclick="window.location.href=\'index/' + obj.Id + '\'">' + obj.DistanceUnit + '</div>');
    arr.push('<div class="store_list_map"><a href="javascript:onMapClick(\'' + obj.Latitude + '\',\'' + obj.Longitude + '\',\'' + obj.AddressDetail + '\');"><span>查看地图</span></a><a href="tel://' + obj.ContactPhone + '"><span>拨打电话</span></a></div>');
//  arr.push('<i class="icon_angle_right"></i>');
    arr.push('</li>');
    return arr.join("");
}

var fromLatLng = '';
var curpagesize = 10, curpageindex = 1, total = -1, lodeEnd = false, shopId = '';
$(window).scroll(function () {
    totalheight = parseFloat($(window).height()) + parseFloat($(window).scrollTop());     //浏览器的高度加上滚动条的高度
    if ($(document).height() - 10 <= totalheight)     //当文档的高度小于或者等于总的高度的时候，开始动态加载数据
    {
        setTimeout(loadStoresData(), 200);
    }
});
//查看地图
function onMapClick(latitude, longitude, shopbranchAddress) {
    window.location.href = 'http://apis.map.qq.com/tools/routeplan/eword=' + shopbranchAddress + '&epointx=' + longitude + '&epointy=' + latitude + '?referer=myapp&key=OB4BZ-D4W3U-B7VVO-4PJWW-6TKDJ-WPB77';
}
function loadEndProcess() {
    if (lodeEnd) {
        $("#divMore").show();
        $("#divMore").html("没有更多数据了");
    } else {
        $("#divMore").hide();
    }
}