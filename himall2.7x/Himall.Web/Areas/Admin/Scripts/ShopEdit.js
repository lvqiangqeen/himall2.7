
$(function () {

  //  bindCity("consignee_province", "consignee_city", "consignee_county", companyRegionIds);
    //bindCity("bank_consignee_province", "bank_consignee_city", "bank_consignee_county", bankRegionIds);
    //初始化商家地址
    $("#regionSelector").RegionSelector({
        selectClass: "form-control input-sm select-sort",
        valueHidden: "#NewCompanyRegionId"
    });
    $('#submit').click(function () {
        var form = $('#shopEditForm');

        var bankPhoto = $('#bankPhoto').hishopUpload('getImgSrc');

        if (!bankPhoto)
            $.dialog.errorTips('请上传开户银行许可证电子版');
        else {

            //var region = ($("#consignee_county").val() || 0) == 0 ? $("#consignee_city").val() : $("#consignee_county").val();
            //$('input[name="NewCompanyRegionId"]').val(region);
            //var bankregion = ($("#bank_consignee_county").val() || 0) == 0 ? $("#bank_consignee_city").val() : $("#bank_consignee_county").val();
            //$('input[name="NewBankRegionId"]').val(bankregion);
            var data = form.serialize();
            var loading = showLoading();
            $.post('Edit', data, function (result) {
                loading.close();
                if (result.success) {
                    $.dialog.tips("修改成功！");
                    setTimeout(function () {
                        window.location = "./management";
                    }, 2000);
                }
                else
                    $.dialog.errorTips(result.msg);
            });
        }

    });
})


function bindCity(provinceControl, cityControl, countyControl, regionId) {
    var regions = regionId.split(',');
    var shengfen = regions.length > 0 ? regions[0] : 0;
    var city = regions.length > 1 ? regions[1] : 0;
    var county = regions.length > 2 ? regions[2] : 0;
    $('#' + provinceControl + ',#' + cityControl + ',#' + countyControl).cityLink(shengfen, city, county);
}



//; (function ($, data) {
//    $.fn.cityLink = function (a, b, c) {
//        var that = this,
//            province = '',//省
//            city = '',//市
//            area = '',//区域
//            i, e,
//            createElem = function (data, elem, select, id) {// 创建元素
//                if (!data) { return; }
//                for (var i = 0, e; e = data[i++];) {
//                    if (select == e.id) {
//                        elem.append('<option value="' + e.id + '" selected="true" ' + (id ? 'data="' + id + '"' : '') + '>' + e.name + '</option>');
//                    } else {
//                        elem.append('<option value="' + e.id + '"' + (id ? 'data="' + id + '"' : '') + '>' + e.name + '</option>');
//                    }
//                }
//            },
//            fnSelect = function (data, val, tag) {
//                if (!data) { return; }
//                for (var i = 0, e; e = data[i++];) {
//                    if (e.id == val) {
//                        return e[tag];
//                    }
//                }
//            },
//            fnChange = function (dom, target, tag, bool) {
//                dom.click(function (e) {
//                    var t = e.target,
//                        val = $(this).val(),
//                        dataTag = '',
//                        temp = '',
//                        cityData;
//                    if (val != 0) {
//                        if (bool) {
//                            dataTag = $(this).find("option:selected").attr('data');
//                            temp = fnSelect(data, dataTag, bool);
//                            cityData = fnSelect(temp, val, tag);
//                        } else {
//                            cityData = fnSelect(data, val, tag);
//                        }
//                        target.html('');
//                        if (tag == 'city') {
//                            $('.selected-address i').html($(this).find("option:selected").html() + ' ');
//                            $('.selected-address em').html(' ');
//                            $('.selected-address s').html(' ');
//                        } else {
//                            $('.selected-address em').html($(this).find("option:selected").html() + ' ');
//                        }
//                        createElem(cityData, target, val, val);
//                    }
//                    return false;
//                });
//            },
//            init = function (a, b, c) {
//                $(that[0]).html('');
//                if (b == 0) {
//                    $(that[1]).html('<option value="0">请选择</option>');
//                }
//                if (c == 0) {
//                    $(that[2]).html('<option value="0">请选择</option>');
//                }
//                createElem(data, province, a);
//                var cityData = fnSelect(data, a, 'city'),
//                    areaData = fnSelect(cityData, b, 'county');

