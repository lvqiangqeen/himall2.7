// JavaScript source code
$(function () {
    loadGrid();
    $("#btnSearch").click(function () {
        $("#list").hiMallDatagrid('reload', { type: $("#searchType").val(), state: $("#searchState").val(), name: $("#searchName").val() })
    })
});


function loadGrid() {
    $("#list").hiMallDatagrid({
        url: './list',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "Id",
        pageSize: 20,
        pageNumber: 1,
        queryParams: { name: $("#searchName").val() },
        toolbar: /*"#goods-datagrid-toolbar",*/'',
        columns:
        [[
            { field: "activityTitle", title: "刮刮卡名称" },
            { field: "WeiParticipate", title: "参与次数限制" },
            { field: "validTime", title: "有效时间" },
            { field: "totalNumber", title: "参与人数" },
            { field: "winNumber", title: "中奖人数" }

            ,
            {
                field: "operation", operation: true, title: "操作", formatter: function (value, row, index) {
                    var html = "";
                    html += '<span class="btn-a"><a href="/Admin/WeiActivity/WinManagement?pid=' + row.Id + '">中奖详情</a></span>';

                   html += '<span class="btn-a"><a href="/Admin/WeiActivity/Edit/' + row.Id + '">编辑</a></span>';
                
                   html += '<span class="btn-a"><a href="/Admin/WeiActivity/Detail/' + row.Id + '?show=no">链接</a></span>';
                       
                   html += '<span class="btn-a"><a onclick="deleteid(' + row.Id + ' );">删除</a></span>';
                   
                   return html;
                }
            }
        ]]
    });
}
function deleteid(id) {

    $.dialog.confirm('您确定要删除此活动？', function () {
        var loading = showLoading();
        $.post("/Admin/WeiActivity/deleteid", { id: id }, function () {
            $.dialog.tips('已成功删除此活动');
            var pageNo = $("#list").hiMallDatagrid('options').pageNumber;
            $("#list").hiMallDatagrid('reload', { pageNumber: pageNo });
            loading.close();
        })
    })
}