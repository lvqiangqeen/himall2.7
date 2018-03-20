$(function () {
    $("#area-selector").RegionSelector({
        selectClass: "input-sm select-sort-auto",
        valueHidden: "#AddressId"
    });
    $("#area-selector-Delivery").RegionSelector({
        selectClass: "input-sm select-sort-auto",
        valueHidden: "#DeliveryScopAddressId"
    });
    InitStoreBanner();//初始化门店Banner
    initMap();//初始化门店地图
    $("#addDeliveryScop").click(function (e) {
        //打开省市区弹框的时候清除
        $("#DeliveryScopAddressId").val(0);
        $("#area-selector-Delivery").RegionSelector({
            selectClass: "input-sm select-sort-auto",
            valueHidden: "#DeliveryScopAddressId"
        });
        EditRegionId = "";
        AddDeliveryScop();
    });
});
function checkUsernameIsValid(username) {
    var result = false;
    var normalreg = /^([\u4E00-\u9FA5]|[A-Za-z0-9])[\u4E00-\u9FA5\A-Za-z0-9\_\-]{3,19}$/;
    var reg = normalreg;
    if (!username || username == '用户名') {
        $.dialog.tips('管理员账号输入有误');
        return result;
    }
    if ((/^\d+$/.test(username))) {
        $.dialog.tips('不可以使用纯数字管理员账号');
        return result;
    }
    if (!reg.test(username)) {
        $.dialog.tips('管理员账号需要4-20位字符，支持中英文、数字及"-"、"_"的组合');
        return result;
    }
    result = true;
    return result;
}
function checkPasswordIsValid(password) {
    var result = false;
    var reg = /^[^\s]{6,20}$/;
    var result = reg.test(password);
    return result;
}
function checkData() {
    if ($.trim($('#ShopBranchName').val()).length==0) {
        $.dialog.tips('门店名称不能为空');
        return;
    }
    if ($('#ShopBranchName').val().length > 15) {
        $.dialog.tips('门店名称不能超过15个字');
        return;
    }
    if ($('#AddressId').attr('isfinal') == 'false') {
        $.dialog.tips('请选择门店地址');
        return;
    }
    if ($('#AddressDetail').val().length > 50) {
        $.dialog.tips('详细地址不能超过50个字');
        return;
    }
    if (Number($("#Longitude").val()) <= 0 || Number($("#Latitude").val()) <= 0) {
        $.dialog.tips('请搜索地址地图定位');
        return;
    }
    if (Number($("#ServeRadius").val()) <= 0 && $("#txtRegionScop").val().length <= 0) {
        $.dialog.tips('配送半径和配送范围不能同时为空');
        return;
    }
    var reg1 = /^[1-9]\d{10}$/;
    var str = $('input[name=ContactPhone]').val();
    if (!reg1.test(str)) {
        $.dialog.tips('联系方式只能为11位数字');
        return;
    }
    str = $('input[name=UserName]').val();
    if (!checkUsernameIsValid(str))
        return;
    str = $('input[name=PasswordOne]').val();
    if (!checkPasswordIsValid(str)) {
        $.dialog.tips('密码最少6位');
        return;
    }
    if ($('input[name=PasswordOne]').val() != $('input[name=PasswordTwo]').val()) {
        $.dialog.tips('两次密码不一致');
        return;
    }
    $("#ShopImages").val($("#storeBanner").hishopUpload('getImgSrc').toString());//门店Banner
    $('#from_Save1').ajaxSubmit({
        dataType: 'application/json',
        success: function (result) {
            var data = JSON.parse(result);
            if (data.success == true) {
                location.href = $('#urlManagement').val();
            } else {
                $.dialog.tips(data.msg);
            }
        }
    });
}
function InitStoreBanner(storeBanner) {
    $('#storeBanner').hishopUpload({
        title: '<b>*</b>门店Banner：',
        imageDescript: '建议尺寸960*420，支持.jpg .jpeg .bmp .png格式，大小不超过1M',
        displayImgSrc: storeBanner,
        imgFieldName: "storeBannerLogo",
        dataWidth: 8,
        imagesCount: 1
    });
}
var map, searchService, marker, markers = [], infoWin = null;
var initMap = function () {
    var center = new qq.maps.LatLng(39.916527, 116.397128);
    map = new qq.maps.Map(document.getElementById('container'), {
        center: center,
        zoom: 13
    });
    var scaleControl = new qq.maps.ScaleControl({
        align: qq.maps.ALIGN.BOTTOM_LEFT,
        margin: qq.maps.Size(85, 15),
        map: map
    });
    //调用Poi检索类
    searchService = new qq.maps.SearchService({
        //检索成功的回调函数
        complete: function (results) {
            //设置回调函数参数
            var pois = results.detail.pois;
            infoWin = new qq.maps.InfoWindow({
                map: map
            });
            var latlngBounds = new qq.maps.LatLngBounds();
            for (var i = 0, l = pois.length; i < l; i++) {
                var poi = pois[i];
                //扩展边界范围，用来包含搜索到的Poi点
                latlngBounds.extend(poi.latLng);
                (function (n) {
                    var marker = new qq.maps.Marker({
                        map: map
                    });
                    marker.setPosition(pois[n].latLng);
                    markers.push(marker);
                    qq.maps.event.addListener(marker, 'click', function () {
                        infoWin.open();
                        infoWin.setContent('<div style = "width:200px;padding:10px 0;">' + pois[n].address + '<div class="map-import-btn"><input type="button" class="btn btn-xs btn-primary" value="导入门店地址" onclick="chooseShopLoc(this);" address=' + pois[n].address + ' lat =' + pois[n].latLng.getLat() + '  lng =' + pois[n].latLng.getLng() + ' /></div></div>');
                        infoWin.setPosition(pois[n].latLng);
                    });
                })(i);
            }
            //调整地图视野
            map.fitBounds(latlngBounds);
        },
        //若服务请求失败，则运行以下函数
        error: function () {
            alert("很抱歉，未搜索到此地址，请重新输入！");
        }
    });
}
//导入门店信息
function chooseShopLoc(t) {
    var address = $(t).attr("address");
    var storeAreaArr = getSelectArea();
    for (var i = 0; i < storeAreaArr.length; i++) {
        if (i <= 3) {
            address = address.replace(storeAreaArr[i], '');
        }
    }
    var lat = $(t).attr("lat");
    var lng = $(t).attr("lng");
    this.clearMarkers();
    var position = new qq.maps.LatLng(lat, lng);
    marker = new qq.maps.Marker({
        map: map,
        position: position,
        draggable: true
    });
    map.panTo(position);
    map.zoomTo(18);
    $("#Longitude").val(lng);
    $("#Latitude").val(lat);
    qq.maps.event.addListener(marker, 'dragend', function () {
        if (marker.getPosition()) {
            $("#Longitude").val(marker.getPosition().getLng());
            $("#Latitude").val(marker.getPosition().getLat());
        }
    });
    $("#AddressDetail").val(address);
    if (infoWin) {
        infoWin.close();
    }
    $("#map_des").hide();
}
////删除所有标记
function clearMarkers() {
    if (markers) {
        for (i = 0; i < markers.length; i++) {
            markers[i].setMap(null);
        }
        markers.length = 0;
    }
}
function getSelectArea() {
    var storeArr = [];
    $("#area-selector select").each(function (i) {
        if ($(this).find("option:selected").text() != '请选择') {
            storeArr.push($(this).find("option:selected").text());
        }
    });
    return storeArr;
}
//搜索地址，这里需要判断是否选择了省市区
function getResult() {
    if ($("#AddressId").val() <= 0) {
        $.dialog.tips("请先选择地区");
        return;
    }
    if ($.trim($("#AddressDetail").val()).length == 0) {
        $.dialog.tips("请先输入详细地址");
        return;
    }
    if (marker != null) marker.setMap(null);
    clearMarkers();
    if (infoWin) {
        infoWin.close();
    }
    var storeArr = getSelectArea();
    var regions = storeArr[0] + storeArr[1] + storeArr[2];
    var regionText = storeArr.join(',');
    var poiText = regions + $.trim($("#AddressDetail").val());
    searchService.setLocation(regionText);
    searchService.search(poiText);
    $("#map_des").show();
}

