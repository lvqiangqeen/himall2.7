$(function () {
    initGrid();
    initUpdateSequence();
    bindAddProductsBtn();
    initCateogrySelector();
    bindSearchBtn();
});


function bindSearchBtn() {
    $('#productSearchButton').click(function () {
        var productName = $('#productName').val();
        var categoryId = $('#categoryId').val();
        $("#productList").hiMallDatagrid('reload', { keyWords: productName, categoryId: categoryId });
    });
}

function initCateogrySelector() {
    $('#category1,#category2,#category3').himallLinkage({
        url: '../category/getCategory',
        enableDefaultItem: true,
        defaultItemsText: '全部',
        onChange: function (level, value, text) {
            $('#categoryId').val(value);
        }
    });
}

function bindAddProductsBtn() {
    $('#addBtn').click(function () {
        $.post('GetAllHomeProductIds', { platformType: 3 }, function (data) {
            $.productSelector.show(data, function (selectedProducts) {
                var ids = [];
                $.each(selectedProducts, function () {
                    ids.push(this.id);
                });
                $.post('AddHomeProducts', { productIds: ids.toString(), platformType: 3 }, function (data) {
                    if (data.success)
                        $("#productList").hiMallDatagrid('reload', {});
                    else
                        $.dialog.errorTips(data.msg);
                });
            }, 'distribution', true);
        });
    });
}

function initGrid() {
    //商品表格
    $("#productList").hiMallDatagrid({
        url: 'GetDistributionProducts',
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
        columns:
        [[
             {
                 field: "ProductName", title: '商品名称', width: 130, align: "center",
                 formatter: function (value, row, index) {
                     var html = '<img width="40" height="40" src="' + row.Image + '" /><span class="overflow-ellipsis" style="width:71px">' + value + '</span>';
                     return html;
                 }
             },
            {
                field: "Price", title: '价格', align: "center", formatter: function (value, row, index) { return '￥' + value; }
            },
            {
                field: "Commission", title: '分销佣金', align: "center", width: 90
            },
            {
                field: "ProDisStatus", title: '分销状态', align: "center", width: 90
            },
            {
                field: "Brand", title: '品牌', align: "center", width: 80
            },
            {
                field: "Sequence", title: '排序', align: "center", formatter: function (value, row, index) {
                    return '<input class="text-order" type="text" orivalue="' + value + '" hpId="' + row.Id + '" name="sequence" value="' + value + '">';
                }
            },
            {
                field: "CategoryName", title: '三级分类', width: 100, align: "center"
            },
            {
                field: "Id", title: "操作", width: 90, align: "center",
                formatter: function (value, row, index) {
                    var html = "";
                    html += '<span class="btn-a"><a class="good-check" onclick="del(' + row.Id + ')">删除</a>';
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
        $.post('Delete', { id: id }, function (result) {
            loading.close();
            if (result.success) {
                var pageNumber = $("#productList").hiMallDatagrid('options').pageNumber;
                $("#productList").hiMallDatagrid('reload', { pageNumber: pageNumber });
            }
            else
                $.dialog.errorTips('删除失败！' + result.msg);
        });
    });
}

function initUpdateSequence() {
    $('#productList').on('blur', 'input[name="sequence"]', function () {
        var id = $(this).attr('hpId');
        var sequence = $(this).val();
        var sequence = parseInt(sequence);
        if (isNaN(sequence) || sequence < 0) {
            $.dialog.errorTips('数字格式不正确,请输入大于0的正数');
            $(this).val($(this).attr('orivalue'));
        }
        else {
            $.post('UpdateSequence', { id: id, sequence: sequence }, function (data) {
                if (data.success) {
                    $(this).attr('orivalue', sequence);
                    // $.dialog.tips('更新显示顺序成功');
                }
                else
                    $.dialog.errorTips('更新显示顺序失败！' + result.msg);
            });
        }
    });
}
