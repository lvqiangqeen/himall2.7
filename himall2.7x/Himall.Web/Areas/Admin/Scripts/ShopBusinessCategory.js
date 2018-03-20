// JavaScript source code
var categoryArray = new Array();
$(function () {

    var _order;
    $('.container').on('focus', '.text-order', function () {
        _order = parseFloat($(this).val());
    });

    $('.container').on('blur', '.text-order', function () {
        if ($(this).hasClass('text-order')) {
            if (isNaN($(this).val()) || parseInt($(this).val()) <= -1 || parseInt($(this).val()) >= 100) {
                $.dialog({
                    title: '更新分类信息',
                    lock: true,
                    width: '400px',
                    padding: '20px 60px',
                    content: ['<div class="dialog-form">您输入的分佣比例不合法,此项只能是大于0且小于100的数字.</div>'].join(''),
                    button: [
                    {
                        name: '关闭',
                    }]
                });
                $(this).val(_order);
            } else {
                if (parseFloat($(this).val()) === _order) return;
                var id = $(this).parents("tr.bcategoryLines").attr('id');
                var commisRate = $(this).val();
                var businessCategroyId = $(this).parents("tr.bcategoryLines").attr('businessCategoryId');
                categoryArray.remove(id + '|' + _order);
                categoryArray.push(id + '|' + commisRate);
                //$.post('UpdateShopCommisRate', { businessCategoryId: businessCategroyId, commisRate: commisRate }, function (result) {
                //    if (result.success)
                //        $.dialog.tips('保存成功');
                //    else
                //        $.dialog.errorTips('保存失败！' + result.msg);
                //})


            }
        }
    });

    (function () {
        $(".bcategoryLines").each(function () {
            var id = $(this).attr('id');
            var commisRate = $(this).find('.commisRate').val();
            categoryArray.push(id + '|' + parseFloat(commisRate));
        });

    })();

    var categoryId;
    var categoryName = new Array();
    $('.add-business').click(function () {
        $.dialog({
            title: '新增经营类目',
            lock: true,
            id: 'addBusiness',
            content: ['<div class="dialog-form">',
            '<div class="form-group">',
                '<label class="label-inline fl" for="">经营类目</label>',
                '<select id="category1" class="form-control input-sm select-sort"><option></option></select>',
                '<select id="category2" class="form-control input-sm select-sort"><option></option></select>',
                '<select id="category3" class="form-control input-sm select-sort"><option></option></select>',
            '</div>',
            '<div class="form-group">',
                '<label class="label-inline fl" for="">分佣比例</label>',
                '<input class="form-control input-sm input-num" type="text" id="CommisRate"> %',
            '</div>',
        '</div>'].join(''),
            padding: '0 40px',
            okVal: '确认',
            ok: function () {
                var reg = /^[-+]?(0|[1-9]\d*)(\.\d+)?$/;
                if (categoryName.length < 3) {
                    $.dialog.errorTips("请选择完整后再试！");
                    return false;
                }
                var rate = $("#CommisRate").val();
                if (!$("#CommisRate").val()) {
                    $.dialog.errorTips("请填写分佣比例！");
                    return false;
                }
                if (reg.test($("#CommisRate").val()) == false) {
                    $.dialog.errorTips("请填写正确的分佣比例");
                    return false;
                }
                if (isNaN(rate) || parseInt(rate) <= -1 || parseInt(rate) >= 100){
                    $.dialog.errorTips("请填写正确的分佣比例,0-100之间");
                    return false;
                }

                if ($("#" + categoryId).length > 0) {
                    $.dialog.errorTips("新建失败,该类目已经存在！");
                    return false;
                }
                var html = ['<tr class="bcategoryLines" id="' + categoryId + '" businessCategoryId="' + 4 + '">',
                        '<td>' + categoryName.join(' > ') + '</td>',
                        '<td><input class="text-order no-m commisRate" type="text" value="' + $("#CommisRate").val() + '"> %</td>',
                        '<td class="td-operate"><span class="btn-a"><a onclick="deleteCategoryLine(this);" class="a-del">删除</a></span></td>',
                    '</tr>'];
                $("#bcategoryTBody").append($(html.join('')));
                $("#nonData").remove();

                categoryArray.push(categoryId + '|' + $("#CommisRate").val());

                categoryId = 0;
                categoryName.length = 0;
            }
        });
        $('#category1,#category2,#category3').himallLinkage({
            url: '../category/GetValidCategories',
            enableDefaultItem: true,
            defaultItemsText: '请选择',
            onChange: function (level, value, text) {
                categoryId = value;
                if (value) {
                    var categoryNames = [];
                    for (var i = 0; i < level; i++)
                        categoryNames.push(categoryName[i]);
                    categoryNames.push(text);
                    categoryName = categoryNames;
                }
                if (level == 2) {
                    var loading = showLoading();
                    ajaxRequest({
                        type: 'GET',
                        url: "./GetCategoryCommisRate",
                        param: { Id: value },
                        dataType: "json",
                        success: function (data) {
                            loading.close();
                            if (data.successful == true) {
                                $("#CommisRate").val(data.CommisRate);
                            }
                        }, error: function () {
                            loading.close();
                        }
                    });
                }
                //alert(text);
            }
        });
    });


    $("#SaveBtn").click(function () {
        var loading = showLoading();
        ajaxRequest({
            type: 'POST',
            url: "./SaveBusinessCategory",
            param: { shopId: $("#shopId").val(), bcategory: categoryArray.join(',') },
            dataType: "json",
            success: function (data) {
                if (data.Successful == true) {
                    location.href = "./Management";
                }
                loading.close();
            }, error: function () { loading.close(); }
        });
    });

});

function deleteCategoryLine(obj) {
    var id = $(obj).parents("tr.bcategoryLines").attr('id');
    var loading = showLoading();
    ajaxRequest({
        type: 'POST',
        url: "./CanDeleteBusinessCategory",
        param: { id: $("#shopId").val(), cid: id },
        dataType: "json",
        success: function (data) {
            if (data.success == true) {
                var commisRate = $(obj).parents("td").prev().find('.commisRate').val();
                //console.log(id + '|' + parseInt( commisRate));
                categoryArray.remove(id + '|' + parseInt(commisRate));
                $("tr#" + id).remove();
                //console.log(categoryArray);
            } else {
                $.dialog.errorTips('不可以删除，用户有运营商品被购买！');
            }
            loading.close();
        }, error: function () { loading.close(); }
    });
}

