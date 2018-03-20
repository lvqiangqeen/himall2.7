$(function () {
    var status = GetQueryString("status");
    if (status == '' || status == null) { status = -1; }
    else { $("#Status").val(status); }
    query();
    $("#searchBtn").click(function () { query(); });
    AutoComplete();
})

function query() {
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
        pageSize: 10,
        pageNumber: 1,
        queryParams: { userName: $("#autoTextBox").val(), Status: $("#Status").val() },
        toolbar: /*"#goods-datagrid-toolbar",*/'',
        columns:
        [[
            { field: "Id", hidden: true },
            { field: "UserName", title: '帐号' },
            { field: "RealName", title: '姓名' },
            { field: "ShopName", title: '店铺名称' },
            { field: "PassTime", title: '招募时间' },
            { field: "ApplyTime", title: '申请日期', width: 100, },
            {
                field: "Status", title: '状态', width: 80
            },
        {
            field: "operation", operation: true, title: "操作", width: 90,
            formatter: function (value, row, index) {
                 var id = row.Id;
                //var data = JSON.stringify(row);
                var html = ["<span class=\"btn-a\">"];
                if (row.Status == "未审核") {
                    html.push("<a id='link_"+id+"' onclick=\"Audited('" + id + "')\">审核</a>");
                }
                if (row.Status == "已审核") {
                    html.push("<a id='link_" + id + "' onclick=\"Disable('" + id + "')\";'>清退</a>");
                }
                html.push("</span>");
                return html.join("");
            }
        }
        ]]
    });
}
function AutoComplete() {
    //autocomplete
    $('#autoTextBox').autocomplete({
        source: function (query, process) {
            var matchCount = this.options.items;//返回结果集最大数量
            $.post("./getMembers", { "keyWords": $('#autoTextBox').val() }, function (respData) {
                return process(respData);
            });
        },
        formatItem: function (item) {
            return item.value;
        },
        setValue: function (item) {
            return { 'data-value': item.value, 'real-value': item.key };
        }
    });
}

function GetPromoterInfo(id) {
    var data=null;
    $.ajax({
        type: "post",
        async: false,
        url: "GetPromoterInfo",
        data:{id:id},
        success: function (result) {
            if (result.success == true) {
                data= result.data;
            }
            else {
                $.dialog.errorTips("获取用户信息失败！" + result.msg);
            }
        }
    });
    return data;
}



function Audited(id) {
    // alert(row);
    var data = GetPromoterInfo(id);
    if (data == null)
    {
        return;
    }
	var promoterStr='<div class="dialog-form">'+
		'<div class="form-group">'+
			'<label class="label-inline fl">会员帐号</label>'+
			'<p class="help-top">' + data.UserName.replace(/>/g, '&gt;').replace(/</g, '&lt;') + '</p>'+
		'</div>'+
		 '<div class="form-group">'+
			'<label class="label-inline fl">注册时间</label>'+
			'<p class="help-top">' + data.RegDate.replace(/>/g, '&gt;').replace(/</g, '&lt;') + '</p>'+
	    '</div>';
		if(data.CellPhone){
			promoterStr+='<div class="form-group">'+
				'<label class="label-inline fl">手机号码</label>'+
				'<p class="help-top">' + data.CellPhone + '</p>'+
			'</div>';
		}
		if($.trim(data.Address)!=''){
			promoterStr+='<div class="form-group">'+
				'<label class="label-inline fl">住址</label>'+
				'<p class="help-top">' + data.Address + '</p>'+
			'</div>';
		}
		if($.trim(data.ShopName)!=''){
			promoterStr+='<div class="form-group">'+
				'<label class="label-inline fl">店铺名称</label>'+
				'<p class="help-top">' + data.ShopName + '</p>'+
			'</div>';
		}
	   
	promoterStr+='</div>';
    $.dialog({
        title: '销售员审核',
        width: 500,
        lock: true,
        id: 'goodCheck',
        content: promoterStr,
        padding: '0 40px',
        button: [
        {
            name: '同意',
            callback: function () {
                Agree(id);
            },
            focus: true
        }, {
            name: '拒绝',
            callback: function () {
                disAgree(id);
            },
            focus: true
        }]
    });
}

function Agree(id) {
    $.post("./Agree",{id:id}, function (data) {
        if (data.success) {
            $.dialog.succeedTips("审核成功！", function () { window.location = window.location; });
            window.location = window.location;
        }
        else {
            $.dialog.errorTips("操作失败" + data.msg);
        }
    });
}

function disAgree(id) {
    $.post("./DisAgree",{id:id}, function (data) {
        if (data.success) {
            $.dialog.succeedTips("已拒绝该申请！", function () { window.location = window.location; });
        }
        else {
            $.dialog.errorTips("操作失败" + data.msg);
        }
    });
}

function Disable(id) {
    $.post("./Disable", { id: id }, function (data) {
        if (data.success) {
            $.dialog.succeedTips("已成功清退！", function () {window.location = window.location;});

        }
        else {
            $.dialog.errorTips("操作失败" + data.msg);
        }
    });
}