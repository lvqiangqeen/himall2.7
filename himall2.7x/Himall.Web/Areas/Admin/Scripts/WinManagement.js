// JavaScript source code
$(function () {
        var url = location.search; //获取url中"?"符后的字串 
        var theRequest = new Object(); 
        var pid = url.substr(1).split('=')[1];
        //初始化查询
        reload("2", pid);
    
    //设置切换面板点击事件
        $('.nav-tabs-custom li[type="statusTab"]').click(function (e) {
        var _t = $(this);
        $(this).addClass('active').siblings().removeClass('active');
        var showstatus = _t.attr("value");
        reload(showstatus, pid);
    });
   
});

function reload(showstatus, pid) {
    $("#list").hiMallDatagrid({
        url: './WinList',
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
        queryParams: { text: showstatus,id:parseInt(pid) },
        toolbar: /*"#goods-datagrid-toolbar",*/'',
        columns:
        [[
            { field: "userName", title: "用户" },
            { field: "strAddDate", title: "参与时间" }
             ,
            {
                field: "awardName", operation: true, title: "奖品", formatter: function (value, row, index) {
                    var html = "";
                    var strname = "";
                    if (row.awardName.length>0) {
                        var name = row.awardName.split(')');
                        if (name.length > 1) {
                            strname += name[0] + ")<br/>" + name[1] + ")";
                        }
                        else {
                            strname = row.awardName;
                        }
                    }
                  
                    html += '<span class="btn-a">'+strname+'</span>';

                    return html;
                }
            }
        ]]
    });

}



