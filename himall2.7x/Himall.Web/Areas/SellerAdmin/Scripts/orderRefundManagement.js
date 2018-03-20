$(function () {
    var status = GetQueryString('status');
    var OrderId = GetQueryString('orderid')
    var showtype = $("#Showty").val();
    if (status && status > 0) {
        typeChoose('1');
    }
    else {
        typeChoose('')
    }

    function typeChoose(val) {
        $('.nav-tabs-custom li').each(function () {
            if ($(this).val() == val) {
                $(this).addClass('active').siblings().removeClass('active');
            }
        });
    }

    //组合显示字段
    try {
        showtype = parseInt(showtype, 10);
    } catch (ex) {
        showtype = 0;
    }
    var isOpenStore = { field: "门店未授权" };
    if ($("#isOpenStore").val() == "True") {
        isOpenStore = { field: "ShopBranchName", title: "门店名称", width: 140, align: "center" };
    }
    datacols = [[
            {
                field: "OrderId", title: '订单号', width: 120,
                formatter: function (value, row, index) {
                    return '<a href="/SellerAdmin/order/Detail/' + value + '" target="_blank">' + value + '</a>';
                }
            },
            { field: "ProductName", title: "商品", width: 280, align: "center" },
            isOpenStore,
            { field: "UserName", title: "买家", width: 80, align: "center" },
            { field: "ApplyDate", title: "申请日期", width: 140, align: "center" },
            {
                field: "Amount", title: "退款金额", width: 90, align: "center",
                formatter: function (value, row, index) {
                    return '￥' + value;
                }
            }]];
    switch (showtype) {
        case 0:
        case 3:
            datacols[0].push({ field: "ReturnQuantity", title: "退货数量", width: 80, align: "center" });
            break;
    }

    datacols[0] = datacols[0].concat([
        { field: "RefundStatus", title: "退款状态", width: 90, align: "center" },
        {
            field: "operation", operation: true, title: "操作",
            formatter: function (value, row, index) {
                var html = ["<span class=\"btn-a\">"];
                // html.push("<input type=\"hidden\" name=\"rowdata\" id=\"rowdata-" + row.RefundId + "\" value='" + JSON.stringify(row) + "'>");
                if (row.ShopBranchId > 0) {//门店售后订单只能查看不可进行操作
                    html.push("<a class=\"good-check\" onclick=\"ShowRefundInfo('" + index + "')\">查看原因</a>");
                } else {
                    switch (row.AuditStatus) {
                        case "待商家审核":
                            html.push("<a class=\"good-check\" onclick=\"OpenDealRefund('" + index + "')\">审核</a>");
                            html.push("<br><span style='font-size:12px;color:#666'>还剩" + secondFormat(row.nextSecond) + "</span>");
                            break;
                        case "待商家收货":
                            html.push("<a class=\"good-check\" onclick=\"OpenConfirmGood('" + row.RefundId + "','" + row.ExpressCompanyName + "','" + row.ShipOrderNumber + "')\">审核</a>");
                            html.push("<br><span> 还剩" + secondFormat(row.nextSecond) + "</span>");
                            break;
                        default:
                            html.push("<a class=\"good-check\" onclick=\"ShowRefundInfo('" + index + "')\">查看原因</a>");
                            break;
                    }
                }
                html.push("</span>");
                return html.join("");
            }
        }
    ]);

    //订单表格
    $("#list").hiMallDatagrid({
        url: './list?showtype=' + showtype,
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的退款退货记录',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "RefundId",
        pageSize: 15,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams: { auditStatus: status,orderId:OrderId},
        columns: datacols
    });

    $('#searchButton').click(function (e) {
        searchClose(e);
        var startDate = $("#inputStartDate").val();
        var endDate = $("#inputEndDate").val();
        var orderId = $.trim($('#txtOrderId').val());
        var productName = $.trim($('#txtProductName').val());
        var userName = $.trim($('#txtUserName').val());
        var shopBranchId = $("#shopBranchId").val();
        $("#list").hiMallDatagrid('clearReload', { startDate: startDate, endDate: endDate, orderId: orderId, productName: productName, userName: userName, shopBranchId: shopBranchId });
    })


    $('.nav-tabs-custom li').click(function (e) {
        searchClose(e);
        $(this).addClass('active').siblings().removeClass('active');
        if ($(this).attr('type') == 'statusTab') {//状态分类
            //$('#txtOrderId').val('');
            //$('#txtUserName').val('');
            //$("#txtProducdName").val('');
            $(".search-box form")[0].reset();
            $("#list").hiMallDatagrid('clearReload', { auditStatus: $(this).attr('value') || null });
        }
    });
});

