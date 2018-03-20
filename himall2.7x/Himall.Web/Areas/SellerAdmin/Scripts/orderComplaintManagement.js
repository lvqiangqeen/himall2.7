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
$(function () {
	var tips=$(window.parent.document).find('#UnComplaints').text().replace('(','').replace(')','');
	if(tips&&tips>0){
		typeChoose('1')
	}
	var status = GetQueryString('status')
	if(status&&status>0)
	{
	    typeChoose('1');
	}
	else {
		typeChoose('')
	}
	
	function typeChoose(val){
		$('.nav-tabs-custom li').each(function() {
            if($(this).val()==val){
				$(this).addClass('active').siblings().removeClass('active');
			}
        });
		//订单表格
		$("#list").hiMallDatagrid({
			url: './list',
			nowrap: false,
			rownumbers: true,
			NoDataMsg: '没有找到符合条件的数据',
			border: false,
			fit: true,
			fitColumns: true,
			pagination: true,
			idField: "Id",
			pageSize: 15,
			pagePosition: 'bottom',
			pageNumber: 1,
			queryParams: { complaintStatus:val },
			columns:
			[[
				{ field: "OrderId", title: '订单号', width: 120 },
				{ field: "UserName", title: "买家", width: 100, align: "center" },
				{ field: "ComplaintDate", title: "投诉日期", width: 100, align: "center" },
                {
                    field: "OrderTotalAmount", title: "实付金额", width: 100, align: "center",
                    formatter: function (value, row, index) {
                        var html = "<span class='ftx-04'>" + '￥' + value + "</span>";
                        return html;
                    }
                },				
				{ field: "PaymentTypeName", title: "支付方式", width: 100, align: "center" },
				{ field: "UserPhone", title: "联系方式", width: 100, align: "center" },
				{ field: "ComplaintStatus", title: "状态", width: 100, align: "center" },
				{
					field: "operation", operation: true, title: "操作",
					formatter: function (value, row, index) {
						var html = ["<span class=\"btn-a\">"];
					   
						html.push("<a class=\"good-check\" onclick=\"OpenComplaintReason('" + row.OrderId + "','" + myHTMLEnCode(row.ComplaintReason) + "','" + myHTMLEnCode(row.SellerReply) + "')\">查看投诉</a>");
	
						if (row.ComplaintStatus == "等待商家处理") {
						    html.push("<a class=\"good-check\" onclick=\"OpenDealComplaint('" + row.Id + "','" + row.OrderId + "','" + myHTMLEnCode(row.ComplaintReason) + "','" + row.ShopPhone + "','" + row.UserPhone + "')\">完成处理</a>");
						}
						else {
							
						}
						html.push("</span>");
						return html.join("");
					}
				}
			]]
		});
	}


    $('#searchButton').click(function (e) {
		searchClose(e);
        var startDate = $("#inputStartDate").val();
        var endDate = $("#inputEndDate").val();
        var orderId = $.trim($('#txtOrderId').val());
        var complaintStatus = $("#slelctStatus").val(); 
        var userName = $.trim($('#txtUserName').val());
        if ($('.nav-tabs-custom li.active').attr('value') == 1)
            complaintStatus = 1;
        $("#list").hiMallDatagrid('clearReload', { startDate: startDate, endDate: endDate, orderId: orderId, complaintStatus: complaintStatus, userName: userName });
    })


    $('.nav-tabs-custom li').click(function (e) {
		searchClose(e);
        $(this).addClass('active').siblings().removeClass('active');
        if ($(this).attr('type') == 'statusTab') {//状态分类
            //$('#txtOrderId').val('');
            //$('#txtUserName').val('');
            //$("#txtProducdName").val('');
            $(".search-box form")[0].reset();
            $("#list").hiMallDatagrid('clearReload', { complaintStatus: $(this).attr('value') || null });
        }
    });
});

