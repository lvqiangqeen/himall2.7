var categoryId;



$(function () {

    initArticleSelectors();

    initTable();

    bindSearchBtnClick();
 
    categoryTextEventBind();

    $('#list').on('mouseover', function () {
        var _t = $(this);

        var copybox = $(".copyurl", _t);
        copybox.each(function () {
            var _cur = $(this);
            var _url = _cur.data("url");
            var hasevent = _cur.data("hasevent");
            if (hasevent != 1) {
                _cur.zclip({
                    path: '/Scripts/ZeroClipboard.swf', //记得把ZeroClipboard.swf引入到项目中 
                    copy: function () {
                        var _curdom = $(this);
                        return _curdom.data("url");
                    },
                    afterCopy: function () {
                        $.dialog.succeedTips('复制链接成功！');
                    }
                });
            }
            _cur.data("hasevent", 1);
        });
    });

    

});


function bindSearchBtnClick() {
    $('#search').click(function () {
        reload();
    });
}

function batchDelete() {

    var selected = $("#list").hiMallDatagrid('getSelections');
    var ids = [];
    $.each(selected, function (i, item) {
        ids.push(item.id);
    });
    if (ids.length > 0) {
        $.dialog.confirm('您确定要删除选中的' + ids.length + '篇文章吗？', function () {
            var loading = showLoading();
            $.post('BatchDelete', { ids: ids.toString() }, function (result) {
                loading.close();
                if (result.success) {
                    $.dialog.tips('删除成功');
                    reload();
                }
                else
                    $.dialog.errorTips('删除失败！' + result.msg);
            });
        });
    }
}


function initArticleSelectors() {
    $('#firstLevel,#secondLevel').himallLinkage({
        url: '../articleCategory/getCategories',
        enableDefaultItem: true,
        defaultItemsText: '全部',
        onChange: function (level, value, text) {
            categoryId = value;
        }
    });
}


function initTable() {

    //文章表格
    $("#list").hiMallDatagrid({
        url: 'list',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "id",
        pageSize: 9,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams: {},
        operationButtons: '#batchDelete',
        columns:
        [[
            { checkbox: true, width: 39 },
            //{
            //    field: "displaySequence", title: '排序', width: 50,
            //    formatter: function (value, row, index) {
            //        return '<input class="text-order no-m" type="text" articleId="' + row.id + '" value="' + value + '" oriValue="' + value + '">';
            //    }

            //},
           {
               field: "title", title: '标题', width: 250, align: 'center',
               formatter: function (value, row, index) {
                   return value.replace(/</g, '&lt;').replace(/>/g, '&gt;');
               }},
           { field: "categoryName", title: "分类", width: 85, align: "center" },
        {
            field: "isShow", title: "是否显示", width: 70, align: "center",
            formatter: function (value, row, index) {
                return value ? '是' : '否';
            }
        },
        {
            field: "c", title: "复制地址", width: 90, align: "center",
            formatter: function (value, row, index) {
                var html = "";
                html += '<a class="good-check copyurl" data-url="' + row.showurl + '">复制地址</a>';
                return html;
            },
            styler: function () {
                return 'td-operate';
            }
        },
        {
            field: "s", title: "操作", width: 90, align: "center",
            formatter: function (value, row, index) {
                var html = "";
                html += '<span class="btn-a">';

                html += '<a class="good-check" href="add?id=' + row.id + '">编辑</a>';
                html += '<a class="good-check" onclick="del(\'' + row.title + '\',' + row.id + ')">删除</a>';


                html += '</span>';
                return html;
            },
            styler: function () {
                return 'td-operate';
            }
        }
        ]]
    });

}

function reload() {
    var pageNo = $("#list").hiMallDatagrid('options').pageNumber;
    $("#list").hiMallDatagrid('reload', { categoryId: categoryId, titleKeyWords: $.trim($('#title').val()), pageNumber: pageNo });

}


function del(name, id) {
    $.dialog.confirm('您确定要删除文章 ' + name + ' 吗？', function () {
        var loading = showLoading();
        $.post('Delete', { id: id }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.tips('删除成功!');
                reload();

            }
            else
                $.dialog.errorTips(result.msg);

        })
    });

}


function categoryTextEventBind() {
    var _order = 0;

    $('.container').on('focus', '.text-order', function () {
        _order = parseInt($(this).val());
    });

    $('.container').on('blur', '.text-order', function () {
        var id = $(this).attr('articleId');

        var value = $.trim($(this).val());
        if (!value) {
            $(this).val($(this).attr('oriValue'));
        }
        else {
            if (isNaN($(this).val()) || parseInt($(this).val()) <= 0) {
                $.dialog({
                    title: '更新分类信息',
                    lock: true,
                    width: '400px',
                    padding: '20px 60px',
                    content: ['<div class="dialog-form">您输入的序号不合法,此项只能是大于零的整数.</div>'].join(''),
                    button: [
                    {
                        name: '关闭',
                    }]
                });
                $(this).val(_order);
            } else {
                if (parseInt($(this).val()) === _order) return;
                updateSequence(id, _order, $(this));
            }
        }
    });
}

function updateSequence(id, displaySequence, obj) {
    var loading = showLoading();
    $.post('UpdateDisplaySequence', { id: id, displaySequence: displaySequence }, function () {
        loading.close();
        $.dialog.tips('更新文章的显示顺序成功.');
        obj.attr('oriValue', displaySequence);
    });
}