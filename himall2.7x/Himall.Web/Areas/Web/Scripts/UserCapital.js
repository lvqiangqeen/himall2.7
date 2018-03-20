$(function () {
    typeChoose(0);
    $('#ulstatus li').click(function (e) {
        typeChoose($(this).val());
    });
})

function typeChoose(val) {
    $('#ulstatus li').each(function () {
        var _t = $(this);
        if (_t.val() == val) {
            _t.addClass('active').siblings().removeClass('active');
        }
    });
    var dataColumn = [];
    if (val == 0) {
        dataColumn.push({ field: "CreateTime", title: '时间', width: 120 });
        dataColumn.push({
            field: "Amount", title: '收入', width: 100, align: 'center',
            formatter: function (value, row, index) {
                var html = '';
                if (parseFloat(value) > 0)
                    html = value;
                else if (parseFloat(value) == 0 && row['SourceType'] == 5)  //退款 纯积分支付时 我的账户 账户明细 收入显示0
                    html = value;
                return html;
            }
        });
        dataColumn.push({
            field: "Amount1", title: '支出', width: 100, align: 'center',
            formatter: function (value, row, index) {
                var html = '';
                if (parseFloat(row['Amount']) < 0)
                    html = row['Amount'];
                return html;
            }
        });
        dataColumn.push({
            field: "Remark", title: "备注", width: 200, align: "left",
            formatter: function (value, row, index) {
                if (row['SourceType'] == 4) {
                    var html = '消费,订单号:<a target="_blank" href="/UserOrder/Detail/' + row.SourceData + '">' + row.SourceData + '</a>';
                    return html;
                } else if (row['SourceType'] == 5) {
                    var html = '退款,售后编号:' + row.SourceData;
                    return html;
                } else if (row['SourceType'] == 6) {
                    var html = '佣金收入,订单号:' + row.SourceData;
                    return html;
                }
                else {
                    return value;
                }
            }
        });
    }
    var url = '/UserCapital/List';
    switch (val) {
        case 1:
            dataColumn.push({
                field: "CreateTime", title: '领取时间', width: 120
            });
            dataColumn.push({
                field: "Amount", title: '金额', width: 100, align: 'center',
            });
            break;
        case 2:
            url = '/UserCapital/ChargeList';
            dataColumn.push({ field: "CreateTime", title: '充值时间', width: 120 });
            dataColumn.push({
                field: "ChargeAmount", title: '金额', width: 100, align: 'center',
            });
            dataColumn.push({ field: "ChargeWay", title: '充值方式', width: 80 });
            dataColumn.push({ field: "ChargeStatusDesc", title: '充值状态', width: 80 });
            dataColumn.push({ field: "Id", title: '充值单号', width: 120 });
            dataColumn.push({
                field: "operate", title: '操作', width: 80, formatter: function (value, row, index) {
                    var html = [];
                    if (row['ChargeStatus'] == 1) {
                        html.push("<span class=\"btn-a\">");
                        html.push("<a onclick='DoOperate(\"" + row["Id"] + "\")'>付款</a>");
                        html.push("</span>");
                    }
                    return html.join('');
                }
            });
            break;
        case 3:
            url = '/UserCapital/ApplyWithDrawList';
            dataColumn.push({ field: "ApplyTime", title: '提现时间', width: 120 });
            dataColumn.push({
                field: "ApplyAmount", title: '金额', width: 100, align: 'center',
            });
            dataColumn.push({
                field: "ApplyStatusDesc", title: '提现状态', width: 80
            });
            dataColumn.push({ field: "Id", title: '提现单号', width: 120 });
            break;
        case 4:
            dataColumn.push({ field: "CreateTime", title: '消费时间', width: 120 });
            dataColumn.push({
                field: "Amount", title: '金额', width: 100, align: 'center',
            });
            dataColumn.push({
                field: "Id", title: '单号', width: 120,
                formatter: function (value, row, index) {
                    var html = '<a href="/UserOrder/Detail/' + row.SourceData + '">' + row.SourceData + '</a>';
                    return html;
                }
            });
            break;
        case 5:
            dataColumn.push({ field: "CreateTime", title: '退款时间', width: 120 });
            dataColumn.push({
                field: "Amount", title: '金额', width: 100, align: 'center',
            });
            dataColumn.push({ field: "Id", title: '单号', width: 120 });
            dataColumn.push({
                field: "Remark", title: "备注", width: 200, align: "left",
                formatter: function (value, row, index) {
                    var html = '退款,售后编号:' + row.SourceData;
                    return html;
                }
            });
            break;
        case 6:
            dataColumn.push({
                field: "CreateTime", title: '时间', width: 120
            });
            dataColumn.push({
                field: "Amount", title: '收入', width: 100, align: 'center',
                formatter: function (value, row, index) {
                    var html = '';
                    //if (parseFloat(value) > 0)
                    html = value;
                    return html;
                }
            });
            dataColumn.push({
                field: "Remark", title: "备注", width: 200, align: "left",
                formatter: function (value, row, index) {
                    var html = '佣金收入,订单号:' + row.SourceData;
                    return html;
                }
            });
            break;
    }
    $("#list").empty();
    $("#list").hiMallDatagrid({
        url: url,
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "id",
        pageSize: 15,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams: { capitalType: val },
        columns: [dataColumn],
    });
}
function DoOperate(ids) {
    window.top.open("/Order/ChargePay?orderIds=" + ids, "_self");
}