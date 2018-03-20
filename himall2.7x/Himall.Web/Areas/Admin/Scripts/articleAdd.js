
$(function () {

    initArticleCategory();

    initImageUploader();

    initRichTextEditor();

    bindSubmit();

    $("#Title").focus();
});

var eidtor;
function initRichTextEditor() {
    eidtor = UE.getEditor('contentContainer');

    eidtor.addListener('contentChange', function () {
        $('#contentError').hide();
    });

}

function initImageUploader() {

}

function bindSubmit() {


    $('#submit').click(function () {
        var categoryId = $('#CategoryId').val();
        if (!categoryId)
            $('#categoryError').show();
        var content = eidtor.getContent();
        content = $.trim(content);
        var strLength = eidtor.getContentTxt().length
        //验证字符长度
        if (strLength > 10000) {
            $.dialog.tips('输入字符过长！');
            return;
        }
        
        if (!content)
            $('#contentError').show();
        if ($('form').valid() && categoryId && content) {

            $('input[name="CategoryId"]').val(categoryId);
            var data = $('form').serialize();
            var loading = showLoading();
            $.post('add', data, function (result) {
                loading.close();
                var txt = $('input[name="Id"]').val() ? '保存' : '新增';
                if (result.success) {
                    $.dialog.tips(txt + '文章成功');
                    setTimeout(function () { location.href = 'management'; }, 1300);
                }
                else
                    $.dialog.tips(txt + '文章失败');
            });
        }

    });

}


function initArticleCategory() {

    var categoryId = $('#CategoryId').val();
    var categoryPathArr = $('#ArticleCategoryFullPath').val();
    categoryPathArr = categoryPathArr.split(',');
    $('#articleCategorySelector1,#articleCategorySelector2').himallLinkage({
        url: '../articleCategory/getCategories',
        enableDefaultItem: true,
        defaultItemsText: ['请选择', '不选择二级分类'],
        level: 2,
        displayWhenNull: false,
        styleClass: 'form-control input-sm',
        onChange: function (level, value, text) {
            if (level == 0 || value) {
                categoryId = value;
                $('#CategoryId').val(categoryId);
            }
                
            $('#categoryError').hide();
        },
        defaultSelectedValues: categoryPathArr
    });

}