
$(function () {
    $("#txtName").val(name);
    bindCity();
    buildOrder();
})

function bindCity() {
    $('#consignee_province,#consignee_city,#consignee_county').cityLink(0, 0, 0);
    if (provinceId > 0) {
        $("#consignee_province option").each(function () {
            var value = parseInt($(this).attr("value"));
            if (value == provinceId) {
                $("#consignee_province").val(value);
                $("#consignee_province").trigger("change");
            }
        });
    }
    if (cityId > 0) {
        $("#consignee_city").val(cityId);
        $("#consignee_city").trigger("change");
    }

    if (countyId > 0) {
        $("#consignee_county").val(countyId);
        $("#consignee_county").trigger("change");
    }
}

function buildOrder() {
    $("[name=OrderNumbers]").each(function () {
        var text = $("[name=hidOrderId]", this).val();

        var orders = text.split(",");

        var parent = $(this);
        $.each(orders, function (index, orderId) {
            var orderA = '<a href="/UserOrder/detail/' + orderId + '">' + orderId + ' </a></br>';
            $(parent).append(orderA);
        })
    })
}

function updateSupplierShop(shopId, shopName, ele, status) {
    var eleText = $(ele).text();
    $.dialog.confirm('确定' + eleText + shopName + '吗？', function () {
        var loading = showLoading();
        ajaxRequest({
            type: "POST",
            url: '/UserCenter/UpdateSupplierManagemen',
            param: { shopId: shopId, status: status },
            dataType: "json",
            success: function (data) {
                loading.close();
                if (data.success) {
                    $.dialog.succeedTips('操作成功！',function(){ location.href = "/UserCenter/SupplierManagement";});
                } else {
                    $.dialog.errorTips("操作失败,请重试！", _this);
                }
            },
            error: function (e) {
                loading.close();
                $.dialog.errorTips("操作失败,请重试！", _this);
            }
        });
    });
}

function Query() {
    var name = $("#name").val();
    var consignee_province = $("#consignee_province").val();
    var consignee_city = $("#consignee_city").val();
    var consignee_county = $("#consignee_county").val();
    window.location.href = "?name=" + name + "&consignee_province=" + consignee_province + "&consignee_city=" + consignee_city + " &consignee_county=" + consignee_county;
}


function getRegionIds(regionId) {
    var regionIds = "" + regionId;
    var flag = false;
    $.map(province, function (shengfen) {

        if (shengfen.id == regionId) {

            flag = true;
            $.map(shengfen.city, function (city) {
                regionIds += "," + city.id;
                $.map(city.county, function (county) {
                    regionIds += "," + county.id;
                })
            })
        }
    })
    if (!flag) {
        $.map(province, function (shengfen) {
            $.map(shengfen.city, function (city) {
                if (city.id == regionId) {
                    $.map(city.county, function (county) {
                        regionIds += "," + county.id;
                    })
                }


            })
        });
    }
    return regionIds;
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