//倒计时
function secondFormat(intDiff) {
	var day = 0,
		hour = 0,
		minute = 0,
		second = 0,
		result =0;        
	if (intDiff > 0) {
		day = Math.floor(intDiff / (60 * 60 * 24));
		hour = Math.floor(intDiff / (60 * 60)) - (day * 24);
		minute = Math.floor(intDiff / 60) - (day * 24 * 60) - (hour * 60);
	}
	if (minute <= 9) minute = '0' + minute;
	if (second <= 9) second = '0' + second;
	
	if(day<=0)
		result=hour+'小时';
	else if (day <= 0 && hour <= 0 && minute>20)
	    result = '1小时';
	else if (day <= 0 && hour <= 0 && minute <= 20)
	    result = '即将执行';
	else
		result=day+'天'+hour+'小时';
	
	return result;


}

function OpenDealRefund(rowIndex) {
   // var dobj = $("#rowdata-" + refundId);
    //var data = jQuery.parseJSON(dobj.val());
    var data = $("#list").hiMallDatagrid('getRowByIndex', rowIndex);
    var jettisonRadio = "";

    dlgcontent = ['<div class="dialog-form">',
            '<div class="form-group">',
                '<label class="label-inline fl">售后编号</label>',
                '<p class="only-text">' + data.RefundId + '</p>',
            '</div>',
            '<div class="form-group">',
                '<label class="label-inline fl">商品名称</label>',
                '<p class="only-text">' + data.ProductName + '</p>',
            '</div>'];
    dlgcontent = dlgcontent.concat(['<div class="form-group">',
                '<label class="label-inline fl">退款金额</label>',
                '<p class="only-text"><span class="cor-red">￥' + data.Amount + '</span>（实付：' + data.SalePrice + '）</p>',
            '</div>']);
    if (data.RefundMode != 1) {
        if (data.ReturnQuantity > 0) {
            dlgcontent = dlgcontent.concat(['<div class="form-group">',
                        '<label class="label-inline fl">退货数量</label>',
                        '<p class="only-text"><span class="cor-red">' + data.ReturnQuantity + "</span>（购买：" + data.Quantity + "）" + '</p>',
                    '</div>']);
        }
    } else {
        data.ReturnQuantity = 0;
    }
    dlgcontent = dlgcontent.concat([
            '<div class="form-group">',
                '<label class="label-inline fl">理由</label>',
                '<p class="only-text">' + data.Reason.replace(/>/g, '&gt;').replace(/</g, '&lt;') + '</p>',
            '</div>',
              '<div class="form-group">',
                '<label class="label-inline fl">原因</label>',
                '<p class="only-text">' + data.ReasonDetail+ '</p>',
            '</div>',
            '<div class="form-group">',
                '<label class="label-inline fl">联系人</label>',
                '<p class="only-text">' + data.ContactPerson + "（" + data.ContactCellPhone + "）" + '</p>',
            '</div>',
            '<div class="form-group">',
                '<label class="label-inline fl">期望退款方式</label>',
                '<p class="only-text">' + data.RefundPayType + '</p>',
            '</div>']);
    if (data.CertPic1 || data.CertPic2|| data.CertPic3) {
        dlgcontent = dlgcontent.concat([
                '<div class="form-group">',
                    '<label class="label-inline fl">售后凭证</label>',
                   
                    '<p class="after-service-img">']);
        if (data.CertPic1) {
            dlgcontent = dlgcontent.concat(['<img width="50" height="50" src="' + data.CertPic1 + '">']);
        }
        if (data.CertPic2) {
            dlgcontent = dlgcontent.concat(['<img width="50" height="50" src="' + data.CertPic2 + '">']);
        }
        if (data.CertPic3) {
            dlgcontent = dlgcontent.concat(['<img width="50" height="50" src="' + data.CertPic3 + '">']);
        }
        dlgcontent = dlgcontent.concat([
                    '</p>',
                '</div>'
        ]);
    }
    dlgcontent = dlgcontent.concat([
            '<div class="form-group">',
                '<label class="label-inline fl">回复买家</label>',
                '<p class="only-text">',
                '<textarea class="form-control" type="text" name="txtRefundRemark" id="txtRefundRemark" placeholder="回复买家" style="width:100%;" />',
                '</p>',
            '</div>'
    ]);
    if (data.nextSecond > 0) {
        dlgcontent = dlgcontent.concat([
                '<div class="form-group">',
                    '<p class="only-text" style="font-weight:bold;padding-left:0;text-align:center">还剩<em style="color:#e3393c;">' + secondFormat(data.nextSecond) + '</em>，逾期不处理流程自动进入下一环节</p>',
                '</div>'
        ]);
    }

    var dlgbt = [{
        name: '拒绝售后',
        callback: function () {
            sellerRemark = $('#txtRefundRemark').val();
            if (sellerRemark.length < 1) {
                art.dialog.alert('请输入拒绝理由！');
                return false;
            }
            DealRefund(data.RefundId, 4, sellerRemark);
        }
    }];
    if (data.ReturnQuantity > 0) {
        dlgbt.push({
            name: '同意并弃货',
            callback: function () {
                DealRefund(data.RefundId, 5, $('#txtRefundRemark').val());
            }
        });
    }

    dlgbt.push({
        name: '同意售后',
        callback: function () {
            DealRefund(data.RefundId, 2, $('#txtRefundRemark').val());
        },
        focus: true
    });

    $.dialog({
        title: '退货退款审核',
        lock: true,
        id: 'handlingComplain',
        width:'500px',
        content: dlgcontent.join(''),
        padding: '0 40px',
        init: function () { $("#txtRefundRemark").focus(); },
        button: dlgbt
    });
 
}

