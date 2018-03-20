// JavaScript source code
function deleteShopEvent(id) {
    var loading = showLoading();
    ajaxRequest({
        type: 'POST',
        url: './DeleteShop',
        cache: false,
        param: { Id: id },
        dataType: 'json',
        success: function (data) {
            if (data.Successful == true)
                location.href = './Management';
            loading.close();
        },
        error: function (data) { $.dialog.tips('删除失败,请稍候尝试.'); loading.close(); }
    });
}

function FreezeShop(id, state) {
    var tipsmsg = "冻结店铺将导致商家无法登陆后台,商品会自动下架，请谨慎操作！";
    if (!state) {
        tipsmsg = "解冻店铺后该商家即可登陆后台，请手动上架商品，您确定解冻吗？";
    }
    $.dialog.confirm(tipsmsg,
    function () {
        var loading = showLoading();
        ajaxRequest({
            type: 'POST',
            url: $("#UAF").val(),
            cache: false,
            param: { id: id, state: state },
            dataType: 'json',
            success: function (data) {
                if (data.success == true) {
                    var d = $("#bt_fs" + id);
                    var sd = $("#lb_state_s" + id);
                    if (state) {
                        sd.html('冻结');
                        d.html("解冻");
                        d.attr("href", "javascript:FreezeShop(" + id + ",false)");
                    } else {
                        sd.html('开启');
                        d.html("冻结");
                        d.attr("href", "javascript:FreezeShop(" + id + ",true)");
                    }
                }
                loading.close();
            },
            error: function (data) { $.dialog.tips('操作失败,请稍候尝试.'); loading.close(); }
        });
    });
}

function businessCategoryShopEvent(id) {

}

function Query() {

    var shopName = $("#shopName").val();
    var shopAccount = $("#shopAccount").val();
    var shopGradeId = $("#shopGradeId").val();
    var shopStatus = $("#shopStatus").val();

    $("#shopDatagrid").hiMallDatagrid({
        url: "./List",
        singleSelect: true,
        pagination: true,
        NoDataMsg: '没有找到符合条件的数据',
        idField: "Id",
        pageSize: 15,
        pageNumber: 1,
        queryParams: { "shopName": shopName, "shopAccount": shopAccount, "shopGradeId": shopGradeId, "shopStatus": shopStatus, type: $("#type").val() },
        toolbar: "#shopToolBar",
        columns:
        [[

            { field: "Id", title: "Id", hidden: true },
            { field: "IsSelf", title: "IsSelf", hidden: true },
            {
                field: "Name", title: "店铺名称", width: 140, formatter: function (value, row, index) {
                    var id = row.Id.toString();
                    var html = "";
                    if (row.Status != '被拒绝' && row.Status != '待付款') {
                        html = '<a target="_blank" href="/Shop/Home/' + id + '">' + row.Name + '</a>';
                    }
                    else {
                        html = row.Name;
                    }
                    return html;
                }
            },
            { field: "Account", title: "店铺账号", width: 140 },
            { field: "ShopGrade", title: "等级", width: 140 },
            { field: "EndDate", title: "有效期", width: 140 },
            {
                field: "Status", title: "状态", width: 120, formatter: function (value, row, index) {
                    var id = row.Id.toString();
                    var html = '<span id="lb_state_s' + id + '">';
                    html += value;
                    html += "</span>"
                    return html;
                }
            },
			{ field: 'Balance', title: '余额', width: 120 },
            {
                field: "operation", operation: true, title: "操作",
                formatter: function (value, row, index) {
                    var id = row.Id.toString();
                    var html = ['<span class="btn-a">'];

                    if (row.Status == "不可用") {
                        html.push('<a href="./Details?id=' + id + '">查看</a>');
                    }
                    else if ($("#type").val() == "Auditing" || row.Status == '待付款' || row.Status == '待审核' || row.Status == '待确认') {
                        html.push('<a href="./Auditing?id=' + id + '">审核</a>');
                    } else {
                        html.push('<a href="./Details?id=' + id + '">查看</a>');
                        if (row.Status != '被拒绝') {
                            if (row.BusinessType == 0) {
                                html.push('<a href="./Edit?id=' + id + '">编辑</a>');
                            } else {
                                html.push('<a href="./EditPersonal?id=' + id + '">编辑</a>');
                            }
                        }
                        if (row.IsSelf.toString() == 'false' && row.Status != '被拒绝') {
                            //html.push('<a onclick="deleteShopEvent(' + id + ');">删除</a>');
                            html.push('<a href="./BusinessCategory?id=' + id + '">经营类目</a>');
                            if (row.Status != '冻结') {
                                html.push('<a href="javascript:FreezeShop(' + id + ',true)" id="bt_fs' + id + '">冻结</a>');
                            } else {
                                html.push('<a href="javascript:FreezeShop(' + id + ',false)" id="bt_fs' + id + '">解冻</a>');
                            }
                        }
                    }
                    html.push("</span>");

                    return html.join("");
                }
            }
        ]]
    });

};

$(function () {

    Query();
    $("#searchBtn").click(function (e) {
        Query();
        searchClose(e);
    });
});
