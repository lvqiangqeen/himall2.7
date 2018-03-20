$(function () {
    query();
    getCategories();

    //添加敏感关键词
    $('.add-word').click(function () {
        LoadAddBox();
    });

    $("#importBtn").click(function () {
        uploadfile();
    });
});

function getCategories()
{
    $("#selectCategoryName").empty();
    $.post("./GetCategories", {}, function (data) {
        $("#selectCategoryName").append('<option value="">请选择</option>');
        $.each(data, function (i) {
            var option = "<option value=" + data[i] + ">" + data[i] + "</option>";
            $("#selectCategoryName").append(option);
        });               
    });
}

function Delete(id) {
    $.dialog.confirm('确定删除该条记录吗？', function () {
        var loading = showLoading();
        $.post("./Delete", { id: id }, function (data) {
            loading.close();
            $.dialog.tips(data.msg);
            query();
            getCategories()
        });
    });
}
function BatchDelete() {
    var selectedRows = $("#list").hiMallDatagrid("getSelections");
    var selectids = new Array();

    for (var i = 0; i < selectedRows.length; i++) {
        selectids.push(selectedRows[i].Id);
    }
    if (selectedRows.length == 0) {
        $.dialog.errorTips("你没有选择任何选项！");
    }
    else {
        $.dialog.confirm('确定删除选择的数据？', function () {
            var loading = showLoading();
            $.post("./BatchDelete", { ids: selectids.join(',') }, function (data) {
                loading.close();
                $.dialog.tips(data.msg);
                query();
                getCategories();
            });
        });
    }
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
        pageSize: 10,
        pageNumber: 1,
        queryParams: {},
        toolbar: /*"#goods-datagrid-toolbar",*/'',
        operationButtons: "#batchOperate",
        columns:
        [[
            { checkbox: true, width: 39 },
            { field: "Id", hidden: true },
            { field: "SensitiveWord", title: '敏感关键词' },
            { field: "CategoryName", title: '敏感关键词类别' },
            {
                field: "operation", operation: true, title: "操作",
                formatter: function (value, row, index) {
                    var id = row.Id.toString();
                    var word = row.SensitiveWord.toString();
                    var category = row.CategoryName.toString();
                    var html = ["<span class=\"btn-a\">"];
                    html.push("<a onclick=\"UpdateWord('" + id + "','" + word + "','" + category + "');\">修改</a>");
                    html.push("<a onclick=\"Delete('" + id + "');\">删除</a>");
                    html.push("</span>");
                    return html.join("");
                }
            }
        ]]
    });
}

function UpdateWord(id, word, category) {
    $.dialog({
        title: '修改敏感关键词',
        lock: true,
        id: 'UpdateWord',
        width: '400px',
        content: document.getElementById("addSensitiveWordform"),
        padding: '0 40px',
        okVal: '确定',
        init: function () {
            $("#SensitiveWord").val(word);
            $("#CategoryName").val(category);
        },
        ok: function () {
            var loading = showLoading();
            var word = $("#SensitiveWord").val();
            var category = $("#CategoryName").val();

            if (!CheckAdd(word, category))
                return false;
            $.post("./Eidt",
                { id: id, word: word, category: category },
                function (data) {
                    loading.close();
                    if (data.success) {
                        $.dialog.tips("修改成功", function () {
                            $("#SensitiveWord").val("");
                            $("#CategoryName").val("");
                            query();
                            getCategories();
                        });
                    }
                    else
                        $.dialog.errorTips("修改失败:" + data.msg);
                });
        }
    });
}

function LoadAddBox() {
    $.dialog({
        title: '添加敏感关键词',
        id: 'addSensitiveWord',
        content: document.getElementById("addSensitiveWordform"),
        lock: true,
        okVal: '确认添加',
        padding: '0 40px',
        init: function () {
            $("#SensitiveWord").focus();
        },
        ok: function () {
            var word = $("#SensitiveWord").val();
            var category = $("#CategoryName").val();

            if (!CheckAdd(word, category))
                return false;
            AddSensitiveWord(word, category);
        }
    });
}

function AddSensitiveWord(word, category) {
    var loading = showLoading();
    $.ajax({
        type: 'post',
        url: './Add',
        cache: false,
        async: true,
        data: { word: word, category: category },
        dataType: "json",
        success: function (data) {
            loading.close();
            if (data.success) {
                $.dialog.tips("添加成功！");
                $("#SensitiveWord").val("");
                $("#CategoryName").val("");
                query();
                getCategories();
            }
            else {
                $.dialog.errorTips("添加失败！" + data.msg);
            }
        },
        error: function () {
            loading.close();
        }
    });
}

function CheckAdd(word, category) {
    if (word.length == 0) {
        $.dialog.errorTips("敏感关键词不能为空");
        return false;
    }
    else if (category.length == 0) {
        $.dialog.errorTips("关键词类别不能为空");
        return false;
    }
    else
        return true;
}

function uploadfile() {
    var attaupfile = $("#upExcel"); //上传控件
    if (attaupfile.val() == "") {
        $.dialog.errorTips("请选择要上传的文件！");
        return false;
    }
    else {
        if (!checkImgType(attaupfile.val())) {
            $.dialog.errorTips("上传格式为xls、xlsx", '', 3);
            return false;
        }
    }

    //准备表单
    var myform = document.createElement("form");
    myform.action = './ImportExcel';
    myform.method = "post";
    myform.enctype = "multipart/form-data";
    myform.style.display = "none";
    //将表单加当document上，
    document.body.appendChild(myform);  //重点
    var form = $(myform);

    var fu = attaupfile.clone(true).val(""); //先备份自身,用于提交成功后，再次附加到span中。
    var fu1 = attaupfile.appendTo(form); //然后将自身加到form中。此时form中已经有了file元素。
    $(fu).prependTo($("#lblfile"));

    //开始模拟提交表当。
    form.ajaxSubmit({
        success: function (data) {
            if (data.success == false) {
                $.dialog.errorTips(data.msg);
            }
            else
            {
                $.dialog.tips("导入数据成功！");
                query();               
            }
            form.remove();
        }
    });
}

//检查上传的附件格式
function checkImgType(filename) {
    var pos = filename.lastIndexOf(".");
    var str = filename.substring(pos, filename.length)
    var str1 = str.toLowerCase();
    if (!/\.(xls|xlsx)$/.test(str1)) {
        return false;
    }
    return true;
}