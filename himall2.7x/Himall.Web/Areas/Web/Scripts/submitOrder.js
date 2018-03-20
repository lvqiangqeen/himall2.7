var shippingAddressIdChange = false;
var selectShopBranch = {};
$(function () {
    bindRecieverEdit();
    bindSubmit();
    initAddress();
    bindAddressRadioClick();
    InvoiceInit();
    InvoiceOperationInit();

    $('#shippingAddressRegionId').change(function () {
        shippingAddressIdChange = true;

        disableAllSelfTake();

        var regionId = $('#shippingAddressRegionId').val();
        if ($.isNullOrEmpty(regionId))
            return;
        var loading = showLoading();
        $('label.deliverytype').each(function () {
            var btn = $(this);
            var shopId = btn.closest('.ordsumbox').data('shopid');
            var pids = btn.data('pids').toString().split(',');

            $.ajax({
                url: '/shop/ExistShopBranch?shopId={0}&regionId={1}&productIds={2}'.format(shopId, regionId, pids.join('&productIds=')),
                context: { element: btn },
                async: false,
                success: function (result) {
                    if (result == true)
                        enableSelfTake(this.element);
                }
            });
        });

        $.get('/order/IsCashOnDelivery?addreddId=' + regionId, function (result) {
            loading.close();
            if (!result) {
                disableCashOnDelivery();
            } else {
                disableCashOnDelivery(false);
            }
        }).error(function () {
            disableCashOnDelivery();
            loading.close();
        });

        refreshFreight(regionId);
    });

    $("input[name=pay-offline]").change(function () {
        if ($(this).val() == "1") {
            $(".payment-dialog").hide();
            $(".sticky-wrap .offline-icon").html("在线支付");

            disableAllSelfTake(false);
        }
        else {
            $(".payment-dialog").show();
            $(".id_alpha").addClass("alpha");
            $("#submit").attr("disabled", "");
        }
    })

    $(".pd-submit").click(function () {
        $(".payment-dialog").hide();
        $("input[name=pay-offline]:eq(0)").attr("checked", "checked");
        $(".id_alpha").hide();
        $("#submit").removeAttr("disabled");
    })

    $(".pd-commit").click(function () {
        $(".sticky-wrap .offline-icon").html("货到付款")
        $(".payment-dialog").hide();
        $(".id_alpha").hide();
        $("#submit").removeAttr("disabled");

        var selfShopId = 1;
        disableAllSelfTake(true, selfShopId);
        $('#orderdata_{0} label.deliverytype'.format(selfShopId)).each(function () {
            $(this).parent().find('.tip').addClass('hide');
        });
    })

    $(document).on('click', '.thickclose', function () {
        $('.thickbox,.thickdiv').hide();

        var shopbranchDialog = $(this).closest('.shopbranch.dialog');
        if (shopbranchDialog.length > 0) {
            var sourceElement = $(shopbranchDialog.get(0).sourceElement);
            var shopId = sourceElement.closest('.ordsumbox').data('shopid');
            if (selectShopBranch[shopId] == null) {
                var ckb = sourceElement.prev().children();
                ckb.get(0).checked = true;
                ckb.change();
            }
        }

        return false;
    });

    var freight = $('#modelFreight').val();
    var intRule = $('#MoneyPerIntegral').data('rule');

    $("#integral").bind("keyup", function () {
        ShowPaidPrice();
    });

    $("#IsUsedIntegral").change(function () {
        if (this.checked) {
            $(this).siblings().show().end().parent().siblings().show();
        } else {
            $(this).siblings().hide().end().parent().siblings().hide();
            $('#integral').val(0);
            $("#integral").trigger("keyup");
        }
    });

    $("#MoneyPerIntegral").html();

    var price = function () {
        var a = 0;
        $('.shopb').each(function (i, e) {
            $(e).children().each(function (l, k) {
                if ($(k).attr('selected')) {
                    var b = $(k).attr('data-p');
                    a += (b | 0);
                }
            });
        });
        return a;
    },
    total = $('#payPriceId').attr('data');

    //优惠切换绑定
    $('.shopb').bind('change', function (t) {
        var _t = $(this);
        var shopid = _t.data("shopid");
        ComputeOrder(shopid);
    });

    //下面两个调用需同步执行
    //优惠初始
    $('.shopb').each(function (i, e) {
        var _t = $(e);
        _t.find("option").eq(1).attr("selected", true);
    });

    //初始计算
    $(".ordsumbox").each(function (i, e) {
        var _t = $(e);
        var shopid = _t.data("shopid")
        ComputeOrder(shopid);
    });

    $('label.deliverytype input').change(function () {
        if (this.checked) {
            $(this).parent().siblings('.selectStore').show();
        }
    });

    $('input.express').change(function (e) {
        if (this.checked) {
            $(this).parent().siblings('.selectStore').hide();
            var shopId = $(this).closest('.ordsumbox').data('shopid');
            $('.shopbranchname[shopid={0}]'.format(shopId)).html('').parent().hide();
            $('.shopbranchaddress[shopid={0}]'.format(shopId)).html('').parent().hide();
            selectShopBranch[shopId] = null;
            ComputeOrder(shopId);
            if (e && shopId == 1)
                $('.payment-selected input.CashOnDelivery[name="pay-offline"]').parent().show();
        }
    });

    $('.selectStore').click(function () {
        var regionId = $('#shippingAddressRegionId').val();

        if ($.isNullOrEmpty(regionId)) {
            $.dialog.tips('请选择或新建收货地址');
            return;
        }

        var btn = $(this).siblings('label.deliverytype');
        if (btn.is('[disabled]'))
            return;
        $('.thickdiv').show();
        var shopId = btn.closest('.ordsumbox').data('shopid');
        var dialog = $('.shopbranch.dialog#' + shopId).show();
        var dialogElement = dialog.get(0);
        if (dialogElement.isInit != true || shippingAddressIdChange == true) {
            shippingAddressIdChange = false;
            var skuIds = btn.attr('skuIds').split(',');
            var counts = btn.attr('counts').split(',');
            var width = dialog.width() / 2;
            var height = dialog.height() / 2;
            dialog.css({ top: '40%', left: '50%', marginLeft: '-' + width + 'px', marginTop: '-' + height + 'px', position: 'fixed' });
            var pid = dialog.data('pid');
            dialogElement.isInit = true;
            dialogElement.sourceElement = this;
            dialogElement.source = [];

            $.get('/shop/GetCitySubRegions?regionId=' + regionId, function (data) {

                if (data && data.regions && data.regions.length > 0) {
                    var str = '<option value="">所有区域</option>';
                    for (var i = 0; i < data.regions.length; i++) {
                        str += '<option value="' + data.regions[i].id + '" ' + (data.regions[i].id == data.selectId ? 'selected' : '') + '>' + data.regions[i].name + '</option>';
                    }
                    $('.districtSelect', dialog).html(str).change(function () {
                        if (!this.value) {
                            $('.list', dialog).hiMallDatagrid('reload', { regionId: regionId, getParent: true });
                        } else {
                            $('.list', dialog).hiMallDatagrid('reload', { regionId: this.value, getParent: false });
                        }
                    });
                }
            });

            $(".list", dialog).hiMallDatagrid({
                url: '/m-wap/order/GetShopBranchs',
                NoDataMsg: '该地区没有可自提门店',
                pagination: false,
                idField: "id",
                queryParams: { shopId: shopId, regionId: regionId, getParent: false, skuIds: skuIds, counts: counts, shippingAddressId: $("#shippingAddressId").val() },
                columns:
				[[
					{
					    width: 57,
					    formatter: function (value, row, index) {
					        var html = '<input id="shopbranch{0}" name="shopBranchId" type="radio" value="{0}" onchange="onSelectShopBranch(this)"';
					        if (row.enabled == false) {
					            html += ' disabled ';
					        }
					        html += "/>";
					        dialogElement.source.push(row);
					        return html.format(row.id);
					    }
					},
					{
					    field: "shopBranchName",
					    formatter: function (value, row, index) {
					        return '<div style="margin-bottom:15px; font-size:15px; color:#000;">{0}{5}</div><div style="margin-bottom:10px;">自提地址：{1}</div><div style="margin-bottom:30px">联系人：{2} 联系方式{3}</div>'
								.format(row.shopBranchName, row.addressDetail, row.contactUser, row.contactPhone, row.id, row.enabled == false ? '<label style="color: #fff; background: #f8413d;padding: 0 10px; border-radius: 2px;margin-left:5px;">!该门店缺货</label>' : '');
					    }
					}
				]]
            });
        } else {
            var checkRadio = $('input:radio[name=shopBranchId]:checked', dialog);
            if (checkRadio.length > 0) {
                var data = dialogElement.source.first(function (p) { return p.id == checkRadio.val(); });
                $('.shopbranchname[shopid={0}]'.format(shopId)).html(data.shopBranchName);
                $('.shopbranchaddress[shopid={0}]'.format(shopId)).html('{0} {1} {2}'.format(data.addressDetail, data.contactUser, data.contactPhone));
                freeFreight(shopId);
            }
        }

        $('.shopbranchname[shopid={0}]'.format(shopId)).parent().show();
        $('.shopbranchaddress[shopid={0}]'.format(shopId)).parent().show();
    });
});

