var categoryId;



$(function () {

    initTable();

    bindSearchBtnClick();

    categoryTextEventBind();
});


function bindSearchBtnClick() {
    $('#search').click(function () {
        reload(0);
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
        idField: "Id",
        pageSize: 9,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams: {},
        columns:
        [[
           {
               field: "GiftName", title: '礼品名称', align: 'center',
               formatter: function (value, row, index) {
                   return value.replace(/</g, '&lt;').replace(/>/g, '&gt;');
               }
           },
            {
                field: "AddDate", title: '添加时间', width: 100,
                formatter: function (value, row, index) {
                    return date_string(value);
                }
            },
            {
                field: "EndDate", title: '兑换截止', width: 100,
                formatter: function (value, row, index) {
                    return date_string(value);
                }

            },
            {
                field: "NeedIntegral", title: '积分', width: 80

            },
            {
                field: "ShowLimtQuantity", title: '限兑', width: 60

            },
            {
                field: "StockQuantity", title: '库存', width: 60

            },
            {
                field: "RealSales", title: '销量', width: 50,
                formatter: function (value, row, index) {
                    return '<span>' + value + '</span>';
                }

            },
            {
                field: "ShowSalesStatus", title: '状态', width: 80,
                formatter: function (value, row, index) {
                    return '<span class="salesstate' + row.GetSalesStatus + '">' + value + '</span>';
                }

            },
            {
                field: "Sequence", title: '排序', width: 60,
                formatter: function (value, row, index) {
                    return '<input class="text-order no-m" type="text" data-giftid="' + row.Id + '" value="' + value + '" oriValue="' + value + '">';
                }

            },
            {
                field: "s", title: "操作", width: 100, align: "center",
                formatter: function (value, row, index) {
                    var html = "";
                    html += '<span class="btn-a">';
                    html += '<a class="good-check" href="Edit?id=' + row.Id + '">编辑</a>';
                    switch (row.GetSalesStatus) {
                        case 1:
                            html += '<a class="good-check" onclick="editsalesstate(\'' + row.GiftName + '\',' + row.Id + ',false)">下架</a>';
                            break;
                        case 0:
                            html += '<a class="good-check" onclick="editsalesstate(\'' + row.GiftName + '\',' + row.Id + ',true)">上架</a>';
                            break;
                    }

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

function categoryTextEventBind() {
    var _order = 0;

    $('.container').on('focus', '.text-order', function () {
        _order = parseInt($(this).val());
    });

    $('.container').on('blur', '.text-order', function () {
        var _t = $(this);
        var id = _t.data('giftid');

        var value = $.trim(_t.val());
        if (!value) {
            _t.val(_t.attr('oriValue'));
        }
        else {

            if (isNaN(value) || parseInt(value) <= 0) {
                $.dialog.errorTips("您输入的序号不合法,此项只能是大于零的整数.");
                _t.val(_t.attr('oriValue'));
            } else {
                if (parseInt(value) === _order) return;
                updateSequence(id, value, _t);
            }
        }
    });
}

function reload(page) {
    var pageNo = page || $("#list").hiMallDatagrid('options').pageNumber;
    $("#list").hiMallDatagrid('reload', { skey: $.trim($('#title').val()), status: $('#status').val(), pageNumber: pageNo });

}

function updateSequence(id, sequence, obj) {
    var loading = showLoading();
    $.post('UpdateSequence', { id: id, sequence: sequence }, function () {
        loading.close();
        $.dialog.tips('更新礼品的显示顺序成功.');
        obj.attr('oriValue', sequence);
        reload();
    });
}

function editsalesstate(name, id, state) {
    var tipsmsg="您确定下架礼品【"+name+"】吗？";
    if(state)
    {
        tipsmsg = "您确定上架礼品【" + name + "】吗？";
    }
    $.dialog.confirm(tipsmsg,
    function () {
        var loading = showLoading();
        $.post('ChangeStatus', { id: id, state: state }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.tips('调整成功!');
                reload();
            }
            else
                $.dialog.errorTips(result.msg);
        });
    });
}