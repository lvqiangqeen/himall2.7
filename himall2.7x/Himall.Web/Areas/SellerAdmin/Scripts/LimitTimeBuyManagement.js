// JavaScript source code
$(function () {

    $('#searchButton').click(function (e) {
        searchClose(e);
        LoadLimitBuy();     
    });


    LoadLimitBuy();

    function LoadLimitBuy() {

        var startDate = $("#inputStartDate").val();
        var endDate = $("#inputEndDate").val();
        var productName = $.trim($('#txtProductName').val());

        $("#list").hiMallDatagrid({
            url: './GetItemList',
            nowrap: false,
            rownumbers: true,
            NoDataMsg: '没有找到符合条件的限时购活动',
            border: false,
            fit: true,
            fitColumns: true,
            pagination: true,
            idField: "Id",
            pageSize: 15,
            pagePosition: 'bottom',
            pageNumber: 1,
            queryParams: { status: $("#AuditStatus").val(), startDate: startDate, endDate: endDate, productName:productName},
            columns:
            [[
                { field: "Id", title: 'Id', hidden: true, width: 150 },
                { field: "ProductId", title: 'ProductId', hidden: true, width: 100 },
                {
                    field: "ProductName", title: "商品名称", align: "center", width: 180,
                    formatter: function (value, row, index) {
                        var html = '<span class="overflow-ellipsis" style="width:180px"><a title="' + value + '" href="/product/detail/' + row.ProductId + '" target="_blank">' + value + '</a></span>';
                        return html;
                    }
                },
                { field: "BeginDate", title: "开始时间", width: 150, align: "center" },
                { field: "EndDate", title: "结束时间", width: 150, align: "center" },
        { field: "LimitCountOfThePeople", title: "限购数", width: 60, align: "center" },
                { field: "SaleCount", title: "销售数", width: 60, align: "center" },
               {
                   field: "StatusStr", title: "状态", width: 80, align: "center",
                   formatter: function (value, row, index) {
                       var html = '<span class="audit_' + row.StatusNum + '">' + value + '</span>';
                       return html;
                   }
               },
            //{
            //    field: "CancelReson", title: "平台审批",width:100,
            //    formatter: function (value, row, index) {
            //        var html = "<a href=\"javascript:showReason('" + value + "')\" >" + (value ? "查看意见" : "") + "</a>";
            //        return html;
            //    }
            //},
            {
                field: "operation", operation: true, width: 150, title: "操作",
                formatter: function (value, row, index) {
                    var id = row.Id.toString();
                    var html = ["<span class=\"btn-a\">"];
                    switch (row.StatusNum) {
                        case 1:
                            html.push("<a class=\"good-check\" href=\"./Edit/" + id + "\">编辑</a>");
                            break;
                        case 2:
                            if (row.IsStarted) {
                                html.push("<a class=\"good-check\" href=\"./Edit/" + id + "\">编辑</a>");
                            }
                            html.push("<a class=\"good-check\" onclick=\"copyurl('\/LimitTimeBuy\/Detail\/" + id + "')\">复制链接</a>");
                            break;
                    }
                    html.push("<a class=\"good-check\" href=\"./detail/" + id + "\">查看</a>");
                    html.push("<a class=\"good-check\" onclick=\"Delete('" + id + "')\">删除</a>");
                    html.push("</span>");
                    return html.join("");
                }
            }
            ]]
        });

    }



    $("#AddItem").click(function () {
        location.href = "./AddLimitItem";
    });
});

function Delete(id)
{
    $.dialog.confirm('删除后该活动相关商品将不享受优惠，是否确认删除？', function () {
        var loading = showLoading();
        ajaxRequest({
            type: 'POST',
            url: "./DeleteItem",
            param: { id: id },
            dataType: "json",
            success: function (data) {
                if (data.success == true) {
                    $.dialog.succeedTips("删除成功！", function () {
                        var pageNo = $("#list").hiMallDatagrid('options').pageNumber;
                        $("#list").hiMallDatagrid('reload', { pageNumber: pageNo });
                    }, 1);
                    loading.close();
                } else {
                    $.dialog.errorTips(data.msg);
                }
            }, error: function () {
                loading.close();
            }
        });
    });
}


function copyurl(url) {
    url = window.location.protocol + "//" + window.location.host + url;
    $.dialog({
        title: '限时购链接',
        lock: true,
        id: 'copydlg',
        content: ['<div class="dialog-form">',
            '<div class="form-group">',
                '<input type="text" id="txturl" value="' + url + '" class="form-control" style="width:300px"/>',
            '</div>',
        '</div>'].join(''),
        padding: '0 40px',
        init: function () { $("#txturl").focus(); }
    });
}

function showReason(msg) {
    $.dialog({
        title: '平台审批意见',
        lock: true,
        id: 'preasondlg',
        content: ['<div class="dialog-form">',
            '<div class="form-group">',
                msg,
            '</div>',
        '</div>'].join(''),
        padding: '0 40px'
    });
}