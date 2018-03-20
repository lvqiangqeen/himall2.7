$(function () {
    $.ajax({
        type: 'get',
        url: '/Search/GetSearchFilter',
        data: { keyword: keyword, a_id: a_id, b_id: b_id, cid: cid },
        dataType: 'JSON',
        cache: true,// 开启ajax缓存
        success: function (data) {
            if (data.success) {
                //console.log(data);
                renderBrand(data.Brand);
                renderAttr(data.Attr);
                renderCategory(data.Category);
                initFilter();
            }
        }
    });

    $.ajax({
        type: 'get',
        url: '/Search/GetSalePrice',
        data: {},
        dataType: 'JSON',
        cache: true,// 开启ajax缓存
        success: function (data) {
            if (data.success) {
                FlushPrice(data);
            }
        }
    });

    $("#searchBox").val($('#searchKeywords').text());

    //关注商品
    $(".btn-coll").click(function () {
        var gid = $(this).attr('gid');
        checkLogin(function (func) {
            addFavoriteFun(gid, func);
        });
    });
});

function FlushPrice(data) {
    var discount = data.Discount;
    var selfShopid = data.SelfShopId;
    //会员价
    $("#plist .p-price[shopId='" + selfShopid + "']").each(function () {
        var disprice = parseFloat($(this).attr("price")) * parseFloat(discount);
        var showdisprice = disprice.toFixed(3);
        $(this).find("strong:first").text("￥" + (parseFloat(showdisprice)).toFixed(2));
    });
    //限时购
    $(data.LimitProducts).each(function () {
        $("#plist .p-price[productId='" + this.ProductId + "']").find("strong:first").text("￥" + this.MinPrice.toFixed(2));
    });
}

function renderAttr(data) {
    var url = "SearchAd?b_id=" + b_id + "&cid=" + cid + "&keywords=" + keyword + "&pageNo=1&orderType=" + orderType + "&orderKey=" + orderKey;
    var template = '<div class="attr"><div class="a-key">{0}</div><div class="a-values"><div class="v-fold"><ul class="f-list">{1}</ul></div><div class="v-option"><span class="o-more"><b></b></span></div></div></div>';
    var html = "";
    var second = "";
    var selected = a_id.split(',');
    $(data).each(function () {
        second = "";
        $(this.AttrValues).each(function () {
            second += "<li valid='" + this.Id + "'><a href='" + url + "&a_id=" + a_id + (a_id != "" ? "," : "") + this.Id + "'>" + this.Name + "</a></li>";
        });
        html += template.replace("{0}", this.Name).replace("{1}", second);
    });
    $("#attrlist").append(html);

    $("#attrlist li").each(function () {
        if ($.inArray(String($(this).attr("valid")), selected) != -1) {
            $(this).parents('.attr').remove();
            $(".select-value:first").append("<li><a onclick=\"reSearch('" + $(this).attr("valid") + "','attr');\"><strong>" + $(this).text() + "</strong><b>×</b></a></li>");
        }
    });

    var moreBtn = $('#advanced'),
    	propAttrs = $('.prop-attrs .attr');
    if (propAttrs.length > 3) {
        propAttrs.slice(0, 3).show();
        moreBtn.show();
        moreBtn.find('.attr-extra').click(function () {
            if ($(this).hasClass('open')) {
                $(this).removeClass('open').children().html('更多选项<b></b>');
                propAttrs.slice(3, propAttrs.length).hide();
            } else {
                $(this).addClass('open').children().html('收起<b></b>');
                propAttrs.show();
            }

        });
    } else {
        propAttrs.show();
    }
}
function reSearch(id, type) {
    var url = "";
    if (type == "attr") {
        var attr = a_id.split(',');
        url = "SearchAd?b_id=" + b_id + "&cid=" + cid + "&keywords=" + keyword + "&pageNo=1&orderType=" + orderType + "&orderKey=" + orderKey;
        window.location = url + "&a_id=" + attr.splice($.inArray(id, attr), 1);
    } else if (type == "brand") {
        url = "SearchAd?a_id=" + a_id + "&cid=" + cid + "&keywords=" + keyword + "&pageNo=1&orderType=" + orderType + "&orderKey=" + orderKey;
        window.location = url;
    }
}

function renderCategory(data) {
    var url = "SearchAd?a_id=" + a_id + "&b_id=" + b_id + "&keywords=" + keyword + "&pageNo=1&orderType=" + orderType + "&orderKey=" + orderKey;
    var template = '<div class="item fore hover"><h3><b></b><a href="{0}" class="{1}">{2}</a></h3><ul>{3}</ul>';
    var second = "";
    var third = "";
    $(data).each(function () {
        second = template.replace("{0}", url + "&cid=" + this.Id).replace("{1}", (cid == this.Id ? "curr" : "")).replace("{2}", this.Name);
        if (this.SubCategory.length == 0) {
            second = second.replace("{3}", "");
        }
        third = "";
        $(this.SubCategory).each(function () {
            third += '<li ' + (cid == this.Id ? "class='curr'" : "") + '><a href="' + url + "&cid=" + this.Id + '">' + this.Name + '</a></li>';
        });
        second = second.replace("{3}", third);
        $("#categorylist").append(second);
    });
}