function getCount() {
    var result = [];
    $('.order-table[shopid]').each(function () {
        var shopId = $(this).attr('shopid');
        $('tr[pid][quntity]', this).each(function () {
            var pid = $(this).attr('pid');
            var count = $(this).attr('quntity');
            result.push({ shopId: shopId, productId: pid, count: count });
        });
    });

    return result;
}

//刷新运费
function refreshFreight(regionId) {
    //获取运费
    var data = getCount();
    $.post('/order/CalcFreight', { parameters: data, addressId: regionId }, function (result) {
        if (result.success == true) {
            for (var i = 0; i < result.freights.length; i++) {
                var item = result.freights[i];
                var shopId = item.shopId;
                var freight = item.freight;

                var shopDiv = $('#orderdata_' + shopId);
                var amount = parseFloat($('.shopd', shopDiv).data('v'));
                var freeFreight = parseFloat($('.shopf', shopDiv).data('free'));
                if (freeFreight <= 0 || amount < freeFreight) {
                    $('.shopd', shopDiv).attr('data', amount + freight).html('￥' + (amount + freight).toFixed(2));
                    shopDiv.find('.shopf').attr('data', freight).data('profrei', freight).html('￥' + freight.toFixed(2));
                }
            }
            ShowPaidPrice();
        } else
            $.dialog.errorTips(result.msg);
    });
}

