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
        var loading = showLoading();

        var giftid = $('#giftid').val();
        var count = $('#count').val();
        var recieveAddressId = $('#shippingAddressId').val();

        recieveAddressId = parseInt(recieveAddressId);
        recieveAddressId = isNaN(recieveAddressId) ? null : recieveAddressId;

        if (!recieveAddressId) {
            loading.close();
            $.dialog.tips('请选择或新建收货地址');
        } else {
            //提交订单
            $.post('/GiftOrder/SubmitOrder', { id: giftid, regionId: recieveAddressId, count: count }, function (result) {
                if (result.success) {
                    location.href = '/GiftOrder/OrderSuccess/' + result.msg;
                }
                else {
                    $.dialog.errorTips(result.msg);
                }
                loading.close();
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
    //arr[2] = arr[2] || '';
    var fullRegionName = address == null ? '<i></i><em></em>' : '<i>' + arr[0] + ' </i><em>' + arr[1] + ' </em>' + arr[2] + '';

    $('input[name="shipTo"]').val(shipTo);
    $('input[type="text"][name="address"]').val(addressName);
    $('input[name="phone"]').val(phone);
    $('span[name="regionFullName"]').html('');
    $('span[name="regionFullName"]').html(fullRegionName);

    $('#addressEditArea').show();
    if (id === 0) {
        //$('#consignee_province').val(0);
        //$('#consignee_city').val(0);
        //$('#consignee_county').val(0);
        $('#regionSelector select').find('option:first').prop('selected', true);
        $('#NewAddressId').attr('level', 0);
        return;
    }
    else
    {
        $('#NewAddressId').attr('level', 4);
    }

    var regionPath = address.fullRegionIdPath.split(',');
    //$('#consignee_province').val(regionPath[0]);
    //$('#consignee_province').trigger('change');
    //$('#consignee_city').val(regionPath[1]);
    //$('#consignee_city').trigger('change');
    $("#NewAddressId").val(regionPath[regionPath.length - 1]);
    $("#regionSelector").RegionSelector({
        valueHidden: "#NewAddressId"
    });
    //if (regionPath.length == 3) {
    //    $('#consignee_county').val(regionPath[2]);
    //    $('#consignee_county').trigger('change');
    //}
}


  



$(function () {

    $('#saveConsigneeTitleDiv').bind('click', function () {
        //var a = getOption($('#consignee_province'), 0),
        //    b = getOption($('#consignee_city'), 0),
        //    c = getOption($('#consignee_county'), 0),
        //    province = getOption($('#consignee_province'), 1),
        //    city = getOption($('#consignee_city'), 1),
        //    county = getOption($('#consignee_county'), 1)|| '',
        //    value = a + ',' + b + ',' + c,
        //  str = province + ' ' + city + ' ' + county,
        indexId = $('input[type="radio"][name="address"]:checked').val();


        if ($('#addressEditArea').css('display') == 'none') {
            var selectedAddress = shippingAddress[indexId];

            var newSelectedText = selectedAddress.shipTo + ' &nbsp;&nbsp;&nbsp; ' + selectedAddress.phone + ' &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<br />' + selectedAddress.fullRegionName + ' &nbsp; &nbsp;' + selectedAddress.address + '&nbsp;';
            $('#shippingAddressId').val(indexId).change();
            $('#addressEditArea').hide();
            $('#selectedAddress').html(newSelectedText);
            $('#addressListArea').hide();
            $('#consignee-list').hide();
            $('#step-1').removeClass('step-current');
            $('#shippingAddressRegionId').val(selectedAddress.regionId).change();
            var regionId = $("#shippingAddressId").val();
            var giftid = $('#giftid').val();
            var count = $('#count').val();

            window.location.href = "/GiftOrder/SubmitOrder/" + giftid + "?count=" + count + "&regionId=" + regionId;
        }
        else {
            var shipTo = $('input[name="shipTo"]').val();
            var regTel = /([\d]{11}$)|(^0[\d]{2,3}-?[\d]{7,8}$)/;
            var phone = $('input[name="phone"]').val();
            var address = $('input[type="text"][name="address"]').val();
            var regionId = $('#consignee_county').val();

            if (!shipTo) {
                $.dialog.tips('请填写收货人');
                return false;
            }
            else if ($.trim(shipTo).length == 0) {
                $.dialog.tips('请填写收货人');
                return false;
            }
            else if (!phone) {
                $.dialog.tips('请填写电话');
                return false;
            }
            else if (!regTel.test(phone)) {
                $.dialog.tips('请填写正确的电话');
                return false;
            }
            else //RegionBind.js
                if ($("#NewAddressId").val() <= 0 || $("#NewAddressId").attr("isfinal") != "true") {
                    $.dialog.tips('请填选择所在地区');
                    return false;
                }
                else if (!address) {
                    $.dialog.tips('请填写详细地址');
                    return false;
                }
                else if ($.trim(address).length == 0) {
                    $.dialog.tips('请填写详细地址');
                    return false;
                }
                else {
                    regionId = $("#NewAddressId").val()
                    saveAddress(indexId, regionId, shipTo, address, phone, function (result) {
                        var newSelectedText = shipTo + ' &nbsp;&nbsp;&nbsp; ' + phone + ' &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<br />' + $('#areaName .selected-address').text() + ' &nbsp; &nbsp;' + address + '&nbsp;';
                      //  $('#selectedAddress').html(newSelectedText);

                        indexId = isNaN(parseInt(indexId)) ? '' : parseInt(indexId);
                        if (indexId == 0) {
                            indexId = result.id;
                            shippingAddress[indexId] = {};
                        }

                        $('#shippingAddressId').val(indexId).change();;
                        $('#addressEditArea').hide();
                        $('#selectedAddress').html(newSelectedText);
                        $('#addressListArea').hide();
                        $('#consignee-list').hide();
                        $('#step-1').removeClass('step-current');
                        //if (shippingAddress) {
                        //    shippingAddress[indexId].fullRegionIdPath = value;
                        //    shippingAddress[indexId].fullRegionName = str;
                        //}
                        $('#shippingAddressRegionId').val(regionId).change();
                        var regionId = $("#shippingAddressId").val();
                        var giftid = $('#giftid').val();
                        var count = $('#count').val();

                        window.location.href = "/GiftOrder/SubmitOrder/" + giftid + "?count=" + count + "&regionId=" + regionId;

                    });
                }

        }
    });

    // 重新选择select修改选择地址
    $("#regionSelector").on('change', 'select', function () {
        var selects = $("#regionSelector select"),
            str = '';
        for (var i = 0; i < selects.length; i++) {
            var emStr = selects[i].options[selects[i].options.selectedIndex].text;
            if (emStr == '请选择') {
                str += '';
            } else {
                str += '<em>' + emStr + ' </em> ';
            }
        }
        $('#areaName span').html(str);
    });
    $("#regionSelector").RegionSelector({
        valueHidden: "#NewAddressId"
    });
});