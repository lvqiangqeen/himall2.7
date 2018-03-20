// JavaScript source code
function deleteTypeEvent(id) {
    $.dialog.confirm('您确定要删除该类型吗？', function () {
        var loading = showLoading();
        ajaxRequest({
            type: 'POST',
            url: './DeleteType',
            cache: false,
            param: { Id: id },
            dataType: 'json',
            success: function (data) {
                loading.close();
                if (data.success == true)
                    location.href = './Management';
                else {
                    $.dialog.tips(data.msg);
                }
            },
            error: function (data) {
                loading.close(); $.dialog.tips(data.msg);
            }
        })
    });
}

function Query() {

    var que = $("#searchKeyWord").val();
    $("#typeDatagrid").hiMallDatagrid({
        url: "./DataGridJson",
        singleSelect: true,
        pagination: true,
        NoDataMsg: '没有找到符合条件的数据',
        idField: "Id",
        pageSize: 20,
        pageNumber: 1,
        queryParams: { "searchKeyWord": que },
        toolbar: "#typeToolBar",
        columns:
        [[

            { field: "Id", title: "Id", width: 105, hidden: true },
            { field: "Name", title: "名称", width: 640, align: "left" },
            {
                field: "operation", operation: true, title: "操作", align: "right",
                formatter: function (value, row, index) {
                    var id = row.Id.toString();
                    var html = ['<span class="btn-a">'];
                    html.push('<a href="./Edit?id=' + id + '">编辑</a>');
                    html.push('<a onclick="deleteTypeEvent(' + id + ');">删除</a>');
                    html.push("</span>");

                    return html.join("");
                }
            }
        ]]
    });

};

$(function () {

    Query();
    $("#searchBtn").click(function () { Query(); });
    $("#showall").click(function () {
        $("#searchKeyWord").val("");
        Query();
    });
});