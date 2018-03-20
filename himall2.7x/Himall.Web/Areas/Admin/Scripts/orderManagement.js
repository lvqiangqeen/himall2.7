
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
    //$( ".start_datetime" ).click( function ()
    //{
    //    $( '.end_datetime' ).datetimepicker( 'show' );
    //} );
    //$( ".end_datetime" ).click( function ()
    //{
    //    $( '.start_datetime' ).datetimepicker( 'show' );
    //} );

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
    


});

$(function () {
    var status = GetQueryString("status");
    var li = $("li[value='" + status + "']");
    if (li.length > 0) {
        li.addClass('active').siblings().removeClass('active');
    }

    $("#list").hiMallDatagrid({
        url: './list',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "OrderId",
        pageSize: 15,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams: { status: status, paymentType: 0 },
        operationButtons: "#orderOperate",
        rowHeadFormatter: function (target, index, row) {
		    return '<tr class="child-title"><td colspan="7" style="padding:10px 15px; background:#fff;border:0;font-size: 12px; color:#6b6c6e;"><img src="' + row.IconSrc + '" title="' + (row.PlatformText == 'Android' ? 'App' : row.PlatformText) + '订单' + '" width="16" style="margin:0 8px 0 0;position:relative;top:-1px;" /> 订单号： ' + row.OrderId +(row.OrderType == 3?'<span style="margin-left:10px">(拼团订单)</span>':'')+' &nbsp;&nbsp;&nbsp;&nbsp; ' + row.OrderDate + '<span class="pull-right">'+(row.PaymentTypeStr||'')+'</span></td></tr>';
		},
		rowFootFormatter: function (target, index, row) {
		    var html = [];
		    var Remaval = row.SellerRemarkFlag;
		    if (row.SellerRemark){
			    html.push('<tr class="child-title"><td colspan="7" style="background:#fff; padding:6px 10px 6px 18px; word-break:break-all;"><div class="form-group mb0">');
			    if (Remaval == 1) {
			        html.push("<span class='iconfont f01' style='padding-right: 2px;'>&#xe630;</span> <em class='flag-states'></em>");
			    } else if (Remaval == 2) {
			        html.push("<span class='iconfont f02' style='padding-right: 2px;'>&#xe630;</span> <em class='flag-states'></em>");
			    } else if (Remaval == 3) {
			        html.push("<span class='iconfont f03' style='padding-right: 2px;'>&#xe630;</span> <em class='flag-states'></em>");
			    } else if (Remaval == 4) {
			        html.push("<span class='iconfont f04' style='padding-right: 2px;'>&#xe630;</span> <em class='flag-states'></em>");
			    }
			    html.push('备注:' + row.SellerRemark);
			    html.push('</div></td></tr>');
		    }
		    html.push('<tr class="child-title"><td colspan="7" style="background:#f0f0f0; border-top:1px solid #e7e7e7; padding:4px 0 3px;border-bottom:0;"></td></tr>');
		    return html.join('');
		},
        columns:
        [[
            {
                field: "ProductName", title: '商品', width: 250,
                formatter: function (value, row, index) {
                    var html=[];
                    for(var i=0;i<row.OrderItems.length;i++)
                    {
                        var showUnit = row.OrderItems[i].Unit || "";
                    	html.push('<div class="img-list" style="margin-left:15px;">'+
			            	'<img src="'+row.OrderItems[i].ThumbnailsUrl+'">'+
			            	'<span class="overflow-ellipsis"><a title="' + row.OrderItems[i].ProductName + '" href="/product/detail/' + row.OrderItems[i].ProductId + '" target="_blank" >' + row.OrderItems[i].ProductName + '</a>'+
			            	'<p>￥' + row.OrderItems[i].SalePrice.toFixed(2) + ' &nbsp; ' + row.OrderItems[i].Quantity + showUnit  + '</p></span>' +
			            	'</div>');
                    }
                    return html.join('');
                }
            },
            {
                field: "TotalPrice", title: "订单总额", width: 80, align: "center",
                formatter: function (value, row, index) {
                    var html = "<span class='ftx-04'>" + '￥' + value.toFixed(2) + "</span>";
                    return html;
                }
            },
        {
            field: "UserName", title: "买家", width: 70, align: "center", formatter: function (value, row, index) {
                return row.UserName + '<br/>' + row.CellPhone;
            }
        },
		{ field: "ShopName", title: "店铺名称", width: 140, align: "center" },
        { field: "OrderStatus", title: "订单状态", width: 80, align: "center" },
        {
            field: "operation", operation: true, title: "操作", width: 140,
            formatter: function (value, row, index) {
                var id = row.OrderId.toString();
                var html = ["<span class=\"btn-a\">"];
                html.push("<a href='./Detail/" + id + "' target=\"_blank\">查看</a>");
                if (row.OrderStatus == "待付款") {
                    html.push("<a class=\"good-check\" onclick=\"OpenConfirmPay('" + id + "')\">确认收款</a>");
                    html.push("<a class=\"good-check\" onclick=\"OpenCloseOrder('" + id + "')\">取消</a>");
                }
                html.push("</span>");
                return html.join("");
            }
        }
        ]]
    });

    $('#searchButton').click(function (e) {
        searchClose(e);
        var startDate = $("#inputStartDate").val();
        var endDate = $("#inputEndDate").val();
        var orderId = $.trim($('#txtOrderId').val());
        var shopName = $.trim($('#txtShopName').val());
        var userName = $.trim( $( '#txtUserName' ).val() );
        var paymentTypeGateway = $.trim( $( '#selectPaymentTypeName' ).val() );
        var paymentType = $.trim($('#selectPaymentType').val());
        var txtUserContact = $.trim($('#txtUserContact').val());
        var orderType = $("#orderType").val();
        $("#list").hiMallDatagrid('reload', {
        	startDate: startDate, endDate: endDate, orderId: orderId, shopName: shopName,
        	orderType: orderType, userName: userName, paymentTypeGateway: paymentTypeGateway, paymentType: paymentType,
        	userContact: txtUserContact
        });
    })


    $('.nav-tabs-custom li').click(function (e) {
        searchClose(e);
        $(this).addClass('active').siblings().removeClass('active');
        if ( $( this ).attr( 'type' ) == 'statusTab' )
        {//状态分类
            $('#txtOrderId').val('');
            $('#txtShopName').val('');
            $('#txtuserName').val('');
            $(".search-box form")[0].reset();
            $("#list").hiMallDatagrid('clearReload', { status: $(this).attr('value') == 0 ? null : $(this).attr('value') });
        }
    });
});

