$(function () {
    bindRecieverEdit();
    bindSubmit();
    initAddress();
    bindAddressRadioClick();
})


function initAddress() {
    if (!$('#selectedAddress').html()) {
        $('#editReciever').click();
    }
}

var shippingAddress = [];

function bindRecieverEdit() {
    $('#editReciever').click(function () {
        $.post('/order/GetUserShippingAddresses', {}, function (addresses) {
            var html = '';
            var currentSelectedId = parseInt($('#shippingAddressId').val());
            $.each(addresses, function (i, address) {
                shippingAddress[address.id] = address;
                html += '<div class="item" name="li-' + address.id + '">\
                          <label>\
                             <input type="radio" class="hookbox" name="address" '+ (address.id == currentSelectedId ? 'checked="checked"' : '') + ' value="' + address.id + '" />\
                             <b>' + address.shipTo + '</b>&nbsp; ' + address.fullRegionName + ' &nbsp; ' + address.address + ' &nbsp; ' + address.phone + ' &nbsp\
                          </label>\
                          <span class="item-action">\
                              <a href="javascript:;" onclick="showEditArea(\'' + address.id + '\')">编辑</a> &nbsp;\
                              <a href="javascript:;" onclick="deleteAddress(\'' + address.id + '\')">删除</a>&nbsp;\
                          </span>\
                      </div>';
            });
            $('#consignee-list').html(html).show();
            $('#step-1').addClass('step-current');
            $('#addressListArea').show();

            $('input[name="address"]').change(function () {
                var shippingAddressId = $(this).val();
                $('#shippingAddressId').val(shippingAddressId);
            });
        });
    });
}


function bindAddressRadioClick() {
    $('#consignee-list').on('click', 'input[type="radio"]', function () {
        $('#addressEditArea').hide();

    });
}


function deleteAddress(id) {
    $.dialog.confirm('您确定要删除该收货地址吗？', function () {
        var loading = showLoading();
        $.post('/UserAddress/DeleteShippingAddress', { id: id }, function (result) {
            loading.close();
            if (result.success) {
                var current = $('div[name="li-' + id + '"]');
                if ($('input[type="radio"][name="address"]:checked').val() == id) {
                    $('input[type="radio"][name="address"]').first().click();
                    $('#selectedAddress').html('');
                    $('#shippingAddressId').val('');
                }
                current.remove();

                CalcFright();
            }
            else
                $.dialog.errorTips(result.msg);
        });
    });

}


function saveAddress(id, regionId, shipTo, address, phone, callBack) {
    id = isNaN(parseInt(id)) ? '' : parseInt(id);

    var url = '';
    if (id)
        url = '/UserAddress/EditShippingAddress';
    else
        url = '/UserAddress/AddShippingAddress';

    var data = {};
    if (id)
        data.id = id;
    data.regionId = regionId;
    data.shipTo = shipTo;
    data.address = address;
    data.phone = phone;

    var loading = showLoading();
    $.post(url, data, function (result) {
        loading.close();
        if (result.success)
            callBack(result);
        else
            $.dialog.errorTips('保存失败!' + result.msg);
    });
}


function bindSubmit() {
    $('#submit').click(function () {
        var fn = function () {
            var arr = [];
            $('.shopb').each(function (i, e) {
                $(e).children().each(function (l, k) {
                    if ($(k).attr('selected')) {
                        var b = $(k).val();
                        arr.push(b);
                    }
                });
            });
            return arr.join(',');
        };
        var skuIds = QueryString('skuIds');
        var counts = QueryString('counts');
        var action = "SubmitOrder";
        var couponIds = fn();

        var cartItemIds = QueryString('cartItemIds');
        var recieveAddressId = $('#shippingAddressId').val();
        recieveAddressId = parseInt(recieveAddressId);
        recieveAddressId = isNaN(recieveAddressId) ? null : recieveAddressId;

        $('input:radio[name="sex"]').is(":checked")

        var invoiceType = 0;
        if ($("input[name='invoiceType']").is(":checked"))
            invoiceType = $("input[name='invoiceType']:checked").val();
        var invoiceTitle = $("#invoiceTitle").val();

        if ($("#isInvoice").attr('checked')) {
            if (!$("input[name='invoiceType']").is(":checked")) {
                $.dialog.tips('请选择发票类型');
                return false;
            }
            if ($("#invoiceTitle").val() == null || $("#invoiceTitle").val() == '') {
                $.dialog.tips('请输入发票抬头');
                return false;
            }
        }

        if (!recieveAddressId)
            $.dialog.tips('请选择或新建收货地址');
        else {
            if (skuIds) {
                action = "SubmitOrderByProductId";
            }
            var loading = showLoading();
            $.post('/order/' + action, { couponIds: couponIds, skuIds: skuIds, counts: counts, cartItemIds: cartItemIds, recieveAddressId: recieveAddressId, invoiceType: invoiceType, invoiceTitle: invoiceTitle }, function (result) {
                loading.close();
                if (result.success) {
                    location.href = '/order/pay?orderIds=' + result.orderIds.toString();
                }
                else {
                    $.dialog.errorTips(result.msg);
                }
            });
        }
    });
}

