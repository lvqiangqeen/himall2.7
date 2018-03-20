$(function () {
    $('#consigneeAddress').blur(function () {
        var obj = $(this);
        setTimeout(function () {
            $(".select-container").hide();
            var str = $(obj).val();
            if (str) {
                $('#consigneeAddressNote').hide();
            } else {
                $('#consigneeAddressNote').show();
            }
        }, 300);
    });
    $('#consigneeAddress').focus(function () {
        $(".select-container").show();
        setTimeout(function () {
            if ($("#consigneeAddress").val().length > 0) {
                searchKeyword(1);
            }
        }, 300);
    });
    $('.select-container li').click(function () {
        $('#consigneeAddress').val($(this).attr('data-addr'));
        $('#consigneeAddress').blur();
    });
});

var searchService;
var pageIndex = 0;
var pageCapacity = 1000;
var geocoder;
var hasData = true;
$(document).ready(function () {
    //设置Poi检索服务，用于本地检索、周边检索
    searchService = new qq.maps.SearchService({
        //检索成功的回调函数
        complete: function (results) {
            //设置回调函数参数
            var pois = results.detail.pois;
            if (pois == undefined) {
                $("#divMore").html("查询不到数据");
            }
            else {
                for (var i = 0, l = pois.length; i < l; i++) {
                    var poi = pois[i];
                    if (typeof (poi.address) != "undefined") {
                        var parText = "\'" + poi.latLng + "\'" + "," + "\'" + poi.address + "\'" + "," + "\'" + poi.name + "\'";
                        $("#nearAddress").append('<li data-addr=\"' + poi.name + '\" onclick="choosePosition(' + parText + ')"> <i class="ic_locate"></i><span class="addr-name">' + poi.name + '&nbsp;,&nbsp;</span><span class="addr-detail">' + poi.address + '</span></li>');
                    }
                }
                if (pois.length < 10) {
                    $("#divMore").html("附近没有更多地址了");
                    hasData = false;
                }
                else {
                    //$("#divMore").html("加载更多...");
                }
                pageIndex++;
            }
        },
        //若服务请求失败，则运行以下函数
        error: function () {
            $("#divMore").html("查询不到数据");
        }
    });
    $("#divAdr").hide();
    $("#consigneeAddress").bind('input propertychange', function () {
        var keyword = $("#consigneeAddress").val();
        if (keyword != "" && keyword != null) {
            searchKeyword(1);
        }
    });
});

//设置搜索的范围和关键字等属性
function searchKeyword(index) {
    if (!isRefresh && index == 2) {
        isRefresh = true;
        return;
    }
    //$("#container").hide();
    $("#divAdr").show();
    var keyword = $("#consigneeAddress").val();
    if (keyword == "") {
        $.dialog.tips("请输入要搜索的地址信息");
        return;
    }
    var region = $("#regionSelector select:eq(2)").find("option:selected").text();//优先区县
    if (region == "请选择") {
        region = $("#regionSelector select:eq(1)").find("option:selected").text();
    }
    if (region == "请选择") {
        region = '';
    }
    if (index == 1) {
        pageIndex = 0;
        hasData = true;
        $("#nearAddress").empty();
    }
    else {
        if (!hasData) {
            return;
        }
    }
    //根据输入的城市设置搜索范围
    searchService.setLocation(region);
    //设置搜索页码
    searchService.setPageIndex(pageIndex);
    //设置每页的结果数
    searchService.setPageCapacity(pageCapacity);
    //根据输入的关键字在搜索范围内检索
    searchService.search(keyword);
}
//--------滚动时，往下加载数据 start--------------

function scrollLoadData() {
    var scrollTop = $(this).scrollTop();
    var scrollHeight = $(this)[0].scrollHeight;
    var windowHeight = $(this).height();
    if (scrollTop + windowHeight >= scrollHeight) {
        setTimeout(searchKeyword(2), 200);
    }
}
$(window).scroll(function () {
    if (hasData)
        scrollLoadData();
});
var isRefresh = true;
var proId = 0, cityId = 0, districtId = 0, streetId = 0;
function choosePosition(latLng, address,name) {
    $("#Longitude").val(latLng.split(',')[1].trim());
    $("#Latitude").val(latLng.split(',')[0].trim());
    isRefresh = false;
    $("#divAdr").hide();
    var queryData = {
        fromLatLng: latLng.split(',')[0].trim() + ',' + latLng.split(',')[1].trim(), url: "/UserAddress/InitRegion"
}
$.ajax({
    type: "GET",
    url: queryData.url,
    data: queryData,
    async: false,
    dataType: "json",
    success: function (data) {
        var fullPath = data.fullPath;//省，市，区，县 :1812,1813,1814,27074
        if (fullPath != '') {
            var arr = fullPath.split(',');
            if (arr.length >= 3) {
                proId = arr[0]; cityId = arr[1]; districtId = arr[2];
            }
            if (arr.length >= 4) {
                streetId = arr[3];
            }
        }
        name = getNewAddress(name, data.showCity, data.street);// 过滤掉选择地址名称中的省市区街道
        $("#consigneeAddress").val(name);
        //初始化街道组合
        $("#NewAddressId").val(streetId > 0 ? streetId : districtId);
        $("#regionSelector").RegionSelector({
            valueHidden: "#NewAddressId"
        });
        $("#regionSelector select").change(function () {//每次选择地址后都要将详细地址清空，防止经纬度和地区不匹配
            $("#consigneeAddress").val("");
        });
    },
    error: function () {
        $.dialog.tips("系统繁忙，请刷新重试");
    }
});
}
function getNewAddress(address, showCity, street) {
    if (showCity != '') {
        var storeAreaArr = showCity.split(' ');
        if (street != '') {
            storeAreaArr.push(street);
        }
        for (var i = 0; i < storeAreaArr.length; i++) {
            address = address.replace(storeAreaArr[i], '');
        }
    }
    return address;
}