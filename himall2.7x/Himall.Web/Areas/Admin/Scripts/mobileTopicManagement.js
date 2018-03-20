


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
                field: "name", title: '专题名称',  align: "left"
            },
            {
                field: "tags", title: '标签', align: "center",width:500, 
				formatter: function (value, row, index) {
					return '<span style="width:500px; word-break:break-all;">'+value+'</span>';
				}
            },
            {
                field: "s", title: "操作", align: "right",
                formatter: function (value, row, index) {
                    var html = '<span class="btn-a"><input type="hidden" value="' + row.url + '" id="url' + row.id + '" />';
                    html += '<a class="good-check" target="_blank" href="/Admin/VTemplate/EditTemplate?client=11&tName=' + row.id + '">编辑</a><a class="good-check" onclick="deleteTopic(' + row.id + ',\'' + row.name + '\')">删除</a>';
                    html += '<a class="good-check" onclick="copyurl(\'' + row.id + '\')">复制链接</a>';
                    html += '<a class="glyphicon glyphicon-eye-open view-mobile-shop" title="预览专题" data-url="/m-wap/topic/detail/' + row.id + '" style="font-size:13px;text-decoration: none; cursor:pointer;"></a>';
                    html += "</span>";
                    return html;
                },
                styler: function () {
                    return 'td-operate';
                }
            }
        ]]
    });


    $('#searchButton').click(function () {
        var titleKeyword = $('#titleKeyword').val();
        var tagsKeyword = $('#tagsKeyword').val();
        $("#topicGrid").hiMallDatagrid('reload', { tagsKeyword: tagsKeyword, titleKeyword: titleKeyword });
    });
});


function copyurl( id )
{
    url = $("#url"+id).val();
    $.dialog( {
        title: '专题链接',
        lock: true,
        id: 'goodCheck',
        content: ['<div class="dialog-form">',
            '<div class="form-group">',
                '<input type="text" id="txturl" value="' + url + '" class="form-control" style="width:300px"/>',
            '</div>',
        '</div>'].join( '' ),
        padding: '0 40px',
        init: function () { $( "#txturl" ).focus(); }
    } );
}

function deleteTopic(id, name) {
    $.dialog.confirm('您确定要删除 ' + name + ' 吗', function () {
        var loading = showLoading();
        $.post('./delete', { id: id }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.succeedTips('删除成功', function () {

                    var pageNo = $("#topicGrid").hiMallDatagrid('options').pageNumber;
                    $("#topicGrid").hiMallDatagrid('reload', { pageNumber: pageNo });
                });
            }
            else
                $.dialog.errorTips('删除失败!' + result.msg);
        }, "json");
    });
};
$(document).on('click', '.view-mobile-shop', function () {
    $("#mobileshow").attr("src", $(this).attr("data-url"));
    $('.mobile-dialog').show();
    $('.cover').fadeIn();
});
$('.cover').click(function () {
    $('.mobile-dialog').hide();
    $("#mobileshow").attr("src", "about:blank");
    $('.cover').fadeOut();
});