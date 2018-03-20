$(function () {


    initNiceScroll();

    var imageUrl = $('input[name="imageCategoryImageUrl"]').val();
    initImageUploader();
    bindTextLinkBtnClickEvent();
    bindProductsBtnClickEvent();
    bindSubmitClickEvent();
    bindHideSelector();
    initBrand();
	
	$('.floor-ex-img .ex-btn').hover(function(){
		$(this).parent().toggleClass('active');
	});
});



function initBrand() {

    /* 拼音查找逻辑代码 16进制生成字典查找 比数组循环快
     * 代码托管地址: https://github.com/githubyang/code/blob/master/charCode.js
     * 单骑闯天下 2014.12.8
     */
    var itemId = $('#homeFloorId').val();
    $.ajax({
        url: './GetBrandsAjax',
        data: 'id=' + itemId,
        type: 'post',
        success: function (d) {
            var data = d.data;
            ; (function (win, data, $, charCode) {
                var charList = ['A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '+'],
                    getPinYin = function (str) {
                        if (typeof str !== 'string') {
                            return;
                        }
                        if ((/[0-9]+/).test(str[0])) {
                            return false;
                        }
                        if (contains(charList, ((str[0]).toUpperCase()))) {
                            return (str[0]).toUpperCase();
                        }
                        var index = str[0],
                            code = index.charCodeAt(0).toString(16);
                        if (char[code] != null && char[code][0]) {
                            return char[code][0];
                        }
                    },
                    contains = function (arr, val) {
                        for (var i = 0, len = arr.length; i < len; i++) {
                            if (arr[i] == val) {
                                return 1;
                            }
                        }
                    },
                    create = function (e, bool) {
                        var str = (typeof e !== 'string') ? ($(e.target).text()) : e,
                            data = (str == '+' ? result['other'] : result[str]),
                            is = true,
                            string = '<div data-tag="' + str + '">';
                        if (!data) { return; }
                        $('.checkbox-group').children().each(function (i, e) {
                            var a = $(e).attr('data-tag');
                            if (a == str) {
                                is = false;
                                $(e).show();
                            } else {
                                $(e).hide();
                            }
                        });
                        if (!is && !bool) {
                            return;
                        }
                        for (var i = 0, len = data.length; i < len; i++) {
                            string += '<label class="checkbox-inline"><input class="brandCheckbox" type="checkbox" value="' + data[i].id + '" ' + (data[i].checked ? 'checked="true"' : '') + '>' + data[i].value + '</label>';
                        }
                        string += '</div>';
                        $('.checkbox-group').append(string);

                    },
                    result = {}, temp, html = '', uid = 1;
                for (var i = 0, len = data.length; i < len; i++) {
                    temp = getPinYin(data[i].value);
                    if (!temp) {
                        if (!result['other']) {
                            result['other'] = [];
                        }
                        result['other'].push({ id: data[i].id, checked: data[i].isChecked, value: data[i].value });
                    } else {
                        if (!result[temp]) {
                            result[temp] = [];
                        }
                        result[temp].push({ id: data[i].id, checked: data[i].isChecked, value: data[i].value });
                    }
                }
                for (var k = 0, l = charList.length; k < l; k++) {
                    charList[k] = (charList[k] == '+' ? 'other' : charList[k]);// 对其它非字母或者中文字符转换
                    if (result[charList[k]]) {
                        if (uid > 0) {
                            html += '<span class="btn-a ac"><a href="javascript:;" class="active">' + charList[k] + '</a></span>';
                            create(charList[k], 1);
                        } else {
                            html += '<span class="btn-a"><a href="javascript:;">' + (charList[k] == 'other' ? '+' : charList[k]) + '</a></span>';
                        }
                        // 选中生成
                        $.each(result[charList[k]], function (i, e) {
                            if (e.checked) {
                                $('#id_s').append('<label class="checkbox-inline"><input type="checkbox" checked="true" value="' + e.id + '" class="brandCheckbox">' + e.value + '</label>');
                            }
                        });
                        uid--;
                    } else {
                        html += '<span class="btn-a"><a href="javascript:;" style="color:#e4e4e4" class="disabled">' + (charList[k] == 'other' ? '+' : charList[k]) + '</a></span>';
                    }
                }
                $('#id_tab').html(html);
                $('#id_tab').bind('click', function (e) {// 点击开始按字母查找
                    var t = (e.target.nodeName.toUpperCase());
                    if (t != 'A') {
                        return;
                    }
                    if ($(e.target).hasClass('disabled')) {
                        return;
                    }
                    $(e.target).parent().siblings().filter('.ac').removeClass('ac').find('a').removeClass('active');
                    $(e.target).addClass('active').parent().addClass('ac');
                    create(e);
                });
                $( '.checkbox-group' ).bind( 'click', function ( e )
                {// 
                    if ( $( "#id_s label" ).length >= 10 )
                    {
                        $.dialog.tips( '只能选择10个品牌' );
                        return;
                    }
                    var t = (e.target.nodeName.toUpperCase());
                    if (t != 'INPUT') {
                        return;
                    }
                    var str = $(e.target).parent().text(),
                        value = $(e.target).val();
                    if ($(e.target).prop('checked')) {
                        $('#id_s').append('<label class="checkbox-inline" style="margin-right:10px;"><input type="checkbox" checked="true" value="' + $(e.target).val() + '" class="brandCheckbox">' + str + '</label>');
                    } else {
                        $('#id_s').children().filter(function (i, e) {
                            var a = $(e).children().val();
                            if (value == a) {
                                return true;
                            }
                        }).remove();
                    }
                });
                $('#id_s').bind('click', function (e) {
                    var t = (e.target.nodeName.toUpperCase());
                    if (t != 'INPUT') {
                        return;
                    }
                    var value = $(e.target).val();
                    if (!$(e.target).prop('checked')) {
                        $(e.target).parent().remove();
                        $('.brandCheckbox').filter(function (i, e) {
                            var a = $(e).val();
                            if (value == a) {
                                return true;
                            }
                        }).attr('checked', false);
                    }
                });
            }(window, data, jQuery, char));
        }
    });
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

    $( "#up21" ).himallUpload( {
        title: '',
        imageDescript: '230*135',
        displayImgSrc: $( "#up21" ).attr( 'url' ),
        imgFieldName: ""
    } );

    $( "#up22" ).himallUpload( {
        title: '',
        imageDescript: '470*450',
        displayImgSrc: $( "#up22" ).attr( 'url' ),
        imgFieldName: ""
    });
    $( "#up23" ).himallUpload( {
        title: '',
        imageDescript: '230*220',
        displayImgSrc: $( "#up23" ).attr( 'url' ),
        imgFieldName: ""
    });
    $( "#up24" ).himallUpload( {
        title: '',
        imageDescript: '230*220',
        displayImgSrc: $( "#up24" ).attr( 'url' ),
        imgFieldName: ""
    });
    $( "#up25" ).himallUpload( {
        title: '',
        imageDescript: '230*220',
        displayImgSrc: $( "#up25" ).attr( 'url' ),
        imgFieldName: ""
    });
    $( "#up26" ).himallUpload( {
        title: '',
        imageDescript: '230*220',
        displayImgSrc: $( "#up26" ).attr( 'url' ),
        imgFieldName: ""
    });
    $( "#up27" ).himallUpload( {
        title: '',
        imageDescript: '230*220',
        displayImgSrc: $( "#up27" ).attr( 'url' ),
        imgFieldName: ""
    });
    $( "#up28" ).himallUpload( {
        title: '',
        imageDescript: '470*220',
        displayImgSrc: $( "#up28" ).attr( 'url' ),
        imgFieldName: ""
    });
    $( "#up29" ).himallUpload( {
        title: '',
        imageDescript: '230*220',
        displayImgSrc: $( "#up29" ).attr( 'url' ),
        imgFieldName: ""
    });
    $( "#up30" ).himallUpload( {
        title: '',
        imageDescript: '230*220',
        displayImgSrc: $( "#up30" ).attr( 'url' ),
        imgFieldName: ""
    } );
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

function bindProductsBtnClickEvent()
{
    var html = ['<tr type="Products" name="">',
                     , '<td><input class="form-control input-xs" type="text" name="name" value="" /></td>'
                     , '<td><span ids="">0</span></td>'
                     , '<td class="td-operate"><span class="btn-a"><a>选择商品</a></span><span class="btn-del"><a href="#">删除</a></span></td>'
             , '</tr>'].join();

    $( '#addProducts' ).click( function ()
    {
        if ( $( 'tr[type="Products"]' ).length === 4 ) { $.dialog.tips( '选项卡最多添加4个.' ); return; }
        $( 'tbody[type="Products"]' ).append( html );
        $( 'tbody[type="Products"] span[class="btn-a"] a' ).unbind( "click" );
        $( 'tbody[type="Products"] span[class="btn-del"] a' ).unbind( "click" );
        $( 'tbody[type="Products"] span[class="btn-a"] a' ).click( function ()
        {
            var aSelf = this;
            var ids = $( aSelf ).parents( "tr" ).find( "td span" )[0].getAttribute( "ids" );
            var selectids = [];

            if ( ids != null && ids != undefined && ids.length > 0 )
            {
                var array = ids.split( ',' );
                for ( var i = 0 ; i < array.length; i++ )
                {
                    selectids.push( parseInt( array[i] ) );
                }
            }
            $.productSelector.show( selectids, function ( selectedProducts )
            {
                var ids = [];
                $.each( selectedProducts, function ()
                {
                    ids.push( this.id );
                } );

                if ( ids.length > 10 )
                {
                    $.dialog.errorTips( "商品不允许超过10个" );
                    return false;
                }
                else
                {
                    $( aSelf ).parents( "tr" ).find( "td span" )[0].innerHTML = ids.length;
                }
                
                $( aSelf ).parents( "tr" ).find( "td span" )[0].setAttribute( "ids", ids );
            } );
        } )
        $( 'tbody[type="Products"] span[class="btn-del"] a' ).click( function ()
        {
            $( this ).parent().parent().parent().remove();
        } )
    } );


    $( 'tbody[type="Products"] span[class="btn-a"] a' ).click( function ()
    {
        var aSelf = this;
        var ids = $( aSelf ).parents( "tr" ).find( "td span" )[0].getAttribute( "ids" );
        var selectids = [];

        if ( ids != null && ids != undefined && ids.length > 0 )
        {
            var array = ids.split( ',' );
            for ( var i = 0 ; i < array.length; i++ )
            {
                selectids.push( parseInt( array[i] ) );
            }
        } 
        $.productSelector.show( selectids, function ( selectedProducts )
        {
            var ids = [];
            $.each( selectedProducts, function ()
            {
                ids.push( this.id );
            } );

            if ( ids.length > 10 )
            {
                $.dialog.errorTips( "商品不允许超过10个" );
                return false;
            }
            else
            {
                $( aSelf ).parents( "tr" ).find( "td span" )[0].innerHTML = ids.length;
            }
            $( aSelf ).parents( "tr" ).find( "td span" )[0].setAttribute( "ids", ids );
        } );
    } )
    
    $( 'tbody[type="Products"] span[class="btn-del"] a' ).click( function ()
    {
        $( this ).parent().parent().parent().remove();
    } )

}

function getProducts()
{
    var productRows = $( 'tr[type="Products"]' );
    var textLinks = [];
    $.each( productRows, function ()
    {
        var id = $( this ).attr( 'name' );
        var name = $(this).find('input[name="name"]').val();
        var ids = $( this ).find( "td span" )[0].getAttribute( "ids" ).split( ',' );
        if ( ids.length == 1 && ids[0] == "" )
        {
            throw Error( '请为选项卡选择商品' );
        }
        var tabs = [];
        for ( var idx = 0 ; idx < ids.length; idx++ )
        {
            tabs.push( { tabId: id, productId: ids[idx] } );
        }
        if ( !name )
        {
            throw Error( '请将选项卡名称填写完整' );
        }
        else if ( GetStringLength(name) > 12 )
        {
            throw Error( '选项卡名称不能超过6个字符' );
        }
        textLinks.push( { id: id ? id : 0, name: name, detail: tabs } );
    });
    if (textLinks.length < 1)
    {
        //可以没有商品选项卡
        //throw Error('最少需要一个选项卡');
    }
    return textLinks;
}

function getTextLinks() {
    var textLinkRows = $('tr[type="textLink"]');
    var textLinks = [];
    $.each(textLinkRows, function () {
        var id = $(this).attr('name');
        var name = $(this).find('input[name="name"]').val();
        var url = $(this).find('input[name="url"]').val();
        if ( !name || !url )
        {
            throw Error( '请将文字链接填写完整' );
        }
        else if (name.length > 6)
        {
            throw Error( '文本模块名称不能超过6个字' );
        }
        textLinks.push({ id: id ? id : 0, name: name, url: url });
    });
    if (textLinks.length === 0) {
        throw Error('未设置任何文本模块');
    }
    return textLinks;
}

function GetStringLength( str )
{
    ///获得字符串实际长度，中文2，英文1 
    ///要获得长度的字符串 
    var realLength = 0, len = str.length, charCode = -1;
    for ( var i = 0; i < len; i++ )
    {
        charCode = str.charCodeAt( i );
        if ( charCode >= 0 && charCode <= 128 )
            realLength += 1;
        else realLength += 2;
    }
    return realLength;
};

function getPLink() {
    var plinks = $(".pLink");
    var pLinksArray = [];
    var index = 1;
    $.each(plinks, function () {
        var position = $(this).attr("position");
        var imageURL = $(this).parent().prev().find(".hiddenImgSrc").val();
        var url = $(this).val();
        if ( !imageURL)
        {
            throw Error( '第' + index + '个区域未上传图片' );
        }
        if ( !url)
        {
            throw Error( '第' + index + '个区域未填写链接' );
        }
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
        floorDetail.id = $( '#homeFloorId' ).val();
        floorDetail.DefaultTabName = $('#DefaultTabName').val();
        try {
            if (floorDetail.DefaultTabName.length < 1) {
                throw Error('请填写默认选项卡名称');
            } else if (floorDetail.DefaultTabName.length > 6) {
                throw Error('默认选项卡名称不能超过6个字符');
            }

            floorDetail.textLinks = getTextLinks();
            floorDetail.brands = getSelectedBrands();
            floorDetail.productLinks = getPLink();
            floorDetail.name = getFloorName();
            floorDetail.subName = $( "#SubName" ).val(); 
            floorDetail.tabs = getProducts();
            floorDetail.styleLevel = 4;
            submit(floorDetail);
        }
        catch (e) {
            $.dialog.errorTips(e.message);
        }
    });
}

function getFloorName() {
    var name=$("#FloorName").val();
    if (name.replace(/[ ]/g, "").length == 0) {
        $("#FloorName").focus();
        throw Error('请填写楼层名称');
    }
    if ( name.length <= 1 )
    {
        $( "#FloorName" ).focus();
        throw Error( '楼层名称最少为两位字符' );
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


