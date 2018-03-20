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
        queryParams: { type: $("#searchType").val(), state: $("#searchState").val(), name: $("#searchName").val() },
        toolbar: /*"#goods-datagrid-toolbar",*/'',
        columns:
        [[
            { field: "Name", title: "活动名称" },
            { field: "TypeStr", title: "红包类型" },
            { field: "TotalPrice", title: "总面额" },
            { field: "ReceiveCount", title: "领取人数" },
            { field: "StartTimeStr", title: "开始日期" },
             {
                 field: "EndTimeStr", title: "结束时间", formatter: function (value, row, index) {
                     //var html = "";
                     //if (row.Type == 3) {
                     //    html += '<span class="btn-a" >————</span>';
                     //}
                     //else {
                     //    html += '<span class="btn-a" >' + row.EndTimeStr + '</span>';
                     //}
                     //return html;
                     return '<span class="btn-a" >' + row.EndTimeStr + '</span>';
                 }
             },
            {
                field: "IsInvalid", title: "状态", formatter: function (value, row, index) {
                    return row.StateStr;
                }
            },
            {
                field: "operation", operation: true, title: "操作", formatter: function (value, row, index) {
                    var html = "";
                    html += '<span class="btn-a"><a href="/Admin/Bonus/Detail/' + row.Id + '">详情</a></span>';

                    var str = row.EndTimeStr + ' 23:59:59';
                    str = str.replace(/-/g, "/");
                    var enddate = new Date(str);

                    if (new Date() > enddate) {
                        return html;
                    }

                    if (!row.IsInvalid || row.StartTime > new Date()) {
                        html += '<span class="btn-a"><a href="/Admin/Bonus/Edit/' + row.Id + '">编辑</a></span>';
                    }
                    if (!row.IsInvalid) {
                        if (row.Type == 1) {
                            html += '<span class="btn-a"><a href="/Admin/Bonus/Apportion/' + row.Id + '">发放</a></span>';
                        }
                        html += '<span class="btn-a"><a onclick="invalid(' + row.Id + ' , ' + row.IsInvalid + ');">失效</a></span>';
                    }

                    return html;
                }
            }
        ]]
    });
}

function invalid(id, isInvalid) {
    if (isInvalid) {
        $.dialog.tips('此活动已失效!');
        return;
    }

    $.dialog.confirm('您确定要失效此活动？', function () {
        var loading = showLoading();
        $.post("/Admin/Bonus/Invalid", { id: id }, function () {
            $.dialog.tips('已成功失效此活动');
            var pageNo = $("#list").hiMallDatagrid('options').pageNumber;
            $("#list").hiMallDatagrid('reload', { pageNumber: pageNo });
            loading.close();
        })
    })
}