// JavaScript source code
$( function ()
{
    loadGrid();
    function loadGrid()
    {
        $( "#list" ).hiMallDatagrid( {
            url: '/Admin/Bonus/DetailList',
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
            queryParams: { id :$("#VBid").val()},
            toolbar: /*"#goods-datagrid-toolbar",*/'',
            columns:
            [[
                { field: "OpenId", title: "OpenId" , formatter: function ( value, row, index ){
                    if(row.OpenId == null)
                    {
                        return "-";
                    }
                    else
                    {
                        return row.OpenId;
                    }
                } },
                { field: "ReceiveTime", title: "领取日期" , formatter: function ( value, row, index ){
                    if(row.ReceiveTime == null || row.ReceiveTime == "")
                    {
                        return "-";
                    }
                    else
                    {
                        return row.ReceiveTime;
                    }
                } },
                { field: "UserName", title: "会员账户" , formatter: function ( value, row, index ){
                    if(row.UserName == null || row.UserName == "")
                    {
                        return "-";
                    }
                    else
                    {
                        return row.UserName;
                    }
                } },
                { field: "IsTransformedDeposit", title: "存至预存款" , formatter: function ( value, row, index ){
                    if(row.IsTransformedDeposit)
                    {
                        return "是";
                    }
                    else
                    {
                        return "否";
                    }
                } },
                { field: "Price", title: "金额" }
            ]]
        } );
    }
} )
