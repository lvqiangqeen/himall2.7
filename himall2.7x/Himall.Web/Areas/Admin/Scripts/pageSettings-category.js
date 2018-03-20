$(function () {
    bindCateogryEditBtnClickEvent();
    bindSortEvent();
    bindTopicChooseBtnClickEvent();

});


function bindTopicChooseBtnClickEvent() {

    //专题选择
    $('.topic-choose').click(function () {
        var rowNumber = $(this).attr('rowNumber');
        $.dialog({
            title: '专题选择',
            lock: true,
            width: 550,

            id: 'topicChoose',
            content: ['<table class="table table-bordered">',
                  '<thead>',
                    '<tr>',
                      '<th width="190">图片上传</th>',
                      '<th>跳转链接</th>',
                    '</tr>',
                  '</thead>',
                  '<tbody>',
                    '<tr>',
                      '<td><div class="upload-img" id="img1">',
                           '</div>',
                      '</td>',
                      '<td><input class="form-control input-xs" type="text" id="url1"/></td>',
                    '</tr>',
                    '<tr>',
                      '<td><div class="upload-img" id="img2">',
                           '</div>',
                      '</td>',
                      '<td><input class="form-control input-xs" type="text" id="url2"/></td>',
                    '</tr>',
                  '</tbody>',
			'</table>'].join(''),
            okVal: '确认',
            ok: function () {
                var img1 = $("#img1").himallUpload('getImgSrc'),
                    url1 = $('#url1').val(),
                    img2 = $("#img2").himallUpload('getImgSrc'),
                    url2 = $('#url2').val();

                if (saveTopic(rowNumber, img1, url1, img2, url2))
                    $.dialog.tips('保存成功');
                else {
                    return false;
                }
            }
        });


        var topic;
        $.ajax({
            type: 'post',
            url: 'GetHomeCategoryTopics',
            cache: false,
            async: false,
            data: { rowNumber: rowNumber },
            dataType: "json",
            success: function (data) {
                topic = data;
            },
            error: function () {
            }
        });

        $('#url1').val(topic.url1);
        $('#url2').val(topic.url2);


        $("#img1").himallUpload(
        {
            title: '',
            imageDescript: '194*70',
            displayImgSrc: topic && topic.imageUrl1
        });

        $("#img2").himallUpload(
        {
            title: '',
            imageDescript: '194*70',
            displayImgSrc: topic && topic.imageUrl2
        });




    });

}


function saveTopic(rowNumber, imgUrl1, url1, imgUrl2, url2) {
    var loading = showLoading();
    var result = false;
    $.ajax({
        type: 'post',
        url: 'SaveHomeTopic',
        cache: false,
        async: false,
        data: { rowNumber: rowNumber, imgUrl1: imgUrl1, url1: url1, imgUrl2: imgUrl2, url2: url2 },
        dataType: "json",
        success: function (data) {
            loading.close();
            result = data.success;
            if (!result)
                $.dialog.errorTips('保存失败' + data.msg);
        },
        error: function () {
            loading.close();
        }
    });
    return result;
}






//绑定分类编辑按钮单击事件
function bindCateogryEditBtnClickEvent() {
    //分类编辑
    $('.categorys-edit').click(function () {
        editHomeCategoies($(this).attr('rowNumber'), $(this).attr('categoryIds'));
        initNiceScroll();
    });
}

function bindSortEvent() {
    //排序
    $(".table tbody tr").first().find('.glyphicon-circle-arrow-up').addClass('disabled');
    $(".table tbody tr").last().find('.glyphicon-circle-arrow-down').addClass('disabled');
    $(".table").on("click", '.glyphicon-circle-arrow-up', function () {
        var thisObj = this;
        var p = $(this).parents('tr');
        var index = p.parent().find('tr').index(p);
        if (index == 0)
            return false;
        else {
            var oriRowNumber = parseInt(p.attr('rowNumber'));
            changeSequence(oriRowNumber, oriRowNumber - 1, function () {
                p.attr('rowNumber', oriRowNumber - 1);
                p.find('[rowNumber]').attr('rowNumber', oriRowNumber - 1);
                p.prev().attr('rowNumber', oriRowNumber);
                p.prev().find('[rowNumber]').attr('rowNumber', oriRowNumber);
                p.prev().before(p);
                reDrawArrow(thisObj);
            });
        }
    });


    $(".table").on("click", '.glyphicon-circle-arrow-down', function () {
        var thisObj = this;
        var p = $(this).parents('tr');
        var count = p.parent().find('tr').length;
        var index = p.parent().find('tr').index(p);
        if (index == (count - 1))
            return false;
        else {
            var oriRowNumber = parseInt(p.attr('rowNumber'));
            changeSequence(oriRowNumber, oriRowNumber + 1, function () {
                p.attr('rowNumber', oriRowNumber + 1);
                p.find('[rowNumber]').attr('rowNumber', oriRowNumber + 1);
                p.next().attr('rowNumber', oriRowNumber);
                p.next().find('[rowNumber]').attr('rowNumber', oriRowNumber);
                p.next().after(p);
                reDrawArrow(thisObj);
            });
        }
    });

}

function reDrawArrow(obj) {
    $(obj).parents('tbody').find('.glyphicon').removeClass('disabled');
    $(obj).parents('tbody').find('tr').first().find('.glyphicon-circle-arrow-up').addClass('disabled');
    $(obj).parents('tbody').find('tr').last().find('.glyphicon-circle-arrow-down').addClass('disabled');
}


