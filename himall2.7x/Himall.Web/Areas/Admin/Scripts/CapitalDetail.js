var curType = 0;
$(function () {
    typeChoose(0);
    $('#ulstatus li').click(function (e) {
        curType = $(this).val();
        typeChoose($(this).val());
    });
    $(".start_datetime").datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd',
        autoclose: true,
        weekStart: 1,
        minView: 2
    });
    $(".end_datetime").datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd',
        autoclose: true,
        weekStart: 1,
        minView: 2
    });
    //$(".start_datetime").click(function () {
    //    $('.end_datetime').datetimepicker('show');
    //});
    //$(".end_datetime").click(function () {
    //    $('.start_datetime').datetimepicker('show');
    //});

    $('.start_datetime').on('changeDate', function () {
        if ($(".end_datetime").val()) {
            if ($(".start_datetime").val() > $(".end_datetime").val()) {
                $('.end_datetime').val($(".start_datetime").val());
            }
        }

        $('.end_datetime').datetimepicker('setStartDate', $(".start_datetime").val());
    });
    $('#searchButton').click(function () {
        typeChoose(curType);
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
        if (val == 0)
        {
            dataColumn.push({ field: "CreateTime", title: '时间', width: 120 });
            dataColumn.push({
                field: "Amount", title: '收入', width: 100, align: 'center',
                formatter: function (value, row, index) {
                    var html = '';
                    if (parseFloat(value) > 0)
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
                field: "Remark", title: "备注", width: 200, align: "center",
            });
        }
        var url =  '../List';
        switch(val)
        {
            case 1:
                dataColumn.push({
                    field: "CreateTime", title: '领取时间', width: 120});
                dataColumn.push({
                    field: "Amount", title: '金额', width: 100, align: 'center',
                });
                break;
            case 2:
                dataColumn.push({ field: "CreateTime", title: '充值时间', width: 120 });
                dataColumn.push({
                    field: "Amount", title: '金额', width: 100, align: 'center',
                });
                dataColumn.push({ field: "PayWay", title: '充值方式', width: 80 });
                dataColumn.push({ field: "Id", title: '充值单号', width: 120 });
                break;
            case 3:
                url = '../ApplyWithDrawListByUser';
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
                dataColumn.push({ field: "Id", title: '单号', width: 120 });
                break;
            case 5:
                dataColumn.push({ field: "CreateTime", title: '退款时间', width: 120 });
                dataColumn.push({
                    field: "Amount", title: '金额', width: 100, align: 'center',
                });
                dataColumn.push({ field: "Id", title: '单号', width: 120 });
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
            queryParams: { capitalType: val, userid: $('#userid').val(), startTime: $('#inputStartDate').val(), endTime: $('#inputEndDate').val() },
            columns: [dataColumn],
        });
    }