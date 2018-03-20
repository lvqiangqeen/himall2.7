$(function () {
    LoadData();
});
function LoadData() {
    $("#list").hiMallDatagrid({
        url: 'List',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有运费模版',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: false,
        idField: "id",
        pageSize: 15,
        pageNumber: 1,
        queryParams: {},
        toolbar: /*"#goods-datagrid-toolbar",*/'',
        columns:
        [[
            { field: "id", hidden: true },
            { field: "name", title: '模版名称', align: "left",
                formatter: function(value, row, index) {
                    var html = '';
                    html = '<p class="single-ellipsis w350">' + value + '</p>';
                    return html;
                }
            },
            {
                 field: "isFree", title: '是否包邮', align: "center", width: 200, formatter: function (value, row, index) {
                     if (value)
                         return '<i class="glyphicon glyphicon-ok" style="color:#009e1a"></i>';
                     return '<i class="glyphicon glyphicon-remove" style="color:#ff551f"></i>';
                 }
            },
              { field: "valuationMethod", title: '计价方式', align: "center", width:200 },
        {
            field: "operation", operation: true, title: "操作", width: 200,
            formatter: function (value, row, index) {
                var id = row.id;
                //  var html = ["<span class=\"btn-a\"><a href='Detials?id=" + id + "'>查看详情</a></span>"];
                var html = [];
                html.push("<span class=\"btn-a\"><a  href='Add?id=" + id + "'>编辑</a></span>");
                html.push("<span class=\"btn-a\"><a  onclick='DeleteTemplate(" + id + ")'>删除</a></span>");
                html.push("<span class=\"btn-a\"><a  onclick='CopyTemplate(" + id + ")'>复制模板</a></span>");
                return html.join("");
            }
        }
        ]]
    });
};

function CopyTemplate(id)
{
    $.dialog.confirm('确认复制此模板吗？', function () {
        var loading = showLoading();
        $.post('CopyTemplate', { id: id }, function (result) {
            loading.close();
            if (result.successful) {
                $.dialog.tips('复制成功！');
                LoadData();
            }
            else {
                $.dialog.errorTips(result.msg);
            }
        });
    });
}





function DeleteTemplate(id) {
    $.dialog.confirm('确认删除此模板吗？', function () {
        var loading = showLoading();
        $.post('DeleteTemplate', { id: id }, function (result) {
            loading.close();
            if (result.successful) {
                $.dialog.tips('删除成功！');
                LoadData();
            }
            else {
                $.dialog.errorTips(result.msg);
            }
        });
    });

}

