$(function () {
    query();
    $("#searchBtn").click(function () { query(); });
})

function query() {
    url=$("#url").val();
    $("#list").hiMallDatagrid({
        url:url ,
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到任何的会员积分信息',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "Id",
        pageSize: 10,
        pageNumber: 1,
        queryParams: { startDate: $("#inputStartDate").val(), endDate: $("#inputEndDate").val(), type: $("#type").val(),userId:$("#userId").val() },
        toolbar: /*"#goods-datagrid-toolbar",*/'',
        operationButtons: "",
        columns:
        [[
            { field: "Id", hidden: true },
            { field: "UserName", title: '会员名' },
            { field: "Integral", title: '积分值' },
            { field: "Type", title: '积分类型' },
            { field: "RecordDate", title: '时间' },
            { field: "Remark", title: '备注' },

        ]]
    });
}