function changeSequence(oriRowNumber, newRowNumber, callback) {
    var loading = showLoading();
    var result = false;
    $.ajax({
        type: 'post',
        url: 'ChangeSequence',
        cache: false,
        async: true,
        data: { oriRowNumber: oriRowNumber, newRowNumber: newRowNumber },
        dataType: "json",
        success: function (data) {
            loading.close();
            if (!data.success)
                $.dialog.errorTips('调整顺序出错!' + data.msg);
            else
                callback();
        },
        error: function () {
            loading.close();
        }
    });
}

function editHomeCategoies(rowNumber, selectedCategoryIds) {

    var html = '<div class="categorys-choose clearfix">\
        	<p>选择一级分类，多选则合并展示<span>选择需要显示的三级分类</span></p>\
            <div class="categorys-left" style="overflow-y: scroll">\
            	<ul class="clearfix" id="topLevelPanel">\
                </ul>\
            </div>\
            <div class="categorys-right">\
                <ul id="othersLevelPanel">\
                </ul>\
            </div>\
            <p class="text-right">菜单展开面板中的品牌在品牌管理中可通过勾选推荐显示</p>\
        </div>';


    $.dialog({
        title: '分类选择',
        lock: true,
        content: html,
        padding: '0',
        okVal: '确认选择',
        ok: function () {
            var loading = showLoading();
            var checkedCheckBox = $('#topLevelPanel input[type="checkbox"]:checked,#othersLevelPanel input[type="checkbox"]:checked');
            var ids = [], names = [];
            $.each(checkedCheckBox, function (i, checkBox) {
                ids.push($(checkBox).val());
                var name = $(checkBox).attr('name');
                if (name)
                    names.push(name);
            });
            var close = true;
            $.ajax({
                type: 'post',
                url: 'SaveHomeCategory',
                cache: false,
                async: true,
                data: { categoryIds: ids.toString(), rowNumber: rowNumber },
                dataType: "json",
                success: function (result) {
                    loading.close();
                    if (result.success) {
                        $.dialog.tips('选择保存成功！');
                        var nameList = names.toString();
                        if (!nameList)
                            nameList = '未设置';
                        $('tr[rowNumber="' + rowNumber + '"] td[type="names"]').html(nameList);
                        $('tr[rowNumber="' + rowNumber + '"] a[rowNumber="' + rowNumber + '"]').attr('categoryIds', ids.toString());

                    }
                    else {
                        $.dialog.errorTips('选择保存失败！' + result.msg);
                        close = false;
                    }
                },
                error: function () {
                    loading.close();
                }
            });
            return close;
        }
    });

    var topLevelPanel = $('#topLevelPanel');

    var loading = showLoading();
    $.getJSON('../Category/GetCategoryByParentId', { id: 0 }, function (data) {
        var categories = data.Category;
        var content = '';
        $.each(categories, function (i, category) {
            content += '<li><label><input type="checkbox" name="' + category.Name + '"  value="' + category.Id + '"/>' + category.Name + '</label></li>';
        });
        topLevelPanel.html(content);

        if (selectedCategoryIds && selectedCategoryIds.length > 0) {
            var topCheckbox = $('#topLevelPanel input[type="checkbox"]');
            $.each(topCheckbox, function (i, checkBox) {
                var categoryId = $(checkBox).val();
                if ((',' + selectedCategoryIds + ',').indexOf(',' + categoryId + ',') > -1) {
                    $(checkBox).prop('checked', true);
                    loadSubCategories(categoryId, selectedCategoryIds);
                }
            });
        }
        loading.close();
    });

    topLevelPanel.on('change', 'input[type="checkbox"]', function () {
        initNiceScroll();
        var checked = $(this).prop('checked');
        var parentId = $(this).val();
        if (checked) {
            loadSubCategories(parentId);
        }
        else {
            $('#othersLevelPanel li[parentId="' + parentId + '"]').remove();
        }
    });
}



function loadSubCategories(parentId, selectedCategoryIds) {
    var othersLevelPanel = $('#othersLevelPanel');
    $.getJSON('../Category/GetSecondAndThirdCategoriesByTopId', { id: parentId }, function (result) {
        if (result.success) {
            var content = '';
            $.each(result.categoies, function (i, category) {
                if (category.Children.length > 0) {
                    content += '<li parentId="' + parentId + '"><p>' + category.Name + '</p><div>';
                    $.each(category.Children, function (j, child) {
                        content += '<label><input type="checkbox" value="' + child.Id + '" />' + child.Name + '</label>';
                    });
                    content += '</div></li>';
                }
            });
            othersLevelPanel.append(content);
            if (selectedCategoryIds && selectedCategoryIds.length > 0) {
                selectedCategoryIds = selectedCategoryIds.split(',');
                $.each(selectedCategoryIds, function (i, categoryId) {
                    var checkBox = $('#othersLevelPanel input[value="' + categoryId + '"]');
                    if (checkBox)
                        checkBox.attr('checked', true);
                });
            }
        }
    });
}

function initNiceScroll() {
    //初始化NiceScroll
	if(+[1,]){
		$(".categorys-right").niceScroll({
			cursorcolor: "rgba(0,0,0,0.2)",
			cursorwidth: 6,
		});
	}
}

