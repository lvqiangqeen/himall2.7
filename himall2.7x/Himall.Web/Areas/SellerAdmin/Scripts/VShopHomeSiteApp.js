// JavaScript source code


$('#addBtn').click(function () {
    $.post('/SellerAdmin/mobileHomeProducts/GetAllHomeProductIds', { platformType: 1 }, function (data) {
        $.productSelector.show(data, function (selectedProducts) {
            var ids = [];
            $.each(selectedProducts, function () {
                ids.push(this.id);
            });
            var loading = showLoading();
            $.post('/SellerAdmin/mobileHomeProducts/AddHomeProducts', { productIds: ids.toString(), platformType: 1 }, function (data) {
                loading.close();
                if (data.success)
                    $("#list").hiMallDatagrid('reload', {});
                else
                    $.dialog.success(data.msg);
            });
        }, 'selleradmin');
    });
});



$('#searchBtn').click(function () {
    var productName = $('#productName').val();
    var categoryId = $('#categoryId').val();
    $("#list").hiMallDatagrid('reload', { brandName: productName, productName: productName, categoryId: categoryId });
});
$('#btnSerachCoupon').click(function () {
    $("#tableCouponList").hiMallDatagrid({
        url: 'GetCouponList',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的优惠券',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "Id",
        pageSize: 5,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams: { couponName: $('#inputCouponName').val() },
        operationButtons: "#saleOff",
        columns:
        [[
            {
                field: "Select", title: '选择', width: 39, formatter: function (value, row, index) {
                    var html = "<input name=\"select\" cpid=\"" + row.Id + "\" type=\"checkbox\">";
                    if (row.IsSelect) {
                        html = "<input name=\"select\" cpid=\"" + row.Id + "\" type=\"checkbox\" checked=\"checked\">"
                    }
                    return html;
                }
            },
            { field: "CouponName", title: '优惠券名称', width: 70 },
            { field: "Price", title: "价值", width: 40, align: "center" },
            { field: "OrderAmount", title: "使用条件", width: 50, align: "center" },
            { field: "PerMax", title: "每人限领", width: 60, align: "center" },
            {
                field: "DateTime", title: "有效期", width: 90, align: "center"
                , formatter: function (value, row, index) {
                    var html = row.StartTime + '至' + row.EndTime;
                    return html;
                }
            }
        ]],
        onLoadSuccess: function () {

        }
    });
});
$('#btnSubmit').click(function () {
    var cpids = '', selectValues = '';
    $('input[name="select"]').each(function (idx, el) {
        var checked = $(el).attr('checked') || '';
        if (checked == 'checked') {
            selectValues = [selectValues, '1'].join(',');
        }
        else {
            selectValues = [selectValues, '0'].join(',');
        }
        cpids = [cpids, $(el).attr('cpid')].join(',');
    });
    if (cpids != '') {
        cpids = cpids.substring(1);
        selectValues = selectValues.substring(1);
    }
    else {
        return;
    }
    var loading = showLoading();
    $.ajax({
        url: 'SaveGouponSetting',
        data: { ids: cpids, values: selectValues },
        type: 'get',
        success: function (result) {
            loading.close();
            if (result.success) {
                $.dialog.alert('提交成功！');
            }
        }
    });
});

function initCouponList() {
    $('#btnSerachCoupon').click();
}


$('#category1,#category2').himallLinkage({
    url: '../category/getCategory',
    enableDefaultItem: true,
    defaultItemsText: '全部',
    onChange: function (level, value, text) {
        $('#categoryId').val(value);
    }
});


$('#list').on('blur', 'input[name="sequence"]', function () {
    var id = $(this).attr('hpId');
    var sequence = $(this).val();
    var sequence = parseInt(sequence);
    if (isNaN(sequence)) {
        $.dialog.errorTips('数字格式不正确');
        $(this).val($(this).attr('orivalue'));
    }
    else {
        $.post('/SellerAdmin/MobileHomeProducts/UpdateSequence', { id: id, sequence: sequence }, function (data) {
            if (data.success) {
                $(this).attr('orivalue', sequence);
                // $.dialog.tips('更新显示顺序成功');
            }
            else
                $.dialog.errorTips('更新显示顺序失败！' + result.msg);
        });
    }
});

function initHomeProduct() {
    $("#list").hiMallDatagrid({
        url: '/SellerAdmin/mobileHomeProducts/GetMobileHomeProducts',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "id",
        pageSize: 8,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams: { platformType: 1 },
        columns:
        [[
             {
                 field: "name", title: '商品', width: 300, align: "center",
                 formatter: function (value, row, index) {
                     var html = '<img style="margin-left:15px;" width="40" height="40" src="' + row.image + '" /><span class="overflow-ellipsis" style="width:200px">' + value + '</span>';
                     return html;
                 }
             },
            {
                field: "brand", title: '品牌', align: "center"
            },
            {
                field: "sequence", title: '排序', align: "center", formatter: function (value, row, index) {
                    return '<input class="text-order" type="text" orivalue="' + value + '" hpId="' + row.id + '" name="sequence" value="' + value + '">';
                }
            },
            {
                field: "categoryName", title: '商品分类', align: "center"
            },
            {
                field: "s", title: "操作", width: 90, align: "center",
                formatter: function (value, row, index) {
                    var html = "";
                    html += '<span class="btn-a"><a class="good-check" onclick="del(' + row.id + ')">删除</a>';
                    html += '</span>';
                    return html;
                }
            }
        ]]
    });
}


function del(id) {
    $.dialog.confirm('确定要从首页删除该商品吗?', function () {
        var loading = showLoading();
        $.post('/SellerAdmin/MobileHomeProducts/Delete', { id: id }, function (result) {
            loading.close();
            if (result.success) {
                $("#list").hiMallDatagrid('reload', {});
            }
            else
                $.dialog.errorTips('删除失败！' + result.msg);
        });
    });
};
$(function () {
    $('.page-tab-hd li').click(function () {
        var top = $(this).position().top;
        var h = $(this).height();
        var ind = $(this).index();
        console.log(ind);
        switch (ind) {
            case 0:
                initSlideImagesTable();
                break;
                //case 2:
                //优惠券设置
                //     initCouponList();
                //     break;
            //case 2:
            //    initVShopBannerTable();
            //    break;
            case 1:
                initHomeProduct();
                break;
        }
        $('.page-tab-bd').css('marginTop', top).show().children().eq($(this).index()).show().siblings().hide();

        $('.arrow').css('top', top + h / 2 - 5).show();
    });
});