function showEditArea(id) {
    $('input[name="address"][value="' + id + '"]').click();

    var address = shippingAddress[id];
    var shipTo = address == null ? '' : address.shipTo;
    var addressName = address == null ? '' : address.address;
    var phone = address == null ? '' : address.phone;
    if (address) {
        var arr = (address.fullRegionName).split(' ');
    }
    var fullRegionName = address == null ? '<i></i><em></em><s></s>' : '<i>' + arr[0] + ' </i><em>' + arr[1] + ' </em><s>' + arr[2] + '</s>';

    $('input[name="shipTo"]').val(shipTo);
    $('input[type="text"][name="address"]').val(addressName);
    $('input[name="phone"]').val(phone);
    $('span[name="regionFullName"]').html(fullRegionName);

    $('#addressEditArea').show();
    if (id === 0) {
        $('#consignee_province,#consignee_city,#consignee_county').cityLink(0, 0, 0);
        return;
    }
    var regionPath = address.fullRegionIdPath.split(',');
    $('#consignee_province,#consignee_city,#consignee_county').cityLink(regionPath[0], regionPath[1], regionPath[2]);
}




; (function ($, data) {
    $.fn.cityLink = function (a, b, c) {
        var that = this,
            province = '',//省
            city = '',//市
            area = '',//区域
            i, e,
            createElem = function (data, elem, select, id) {// 创建元素
                if (!data) { return; }
                if (select) {
                    elem.append('<option value="0">请选择</option>');
                } else {
                    elem.append('<option value="0" selected="true">请选择</option>');
                }
                for (var i = 0, e; e = data[i++];) {
                    if (select == e.id) {
                        elem.append('<option value="' + e.id + '" selected="true" ' + (id ? 'data="' + id + '"' : '') + '>' + e.name + '</option>');
                    } else {
                        elem.append('<option value="' + e.id + '"' + (id ? 'data="' + id + '"' : '') + '>' + e.name + '</option>');
                    }
                }
            },
            fnSelect = function (data, val, tag) {
                if (!data) { return; }
                for (var i = 0, e; e = data[i++];) {
                    if (e.id == val) {
                        return e[tag];
                    }
                }
            },
            fnChange = function (dom, target, tag, bool) {
                dom.change(function (e) {
                    var t = e.target,
                        val = $(this).val(),
                        dataTag = '',
                        temp = '',
                        cityData;
                    if (val != 0) {
                        if (bool) {
                            dataTag = $(this).find("option:selected").attr('data');
                            temp = fnSelect(data, dataTag, bool);
                            cityData = fnSelect(temp, val, tag);
                        } else {
                            cityData = fnSelect(data, val, tag);
                        }
                        target.html('');
                        if (tag == 'city') {
                            $('.selected-address i').html($(this).find("option:selected").html() + ' ');
                            $('.selected-address em').html(' ');
                            $('.selected-address s').html(' ');
                        } else {
                            $('.selected-address em').html($(this).find("option:selected").html() + ' ');
                        }
                        createElem(cityData, target, val, val);
                    }
                    return false;
                });
            },
            init = function (a, b, c) {
                $(that[0]).html('');
                if (b == 0) {
                    $(that[1]).html('<option value="0">请选择</option>');
                }
                if (c == 0) {
                    $(that[2]).html('<option value="0">请选择</option>');
                }
                createElem(data, province, a);
                var cityData = fnSelect(data, a, 'city'),
                    areaData = fnSelect(cityData, b, 'county');

                if (province && city) {
                    createElem(cityData, city, b);
                    fnChange(province, city, 'city');//@@province dom //当前点击事件的dom @@city市级dom @@市级字符串用来读取json数据
                }
                if (province && city && area) {
                    createElem(areaData, area, c);
                    fnChange(city, area, 'county', 'city');
                }
                $('#consignee_county').change(function (e) {
                    var id = $(this).val();
                    if (id != 0) {
                        $('.selected-address s').html($(this).find("option:selected").html());
                    }
                });
            };
        that.each(function (i, e) {
            switch (i) {
                case 0: province = $(e); break;
                case 1: city = $(e); break;
                case 2: area = $(e); break;
                default: break;
            }
        });
        init(a, b, c);// 初始化
    };
}(jQuery, province));
//$('#consignee_province,#consignee_city,#consignee_county').cityLink(0, 0, 0);

