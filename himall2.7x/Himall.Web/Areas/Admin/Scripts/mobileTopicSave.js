$(function () {
    $('#Name').focus();
    $("#topImage").himallUpload(
    {
        title: '头部图片',
        imageDescript: '请上传600*356的图片',
        displayImgSrc: $('#topImageBox').val(),
        imgFieldName: "TopImage"
    });


//    $("#backgroudImage").himallUpload(
//    {
//        title: '背景图片',
//        imageDescript: '大小不限制',
//        displayImgSrc: $('#backgroudImageBox').val(),
//        imgFieldName: "BackgroundImage"
//    });

//    $("#frontCoverImage").himallUpload(
//{
//    title: '封面图片',
//    imageDescript: '大小不限制',
//    displayImgSrc: $('#frontCoverImageBox').val(),
//    imgFieldName: "FrontCoverImage"
//});




    //新增专题模块
    $('#moduleContainer').on('click', 'a.choose-goods', function () {
        var moduleIndex = $(this).attr('index');
        !moduleIndex && (moduleIndex = 0);
        var ids = null;
        if (moduleProducts[moduleIndex]) {//当前模块已选择过商品，则获取所有本模块商品的编号
            ids = [];
            $.each(moduleProducts[moduleIndex], function (i, product) {
                ids.push(product.id);
            });
        }

        $.productSelector.show(ids, function (selectedProducts) {
            //记录当前选中的商品
            moduleProducts[moduleIndex] = selectedProducts;
            $('tr[index="' + moduleIndex + '"] td[type="selectedNumber"]').html(selectedProducts.length);
        });
    });

    $('#moduleContainer').on('click', 'a.a-del', function () {
        var moduleIndex = $(this).attr('index');
        removeModule(moduleIndex);
    });

    var moduleProductsString = $('#selectedProductIds').val();
    if (moduleProductsString) {
        moduleProducts = JSON.parse(moduleProductsString);
    }

});

//模块商品,用于装载各模块已选择的商品
var moduleProducts = [];


//添加模块
function addModule() {
    if ($('#moduleContainer tr').length >= 20) {
        $.dialog.tips('模块设置不能超过20个！');
        return;
    }
    var container = $('#moduleContainer');
    var moduleIndex = $('#moduleContainer tr').length;//模块序号，用于定位模块

    var html = ' <tr index="' + moduleIndex + '">\
                            <td><input class="text-module" type="text" value="默认模块' + (moduleIndex + 1) + '" placeholder="默认模块' + (moduleIndex + 1) + '" /></td>\
                            <td type="selectedNumber">未选择</td>\
                            <td class="td-operate"><span class="btn-a"><a class="choose-goods" index="' + moduleIndex + '">选择商品</a><a class="a-del" index="' + moduleIndex + '">删除</a></span></td>\
                 </tr>';
    container.append(html);
}

//移除模块
function removeModule(moduleIndex) {
    var moduleName = $('#moduleContainer tr[index="' + moduleIndex + '"] input[type="text"]').val();
    $.dialog.confirm('您确定要删除模块' + moduleName + '吗?', function () {
        $('#moduleContainer tr[index="' + moduleIndex + '"]').remove();
        $.dialog.tips('删除成功！');
    });
}


function generateTopicInfo() {
    //专题对象
    var topic = {
        id: $('#topicId').val(),
        name: null,
        topImage: null,
        backgroundImage: null,
        topicModuleInfo: []
    };

    !topic.id && (topic.id = null);
    topic.name = $('#Name').val();
    topic.frontCoverImage = $("#topImage").himallUpload('getImgSrc');
    topic.topImage = $("#topImage").himallUpload('getImgSrc');
    topic.backgroundImage = $("#backgroudImage").himallUpload('getImgSrc');
    topic.topicModuleInfo = [];
    topic.tags = $('#Tags').val();

    var modules = $('#moduleContainer tr');
    $.each(modules, function (i, moduleItem) {
        var moduleIndex = $(moduleItem).attr('index');
        !moduleIndex && (moduleIndex = 0);
        var moduleInfo = {
            name: $(moduleItem).find('input').val(),
            moduleProductInfo: []
        };

        if (!moduleProducts[moduleIndex] || moduleProducts[moduleIndex].length == 0)
            throw new Error('“' + moduleInfo.name + '”至少要选择一件商品');
        $.each(moduleProducts[moduleIndex], function (i, moduleProduct) {
            moduleInfo.moduleProductInfo.push({
                productId: moduleProduct.id,
                displaySequence: i + 1
            });
        });

        topic.topicModuleInfo.push(moduleInfo);
    });
    return topic;
}


var isPosting = false;
function submitTopic() {
    var object;
    if ($('form').valid()) {
        try {
            object = generateTopicInfo();
            if (!object.topImage)
                $.dialog.tips("请上传头部图片");
            else {
                var objectString = JSON.stringify(object);
                var loading = showLoading();
                if (isPosting) return;  //防重复提交
                isPosting = true;
                $.post('/admin/mobileTopic/add', { topicJson: objectString }, function (result) {
                    loading.close();
                    isPosting = false;
                    if (result.success) {
                        $.dialog.succeedTips('保存成功',function(){location.href = '/admin/mobileTopic/management';});
                    }
                    else
                        $.dialog.errorTips('保存失败！' + result.msg);
                }, "json");
            }
        }
        catch (e) {
            $.dialog.tips(e.message);
        }
    }
}