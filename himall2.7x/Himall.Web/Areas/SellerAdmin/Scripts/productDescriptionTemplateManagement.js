$(function () {

    initGrid();
    bindSearchBtnClickEvent();
})



function initGrid() {
    //商品表格
    $("#list").hiMallDatagrid({
        url: 'list',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "Id",
        pageSize: 9,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams: {},
        operationButtons: null,
        columns:
        [[
            { field: "Name", title: '版式名称', width: 450, align: 'center' },
            { field: "PositionText", title: '版式位置', width: 100, align: 'center' },
        {
            field: "s", title: "操作", width: 200, align: "center",
            formatter: function (value, row, index) {
                var html = "";
                html = '<span class="btn-a">\
                    <a class="good-check" href="Add?id=' + row.Id + '">编辑</a>';
                html += '<a class="good-check" onclick="del(' + row.Id + ',\'' + row.Name + '\')">删除</a></span>';
                return html;
            },
            styler: function () {
                return 'td-operate';
            }
        }
        ]],
        
    });

}

function bindSearchBtnClickEvent() {
    $('#searchBtn').click(function () {
        search();

    });

}

function del(id,name) {
    $.dialog.confirm('您是否确定要删除模板 “' + name + "”吗？", function () {
        var loading = showLoading();
        $.post('delete', { id: id }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.succeedTips('删除成功!', function () { search(); });
            }
            else
                $.dialog.errorTips('删除失败!' + result.msg);
        });
    });
}


function search() {
    var position = $('#position').val();
    var name = $.trim($('#name').val());

    $("#list").hiMallDatagrid('reload', { name: name, position: position });

}