function ShowRefundInfo(rowIndex) {
   // var dobj = $("#rowdata-" + refundId);
    //  var data = jQuery.parseJSON(dobj.val());
    var data = $("#list").hiMallDatagrid('getRowByIndex', rowIndex)
    var jettisonRadio = "";

    dlgcontent = ['<div class="dialog-form">',
            '<div class="form-group">',
                '<label class="label-inline fl">售后编号</label>',
                '<p class="only-text">' + data.RefundId + '</p>',
            '</div>',
            '<div class="form-group">',
                '<label class="label-inline fl">商品名称</label>',
                '<p class="only-text">' + data.ProductName + '</p>',
            '</div>'
    ];
    dlgcontent = dlgcontent.concat(['<div class="form-group">',
                '<label class="label-inline fl">退款金额</label>',
                '<p class="only-text"><span class="cor-red">￥' + data.Amount + '</span>（实付：' + data.SalePrice + '）</p>',
            '</div>']);
    if (data.RefundMode != 1) {
        if (data.ReturnQuantity > 0) {
            dlgcontent = dlgcontent.concat(['<div class="form-group">',
                        '<label class="label-inline fl">退货数量</label>',
                        '<p class="only-text"><span class="cor-red">' + data.ReturnQuantity + "</span>（购买：" + data.Quantity + "）" + '</p>',
                    '</div>']);
        }
    } else {
        data.ReturnQuantity = 0;
    }
    dlgcontent = dlgcontent.concat([
            '<div class="form-group">',
                '<label class="label-inline fl">理由</label>',
                '<p class="only-text">' + data.Reason.replace(/>/g, '&gt;').replace(/</g, '&lt;') + '</p>',
            '</div>',
              '<div class="form-group">',
                '<label class="label-inline fl">原因</label>',
                '<p class="only-text">' + data.ReasonDetail + '</p>',
            '</div>',
            '<div class="form-group">',
                '<label class="label-inline fl">联系人</label>',
                '<p class="only-text">' + data.ContactPerson + "（" + data.ContactCellPhone + "）" + '</p>',
            '</div>',
            '<div class="form-group">',
                '<label class="label-inline fl">期望退款方式</label>',
                '<p class="only-text">' + data.RefundPayType + '</p>',
            '</div>'
    ]);
    if (data.CertPic1 || data.CertPic2 || data.CertPic3) {
        dlgcontent = dlgcontent.concat([
                '<div class="form-group">',
                    '<label class="label-inline fl">售后凭证</label>',
                    '<p class="after-service-img">']);
        if (data.CertPic1) {
            dlgcontent = dlgcontent.concat(['<img src="' + data.CertPic1 + '">']);
        }
        if (data.CertPic2) {
            dlgcontent = dlgcontent.concat(['<img src="' + data.CertPic2 + '">']);
        }
        if (data.CertPic3) {
            dlgcontent = dlgcontent.concat(['<img src="' + data.CertPic3 + '">']);
        }
        dlgcontent = dlgcontent.concat([
                    '</p>',
                '</div>'
        ]);
    }
    if (data.SellerRemark) {
        dlgcontent = dlgcontent.concat([
                '<div class="form-group">',
                    '<label class="label-inline fl">商家备注</label>',
                    '<p class="help-top">' + data.SellerRemark.replace(/>/g, '&gt;').replace(/</g, '&lt;') + '</p>',
               ' </div>']);
    }
    if (data.ManagerRemark) {
        dlgcontent = dlgcontent.concat(['<div class="form-group">',
                                '<label class="label-inline fl">平台备注</label>',
                                '<p class="only-text">' + data.ManagerRemark.replace(/>/g, '&gt;').replace(/</g, '&lt;') + '</p>',
                            '</div>']);
    }
    dlgcontent = dlgcontent.concat(['<div class="form-group">',
                '<label class="label-inline fl">当前状态</label>',
                '<p class="only-text"><span class="cor-red">' + data.RefundStatus + '</span></p>',
            '</div>',
            '</div>']);

    var dlgbt = [{
        name: '关闭'
    }];

    $.dialog({
        title: '查看退款申请',
        lock: true,
        id: 'handlingComplain',
        width: 500,
        content: dlgcontent.join(''),
        padding: '0 40px',
        init: function () { $("#txtRefundRemark").focus(); },
        button: dlgbt
    });
}