function myHTMLEnCode(str){
    var s = "";
    if (str.length == 0) return "";
    s = str.replace(/&/g, "&amp;");
    s = s.replace(/</g, "&lt;");
    s = s.replace(/>/g, "&gt;");
    s = s.replace(/ /g, "&nbsp;");
  //  s = s.replace(/\'/g, "&#39;");
  //  s = s.replace(/\"/g, "&quot;");
    s = s.replace(/\'/g, "‘");
    s = s.replace(/\"/g, "“");
   // s = s.replace(/<br>/g, "\n");
    // s = s.replace(/\n/g, "<br>");
   // alert(s);

    // alert(s);
    return s;
}

function myHTMLDeCode(str){
    var s = "";
    if (str.length == 0) return "";
    s = str.replace(/&amp;/g, "&");
    s = s.replace(/&lt;/g, "<");
    s = s.replace(/&gt;/g, ">");
    s = s.replace(/&nbsp;/g, " ");
    s = s.replace(/&#39;/g, "\'");
    s = s.replace(/&quot;/g, "\"");
    s = s.replace(/<br>/g, "\n");
    return s;
}


function OpenDealComplaint(id, orderId, complaintReason, shopPhone, userPhone) {
    $.dialog({
        title: '处理投诉',
        lock: true,
		width:500,
        id: 'goodCheck',
        content: ['<div class="dialog-form">',
				'<div class="form-group">',
					'<label class="label-inline fl">订单号</label>',
					'<p class="only-text">' + orderId + '</p>',
				'</div>',
				'<div class="form-group">',
					'<label class="label-inline fl">投诉原因</label>',
					'<p class="only-text">' + complaintReason + '</p>',
				'</div>',				
				'<div class="form-group">',
					'<label class="label-inline fl">会员联系方式</label>',
					'<p class="only-text">' + userPhone + '</p>',
				'</div>',
				'<div class="form-group">',
					'<label class="label-inline fl">商家联系方式</label>',
					'<p class="only-text">' + shopPhone + '</p>',
				'</div>',
                '<div class="form-group">',
                    '<label class="label-inline fl">回复买家</label>',
                    '<textarea class="form-control" cols="38" rows="3" id="txtReply"></textarea>',
                '</div>',
			'</div>'].join(''),
        padding: '0 20px 20px 20px',
        init: function () { $("#txtReply").focus();},
        button: [
        {
            name: '完成处理',
            callback: function () {
                var reply = $('#txtReply').val().replace(/\n/g, "&lt;br&gt;").replace(/(^\s*)|(\s*$)/g,"");
                if (reply == null || reply == "" || reply == undefined) {
                    $.dialog.errorTips("回复不能为空!");
                    $("#txtReply").focus();
                    return false;
                }
                DealComplaint(id, reply);
            },
            focus: true
        }]
    });
}

function OpenComplaintReason(orderId, complaintReason, sellerReply) {
    $.dialog({
        title: '查看原因',
        lock: true,
		width:400,
        id: 'goodCheck',
        content: ['<div class="dialog-form">',
				'<div class="form-group">',
					'<label class="label-inline fl">订单号</label>',
					'<p class="only-text">'+orderId+'</p>',
				'</div>',
				'<div class="form-group">',
					'<label class="label-inline fl">投诉原因</label>',
					'<p class="only-text">' + complaintReason + '</p>',
				'</div>',
                '<div class="form-group">',
					'<label class="label-inline fl">商家回复</label>',
					'<p class="only-text">' + sellerReply + '</p>',
				'</div>',
			'</div>'].join(''),
        padding: '0 40px',
        okVal: '确定',
        ok: function () {
        }
    });
}


function DealComplaint(id, reply) {
    var loading = showLoading();
    $.post('./DealComplaint', { id: id, reply: reply }, function (result) {
        loading.close();
        if (result.success) {
            $.dialog.succeedTips("操作成功！");
            var pageNo = $("#list").hiMallDatagrid('options').pageNumber;
            $("#list").hiMallDatagrid('reload', { pageNumber: pageNo });
        }
        else
            $.dialog.errorTips("操作失败");
    });
}

