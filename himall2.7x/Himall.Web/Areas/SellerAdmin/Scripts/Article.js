$(function () {

    initGrid();
});
function initGrid() {
    //文章表格
    $("#list").hiMallDatagrid({
        url: 'Article/List',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "id",
        pageSize: 9,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams: {},
        operationButtons: '#batchDelete',
        columns:
        [[
            { field: "id", hidden: true },
           {
               field: "title", title: '标题', width: 250, align: 'left',
               formatter: function (value, row, index) {
                   var html = '<a href="' + location.href + '/Details/' + row.id + '">' + value + '</a>'
                   return html;
               }
           },
           {
               field: "addDate", title: "发布日期", width: 85, align: "center",
               formatter: function (value, row, index) {
                   return value;
               }
           }
        ]]
    });
}