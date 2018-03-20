// JavaScript source code
$(function () {
    $("#list").hiMallDatagrid({
        url: './GetInvoiceContexts',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "Id",
        pageSize: 30,
        pagePosition: 'bottom',
        pageNumber: 1,
        //queryParams: { orderStatus: status },
        columns:
        [[
            { field: "Name", title: "发票内容", width: 150, align: "left" },
            {
                field: "operation", operation: true, title: "操作", width: 150, align: "right",
                formatter: function (value, row, index) {
                    var id = row.Id;
                    var html = ["<span class=\"btn-a\">"];
                    html.push('<a href="#" onclick="edit(' + id + ' , \'' + row.Name + '\')">编辑</a>');
                    html.push("<a href='#' onclick='del(" + id + ")'>删除</a>");
                    html.push("</span>");
                    return html.join("");
                }
            }
        ]]
    });



    $("#btnAdd").click(function () {
        Save(-1, "");
    })
})

function del(id) {
    
    $.dialog.confirm("您确定要删除此发票吗？", function () {
        var loading = showLoading();
        $.post("./DeleteInvoiceContexts", { id: id }, function (result) {
            loading.close();
            if (result == true) {
                $.dialog.tips("删除成功！");
                $("#list").hiMallDatagrid('reload', { pageNumber: 1 });
            }
            else {
                $.dialog.tips("删除失败！");
            }
        })
    });
}

function edit(id, value) {
    Save(id, value);
}

function Save(id, value) {
    $.dialog({
        title: '新增发票内容',
        lock: true,
        id: 'goodCheck',
        content: ['<div class="dialog-form">',
            '<div class="form-group">',
                '<p class="help-esp">发票内容</p>',
                '<input type="text" style="margin-bottom:20px;" id="txtIName" value="' + value + '" class="form-control"/>',
            '</div>',
        '</div>'].join(''),
        padding: '0 40px',
        init: function () { $("#txtPayRemark").focus(); },
        button: [
        {
            name: '确认',
            callback: function () {
                if ($.trim($("#txtIName").val()).trim() == "") {
                    $.dialog.tips("发票不能为空");
                    return;
                }
                var loading = showLoading();
                $.post("./SaveInvoiceContext", { name: $("#txtIName").val(), id: id }, function (result) {
                    loading.close()
                    if (result == true) {
                        $.dialog.tips("保存成功");
                        $("#list").hiMallDatagrid('reload', { pageNumber: 1 });
                    }
                    else {
                        $.dialog.tips("保存失败！");
                    }
                })
            },
            focus: true
        }]
    });
}