//                if (province && city) {
//                    createElem(cityData, city, b);
//                    fnChange(province, city, 'city');//@@province dom //当前点击事件的dom @@city市级dom @@市级字符串用来读取json数据
//                }
//                if (province && city && area) {
//                    createElem(areaData, area, c);
//                    fnChange(city, area, 'county', 'city');
//                }
//                $('#consignee_county').change(function (e) {
//                    var id = $(this).val();
//                    if (id != 0) {
//                        $('.selected-address s').html($(this).find("option:selected").html());
//                    }
//                });
//            };
//        that.each(function (i, e) {
//            switch (i) {
//                case 0: province = $(e); break;
//                case 1: city = $(e); break;
//                case 2: area = $(e); break;
//                default: break;
//            }
//        });
//        init(a, b, c);// 初始化
//    };
//}(jQuery, province));
//$('#consignee_province,#consignee_city,#consignee_county').cityLink(0, 0, 0);

//; (function ($, data) {
//    var getOption = function (elem, bool) {
//        var s, t;
//        if (bool) {
//            elem.children().each(function (i, e) {
//                s = e.selected;
//                if (s == true) {
//                    t = $(e).html();
//                    return;
//                }
//            });
//        } else {
//            elem.children().each(function (i, e) {
//                s = e.selected;
//                if (s == true) {
//                    t = $(e).val();
//                    return;
//                }
//            });
//        }
//        return t;
//    };
//    $('#saveConsigneeTitleDiv').bind('click', function () {
//        var a = getOption($('#consignee_province'), 0),
//            b = getOption($('#consignee_city'), 0),
//            c = getOption($('#consignee_county'), 0),
//            province = getOption($('#consignee_province'), 1),
//            city = getOption($('#consignee_city'), 1),
//            county = getOption($('#consignee_county'), 1),
//            value = a + ',' + b + ',' + c,
//            str = province + ' ' + city + ' ' + county,
//            indexId = $('input[type="radio"][name="address"]:checked').val();



//        if ($('#addressEditArea').css('display') == 'none') {
//            var selectedAddress = shippingAddress[indexId];

//            var newSelectedText = selectedAddress.shipTo + ' &nbsp;&nbsp;&nbsp; ' + selectedAddress.phone + ' &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<br />' + selectedAddress.fullRegionName + ' &nbsp; &nbsp;' + selectedAddress.address + '&nbsp;';
//            $('#shippingAddressId').val(indexId);
//            $('#addressEditArea').hide();
//            $('#selectedAddress').html(newSelectedText);
//            $('#addressListArea').hide();
//            $('#consignee-list').hide();
//            $('#step-1').removeClass('step-current');
//        }
//        else {
//            var shipTo = $('input[name="shipTo"]').val();
//            var phone = $('input[name="phone"]').val();
//            var address = $('input[type="text"][name="address"]').val();
//            var regionId = $('#consignee_county').val();

//            if (!shipTo)
//                $.dialog.tips('请填写收货人');
//            else if (!phone)
//                $.dialog.tips('请填写电话');
//            else if (!regionId)
//                $.dialog.tips('请填选择所在地区');
//            else if (!address)
//                $.dialog.tips('请填写详细地址');
//            else {
//                saveAddress(indexId, regionId, shipTo, address, phone, function (result) {
//                    var newSelectedText = shipTo + ' &nbsp;&nbsp;&nbsp; ' + phone + ' &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<br />' + str + ' &nbsp; &nbsp;' + address + '&nbsp;';
//                    $('#selectedAddress').html(newSelectedText);

//                    indexId = isNaN(parseInt(indexId)) ? '' : parseInt(indexId);
//                    if (indexId == 0) {
//                        indexId = result.id;
//                        shippingAddress[indexId] = {};
//                    }

//                    $('#shippingAddressId').val(indexId);
//                    $('#addressEditArea').hide();
//                    $('#selectedAddress').html(newSelectedText);
//                    $('#addressListArea').hide();
//                    $('#consignee-list').hide();
//                    $('#step-1').removeClass('step-current');
//                    if (shippingAddress) {
//                        shippingAddress[indexId].fullRegionIdPath = value;
//                        shippingAddress[indexId].fullRegionName = str;
//                    }

//                });
//            }

//        }

//    });
//}(jQuery, province));