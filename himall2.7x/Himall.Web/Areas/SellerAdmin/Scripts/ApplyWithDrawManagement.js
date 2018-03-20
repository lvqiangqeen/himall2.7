$(function () {
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

    $('.start_datetime').on('changeDate', function () {
        if ($(".end_datetime").val()) {
            if ($(".start_datetime").val() > $(".end_datetime").val()) {
                $('.end_datetime').val($(".start_datetime").val());
            }
        }

        $('.end_datetime').datetimepicker('setStartDate', $(".start_datetime").val());
    });

    $(".start_datetimes").datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd',
        autoclose: true,
        weekStart: 1,
        minView: 2
    });
    $(".end_datetimes").datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd',
        autoclose: true,
        weekStart: 1,
        minView: 2
    });

    $('.start_datetimes').on('changeDate', function () {
        if ($(".end_datetimes").val()) {
            if ($(".start_datetimes").val() > $(".end_datetimes").val()) {
                $('.end_datetimes').val($(".start_datetimes").val());
            }
        }

        $('.end_datetimes').datetimepicker('setStartDate', $(".start_datetimes").val());
    });

    var Id = GetQueryString('id')

    //商品表格
    $("#list").hiMallDatagrid({
        url: 'List',
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
        operationButtons: "#operationButtons",
        pageNumber: 1,
        queryParams: { Status: 0,id:Id},
        columns:
        [[
            { field: "DealTime", title: "审核时间", hidden: true },
            { field: "ApplyTime", title: "申请时间", width: 140 },
            { field: "CashAmount", title: "提现金额", width: 140 },
            { field: "CashType", title: "提现方式", width: 130 },
            { field: "Account", title: "账户", width: 130 },
            { field: "AccountName", title: "收款账户姓名", width: 130 },
            { field: "AccountNo", title: "交易流水号", width: 140 }
        ]]
    });

    //搜索
    $('#searchButton').click(function (e) {
        searchClose(e);
        var startDate = $("#inputStartDate").val();
        var endDate = $("#inputEndDate").val();
        var startDates = $("#inputStartDates").val();
        var endDates = $("#inputEndDates").val();
        var WithdrawStaus = $("#WithdrawStaus").val();

        $("#list").hiMallDatagrid('reload', { startDate: startDate, endDate: endDate, startDates: startDates, endDates: endDates, WithdrawStaus: WithdrawStaus });
    });
});

function ExportExecl() {
	var applyStartTime = $("#inputStartDate").val();
	var applyEndTime = $("#inputEndDate").val();
	var auditedStartTime = $("#inputStartDates").val();
	var auditedEndTime = $("#inputEndDates").val();
	var status = $("#WithdrawStaus").val();
	var href = $(this).attr('href').split('?')[0] + '?applyStartTime={0}&applyEndTime={1}&auditedStartTime={2}&auditedEndTime={3}&staus={4}'.format(applyStartTime, applyEndTime, auditedStartTime, auditedEndTime, status);
	$(this).attr('href', href);
}