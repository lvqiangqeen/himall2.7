

$(function () {

    //商品表格
    $("#topicGrid").hiMallDatagrid({
        url: './list',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "id",
        pageSize: 16,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams: { auditStatus: 1 },
        columns:
        [[
            {
                field: "name", title: '专题名称', width: 400, align: "left", formatter: function (value, row, index) {
                    var html = '<a href="/topic/detail/' + row.id + '" target="_blank">' + row.name + '</a>';
                    return html;
                }
            },
            {
                field: "IsRecommend", title: '是否推荐', width:100, align: "center", formatter: function (value, row, index) {
                    var html = value==1?"是":"否";
                    return html;
                }
            },
            {
                field: "s", title: "操作",  align: "right",
                formatter: function (value, row, index) {
                    var html = "";
                    html += '<span class="btn-a"><input type="hidden" value="' + row.url + '" id="url' + row.id + '" />';
                    html += '<a class="good-check" href="./add/' + row.id + '">编辑</a><a class="good-check" onclick="deleteTopic(' + row.id + ',\'' + row.name + '\')">删除</a>';
                    html += '<a class="good-check" onclick="copyurl(\'' + row.id + '\')">复制链接</a>';
                    html += '</span>';
                    return html;
                },
                styler: function () {
                    return 'td-operate';
                }
            }
        ]]
    });
});

function copyurl(id) {
    url =  $("#url"+id).val();
    $.dialog({
        title: '专题链接',
        lock: true,
        id: 'goodCheck',
        content: ['<div class="dialog-form">',
            '<div class="form-group">',
                '<input type="text" id="txturl" value="' + url + '" class="form-control" style="width:300px"/>',
            '</div>',
        '</div>'].join(''),
        padding: '0 40px',
        init: function () { $("#txturl").focus(); }
    });
}

function deleteTopic(id, name) {
    $.dialog.confirm('您确定要删除 ' + name + ' 吗', function () {
        var loading = showLoading();
        $.post('./delete', { id: id }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.succeedTips('删除成功');
                var pageNo = $("#topicGrid").hiMallDatagrid('options').pageNumber;
                $("#topicGrid").hiMallDatagrid('reload', { pageNumber: pageNo });
            }
            else
                $.dialog.errorTips('删除失败!' + result.msg);
        }, "json");
    });
}

