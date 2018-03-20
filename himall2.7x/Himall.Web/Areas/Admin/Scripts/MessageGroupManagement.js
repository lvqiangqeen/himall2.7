// JavaScript source code
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

    $('.start_datetime,.end_datetime').keydown(function (e) {
        e = e || window.event;
        var k = e.keyCode || e.which;
        if (k != 8 && k != 46) {
            return false;
        }
    });

    $("#list").hiMallDatagrid({
        url: "./GetSendRecords",
        singleSelect: true,
        pagination: true,
        NoDataMsg: '没有找到符合条件的数据',
        idField: "Id",
        pageSize: 15,
        pageNumber: 1,
        queryParams: {},
        columns:
        [[
            { field: "MsgType", title: "类型", width: 200, },
            {
                field: "UseRate", title: "整体使用率", width: 200, formatter: function (value, row, index) {
                    if (row.CurrentCouponCount > 0 && row.MsgType == "优惠券") {
                        var rate = row.CurrentUseCouponCount / row.CurrentCouponCount;
                        rate = rate.toFixed(2) * 100;
                        return rate + "%";
                    }
                    else if (row.CurrentCouponCount == 0 && row.MsgType == "优惠券") {
                        return 0 + "%";
                    } else if (row.MsgType != "优惠券") {
                        return "";
                    }
                }
            },
            { field: "SendTime", title: "发送时间", width: 200 },
            { field: "SendToUser", title: "发送对象", width: 120 },
            { field: "SendState", title: "发送状态", width: 120 }
        ]]
    });
    $('#searchBtn').click(function (e) {
        searchClose(e);
        var startDate = $("#inputStartDate").val();
        var endDate = $("#inputEndDate").val();
        var messageType = $.trim($('#MessageType').val());
        var sendState = $.trim($('#SendState').val());
        $("#list").hiMallDatagrid('reload', {
            startDate: startDate, endDate: endDate, 
            msgType: messageType, sendState: sendState
        });
    })
});