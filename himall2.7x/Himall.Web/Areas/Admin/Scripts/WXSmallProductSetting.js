
$(function () {
    initGrid();
    bindAddProductsBtn();
});

function bindAddProductsBtn() {
    $('#SelectProduct').click(function () {
        $.productSelector.show(null, function (selectedProducts) {
            var ids = [];
            $.each(selectedProducts, function () {
                ids.push(this.id);
            });
            $.post('AddWXSmallProducts', { productIds: ids.toString() }, function (data) {
                if (data.success)
                    $("#productList").hiMallDatagrid('reload', {});
                else
                    $.dialog.errorTips(data.msg);
            });
        }, 'admin', true);
    });
}

function initGrid() {
    //商品表格
    $("#productList").hiMallDatagrid({
        url: 'GetWXSmallProducts',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "Id",
        pageSize: 10,
        pageNumber: 1,
        queryParams: {},
        toolbar: /*"#goods-datagrid-toolbar",*/'',
        operationButtons: "#batchOperate",
        columns:
        [[
            { checkbox: true, width: 39 },
            { field: "id", hidden: true },
            {
                field: "name", title: '商品名称', align: "left", width: 400,
                formatter: function (value, row, index) {
                    var html = '<a title="' + value + '" href="/product/detail/' + row.id + '" target="_blank" href="/product/detail/' + row.id + '"><img class="ml15 fl" width="40" height="40" src="' + row.imgUrl + '" /><span class="single-ellipsis lh40">' + value + '</a></span>';
                    return html;
                }
            },
            {
                field: "price", title: '价格', align: "center", formatter: function (value, row, index) { return '￥' + value; }
            },
            {
                field: "state", title: "商品状态", align: "center"
            },
            {
                field: "Id", title: "操作", width: 90, align: "center",
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
    $.dialog.confirm('确定要删除该商品吗?', function () {
        var loading = showLoading();
        $.post('DeleteWXSmallProductById', { id: id }, function (result) {
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
function DeleteList() {
    var selectedRows = $("#productList").hiMallDatagrid("getSelections");
    var selectids = new Array();

    for (var i = 0; i < selectedRows.length; i++) {
        selectids.push(selectedRows[i].id);
    }
    if (selectedRows.length == 0) {
        $.dialog.errorTips("你没有选择任何选项！");
    }
    else {
        $.dialog.confirm('确定删除选择的商品吗？', function () {
            var loading = showLoading();
            $.post("./DeleteList", { ids: selectids.join(',') },
                function (data)
                {
                    $.dialog.tips(data.msg);
                    initGrid();
                    loading.close();
                });
        });
    }
}