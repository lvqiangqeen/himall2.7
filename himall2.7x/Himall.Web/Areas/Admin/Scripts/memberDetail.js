$(function () {
    var userId = $('#memberId').val();
    queryBuyList();
    queryStatistics(userId);
    $("#searchButton").click(function () { queryBuyList(); });
})
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
    $("#inputStartDate").on('changeDate', function () {
        if ($("#inputEndDate").val()) {
            if ($("#inputStartDate").val() > $("#inputEndDate").val()) {
                $("#inputEndDate").val($("#inputStartDate").val());
            }
        }
        $("#inputEndDate").datetimepicker('setStartDate', $("#inputStartDate").val());
    });
    $('#liBuyRecord').click(function () {
        var _t = $(this);
        _t.siblings().removeClass("active");
        _t.addClass("active");
        $('#maintenanceRecord').hide();
        $('#buyRecord').show();
    });
});

function queryBuyList() {
    var rtstart = $("#inputStartDate").val();
    var rtend = $("#inputEndDate").val();
    var memId = $('#memberId').val();
    $("#recordTable").hiMallDatagrid({
        url: '/Admin/member/GetMemberBuyList',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "Id",
        pageSize: 20,
        pageNumber: 1,
        queryParams: {
            timeStart: rtstart, timeEnd: rtend,
            id: memId
        },
        toolbar: /*"#goods-datagrid-toolbar",*/'',
        operationButtons: "#batchOperate",
        columns:
        [[
              {
                  field: "orderId", title: '订单号', formatter: function (value, row, index) {
                      var id = row.orderId.toString();
                      var html = ["<span class=\"btn-a\">"];
                      html.push("<a href='../../Order/Detail/" + id + "'>" + id + "</a>");
                      html.push("</span>");
                      return html.join("");
                  }
              },
            { field: "shopName", title: '下单门店' },
            { field: "createTimeStr", title: '下单时间' },
            { field: "totalAmount", title: '订单实付金额' },
            { field: "actualPayAmount", title: '订单实收金额' }
        ]]
    });
}
function queryStatistics(userId)
{
    $.ajax({
        type: 'get',
        dataType: 'json',
        data:{userId:userId},
        url: '/Admin/member/GetMemberBuyStatistics',
        success: function (result) {
            if (result.success)
            {
                $('#buyCount').text(result.data.tradeCount);
                $('#buyAmount').text(result.data.tradeAmount);
                if (result.data.sleepDays != null)
                    $('#noBuyDay').text(result.data.sleepDays + '天');
                else {
                    $('#noBuyDay').text('--');
                }
            }
        }
    });
}


