$(function () {
    GetData();
    $('#searchButton').click(function () {
        GetData();
    });
})

    function GetData() {

        var dataColumn = [];

        dataColumn.push({ field: "UserCode", title: '会员帐号', width: 120 });
        dataColumn.push({ field: "UserName", title: '会员姓名', width: 120 });
        dataColumn.push({
            field: "Balance", title: '账户可用金额', width: 100, align: 'center'
        });
        dataColumn.push({
            field: "FreezeAmount", title: '冻结金额', width: 100, align: 'center'
        });
        dataColumn.push({
            field: "ChargeAmount", title: "累计充值金额", width: 100, align: "center"
        });
        dataColumn.push({
            field: "operate", title: "操作", width: 140, align: "center",
            formatter: function (value, row, index) {
                var id = row['UserId'].toString();
                var html = ["<span class=\"btn-a\">"];
                html.push("<a href='./Detail/" + id + "'>查看明细</a>");
                html.push("</span>");
                return html.join("");
            }
        });
        
        var url = 'GetMemberCapitals';
        var username =$.trim($('#txtMemName').val());
        $("#list").empty();
        $("#list").hiMallDatagrid({
            url: url,
            nowrap: false,
            rownumbers: true,
            NoDataMsg: '没有找到符合条件的数据',
            border: false,
            fit: true,
            fitColumns: true,
            pagination: true,
            idField: "id",
            pageSize: 15,
            pagePosition: 'bottom',
            pageNumber: 1,
            queryParams: { user: username },
            columns: [dataColumn]
        });
    }