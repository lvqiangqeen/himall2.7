// JavaScript source code
$(function () {

    Query();
    $("#searchBtn").click(function () {
        Query();
    });
});


    function Query() {

        var id = $("#cashDepositId").val();
        var name = $("#name").val();
        var startDate = $("#inputStartDate").val();
        var endDate = $("#inputEndDate").val();

        $("#shopDatagrid").hiMallDatagrid({
            url: "/Admin/CashDeposit/CashDepositDetailList",
            singleSelect: true,
            pagination: true,
            NoDataMsg: '没有找到符合条件的数据',
            idField: "Id",
            pageSize: 15,
            pageNumber: 1,
            queryParams: { "id": id, "name": name, "startDate": startDate, "endDate": endDate },
            toolbar: "#shopToolBar",
            columns:
            [[

                { field: "Id", title: "Id", hidden: true },
                { field: "Date", title: "时间", width: 140 },
                { field: "Balance", title: "金额", width: 140 },
                { field: "Operator", title: "操作人", width: 140 },
                { field: "Description", title: "说明", width: 140 }
            ]]
        });

    };