function renderBrand(data) {
    if (data.length == 0)
        $("#brandlist .brand-attr").empty();

    if (b_id != 0) {
        $(data).each(function () {
            if (this.Id == b_id) {
                $(".select-value:first").append("<li><a onclick=\"reSearch('" + b_id + "','brand');\">品牌：<strong>" + this.Name + "</strong><b>×</b></a></li>");
                return false;
            }
        });
        $("#brandlist .brand-attr").empty();
        $("#brandlist").show();
        return;
    }

    var url = "SearchAd?a_id=" + a_id + "&cid=" + cid + "&keywords=" + keyword + "&pageNo=1&orderType=" + orderType + "&orderKey=" + orderKey;
    var html = "";
    $(data).each(function () {
        html += '<li><a href="' + url + '&b_id=' + this.Id + '" title="' + this.Name + '"><img src="' + this.Logo + '" width="108" height="36" alt="' + this.Name + '" /></a></li>';
    });

    $("#brandlist .f-list").append(html);
    $("#brandlist").show();
}

function initFilter() {
    $('.v-option .o-more').click(function () {
        if ($(this).hasClass('fold')) {
            $(this).removeClass('fold');
            $(this).parent().siblings().addClass('v-unfold');
        } else {
            $(this).addClass('fold');
            $(this).parent().siblings().removeClass('v-unfold');
        }
    });
    $('.brand-attr .a-values').each(function () {
        var h = $(this).find('.f-list').height();
        if (h > 65) {
            $(this).find('.v-option').show();
            $(this).find('.v-option .o-more').addClass("fold");
        } else {
            $(this).find('.v-option').hide();
        }
    });
    $(".brand-attr ul.f-list li a").each(function () {
        $(this).append('<b>' + $(this).find('img').attr('alt') + '</b>')
    });
    $(".brand-attr ul.f-list li a").hover(function () {
        $(this).find("img").hide().next().show();
    }, function () {
        $(this).find("b").hide().prev().show();
    });

    $('.mc .a-values').each(function () {
        var h = $(this).find('.f-list').height();
        if (h > 26) {
            $(this).find('.v-option').show();
            $(this).find('.v-option .o-more').addClass("fold");
        } else {
            $(this).find('.v-option').hide();
        }
    });
}

function addFavoriteFun(gid, callBack) {
    $.post('/Product/AddFavoriteProduct', { pid: gid }, function (result) {
        if (result.successful == true) {
            if (result.favorited == true) {
                $.dialog.alert('<p><em>' + result.mess + '</em></p>');
            } else {
                $.dialog.succeedTips(result.mess, null, 0.5);
            }
        }
        (function () { callBack && callBack(); })();

    });
}

function checkLogin(callBack) {
    var memberId = $.cookie('Himall-User');
    if (memberId) {
        callBack();
    }
    else {
        $.fn.login({}, function () {
            callBack(function () { location.reload(); });
        }, '', '', '/Login/Login');
    }
}

//点击图片预览
$(".scale-img img").on("click", function () {
    $(this).parent().parent().parent().find(".p-img img").attr("src", $(this).attr("data-url-max"));
})


//加入购物车
$("label[id^='addCart_']").click(function (e) {
    var gid = $(this).attr("id").split('_')[1];//商品ID
    var sku = gid + "_0_0_0";

    $.post("/cart/verificationToCart", { id: gid }, function (data) {
        if (data.success) {
            var loading = showLoading();
            $.ajax({
                type: 'POST',
                url: "/cart/AddProductToCart?skuId=" + sku + "&count=1",
                dataType: 'json',
                success: function (data) {
                    loading.close();
                    if (data.success == true) {
                    	if(navigator.userAgent.indexOf("MSIE 8.0")>0) {
                    		refreshCartProducts();
                    	}else{
							var cartOffset = $("#right_cart").offset(),
                            h = $(document).scrollTop();
	                        flySrc = $("#addCart_" + gid).parent().parent().find(".p-img img").attr("src");
	
	                        flyer = $('<img class="cart-flyer" src="' + flySrc + '"/>');
	                        flyer.fly({
	                            start: {
	                                left: e.pageX,
	                                top: e.pageY - h - 30
	                            },
	                            end: {
	                                left: cartOffset.left,
	                                top: cartOffset.top - h + 30,
	                                width: 20,
	                                height: 20
	                            },
	                            onEnd: function () {
	                                this.destory(); //移除dom 
	                                refreshCartProducts();
	                            }
	                        });
						}
                        
                    } else {
                        loading.close();
                        $.dialog.errorTips(data.msg);
                    }
                },
                error: function (e) {
                    //loading.close();
                    $.dialog.errorTips('加入购物车失败');
                }
            });
        } else {
            window.location.href = "/Product/Detail/" + gid;
        }
    }, "json")
});