//禁用货到付款
function disableCashOnDelivery(flag) {
    $('.payment-selected input[name="pay-offline"]')[0].checked = true;
    var lbl = $('.payment-selected input.CashOnDelivery[name="pay-offline"]').parent();
    if (flag != false) {
        lbl.hide();
    } else if ($('#orderdata_1').length > 0)//判断是否有平台店
        lbl.show();
}

//启用到店自提
function enableSelfTake(element) {
    element.removeAttr('disabled');
    element.children().removeAttr('disabled');
    element.parent().find('.tip').addClass('hide');
    element.parent().find('.tip.have').removeClass('hide');
}

//禁用所有门店到店自提
function disableAllSelfTake(flag, shopId) {
    var filter = '';
    if (shopId)
        filter = '#orderdata_{0} '.format(shopId);

    $(filter + '.express[name$=".DeliveryType"]').each(function () {
        this.checked = true;
        selectShopBranch = {};
        $(this).change();
    });
    $(filter + 'label.deliverytype').each(function () {
        var element = $(this);
        if (flag != false) {
            element.attr('disabled', 'disabled');
            element.children().attr('disabled', 'disabled');
            element.parent().find('.tip').removeClass('hide');
            element.parent().find('.tip.have').addClass('hide');
        } else {
            enableSelfTake(element);
        }
    });
}

function freeFreight(shopId) {
    var d_box = $("#orderdata_" + shopId);
    var d_freight = d_box.find(".shopf");
    var freight = d_freight.data('profrei');
    d_freight.attr('data', 0).html('免运费');
    var shopd = d_box.find('.shopd');
    shopd.html('￥' + (parseFloat(shopd.attr('data')) - parseFloat(freight)).toFixed(2));
    ShowPaidPrice();
}