function OpenConfirmPay(orderId) {

    $.dialog({
        title: '确认收款',
        lock: true,
        id: 'goodCheck',
        content: ['<div class="dialog-form">',
            '<div class="form-group">',
                '<p class="help-esp">收款备注</p>',
                '<textarea id="txtPayRemark" class="form-control" cols="40" rows="2" onkeyup="this.value = this.value.slice(0, 50)" ></textarea>\
                 <p id="valid" style="visibility:hidden;color:red">请填写未通过理由</p> ',
            '</div>',
        '</div>'].join(''),
        padding: '10px',
        init: function () { $("#txtPayRemark").focus(); },
        button: [
        {
            name: '确认收款',
            callback: function () {
                ConfirmPay(orderId, $('#txtPayRemark').val());
            },
            focus: true
        }]
    });
}

function OpenCloseOrder(orderId) {
    $.dialog({
        title: '取消订单',
        lock: true,
        id: 'goodCheck',
        content: ['<div class="dialog-form">',
            '<div class="form-group">',
                '<p  style="padding:0">确认要取消订单吗？取消后订单将会是关闭状态。</p>',
            '</div>',
        '</div>'].join(''),
        padding: '20px 60px',
        button: [
        {
            name: '确认取消',
            callback: function () {
                CloseOrder(orderId);
            },
            focus: true
        }]
    });
}


function ConfirmPay(orderId, payRemark) {
    var loading = showLoading();
    $.post('./ConfirmPay', { orderId: orderId, payRemark: payRemark }, function (result) {
        if (result.success) {
            $.dialog.succeedTips("操作成功！");
            var pageNo = $("#list").hiMallDatagrid('options').pageNumber;
            $("#list").hiMallDatagrid('reload', { pageNumber: pageNo });
        }
        else
            $.dialog.errorTips("操作失败"+result.msg);
        loading.close();
    });
}

function CloseOrder(orderId) {
    var loading = showLoading();
    $.post('./CloseOrder', { orderId: orderId }, function (result) {
        if (result.success) {
            $.dialog.succeedTips("操作成功！");
            var pageNo = $("#list").hiMallDatagrid('options').pageNumber;
            $("#list").hiMallDatagrid('reload', { pageNumber: pageNo });
        }
        else
            $.dialog.errorTips("操作失败");
        loading.close();
    });
}

function ExportExecl() {
    var status = $('.nav-tabs-custom li[class="active"]').attr("value") == undefined ? null : $('.nav-tabs-custom li[class="active"]').attr("value");
	if (status == 0)
		status = null;
    var startDate = $("#inputStartDate").val();
    var endDate = $("#inputEndDate").val();
    var orderId = $.trim($('#txtOrderId').val());
    var shopName = $.trim($('#txtShopName').val());
    var userName = $.trim($('#txtUserName').val());
    var paymentTypeGateway = $.trim($('#selectPaymentTypeName').val());
    var selectPaymentType = $.trim($('#selectPaymentType').val());
    var txtUserContact = $.trim($('#txtUserContact').val());

    var href = "/Admin/Order/ExportToExcel?status={0}&startDate={1}&endDate={2}&orderId={3}&shopName={4}&userName={5}&paymentTypeGateway={6}&paymentType={7}&userContact={8}"
	.format(status, startDate, endDate, orderId, shopName, userName, paymentTypeGateway, selectPaymentType, txtUserContact);

    $("#aExport").attr("href", href);
}