function OpenConfirmGood(refundId, expressCompanyName, shipOrderNumber) {
    $.dialog({
        title: '确认收货',
        lock: true,
        id: 'goodCheck',
        content: ['<div class="dialog-form">',
            '<div class="form-group">',
                '<label class="label-inline fl">物流公司</label>',
                 '<p class="help-top">' + expressCompanyName + '</p>',
            '</div>',
            '<div class="form-group">',
                '<label class="label-inline fl">物流单号</label>',
                 '<p class="help-top">' + shipOrderNumber + '</p>',
            '</div>',
            '<div class="form-group">',
                '<label class="label-inline fl">拒绝理由</label>',
                '<p class="help-top">',
                '<input class="form-control" type="text" name="txtRefundRemark" id="txtRefundRemark" placeholder="输入拒绝理由" />',
                '</p>',
            '</div>',
            '<div class="form-group">',
                '<p class="help-top" style="padding-left: 80px;color: #e3393c;font-size: 12px">确认已经收到订单的退货了吗？</p>',
            '</div>',
        '</div>'].join(''),
        padding: '0 40px',
        button: [
        {
            name: '确认收货',
            callback: function () {
                ConfirmGood(refundId);
            },
            focus: true
        }, {
            name: '拒绝退款',
            callback: function () {
                sellerRemark = $('#txtRefundRemark').val();
                if (sellerRemark.length < 1) {
                    art.dialog.alert('请输入拒绝理由！');
                    return false;
                }
                DealRefund(refundId, 4, sellerRemark);
            }
        }]
    });
}

function DealRefund(refundId, auditStatus, sellerRemark) {
    var loading = showLoading();
    $.post('./DealRefund', { refundId: refundId, auditStatus: auditStatus, sellerRemark: sellerRemark }, function (result) {
        loading.close();
        if (result.success) {
            $.dialog.succeedTips("操作成功！");
            var pageNo = $("#list").hiMallDatagrid('options').pageNumber;
            $("#list").hiMallDatagrid('reload', { pageNumber: pageNo });
        }
        else
            $.dialog.errorTips("操作失败：" + result.msg);
    });
}

function ConfirmGood(refundId) {
    var loading = showLoading();
    $.post('./ConfirmRefundGood', { refundId: refundId }, function (result) {
        loading.close();
        if (result.success) {
            $.dialog.succeedTips("操作成功！");
            var pageNo = $("#list").hiMallDatagrid('options').pageNumber;
            $("#list").hiMallDatagrid('reload', { pageNumber: pageNo });
        }
        else
            $.dialog.errorTips("操作失败");
    });
};

$(function () {

    $(document).on("click", ".after-service-img img", function () {
        $(".preview-img").show();
        $(".preview-img img").attr("src", $(this).attr("src"));
        $(".cover").show();
    });
    $(".preview-img").click(function () {
        $(this).hide()
        $(".cover").hide();
    });
    $(".cover").click(function () {
        $(".preview-img").hide();
        $(".cover").hide();
    })
});



var showtype = $("#Showty").val();
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


});