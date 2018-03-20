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

    getPage(status);
    $("#dh_" + status).parent().attr("class", "active");

    //搜索
    $('#searchButton').click(function (e) {
        searchClose(e);
        var startDate = $("#inputStartDate").val();
        var endDate = $("#inputEndDate").val();
        var startDates = $("#inputStartDates").val();
        var endDates = $("#inputEndDates").val();
        var shopName = $("#shopName").val();

        $("#list").hiMallDatagrid('reload', { startDate: startDate, endDate: endDate, startDates: startDates, endDates: endDates, shopName: shopName });
    });
});


function getPage(status) {
    if (status == 1 || status == 4) {
        //提现列表,待处理
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
            pageNumber: 1,
            queryParams: { Status: status },
            columns:
            [[
                { field: "ApplyTime", title: "申请时间", width: 140 },
                { field: "ShopName", title: "商家", width: 140 },
                { field: "CashAmount", title: "申请金额", width: 130 },
                { field: "CashType", title: "提现方式", width: 130 },
                { field: "Account", title: "账户", width: 130 },
                { field: "AccountName", title: "收款账户姓名", width: 140 },
                {
                    field: "Id", operation: true, title: "操作",
                    formatter: function (value, row, index) {
                        var id = row.Id.toString();
                        var WithStatus = parseFloat(row.WithStatus.toString());

                        var html = [""];
                        switch (WithStatus) {
                            case 1:
                                html.push("<span class=\"btn-a\">");
                                html.push("<a onclick='DoOperate(" + id + ",0,\"\",\"" + row.CashType + "\")'>审核</a>");
                                html.push("</span>");
                                break;
                            case 4:
                                html.push("<span class=\"btn-a\">");
                                html.push("<a onclick='DoOperate(" + id + ",0,\"\",\""+row.CashType+"\")'>重新付款</a>");
                                html.push("</span>");
                                break;
                        }

                        return html.join('');
                    }
                }
            ]]
        });
    } else if (status == 2) {
        //提现列表,拒绝
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
            pageNumber: 1,
            queryParams: { Status: 2 },
            columns:
            [[
                { field: "ApplyTime", title: "申请时间", width: 140 },
                { field: "DealTime", title: "拒绝时间", width: 140 },
                { field: "ShopName", title: "商家", width: 140 },
                { field: "CashAmount", title: "申请金额", width: 130 },
                { field: "CashType", title: "提现方式", width: 130 },
                { field: "Account", title: "账户", width: 130 },
                { field: "AccountName", title: "收款账户姓名", width: 140 },
                { field: "PlatRemark", title: "备注", width: 140 }
            ]]
        });
    } else if (status == 3) {
        $(".Deal").show();
        //提现列表,成功
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
            pageNumber: 1,
            queryParams: { Status: 3 },
            columns:
            [[
                { field: "DealTime", title: "审核时间", width: 140 },
                { field: "ApplyTime", title: "申请时间", width: 140 },
                { field: "ShopName", title: "商家", width: 140 },
                { field: "CashAmount", title: "申请金额", width: 130 },
                { field: "CashType", title: "提现方式", width: 130 },
                { field: "Account", title: "账户", width: 130 },
                { field: "AccountName", title: "收款账户姓名", width: 140 },
                { field: "AccountNo", title: "交易流水号", width: 140 },
                { field: "PlatRemark", title: "备注", width: 140 }
            ]]
        });
    }
}

function DoOperate(id, action, msg,cashType) {
    if (action == 1) {
        $.dialog({
            title: '查看原因',
            lock: true,
            id: 'showRemrk',
            content: ['<div class="dialog-form">',
                '<div class="form-group">',
                    '<p class="help-esp">备注</p>',
                    '<textarea id="auditMsgBox" class="form-control" cols="61" rows="2"  >' + msg + '</textarea>\
                 <p id="valid" style="visibility:hidden;color:red;line-height:18px;">请填写未通过理由</p><p id="validateLength" style="visibility:hidden;color:red;line-height:18px;padding:0;">备注在40字符以内</p> ',
                '</div>',
            '</div>'].join(''),
            padding: '0 40px',
            init: function () { $("#auditMsgBox").focus(); },
            button: [
            {
                name: '关闭',
                callback: function () { },
                focus: true
            }
            ]
        })
    }
    else {

        var weiTitle = "处理提现到微信钱包申请";
        var bankTitle = "处理提现到银行申请";

       var bankMsg = "商家提现的账户为银行账户，需要人工给商家转账，是否已确认完成转账？";
        var WeiMsg = "商家提现的账户为微信账户，确认后将自动转账到商家提现账户，是否确认转账？";
        var confrimMsg = bankMsg;
        var title = bankTitle;
        if (cashType == "微信")
        {
            confrimMsg = WeiMsg;
            title = weiTitle;
        }
        else
        {
            confrimMsg = bankMsg;
            title = bankTitle;
        }
      

        $.dialog({
            title: title,
            lock: true,
            id: 'goodCheck',
            content: ['<div class="dialog-form">',
                 '<div class="form-group">',
                   '<p class="help-esp">' + confrimMsg + '</p>',
                 '</div>',
                '<div class="form-group">',
                    '<p class="help-esp">备注</p>',
                    '<textarea id="auditMsgBox" class="form-control" cols="61" rows="2"  >' + msg + '</textarea>\
                 <p id="valid" style="visibility:hidden;color:red;line-height:18px;">请填写未通过理由</p><p id="validateLength" style="visibility:hidden;color:red;line-height:18px;padding:0;">备注在40字符以内</p> ',
                '</div>',
            '</div>'].join(''),
            padding: '0 40px',
            init: function () { $("#auditMsgBox").focus(); },
            button: [
            {
                name: '付款',
                callback: function () {
                    if ($("#auditMsgBox").val().length > 40) {
                        $('#validateLength').css('visibility', 'visible');
                        return false;
                    }
                    ConfirmPay(id, 3, $("#auditMsgBox").val());
                },
                focus: true
            },
            {
                name: '拒绝',
                callback: function () {
                    if (!$.trim($('#auditMsgBox').val())) {
                        $('#valid').css('visibility', 'visible');
                        return false;
                    }
                    else if ($("#auditMsgBox").val().length > 40) {
                        $('#validateLength').css('visibility', 'visible');
                        return false;
                    }
                    else {
                        $('#valid').css('visibility', 'hidden');
                        ConfirmPay(id, 2, $("#auditMsgBox").val());
                    }
                }
            }]
        });
    }
}

function ConfirmPay(id, status, msg) {
    $.post('ConfirmPay', { id: id, status: status, remark: msg }, function (result) {
        if (result.success) {
            $.dialog.succeedTips(result.msg);
            $('#searchButton').trigger('click');
        }
        else {
            $.dialog.alert(result.msg);
        }
    });
}