function onSelectShopBranch(input) {
    input = $(input);
    var shopId = input.closest('.shopbranch.dialog').hide().attr('id');
    var id = input.val();
    $('.thickdiv').hide();
    selectShopBranch[shopId] = id;
    var data = input.closest('.shopbranch.dialog').get(0).source.first(function (p) { return p.id == id; });
    $('.shopbranchname[shopid={0}]'.format(shopId)).html(data.shopBranchName);
    $('.shopbranchaddress[shopid={0}]'.format(shopId)).html('{0} {1} {2}'.format(data.addressDetail, data.contactUser, data.contactPhone));
    ComputeOrder(shopId);
    if (shopId == 1)//如果是自营店
        disableCashOnDelivery();
    //选择门店到店自提时，免运费
    freeFreight(shopId);
}

function InvoiceInit() {
    $("#invoceMsg").hide();
    $("input[name='isInvoce']").click(function () {
        var id = $(this).attr("id");
        if (id == "isInvoce1") {
            $("#invoceMsg").hide();
        }
        else if (id == "isInvoce2") {
            var title = $("#dvInvoice .invoice-tit-list .invoice-item-selected input").val();
            var context = $("#dvInvoice .invoice-list .invoice-item-selected span").html();
            $("#invoiceTitle").html(title);
            $("#invoiceContext").html(context);
            $("#invoceMsg").show();

        }
    })

    $("#dvInvoice .invoice-list div:eq(0)").addClass("invoice-item-selected");
    $("#dvInvoice .invoice-list div").click(function () {
        $("#dvInvoice .invoice-list div").removeClass("invoice-item-selected");
        $(this).addClass("invoice-item-selected");
    })

    $("#dvInvoice .invoice-tit-list div").click(function () {
        $("#dvInvoice .invoice-tit-list div").removeClass("invoice-item-selected");
        $(this).addClass("invoice-item-selected");
    })

    $("#btnAddInvoice").click(function () {
        var _t = $(this);
        _t.hide();
        var html = '<div class="invoice-item invoice-item-selected">';
        html += '<input type="text" value="">';
        html += '<div class="item-btns">';
        html += '<a href="javascript:void(0);" class="ml10 update-tit">保存</a>';
        html += '<a href="javascript:void(0);" class="ml10 del-tit hide">删除</a>';
        html += '</div>';
        html += '</div>';

        $("#dvInvoice .invoice-tit-list .invoice-item").removeClass("invoice-item-selected");
        $("#dvInvoice .invoice-tit-list").prepend(html);
        $("#dvInvoice .invoice-tit-list .invoice-item-selected input")[0].focus();

        InvoiceOperationInit();
    })

    $("#btnOk").click(function () {
        var title = $("#dvInvoice .invoice-tit-list .invoice-item-selected input").val();
        var context = $("#dvInvoice .invoice-list .invoice-item-selected span").html();
        if (title == null || context == null) {

        }
        else if (title.length > 0 && context.length > 0 && $.trim(title) != "") {
            $("#invoiceTitle").html(title);
            $("#invoiceContext").html(context);
            $('.thickbox,.thickdiv').hide();
        }
        else {
            $.dialog.tips("请选择发票信息,信息内容不能为空");
        }

    })
}

