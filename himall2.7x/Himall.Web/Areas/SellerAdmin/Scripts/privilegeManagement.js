/// <reference path="E:\Projects\HiMall\trunk\src\Web\Himall.Web\Scripts/jquery-1.11.1.js" />
/// <reference path="E:\Projects\HiMall\trunk\src\Web\Himall.Web\Scripts/jquery.hiMallDatagrid.js" />


$(function () {
    query();
})

function Delete(id) {
    $.dialog.confirm('确定删除该条记录吗？', function () {
        $.post("./Delete", { id: id }, function (data) { $.dialog.tips(data.msg); query() });
    });
}

function query() {
    $("#list").hiMallDatagrid({
        url: './list',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: false,
        idField: "Id",
        queryParams: {},
        toolbar: /*"#goods-datagrid-toolbar",*/'',
        columns:
        [[
            { field: "Id", hidden: true },
            { field: "Name", title: '权限组名',align:"left" },
           
        {
            field: "operation", operation: true, title: "操作",align:"right",
            formatter: function (value, row, index) {
                var id = row.Id.toString();
                var html = ["<span class=\"btn-a\">"];
                html.push("<a href='./Edit/" + id + "'>编辑</a>");
                html.push("<a onclick=\"Delete('" + id + "');\">删除</a>");
                html.push("</span>");
                return html.join("");
            }
        }
        ]]
    });
}
