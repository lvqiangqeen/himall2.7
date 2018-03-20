$(function () {


    initNiceScroll();

    var imageUrl = $('input[name="imageCategoryImageUrl"]').val();
    initImageUploader();
    bindTextLinkBtnClickEvent();
    bindSubmitClickEvent();
    bindHideSelector();
	
	$('.floor-ex-img .ex-btn').hover(function(){
		$(this).parent().toggleClass('active');
	});
});


function bindHideSelector() {

    $('.choose-checkbox').each(function () {
        var _this = $(this);
        $(this).find('a').click(function () {
            $(this).parent().hide().siblings().fadeIn();
            var h = _this.find('.scroll-box').height();
            _this.find('.enter-choose').show();
            _this.animate({ height: h }, 200);
            _this.css({ paddingBottom: '50px' });
        })

        $(this).find('.btn').click(function () {
            _this.height(_this.find('.scroll-box').height());
            _this.css({ padding: 0 }).find('.scroll-box').hide().siblings('.choose-selected').fadeIn();
            _this.find('.enter-choose').hide();
            _this.animate({ height: '43px' }, 200);
            var id = $(this).attr('name');
            getSelectedText(id);
        });
    });
}



function getSelectedText(type) {
    var text = [];
    if (type == 'categoryBtn') {
        var categoryCheckBoxes = $('input[name="category"]:checked');
        $.each(categoryCheckBoxes, function () {
            text.push($(this).parent().text());
        });
        $('#selectedCategories').html(text.join(','));
    }
    else {
        var brandsCheckBoxes = $('input[name="brand"]:checked');
        $.each(brandsCheckBoxes, function () {
            text.push($(this).parent().text());
        });
        $('#selectedBrands').html(text.join(','));
    }



}


function initNiceScroll() {
    //初始化NiceScroll
    if (+[1, ]) {
        $(".scroll-box").niceScroll({
            styler: 'fb',
            cursorcolor: "#7B7C7E",
            cursorwidth: 6,
        });
    }
}


function getSelectedBrands() {
    //获取选择品牌

    var brandsCheckBoxes = $('.choose-brand input[class="brandCheckbox"]');    
    var brands = [];
    $.each(brandsCheckBoxes, function () {
        brands.push({ id: $(this).val() });
    });
    console.log(brands);
    return brands;
}

function initImageUploader(imageUrl) {
    //初始化图片上传

    $("#one").himallUpload({
        title: '',
        imageDescript: '1190*300',
        displayImgSrc: $("#one").attr('url'),
        imgFieldName: ""
    });
    $("#two").himallUpload({
        title: '',
        imageDescript: '230*300',
        displayImgSrc: $("#two").attr('url'),
        imgFieldName: ""
    });
    $("#three").himallUpload({
        title: '',
        imageDescript: '230*300',
        displayImgSrc: $("#three").attr('url'),
        imgFieldName: ""
    });
    $("#four").himallUpload({
        title: '',
        imageDescript: '230*300',
        displayImgSrc: $("#four").attr('url'),
        imgFieldName: ""
    });
    $("#five").himallUpload({
        title: '',
        imageDescript: '230*300',
        displayImgSrc: $("#five").attr('url'),
        imgFieldName: ""
    });
    $("#six").himallUpload({
        title: '',
        imageDescript: '230*300',
        displayImgSrc: $("#six").attr('url'),
        imgFieldName: ""
    });
    //$("#seven").himallUpload({
    //    title: '',
    //    imageDescript: '199*199',
    //    displayImgSrc: $("#seven").attr('url'),
    //    imgFieldName: ""
    //});
    //$("#eight").himallUpload({
    //    title: '',
    //    imageDescript: '199*199',
    //    displayImgSrc: $("#eight").attr('url'),
    //    imgFieldName: ""
    //});
    //$("#nine").himallUpload({
    //    title: '',
    //    imageDescript: '199*199',
    //    displayImgSrc: $("#nine").attr('url'),
    //    imgFieldName: ""
    //});
    //$("#ten").himallUpload({
    //    title: '',
    //    imageDescript: '199*199',
    //    displayImgSrc: $("#ten").attr('url'),
    //    imgFieldName: ""
    //});
}





