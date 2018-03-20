var loading;
var categoryId = 0;
var categoryPatha = 0;
var categoryPathb = 0;
var categoryPathc = 0;
$(function () {
    LoadData();

    bindSearchBtnClick();

    $('#category1,#category2,#category3').himallLinkage({
        url: '/SellerAdmin/Category/GetSystemCategory',
        enableDefaultItem: true,
        defaultItemsText: '全部',
        onChange: function (level, value, text) {
            categoryId = value;
            if (level == 0)
                categoryPatha = value;
            if (level == 1)
                categoryPathb = value;
            if (level == 2)
                categoryPathc = value;
        }
    });

    $("#addBtn").click(function () {
        loading = showLoading();
        $.post('./GetAllProductIds', null, function (data) {
            loading.close();
            $.productSelector.show(data, function (selectedProducts) {
                var ids = [];
                $.each(selectedProducts, function () {
                    ids.push(this.id);
                });
                loading = showLoading();
                $.post('./AddProducts', { ids: ids.toString(), platformType: 1 }, function (data) {
                    loading.close();
                    if (data.success) {
                        //$("#list").hiMallDatagrid('reload', {});
                        reload(1);
                    }
                    else {
                        $.dialog.alert('添加分销推广商品失败!' + data.msg);
                    }
                });
            }, 'selleradmin');
        });
    });

    CancelEventBind();
    RateTextEventBind();

});

function CancelEventBind() {
    $('#list').on('click', '.btnCancel', function () {
        var name = $(this).siblings('.thisName').val();
        var ids = $(this).siblings('.thisId').val();
        $.dialog.confirm('您确定要取消这件商品的分销推广吗？', function () {
            loading = showLoading();
            $.post('./CancelProduct', { id: ids }, function (result) {
                loading.close();
                if (result.success) {
                    $.dialog.tips('取消推广成功');
                    var pageNo = $("#list").hiMallDatagrid('options').pageNumber;
                    reload(pageNo);
                }
                else {
                    $.dialog.alert('取消推广失败!' + result.msg);
                }
            });
        });
    });

}

//佣金修改
function RateTextEventBind() {
    $('#list').on('blur', '.disrate', function () {
        var _t = $(this);
        var id = _t.data('proid');
        var _rate = _t.attr('oriValue');
        _rate = parseFloat(_rate);

        var value = $.trim(_t.val());
        if (!value) {
            _t.val(_t.attr('oriValue'));
        }
        else {
            var currate = parseFloat(value);
            if (isNaN(value) || currate <= 0 || currate > 90) {
                $.dialog.errorTips("比例需在 0.1% ~ 90% 之间.");
                _t.val(_t.attr('oriValue'));
                return;
            }
            if (/^\d{1,2}(\.\d)?$/g.test(value)) {
                if (currate == _rate) { return; }
                updateRate(id, value, _t);
            } else {
                $.dialog.errorTips("错误的数据格式.仅可保留一位小数");
                _t.val(_t.attr('oriValue'));
                return;
            }

        }
    });
}

function updateRate(id, rate, obj) {
    var loading = showLoading();
    $.post('./updateRate', { id: id, rate: rate }, function (result) {
        loading.close();
        if (result.success) {
            $.dialog.tips('更新佣金比例成功');
            obj.attr('oriValue', rate);
        }
        else {
            $.dialog.alert('调整佣金比例失败!' + result.msg);
        }
    });
}

function bindSearchBtnClick() {
    $('#searchBtn').click(function () {
        var pageNo = $("#list").hiMallDatagrid('options').pageNumber;
        reload(pageNo);
    });
}

function reload(pageNo) {
    var seakey = $.trim($('#skey').val());
    $("#list").hiMallDatagrid('reload', { skey: seakey, categoryPatha: categoryPatha, categoryPathb: categoryPathb, categoryPathc: categoryPathc, categoryId: categoryId, pageNumber: pageNo });
}

function LoadData() {
    $("#list").html('');

    //商品表格
    $("#list").hiMallDatagrid({
        url: './GetProductList',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的商品',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        pageSize: 9,
        pagePosition: 'bottom',
        pageNumber: 1,
        columns:
        [[
            {
                field: "ProductName", title: '商品', width: 150, align: 'left',
                formatter: function (value, row, index) {
                    var html = '<span class="overflow-ellipsis" style="width:300px"><a title="' + value + '" target="_blank" href="/product/detail/' + row.ProductId + '">' + value + '</a></span>';
                    return html;
                }
            },
        {
            field: "DistributorRate", title: "佣金比例", width: 120, align: "center",
            formatter: function (value, row, index) {
                return '<input class="text-order no-m disrate" type="text" data-proid="' + row.ProductId + '" value="' + value + '" oriValue="' + value + '"> %';
            }
        },
        {
            field: "CategoryName", title: "分类", align: "center"
        },
        {
            field: "SellPrice", title: "价格", align: "center"
        },
        {
            field: "ShowProductSaleState", title: "商品状态", align: "center"
        },
        {
            field: "s", title: "操作", width: 150, align: "center",
            formatter: function (value, row, index) {
                var html = "";
                html = '<span class="btn-a"><input class="thisId" type="hidden" value="' + row.ProductId + '"/><input class="thisName" type="hidden" value="' + row.ProductName + '"/>';
                html += '<a class="btnCancel">取消推广</a></span>';
                return html;
            }
        }
        ]]
    });
}