function InvoiceOperationInit() {
    $("#dvInvoice .invoice-tit-list .del-tit").click(function () {
        var self = this;
        var id = $(self).attr("key");
        $.dialog.confirm("确定删除该发票抬头吗？", function () {
            var loading = showLoading();
            $.post("./DeleteInvoiceTitle", { id: id }, function (result) {
                loading.close();
                if (result == true) {
                    $(self).parents(".invoice-item").remove();
                    $(".invoice-tit-list .invoice-item:eq(0)").addClass("invoice-item-selected");
                    $.dialog.tips('删除成功！');
                }
                else {
                    $.dialog.tips('删除失败！');
                }
            })
        });
    })

    $("#dvInvoice .invoice-tit-list .update-tit").click(function () {
        var self = this;
        var name = $(this).parents(".invoice-item").find("input").val();
        if ($.trim(name) == "") {
            $.dialog.tips('不能为空！');
            return;
        }
        var loading = showLoading();
        $.post("./SaveInvoiceTitle", { name: name }, function (result) {
            loading.close();
            if (result != undefined && result != null && result > 0) {
                $(self).parents(".invoice-item").find(".del-tit").removeClass("hide").attr("key", result);
                $(self).addClass("hide");
                $(self).parents(".invoice-item").find("input").attr("disabled", true);
                $(".invoice-item").removeClass("invoice-item-selected");
                $(self).parents(".invoice-item").addClass("invoice-item-selected");

                $("#dvInvoice .invoice-tit-list div").click(function () {
                    $("#dvInvoice .invoice-tit-list div").removeClass("invoice-item-selected");
                    $(this).addClass("invoice-item-selected");
                })
                InvoiceOperationInit();
                $.dialog.tips('保存成功！');
                $("#btnAddInvoice").show();
            }
            else {
                if (result == -1) {
                    $.dialog.tips('发票抬头不可为空！');
                } else {
                    $.dialog.tips('保存失败！');
                }
            }
        })
    })
}


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
                             <input type="radio" class="hookbox pull-left" name="address" '+ (address.id == currentSelectedId ? 'checked="checked"' : '') + ' value="' + address.id + '" />\
                             <span class="item-text pull-left"><b>' + address.shipTo + '</b>&nbsp; ' + address.fullRegionName + ' &nbsp; ' + address.address + ' &nbsp; ' + address.phone + ' </span>&nbsp\
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
                $('#shippingAddressId').val(shippingAddressId).change();
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
                    $('#shippingAddressId').val('').change();
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
    data.Latitude = $("#Latitude").val();
    data.Longitude = $("#Longitude").val();

    var loading = showLoading();
    $.post(url, data, function (result) {
        loading.close();
        if (result.success)
            callBack(result);
        else
            $.dialog.errorTips('保存失败!' + result.msg);
    });
}