function bindTextLinkBtnClickEvent() {


    var html = ['<tr type="textLink" name="">',
                     , '<td><input class="form-control input-xs" type="text" name="name" value="" /></td>'
                     , '<td><input class="form-control input-xs" type="text" name="url" value="" /></td>'
                     , '<td class="td-operate"><span class="btn-a"><a>删除</a></span></td>'
             , '</tr>'].join();

    //添加textLink表单
    $('#addTextLink').click(function () {
        if ($('tr[type="textLink"]').length === 8) {$.dialog.tips('文本链接最多添加8个.'); return; }
        $('tbody[type="textLink"]').append(html);
    });

    //textLink的删除操作
    $('tbody[type="textLink"]').on('click', 'a', function () {
        $(this).parents('tr').remove();
    });
}


function getTextLinks() {
    var textLinkRows = $('tr[type="textLink"]');
    var textLinks = [];
    $.each(textLinkRows, function () {
        var id = $(this).attr('name');
        var name = $(this).find('input[name="name"]').val();
        var url = $(this).find('input[name="url"]').val();
        if (!name || !url)
            throw Error('请将文字链接填写完整');
        textLinks.push({ id: id ? id : 0, name: name, url: url });
    });
    if (textLinks.length === 0) {
        throw Error('未设置任何文本模块');
    }
    return textLinks;
}

function getPLink() {
    var plinks = $(".pLink");
    var pLinksArray = [];
    var index = 1;
    $.each(plinks, function () {
        var position = $(this).attr("position");
        var imageURL = $(this).parent().prev().find(".hiddenImgSrc").val();
        var url = $(this).val();
        if (!imageURL)
            throw Error('第'+index+'个区域未上传图片');
        if ( !url)
            throw Error('第' + index + '个区域未填写链接');
        if (url.toLowerCase().indexOf('http://') < 0 && url.toLowerCase().indexOf('https://') < 0 && url.charAt(0) != '/') {
            throw Error('第' + index + '个链接地址请以"http://"或"https://"开头');
        }
        pLinksArray.push({ id: position, name: imageURL, url: url });
        index++;
    });
    console.log(pLinksArray);
    return pLinksArray;
}





function bindSubmitClickEvent() {
    var floorDetail = {
        id: null,
        brands: [],
        textLinks: [],
    };

    $('button[name="submit"]').click(function () {
        floorDetail.id = $('#homeFloorId').val();
        try {
            //floorDetail.textLinks = getTextLinks();
            floorDetail.brands = getSelectedBrands();
            floorDetail.productLinks = getPLink();
            floorDetail.name = getFloorName();
            floorDetail.subName = $( "#SubName" ).val();
            floorDetail.styleLevel = 0;
            submit(floorDetail);
        }
        catch (e) {
            $.dialog.errorTips(e.message);
        }
    });
}

function getFloorName() {
    var name = $("#FloorName").val().replace(/(^\s*)|(\s*$)/g, '');
    if (name.replace(/[ ]/g, "").length == 0) {
        $("#FloorName").focus();
        throw Error('请填写楼层名称');
    } else if (name.replace(/[ ]/g, "").length > 20) {
        $("#FloorName").focus();
        throw Error('楼层名称不得大于20位数。');
    }
    return name;
}


function submit(floorDetail) {
    var json = JSON.stringify(floorDetail);
    var url = 'SaveHomeFloorDetail';
    var loading = showLoading();
    $.post(url, { floorDetail: json }, function (result) {
        loading.close();
        if (result.success) {
            $.dialog.succeedTips('保存成功!', function () {
                window.location.href = 'HomeFloor';
            });
        }
        else
            $.dialog.errorTips('保存失败！' + result.msg);

    });

}


