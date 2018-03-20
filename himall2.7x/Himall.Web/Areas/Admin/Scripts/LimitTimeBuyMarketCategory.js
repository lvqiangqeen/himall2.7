// JavaScript source code
$(function () {
    $("#list").hiMallDatagrid({
        url: "./GetMarketCategoryJson",
        singleSelect: true,
        pagination: true,
        NoDataMsg: '没有找到符合条件的数据',
        idField: "Id",
        pageSize: 100,
        pageNumber: 1,
        queryParams: {},
        columns:
        [[

            { field: "Id", title: "Id", hidden: true },
            { field: "Name", title: "分类名称", width: 800, align: 'left' },
            {
                field: "operation", operation: true, title: "操作",
                formatter: function (value, row, index) {
                    var html = ['<span class="btn-a">'];
                    html.push('<a class="a-del" onclick="deleteCateEvent(\'' + row.Name + '\');">删除</a>');
                    html.push("</span>");
                    return html.join("");
                }
            }
        ]]
    });

    $("#AddCate").click(function () {
        AddCate();
    });

});
function AddCate() {

    $.dialog({
        title: '添加分类',
        lock: true,
        id: 'goodCheck',
        width: 310,
        content: ['<div class="dialog-form">',
            '<div class="form-group">',
                '<p class="help-esp">名称</p>',
                '<input id="cateName" class="form-control" type="text"  />',
                '<span class="field-validation-error" id="cateNameTip"></span>',
            '</div>',
        '</div>'].join(''),
        padding: '0 40px',
        init: function () { $("#cateName").focus(); },
        button: [
        {
            name: '添加',
            callback: function () {
                var name = $("#cateName").val();
                var reg = /^[a-zA-Z0-9\u4e00-\u9fa5]+$/;
                if (name.length == 0 || name.length > 12) {
                    $("#cateNameTip").text("分类名称在1-12个字符之间");
                    $("#cateName").css({ border: '1px solid #f60' });
                    return false;
                }
                if (reg.test(name) == false) {
                    $("#cateNameTip").text("分类名称有中文、英文、数字、下划线组成");
                    $("#cateName").css({ border: '1px solid #f60' });
                    return false;
                }
                var loading = showLoading();
                ajaxRequest({
                    type: 'POST',
                    url: "./AddMarketCategory",
                    param: { name: name },
                    dataType: "json",
                    success: function (data) {
                        loading.close();
                        if (data.success == true) {
                            var pageNo = $("#list").hiMallDatagrid('options').pageNumber;
                            $.dialog.succeedTips('添加分类成功.', null, 0.5);
                            $("#list").hiMallDatagrid('reload', { pageNumber: pageNo });
                        } else {
                            $.dialog.errorTips(data.msg, null, 1);
                        }
                    }, error: function () {
                        loading.close();
                    }
                });
            },
            focus: true
        }]
    });
}
function deleteCateEvent(name) {
    $.dialog.confirm('您确定要删除吗？', function () {
        var loading = showLoading();
        ajaxRequest({
            type: 'POST',
            url: "./DeleteMarketCategory",
            param: { name: name },
            dataType: "json",
            success: function (data) {
                loading.close();
                if (data.success == true) {
                    var pageNo = $("#list").hiMallDatagrid('options').pageNumber;
                    $.dialog.succeedTips('删除分类成功.', null, 0.5);
                    $("#list").hiMallDatagrid('reload', { pageNumber: pageNo });
                } else {
                    $.dialog.errorTips(data.msg, null, 1);
                }
            }, error: function () {
                loading.close();
            }
        });
    });
}