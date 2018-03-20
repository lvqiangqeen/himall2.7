
$(function () {
   var status=GetQueryString("status");
    var li = $("li[value='" + status + "']");
    if (li.length > 0) {
        typeChoose('3')
    } else {
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
				{ field: "ShopName", title: "店铺", width: 120, align: "center" },
				{ field: "UserName", title: "投诉会员", width: 80, align: "center" },
                {
                    field: "ComplaintReason", title: '投诉原因', width: 280, align: 'center',
                    formatter: function (value) {
                        var html = '<span class="overflow-ellipsis" style="width:300px;color:#333" title="'+value+'">'+ value+'</span>';
                        return html;
                    }
                },
				{ field: "ComplaintDate", title: "投诉日期", width: 70, align: "center" },
				{ field: "ComplaintStatus", title: "状态", width: 100, align: "center" },
				{
					field: "operation", operation: true, title: "操作",
					formatter: function (value, row, index) {
						var html = ["<span class=\"btn-a\">"];
					   
						if (row.ComplaintStatus == "等待平台介入") {
						    html.push("<a class=\"good-check\" onclick=\"OpenDealComplaint('" + row.Id + "','" + row.OrderId + "','" +myHTMLEnCode(row.ComplaintReason) + "','" + row.SellerReply + "','" + row.ShopPhone + "','" + row.UserPhone + "')\">处理</a>");
						}
						else {
						    html.push("<a class=\"good-check\" onclick=\"OpenComplaintReason('" + row.OrderId + "','" + myHTMLEnCode(row.ComplaintReason) + "','" + row.SellerReply + "')\">查看回复</a>");
						}
						html.push("</span>");
						return html.join("");
					}
				}
			]]
		});
	}

	function searchdata()
	{
	    var startDate = $("#inputStartDate").val();
	    var endDate = $("#inputEndDate").val();
	    var orderId = $.trim($('#txtOrderId').val());
	    var complaintStatus = $("#slelctStatus").val();
	    var shopName = $.trim($('#txtShopName').val());
	    var userName = $.trim($('#txtUserName').val());
	    if ($('.nav-tabs-custom li.active').attr('value') == 3) {
	        complaintStatus = 3;
	    }
	    $("#list").hiMallDatagrid('clearReload', { startDate: startDate, endDate: endDate, orderId: orderId, complaintStatus: complaintStatus, shopName: shopName, userName: userName });
	}

    $('#searchButton').click(function (e) {
		searchClose(e);
		searchdata();
    })


    $('.nav-tabs-custom li').click(function (e) {
		searchClose(e);
		$(this).addClass('active').siblings().removeClass('active');
		if ($(this).attr("value") == 3) {
		    $(".dst").hide();
		} else {
		    $(".dst").show();
		}
        if ($(this).attr('type') == 'statusTab') {//状态分类
            //$('#txtOrderId').val('');
            //$('#txtShopName').val('');
            //$('#txtUserName').val('');
            //$("#txtProducdName").val('');
            $(".search-box form")[0].reset();
            searchdata();
        }
    });
});

function OpenDealComplaint(id, orderId, complaintReason, sellerReply, shopPhone, userPhone) {
    $.dialog({
        title: '投诉处理',
        lock: true,
		width:466,
        id: 'goodCheck',
        content: ['<div class="dialog-form">',
				'<div class="form-group">',
					'<label class="label-inline fl">订单号</label>',
					'<p class="only-text">' + orderId + '</p>',
				'</div>',
				'<div class="form-group">',
					'<label class="label-inline fl">投诉原因</label>',
					'<p class="only-text">' + complaintReason + '&nbsp;</p>',
				'</div>',
				'<div class="form-group">',
					'<label class="label-inline fl">商家回复</label>',
					'<p class="only-text">' + sellerReply + '&nbsp;</p>',
				'</div>',
				'<div class="form-group">',
					'<label class="label-inline fl">会员联系方式</label>',
					'<p class="only-text">' + userPhone + '</p>',
				'</div>',
				'<div class="form-group">',
					'<label class="label-inline fl">商家联系方式</label>',
					'<p class="only-text">' + shopPhone + '</p>',
				'</div>',
			'</div>'].join(''),
        padding: '0 40px',
        button: [
        {
            name: '协调完成',
            callback: function () {
                DealComplaint(id);
            },
            focus: true
        }]
    });
}

function OpenComplaintReason(orderId, complaintReason, sellerReply) {
    $.dialog({
        title: '查看原因',
        lock: true,
		width:466,
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
        padding: '0 20px',
        okVal: '确定',
        ok: function () {
        }
    });
}


function DealComplaint(id) {
    var loading = showLoading();
    $.post('./DealComplaint', { id: id }, function (result) {
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

function myHTMLEnCode(str) {
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
};

function myHTMLDeCode(str) {
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