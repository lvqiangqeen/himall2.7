// JavaScript source code
function deleteShopGradeEvent(id) {
    $.dialog.confirm('您确定要删除该店铺套餐吗？', function () {
        var loading = showLoading();
        ajaxRequest({
            type: 'POST',
            url: './DeleteShopGrade',
            cache: false,
            param: { Id: id },
            dataType: 'json',
            success: function (data) {
                loading.close();
                if (data.Successful == true) {
                    location.href = './Management';
                } else {
                    $.dialog.errorTips(data.msg);
                }
            },
            error: function (data) {
                loading.close(); $.dialog.tips('删除失败,请稍候尝试.');
            }
        });
    });
}

function Query() {


    $("#shopGradeDatagrid").hiMallDatagrid({
        url: "./List",
        singleSelect: true,
        pagination: false,
        NoDataMsg: '没有找到符合条件的数据',
        idField: "Id",
        pageSize: 5,
        pageNumber: 1,
        queryParams: {},
        toolbar: "",
        columns:
        [[

            { field: "Id", title: "Id", hidden: true },
            { field: "Name", title: "套餐名称", width: 140 },
            { field: "ProductLimit", title: "可发布商品", width: 180 },
            { field: "ImageLimit", title: "可使用空间(M)", width: 180 },
            { field: "ChargeStandard", title: "年费", width: 180 },
            {
                field: "operation", operation: true, title: "操作",
                formatter: function (value, row, index) {
                    var id = row.Id.toString();
                    var html = ['<span class="btn-a">'];
                    html.push('<a href="./Edit?id=' + id + '">编辑</a>');
                    //html.push('<a onclick="deleteShopGradeEvent(' + id + ');">删除</a>');
                    html.push("</span>");

                    return html.join("");
                }
            }
        ]]
    });

};

$(function () {

    Query();

    //新增套餐
    $('#AddShopG').click(function () {
        $.dialog.open('./Add', {
            title: '新增套餐',
            id: 'addPackage',
            lock: true,
            okVal: '保存',
            init: function () { },
            ok: function () {
                var iframe = this.iframe.contentWindow;
                var form = iframe.document.getElementById("ShopGradeForm");
                if ($(form).valid()) {
                    $(form).submit();
                } else {
                    return false;
                }
            }
        })
    });
});