; (function ($, data) {
    var getOption = function (elem, bool) {
        var s, t;
        if (bool) {
            elem.children().each(function (i, e) {
                s = e.selected;
                if (s == true) {
                    t = $(e).html();
                    return;
                }
            });
        } else {
            elem.children().each(function (i, e) {
                s = e.selected;
                if (s == true) {
                    t = $(e).val();
                    return;
                }
            });
        }
        return t;
    };
    $('#saveConsigneeTitleDiv').bind('click', function () {
        var a = getOption($('#consignee_province'), 0),
            b = getOption($('#consignee_city'), 0),
            c = getOption($('#consignee_county'), 0),
            province = getOption($('#consignee_province'), 1),
            city = getOption($('#consignee_city'), 1),
            county = getOption($('#consignee_county'), 1),
            value = a + ',' + b + ',' + c,
            str = province + ' ' + city + ' ' + county,
            indexId = $('input[type="radio"][name="address"]:checked').val();



        if ($('#addressEditArea').css('display') == 'none') {
            var selectedAddress = shippingAddress[indexId];

            var newSelectedText = selectedAddress.shipTo + ' &nbsp;&nbsp;&nbsp; ' + selectedAddress.phone + ' &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<br />' + selectedAddress.fullRegionName + ' &nbsp; &nbsp;' + selectedAddress.address + '&nbsp;';
            $('#shippingAddressId').val(indexId);
            $('#addressEditArea').hide();
            $('#selectedAddress').html(newSelectedText);
            $('#addressListArea').hide();
            $('#consignee-list').hide();
            $('#step-1').removeClass('step-current');

            CalcFright();
        }
        else {
            var shipTo = $('input[name="shipTo"]').val();
            var phone = $('input[name="phone"]').val();
            var address = $('input[type="text"][name="address"]').val();
            var regionId = $('#consignee_county').val();

            if (!shipTo) {
                $.dialog.tips('请填写收货人');
                return false;
            }
            else if (!phone) {
                $.dialog.tips('请填写电话');
                return false;
            }
            else if (!regionId || regionId == "0") {
                $.dialog.tips('请填选择所在地区');
                return false;
            }

            else if (!address) {
                $.dialog.tips('请填写详细地址');
                return false;
            }
            else {
                saveAddress(indexId, regionId, shipTo, address, phone, function (result) {

                    var newSelectedText = shipTo + ' &nbsp;&nbsp;&nbsp; ' + phone + ' &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<br />' + str + ' &nbsp; &nbsp;' + address + '&nbsp;';
                    $('#selectedAddress').html(newSelectedText);

                    indexId = isNaN(parseInt(indexId)) ? '' : parseInt(indexId);
                    if (indexId == 0) {
                        indexId = result.id;
                        shippingAddress[indexId] = {};
                    }

                    $('#shippingAddressId').val(indexId);
                    $('#addressEditArea').hide();
                    $('#selectedAddress').html(newSelectedText);
                    $('#addressListArea').hide();
                    $('#consignee-list').hide();
                    $('#step-1').removeClass('step-current');
                    if (shippingAddress) {
                        shippingAddress[indexId].fullRegionIdPath = value;
                        shippingAddress[indexId].fullRegionName = str;
                    }

                    CalcFright();

                });
            }

        }
    });
}(jQuery, province));