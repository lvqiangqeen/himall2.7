$(function () {


    initNiceScroll();

    var imageUrl = $('input[name="imageCategoryImageUrl"]').val();
    initImageUploader();
    bindTextLinkBtnClickEvent();
    bindSubmitClickEvent();
    bindHideSelector();
    delProduct();

	$('.floor-ex-img .ex-btn').hover(function(){
		$(this).parent().toggleClass('active');
	});



    




	$('#addProducts').click(function () {

	    var pids = $(".fid");
	    var selectids = [];
	    $.each(pids, function () {
	        var ids = $(this).val();
	        selectids.push(parseInt(ids));
	    });

	    $.productSelector.show(selectids, function (selectedProducts) {
	        var ids = [];
	        var str = "";
	        var index = 0;
	        $.each(selectedProducts, function () {
	            index++;
	            str+="<tr>";
	            str += "<td><img src=\"" + this.imgUrl + "\" alt=\"\" />" + this.name + "</td>";
	            str += "<td>" + this.price + "</td>";
	            str+="<td align=\"center\">";
	            str += "<input type=\"hidden\" value=\"" + this.id + "\" class=\"fid\" />";
	            str+="<a href=\"javascript:;\" class=\"proremove\">删除</a></td>";
	            str+="</tr>";
	        });

	        if (index > 12) {
	            $.dialog.errorTips("商品不允许超过12个");
	            return false;
	        }
	        else {
	            $(".prol").html(str);
	        }
	        delProduct();
	    });
	})
});

function delProduct() {
    //删除商品
    $(".proremove").click(function () {
        $(this).parent().parent().remove();
    })
}

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
        imageDescript: '1190*142',
        displayImgSrc: $("#one").attr('url'),
        imgFieldName: ""
    });
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

//图片
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
    return pLinksArray;
}


//商品
function getProduct() {
    var pids = $(".fid");
    var pIdArray = [];
    var index = 0;
    $.each(pids, function () {
        var ids = $(this).val();
        pIdArray.push({ ProductId: ids });
        index++;
    });
    if (index == 0) {
        throw Error('请选择推荐商品!');
    } else if (index > 12) {
        throw Error('请选择商品不能超过12个!');
    }
    return pIdArray;
}




function bindSubmitClickEvent() {
    var floorDetail = {
        id: null,
        textLinks: [],
    };

    $('button[name="submit"]').click(function () {
        floorDetail.id = $('#homeFloorId').val();
        try {
            //floorDetail.textLinks = getTextLinks();
            //floorDetail.brands = getSelectedBrands();
            floorDetail.ProductModules = getProduct();
            floorDetail.productLinks = getPLink();
            floorDetail.name = getFloorName();
            floorDetail.subName = $( "#SubName" ).val();
            floorDetail.styleLevel = 2;
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