var isSubmitLoading = false;
function bindSubmit() {
    $('#submit').click(function () {
        if (!isSubmitLoading) {
            isSubmitLoading = true;
            var loading = showLoading();
            var fn = function () {
                var arr = [];
                $('.shopb').each(function (i, e) {
                    $(e).children().each(function (l, k) {
                        if ($(k).attr('selected')) {
                            var b = $(k).val();
                            var s = b + "_" + $(k).attr("data-type");
                            arr.push(s);
                        }
                    });
                });
                return arr.join(',');
            };

            //var collIds = $("#collIds").val();
            var collIds =[];
            var couponIds = fn();

            var cartItemIds = QueryString('cartItemIds');
            var recieveAddressId = $('#shippingAddressId').val();

            var orderShops = [], hasError = false;
            $('.order-table[shopid]').each(function () {
                if (hasError == true)
                    return false;

                var shopId = $(this).attr('shopid');
                var orderShop = {};
                orderShop.shopId = shopId;
                orderShop.orderSkus = [];
                $('tbody tr', this).each(function () {
                    orderShop.orderSkus.push({ skuId: $(this).attr('skuid'), count: $(this).attr('quntity') });
                    if ($(this).attr('collpid') > 0) {
                        collIds.push($(this).attr('collpid'));
                    }
                });
                var checkbox = $('input:radio[name="shop{0}.DeliveryType"]:checked'.format(shopId));
                orderShop.deliveryType = checkbox.val();
                orderShop.shopBrandId = selectShopBranch[shopId];
                if (orderShop.shopBrandId == null && checkbox.hasClass('express') == false) {
                    $.dialog.tips('请选择自提门店');
                    hasError = true;
                    return false;
                }

                orderShop.remark = $('.orderRemarks#remark_' + shopId).val();
                if (orderShop.remark.length > 200) {
                    $.dialog.tips('留言信息至多200个汉字！');
                    hasError = true;
                    return false;
                }
                orderShops.push(orderShop);
            });

            if (hasError == true) {
                loading.close();
                isSubmitLoading = false;
                return false;
            }

            var integral = parseInt($("#integral").val());
            integral = isNaN(integral) ? 0 : integral;

            //是否货到付款
            var isCashOnDelivery = false;
            if ($("#icod").val() == "True" && $("input[name=pay-offline]:checked").val() == "2") {
                isCashOnDelivery = true;
            }

            recieveAddressId = parseInt(recieveAddressId);
            recieveAddressId = isNaN(recieveAddressId) ? null : recieveAddressId;

            $('input:radio[name="sex"]').is(":checked")

            var invoiceType = $("input[name='isInvoce']:checked").val();
            //if ($("input[name='invoiceType']").is(":checked"))
            //    invoiceType = $("input[name='invoiceType']:checked").val();

            var invoiceTitle = $("#invoiceTitle").html();
            if (invoiceTitle == null || invoiceTitle == '') {
                invoiceTitle = "";
                //$.dialog.tips( '请选择发票抬头' );
                //return false;
            }
            var invoiceContext = $("#invoiceContext").html();
            if (invoiceContext == null || invoiceContext == '') {
                invoiceContext = "";
                //$.dialog.tips( '请选择发票内容' );
                //return false;
            }

            if (invoiceType == "2") {
                if (invoiceTitle == null || invoiceTitle == '') {
                    $.dialog.tips('请选择发票抬头');
                    loading.close();
                    isSubmitLoading = false;
                    return false;
                }

                if (invoiceContext == null || invoiceContext == '') {
                    $.dialog.tips('请选择发票内容');
                    loading.close();
                    isSubmitLoading = false;
                    return false;
                }
            }

            if (!recieveAddressId) {
                $.dialog.tips('请选择或新建收货地址');
                loading.close();
                isSubmitLoading = false;
            } else {

                $.post('/order/SubmitOrder', {
                    integral: integral, couponIds: couponIds, collpIds: collIds.join(','), cartItemIds: cartItemIds,
                    recieveAddressId: recieveAddressId, invoiceType: invoiceType,
                    invoiceTitle: invoiceTitle, invoiceContext: invoiceContext,
                    isCashOnDelivery: isCashOnDelivery, orderShops: orderShops
                },
					function (result) {
					    if (isCashOnDelivery && $("#onlyshop1").val() == "True") {
					        loading.close();
					        //isSubmitLoading = false;

					        location.replace('/UserCenter?url=/UserOrder&tar=UserOrder');
					    }
					    else if (result.success) {//订单提交成功
					        if (result.orderIds != null && result != undefined) {
					            location.replace('/order/pay?orderIds=' + result.orderIds.toString());
					            loading.close();
					            //isSubmitLoading = false;
					        }
					        else {
					            ///请求次数
					            var requestcount = 0;
					            ///检查订单状态并做处理
					            function checkOrderState() {
					                $.getJSON('/OrderState/Check', { Id: result.Id }, function (r) {
					                    if (r.state == "Processed") {
					                        location.href = '/order/pay?orderIds=' + r.orderIds.toString();
					                    }
					                    else if (r.state == "Untreated") {
					                        requestcount = requestcount + 1;
					                        if (requestcount <= 10)
					                            setTimeout(checkOrderState, 0);
					                        else {
					                            $.dialog.tips("服务器繁忙,请稍后去订单中心查询订单");
					                            loading.close();
					                            isSubmitLoading = false;
					                        }

					                    }
					                    else {
					                        $.dialog.tips('订单提交失败,错误原因:' + r.message);
					                        loading.close();
					                        isSubmitLoading = false;
					                    }
					                });
					            }
					            checkOrderState();
					        }
					    }
					    else {
					        loading.close();
					        isSubmitLoading = false;
					        $.dialog.errorTips(result.msg);
					    }
					});
            }
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
    if (id > 0) {//只有编辑的时候要将经纬度回显
        $("#Latitude").val(address.latitude);
        $("#Longitude").val(address.longitude);
    }
    $('#addressEditArea').show();
    if (id === 0) {
        $('#consignee_province').val(0);
        $('#consignee_city').val(0);
        $('#consignee_county').val(0);
        $('#regionSelector select').find('option:first').prop('selected', true);
        $('#NewAddressId').attr('level', 0);
        return;
    }
    else {
        $('#NewAddressId').attr('level', 4);
    }
    var regionPath = address.fullRegionIdPath.split(',');
    $('#consignee_province').val(regionPath[0]);
    $('#consignee_province').trigger('change');
    $('#consignee_city').val(regionPath[1]);
    $('#consignee_city').trigger('change');
    $("#NewAddressId").val(regionPath[regionPath.length - 1]);
    $("#regionSelector").RegionSelector({
        valueHidden: "#NewAddressId"
    });
    $("#regionSelector select").change(function () {
        $("#consigneeAddress").val("");//每次选择地址后都要将详细地址清空，防止经纬度和地区不匹配
    });
    if (regionPath.length == 3) {
        $('#consignee_county').val(regionPath[2]);
        $('#consignee_county').trigger('change');
    }
}

function invoiceReceipt() {
    var left, top, width, height;
    left = $(window).width() / 2;
    top = $(window).height() / 2;
    var dialog = $('#invoiceDialog');
    dialog.show().removeClass('hide');
    width = dialog.width() / 2;
    height = dialog.height() / 2;
    dialog.css({ top: '50%', left: '50%', marginLeft: '-' + width + 'px', marginTop: '-' + height + 'px', position: 'fixed' }).show();
}
//转换0
function parseFloatOrZero(n) {
    result = 0;
    try {
        if (n.length < 1) n = 0;
        if (isNaN(n)) n = 0;
        if (n != 0) {
            result = parseFloat(n);
        } else {
            result = 0;
        }
    } catch (ex) {
        result = 0;
    }
    return result;
}

//显示实付金额
//By DZY[150706]
function ShowPaidPrice() {
    var d_mpi = $('#MoneyPerIntegral');
    var d_integral = $("#integral");
    //价格初始
    var orderTotalPrice = parseFloatOrZero($("#warePriceId").attr("v"));  //订单总价
    var orderPaidPrice = 0;
    var orderTotalDisPrice = 0;       //订单优惠总价
    var orderTotalFreight = 0;        //订单运费总价
    var orderTotalIntegral = 0;       //订单积分抵扣
    //运费总价
    $(".shopf").each(function (l, k) {
        var _t = $(k);
        orderTotalFreight += parseFloatOrZero(_t.attr("data"));
    });
    //总优惠
    $(".shopc").each(function (l, k) {
        var _t = $(k);
        orderTotalDisPrice += parseFloatOrZero(_t.attr("data"));
    });
    //积分抵扣
    var intRule = parseFloatOrZero(d_integral.data('rule'));   //积分规则
    if (intRule > 0) {
        var useIntegral = parseFloatOrZero(d_integral.val());
        if (useIntegral < 0) {
            useIntegral = 0;
            d_integral.val(useIntegral);
        }
        var canuseint = parseFloatOrZero(d_integral.data("userintegral"));
        if (canuseint < useIntegral) useIntegral = canuseint;
        orderTotalIntegral = (useIntegral / intRule).toFixed(2);
        var _tmpnum = parseInt((orderTotalPrice - orderTotalDisPrice) * 100) / 100;
        if (_tmpnum - orderTotalIntegral < 0) {
            //为零修正
            orderTotalIntegral = _tmpnum;
            useIntegral = Math.floor(orderTotalIntegral * intRule);
            //无法抵价
            if (useIntegral > 0) {
                orderTotalIntegral = (useIntegral / intRule).toFixed(2);
            } else {
                orderTotalIntegral = 0;
                d_integral.val(0);
            }
        }
        //数据补充
        if (useIntegral != 0) {
            d_integral.val(useIntegral);
        } else {
            if (d_integral.val().length > 1) {
                d_integral.val(0);
            }
        }
    } else {
        d_integral.val(0);
    }

    //计算实付
    orderPaidPrice = orderTotalPrice + orderTotalFreight - orderTotalDisPrice - orderTotalIntegral;

    //显示
    var d_ordprice = $("#payPriceId");
    var d_ordFreight = $("#totalFreight");
    var d_ordDisPrice = $("#id_c");
    var d_ordMPI = $("#MoneyPerIntegral");
    var d_orduseInt = $("#integralPrice");
    d_ordprice.attr("v", orderPaidPrice.toFixed(2)).html("￥" + orderPaidPrice.toFixed(2));
    d_ordFreight.html("￥" + orderTotalFreight.toFixed(2));
    d_ordDisPrice.html("￥-" + orderTotalDisPrice.toFixed(2));
    if (!isNaN(orderTotalIntegral)) {
        orderTotalIntegral = parseFloat(orderTotalIntegral);
    }
    d_orduseInt.html("￥-" + orderTotalIntegral.toFixed(2));
    d_ordDisPrice.parent().hide();
    if (orderTotalDisPrice > 0) {
        d_ordDisPrice.parent().show();
    }
    //可获得积分
    var mpirule = parseFloatOrZero(d_ordMPI.data("rule"));
    if ((orderPaidPrice - orderTotalFreight > 0) && mpirule > 0) {
        d_ordMPI.text(Math.floor((orderPaidPrice - orderTotalFreight) / mpirule));
    }
    else {
        d_ordMPI.text(0);
    }
}

//计算订单结果
//By DZY[150707]
function ComputeOrder(shopid) {
    var d_box = $("#orderdata_" + shopid);
    var d_dissel = d_box.find(".shopb");
    var d_freight = d_box.find(".shopf");
    var d_proPrice = d_box.find(".shopd");
    var d_showdis = d_box.find(".shopc");
    var ordSumProPrice = 0, ordSubDisPrice = 0; ordSubFreight = 0, shopFreeFrei = 0;
    ordSumProPrice = parseFloatOrZero(d_proPrice.data("v"));
    shopFreeFrei = parseFloatOrZero(d_freight.data("free"));
    ordSubFreight = parseFloatOrZero(d_freight.data("profrei"));
    if (d_dissel.length > 0) {
        var d_selopt = d_dissel.find("option:selected");
        ordSubDisPrice = parseFloatOrZero(d_selopt.data("p"));
        if (ordSubDisPrice > ordSumProPrice) {
            ordSubDisPrice = ordSumProPrice;
        }
    }
    //计算满额免
    var isFullFreeFrei = false;
    if (shopFreeFrei > 0) {
        if (ordSumProPrice - ordSubDisPrice >= shopFreeFrei) {
            ordSubFreight = 0;
            isFullFreeFrei = true;
        }
    }

    //计算实付
    var ordPaidPrice = ordSumProPrice - ordSubDisPrice + ordSubFreight;

    //显示单条
    d_proPrice.html("￥" + ordPaidPrice.toFixed(2));
    d_freight.html("￥" + ordSubFreight).attr("data", ordSubFreight);
    if (isFullFreeFrei) {
        d_freight.html("免运费");
    }
    d_showdis.html("￥-" + ordSubDisPrice).attr("data", ordSubDisPrice);
    //d_showdis.hide();
    if (parseFloat(ordSubDisPrice) == 0) {
        d_showdis.html("");
    }
    //显示计算结果
    ShowPaidPrice();
}

//绑定地址
$(function () {
    $('#saveConsigneeTitleDiv').click(function () {
        var indexId = $('input[type="radio"][name="address"]:checked').val();

        if ($('#addressEditArea').css('display') == 'none') {
            var selectedAddress = shippingAddress[indexId];

            var newSelectedText = '<span>' + selectedAddress.shipTo + '</span> ' + '<br />' + selectedAddress.fullRegionName + ' &nbsp; &nbsp;' + selectedAddress.address + '</br>' + selectedAddress.phone;
            $('#shippingAddressId').val(indexId).change();
            $('input[type="hidden"][name="ShippingAddressId"]').val(indexId);
            $('#addressEditArea').hide();
            $('#selectedAddress').html(newSelectedText);
            $('#addressListArea').hide();
            $('#consignee-list').hide();
            $('#step-1').removeClass('step-current');
            $('#shippingAddressRegionId').val(selectedAddress.regionId).change();
            //CalcFreight(indexId);
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
                } else if (!address) {
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
                        indexId = isNaN(parseInt(indexId)) ? '' : parseInt(indexId);
                        if (indexId == 0) {
                            indexId = result.id;
                            shippingAddress[indexId] = {};
                        }

                        $('#shippingAddressId').val(indexId).change();
                        $('input[type="hidden"][name="ShippingAddressId"]').val(indexId);
                        $('#addressEditArea').hide();
                        $('#selectedAddress').html(newSelectedText);
                        $('#addressListArea').hide();
                        $('#consignee-list').hide();
                        $('#step-1').removeClass('step-current');
                        $('#shippingAddressRegionId').val(regionId).change();
                        $("#Latitude").val(''); $("#Longitude").val('');//保存成功后将经纬度清空
                        //CalcFreight(indexId);

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
        $("#consigneeAddress").val("");//每次选择地址后都要将详细地址清空，防止经纬度和地区不匹配
    });

    $("#regionSelector").RegionSelector({
        valueHidden: "#NewAddressId"
    });
});