$(function () {
    $("#area-selector").RegionSelector({
        selectClass: "form-control input-sm select-sort",
        valueHidden: "#AddressId"
    });
    $('#searchButton').click(function () {
        var para = GetQueryPara();
        $("#shopDatagrid").hiMallDatagrid('clearReload', para);
    });
    LoadData();
});
function GetQueryPara()
{
    var para = {};
    para.shopBranchName = $.trim($('#shopBranchName').val());
    para.contactPhone = $.trim($('#contactPhone').val());
    para.contactUser = $.trim($('#contactUser').val());
    if ($.trim($('#AddressId').val()) != '0')
        para.AddressId = $.trim($('#AddressId').val());
    return para;
}
function reloadDataGrid()
{
    var para = GetQueryPara();
    $("#shopDatagrid").hiMallDatagrid('clearReload', para);
}
function LoadData()
{
    var para = {};
    para.shopBranchName = '';
    para.contactPhone = '';
    para.contackUser = '';
    $("#shopDatagrid").hiMallDatagrid({
        url: 'list',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的门店',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "Id",
        pageSize: 10,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams: para,
        operationButtons: "#saleOff",
        columns:
        [[
            {
                field: "ShopBranchName", title: '门店名称', align: 'center'
            },
            {
                field: "AddressFullName", title: '门店地址',width: 400, align: 'center'
            },
        {
            field: "ContactUser", title: "联系人", width: 150, align: "center"
        },
        {
            field: "ContactPhone", title: "联系方式", width: 150, align: "center"
        },
        {
            field: "s", title: "操作", width: 150, align: "center",
            formatter: function (value, row, index) {
                var html = "";
                html = '<span class="btn-a"><input class="thisId" type="hidden" value="' + row.Id + '"/><input class="thisName" type="hidden" value="' + row.Name + '"/>';
                if (row.Status == 0) {
                    html += '<a class="good-down" onclick="Freeze(' + row.Id + ')">冻结</a>';
                } else {
                    html += '<a class="good-down" onclick="UnFreeze(' + row.Id + ')">解冻</a>';
                }
                html += '<a class="good-check"  href="edit/' + row.Id + '">编辑</a>';
                html += '<a class="good-del" onclick="StoresLink(' + row.Id + ')">链接</a>';
                //html += '<a class="good-del" onclick="DeleteShopBranch('+row.Id+',\''+row.ShopBranchName+'\')">删除</a></span>';
                return html;
            }
        }
        ]],
        onLoadSuccess: function () {
        }
    });
}
//function DeleteShopBranch(id,shopname)
//{
//    $.dialog.confirm('您确定要删除' + shopname + ' 吗?', function () {
//        var loading = showLoading();
//        $.ajax({
//            url: 'Delete',
//            type: 'post',
//            data: { id: id },
//            success: function (result) {
//                if (loading)
//                    loading.close();
//                $.dialog.alert(result.msg);
//                reloadDataGrid();
//            }
//        });
//    });
//}
function Freeze(shopBranchId)
{
    $.dialog.confirm('冻结之后，门店管理员将不能登录门店后台且门店也不能做为自提点，您确定要冻结？', function () {
        var loading = showLoading();
        $.ajax({
            type: 'post',
            url: 'Freeze',
            async: false,
            data: { shopBranchId: shopBranchId },
            success: function (result) {
                if (loading)
                    loading.close();
                $.dialog.alert(result.msg);
                reloadDataGrid();
            }
        });
    });
}
function UnFreeze(shopBranchId) {
    $.dialog.confirm('门店现在为冻结状态，确定解冻门店吗?', function () {
        var loading = showLoading();
        $.ajax({
            type: 'post',
            url: 'UnFreeze',
            async: false,
            data: { shopBranchId: shopBranchId },
            success: function (result) {
                if (loading)
                    loading.close();
                $.dialog.alert(result.msg);
                reloadDataGrid();
            }
        });
    });
}
//门店链接
function StoresLink(shopBranchId) {
    var url = window.location.protocol + "//" + window.location.host + '/m-wap/shopbranch/index/' + shopBranchId;
    $("#referralsLink").val(url);
    var loading = showLoading();
        $.dialog({
            title: '门店链接',
            lock: true,
            id: 'StoresLink',
            content: document.getElementById("storesLink-form"),
            padding: '0 40px',
            init: function () {
                $.ajax({
                    type: 'GET',
                    url: 'StoresLink',
                    cache: false,
                    data: { 'vshopUrl': url },
                    dataType: 'json',
                    success: function (data) {
                        loading.close();
                        if (data.successful == true) {
                            $("#imgsrc").attr("src", data.qrCodeImagePath);
                        }
                    }, error: function () {
                        loading.close();
                    }
                });
            }
        });
}