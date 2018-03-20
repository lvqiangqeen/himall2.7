// JavaScript source code
function DeleteTopShow(id) {
    $.dialog.confirm('是否移除主推微店?', function () {
        var loading = showLoading();
        $.post('../VShop/DeleteTopShow', { vshopId: id }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.tips("已将主推微店移除");
                window.location.reload();
            }
            else
                $.dialog.alert('删除失败！' + result.msg);
        })
    })
}

$('#searchReplaceVShopButton').click(function () {
    var vshopName = $('#titleKeyword').val();
    $("#repaceList").hiMallDatagrid('reload', { vshopName: vshopName, vshopType: 0 });
});

function ReplaceHotVShop(oldVShopId) {
    $.dialog({
        title: '微店替换',
        lock: true,
        width: 550,
        padding: '0 40px',
        id: 'replaceHotVShopDialog',
        content: $('#replace-HotVShop')[0],
    });

    //商品表格
    $("#repaceList").hiMallDatagrid({
        url: '/admin/VShop/GetVshops',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的专题',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "id",
        pageSize: 16,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams: { vshopType: null, oldVShopId: oldVShopId },
        columns:
        [[
            {
                field: "name", title: '微店名称', align: "center"
            },
            {

                field: "categoryName", title: '经营类目', align: "center", width: 100,
            },
            {

                field: "creatTime", title: '创建时间', align: "center", width: 90,

            },
            {

                field: "visiteNum", title: '进店浏览量', align: "center", width: 80,

            },
            {

                field: "buyNum", title: "成交量", align: "center", width: 60,

            },
            {
                field: "s", title: "操作", align: "center", width: 60,
                formatter: function (value, row, index) {
                    var html = "";
                    html += '<span class="btn-a"><a class="good-setTopshow">替换</a>';
                    html += '<input class="oldVShopId" type="hidden" value="' + oldVShopId + '"/><input class="newVShopId" type="hidden" value="' + row.id + '"/></span>';
                    return html;
                },
            }
        ]]
    });
}

$('#repaceList').on('click', '.good-setTopshow', function () {
    var oldId = $(this).siblings('.oldVShopId').val();
    var newId = $(this).siblings('.newVShopId').val();
    var loading = showLoading();
    $.post('../VShop/ReplaceTopShop', { oldVShopId: oldId, newVShopId: newId }, function (result) {
        loading.close();
        if (result.success) {
            $.dialog.tips("替换成功");
            window.location.reload();
        }
        else
            $.dialog.alert('删除失败！' + result.msg);
    })
})