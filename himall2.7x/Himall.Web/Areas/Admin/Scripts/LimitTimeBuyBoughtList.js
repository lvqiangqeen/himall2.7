// JavaScript source code
$(function () {
    $("#list").hiMallDatagrid({
        url: "./GetBoughtJson",
        singleSelect: true,
        pagination: false,
        NoDataMsg: '没有找到符合条件的数据',
        idField: "Id",
        pageSize: 15,
        pageNumber: 1,
        queryParams: {},
        columns:
        [[

            { field: "Id", title: "Id", hidden: true },
            { field: "ShopName", title: "店铺名称", width: 400, align: "left" },
            { field: "StartDate", title: "开始时间", width: 260 },
            { field: "EndDate", title: "结束时间", width: 260, align: "right", }
        ]]
    });
    $('#searchButton').click(function (e) {
        searchClose(e);
        var shopName = $.trim($('#txtShopName').val());
        $("#list").hiMallDatagrid('reload',
            {
                shopName: shopName,
            });
    });

});
