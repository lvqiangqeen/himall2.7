//$(function () {
//    $("#searchBtn").click(function () { query(); });
//    AutoComplete();
	
//})


function deleteBrand(id)
{
    $.dialog.confirm('您确定删除该品牌吗？', function () {
        var loading = showLoading();
        $.post("./IsInUse", { id: id }, function (data) {
            if (data.success == true) {
                loading.close();
                $.dialog.tips("该品牌正在使用，请不要删除！");
                return false;
            }
            else
            {
                $.post("./Delete", { id: id }, function (data) {
                    $.dialog.tips(data.msg);
                    query();
                    loading.close();
                });
            }
        });
        
    });
}


function audit(id) {
    var loading = showLoading();
    $.post("./Audit", { id: id }, function (data) {
        $.dialog.tips(data.msg,function(){location.href = "./Management";});
        loading.close();
        
    });
}

function refuse(id) {
    var loading = showLoading();
    $.post("./Refuse", { id: id }, function (data) {
        $.dialog.tips(data.msg,function(){location.href = "./Management";});
        loading.close();
    });
}

function deleteApply(id) {
    $.dialog.confirm('您确定删除该申请吗？', function () {
        var loading = showLoading();
        $.post("./DeleteApply", { id: id }, function (data) {
            $.dialog.tips(data.msg); queryApply();
            loading.close();
        });
    });
}


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
        pageSize: 15,
        pageNumber: 1,
        queryParams: { keyWords: $("#brandBox").val()},
        toolbar: /*"#goods-datagrid-toolbar",*/'',
        columns:
        [[
            { field: "Id", hidden: true },
            { field: "BrandName", title: '名称' },
            {
                field: "BrandLogo", title: 'LOGO', align: 'center',
                formatter: function (value, row, index) {
                    var html = "";
                    html += '<img style="width="100" height="24" style="float:none!important" src="' + row.BrandLogo + '" />';
                    return html;
                }
            },
        {
            field: "operation", operation: true, title: "操作",
            formatter: function (value, row, index) {
                var id = row.ID.toString();
                var html = ["<span class=\"btn-a\">"];
                if (row.AuditStatus == 1)//仅未审核的品牌需要审核
                    html.push("<a class=\"good-check\" onclick=\"audit('" + id + "')\">审核</a>");
                    html.push("<a href='./Edit/" + id + "'>编辑</a>");
                html.push("<a onclick=\"deleteBrand('" + id + "');\">删除</a>");
                html.push("</span>");
                return html.join("");
            }
        }
        ]]
    });
}


function AutoComplete() {
    //autocomplete
    $('#brandBox').autocomplete({
        source: function (query, process) {
            var matchCount = this.options.items;//返回结果集最大数量
            $.post("./getBrands", { "keyWords": $('#brandBox').val(), AuditStatus: $("#Audits").parent().attr("class") == "active" ?1:2}, function (respData) {
                return process(respData);
            });
        },
        formatItem: function (item) {
            if (item.envalue != null)
            {
                return item.value + "(" + item.envalue + ")";
            }
            return item.value;  
        },
        setValue: function (item) {
            return { 'data-value': item.value, 'real-value': item.key };
        }
    });
}

function queryApply() {
    $("#applyList").hiMallDatagrid({
        url: './ApplyList',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "Id",
        pageSize: 15,
        pageNumber: 1,
        queryParams: { keyWords: $("#brandBox").val() },
        toolbar: /*"#goods-datagrid-toolbar",*/'',
        columns:
        [[
            { field: "Id", hidden: true },
            { field: "ShopName", title: '申请经营方' },
            { field: "BrandName", title: '品牌名称' },
            { field: "ApplyTime", title: '申请时间' },
            //{
            //    field: "BrandLogo", title: 'LOGO', align: 'center',
            //    formatter: function (value, row, index) {
            //        var html = "";
            //        html += '<img style="width="100" height="24" src="' + row.BrandLogo + '?' + Date() + '" />';
            //        return html;
            //    }
            //},
            {
                field: "operation", operation: true, title: "操作",
                formatter: function (value, row, index) {
                    var id = row.Id.toString();
                    var html = ["<span class=\"btn-a\">"];
                    if (row.AuditStatus == 0)//仅未审核的品牌需要审核
                    {
                        //html.push("<a class=\"good-check\" onclick=\"audit('" + id + "')\">审核通过</a>");
                        //html.push("<a class=\"good-check\" onclick=\"refuse('" + id + "')\">拒绝通过</a>");
                        html.push("<a href='./Show?id=" + id + "'>审核</a>");
                    }

                    html.push("<a onclick=\"deleteApply('" + id + "');\">删除</a>");
                    html.push("</span>");
                    return html.join("");
                }
            }
        ]]
    });
};
$(function () {
    $("#uploadImg").himallUpload(
  {
      imgFieldName: "BrandLogo"
  });
});
$(function () {
    query();
    $("#searchBtn").click(function () {
        if ($("#brandBox").val().length > 20) {
            $.dialog.tips("搜索内容不要超过20个文字！");
            return false;
        }
        query();
    });
    AutoComplete();
});