//配送范围
function AddDeliveryScop(id) {
    $.dialog({
        title: '配送范围',
        lock: true,
        id: 'AddDeliveryScop',
        content: document.getElementById("reply-form"),
        padding: '0 10px',
        okVal: '保存',
        init: function () {
        },
        ok: function () {
            var storeArr = [];
            $("#area-selector-Delivery select").each(function (i) {
                if ($(this).find("option:selected").text() != '请选择') {
                    storeArr.push($(this).find("option:selected").text());
                }
            });
            SaveDeliveryScop($("#DeliveryScopAddressId").val(), storeArr.join(" "));
        }
    });
}

var EditRegionId = "";//当前编辑的区域ID
//保存发货区域信息
function SaveDeliveryScop(RegionId, RegionName) {
    //console.log(RegionId + "-" + RegionName);
    $("#scoplist").show();
    RegionId = RegionId.split(',')[0];
    //区域信息html
    var scopHTML = "<tr id=\"row_{0}\"><td>{1}</td><td><span class=\"btn-a\"><a  href=\"javascript:void(0)\" class=\"editregion\" id=\"EditRegion_{0}\">编辑</a><a  href=\"javascript:void(0)\" class=\"delregion\" id=\"DelRegion_{0}\">删除</a></span></td></tr>".format(RegionId, RegionName);
    //操作对象行
    var row = $("#row_" + RegionId);
    //如果当前编辑的区域ID不为空则将编辑的区域信息替换为新的区域信息
    if (EditRegionId != "") {
        var EditRow = $("#row_" + EditRegionId);//  编辑区域行
        if (EditRow.length > 0 && RegionId != EditRegionId) {//如果编辑区域行存在，且当当前编辑区域ID与返回的区域ID不相同,则替换为新的区域信息
            if ($("#row_" + RegionId).length > 0) return true;//如果已存在该区域行则返回false
            $(scopHTML).insertAfter($(EditRow));//插入新的区域信息
            EditRow.remove();//移除原来的区域信息
            var RegionScop = $("#txtRegionScop").val();
            var RegionScopName = $("#txtRegionScopName").val();
            var RegionScopArr = RegionScop.split(',');
            var RegionScopNameArr = RegionScopName.split(',');
            // console.log(RegionScopArr.length + "-" + RegionScop);
            RegionScop = "";//更新区域信息列表
            RegionScopName = "";
            for (var i = 0; i < RegionScopArr.length; i++) {
                if (RegionScopArr[i] != EditRegionId) {
                    RegionScop = RegionScop + (RegionScop == "" ? "" : ",") + RegionScopArr[i];
                    RegionScopName = RegionScopName + (RegionScopName == "" ? "" : ",") + RegionScopNameArr[i];
                }
                else {
                    RegionScop = RegionScop + (RegionScop == "" ? "" : ",") + RegionId;
                    RegionScopName = RegionScopName + (RegionScopName == "" ? "" : ",") + RegionName;
                }
            }
            EditRegionId = "";
            $("#txtRegionScop").val(RegionScop);
            $("#txtRegionScopName").val(RegionScopName);
            //console.log(RegionScop);
        }
    }
    else {//如果是新的区域ID则插入新行
        if (row.length > 0) return true;//如果已存在该行则返回false
        var tr = $("#scoplist").find('tr:last').clone();
        if (tr.length == 0) {
            $("#scoplist").append(scopHTML);
        } else {
            $(scopHTML).insertAfter($("#scoplist tr:last"));
        }
        var RegionScop = $("#txtRegionScop").val();
        var RegionScopName = $("#txtRegionScopName").val();
        if (RegionScop != undefined && RegionScop != "") {
            RegionScop = RegionScop + ",";
            RegionScopName = RegionScopName + ",";
        }
        $("#txtRegionScop").val(RegionScop + RegionId.split(',')[0]);
        $("#txtRegionScopName").val(RegionScopName + RegionName.split(',')[0]);
        //console.log(RegionScop + "new");

    }
    $(".editregion").on("click", function () {
        var regionId = $(this).attr("id").split("_")[1];
        EditRegionId = regionId;

        $("#DeliveryScopAddressId").val(regionId);
        $("#area-selector-Delivery").RegionSelector({
            selectClass: "input-sm select-sort-auto",
            valueHidden: "#DeliveryScopAddressId"
        });
        AddDeliveryScop(regionId);//传递最后一层ID值编辑
    });
    $(".delregion").on("click", function () {
        var regionId = $(this).attr("id").split("_")[1];
        var RegionScop = $("#txtRegionScop").val();
        var RegionScopArr = RegionScop.split(',');
        var RegionScopName = $("#txtRegionScopName").val();
        var RegionScopNameArr = RegionScopName.split(',');
        RegionScop = "";
        RegionScopName = "";
        for (var i = 0; i < RegionScopArr.length; i++) {
            if (RegionScopArr[i] != regionId) {
                RegionScop = RegionScop + (RegionScop == "" ? "" : ",") + RegionScopArr[i];
                RegionScopName = RegionScopName + (RegionScopName == "" ? "" : ",") + RegionScopNameArr[i];
            }
        }
        $("#txtRegionScop").val(RegionScop);
        $("#txtRegionScopName").val(RegionScopName);
        $(this).parent().parent().parent().remove();
        if ($("#scoplist").find("tr").length == 0) {
            $("#scoplist").hide();
        }
    });
}