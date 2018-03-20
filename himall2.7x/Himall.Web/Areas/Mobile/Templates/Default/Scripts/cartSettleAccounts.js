$(function () {
    loadCartInfo();

    $('#toSettlement').click(function () {
        bindToSettlement();
    });
});
var data = {};
function loadCartInfo() {
	$.post('/m-wap/Cart/GetCartProducts', {}, function (cart) {
		data = {};
		$.each(cart.products, function (i, e) {
			if (data[e.shopId]) {
				if (!data[e.shopId]['name']) {
					data[e.shopId]['name'] = e.shopName;
				}
				data[e.shopId]['shop'].push(e);
			} else {
				data[e.shopId] = {};
				data[e.shopId]['shop'] = [];
				data[e.shopId]['name'] = e.shopName;
				data[e.shopId]['shop'].push(e);
			}
		});

		var str = '';
		//var memberId = $.cookie('Himall-User');
		//if (memberId) {
		//    $('.cart-inner .message').find('.unLogin').hide();
		//}
		if (cart.products.length == 0) {
			$('.cart-inner').addClass('cart-empty');
		} else {
			$.each(data, function (i, e) {
				if (e.shop[0].vshopId != 0) {
					str += '<li class="list-group-item"><div class="cart-shop"><i class="check-custom"></i><span><a href="/' + areaName + '/vshop/detail/' + e.shop[0].vshopId + '">'
					if (e.shop[0].shopLogo == null) {
					    str += '<img src="~/Areas/Web/Images/40x24.png" />';
					}
					else
						str += e.name + '</a></span></div>';
				}
				else {
					str += '<li class="list-group-item"><div class="cart-shop"><i class=" check-custom"></i><span>';
					if (e.shop[0].shopLogo == null) {
					    str += '<img src="~/Areas/Web/Images/40x24.png" />'
					}
					else
						str += e.name + '<span></div>';
				}
				//在controller中修改了图片
				$.each(e.shop, function (j, product) {
					if (product.status == 1) {
						str += '\
                        <div class="cart-goods">\
                            <i class="check-custom"></i>\
                            <a><img src="'+ product.imgUrl + '" /></a>\
                                <p><a href="/'+ areaName + '/product/detail/' + product.id + '">' + product.name + '</a></p><h5>'+product.skuDetails+'</h5><span id="price">¥' + product.price.toFixed(2) + '</span>\
                            <div class="wrap-num">\
                            <a class="glyphicon glyphicon-minus" sku="' + product.skuId + '" href="javascript:;"></a>\
                                <input name="count" type="text" onkeyup="(this.v=function(){this.value=this.value.replace(/[^0-9-]+/,\'\');}).call(this)" onblur="this.v()" data-cartid="' + product.cartItemId + '" value="' + product.count + '" name="count" sku="' + product.skuId + '"/>\
                            <a class="glyphicon glyphicon-plus" sku="' + product.skuId + '" href="javascript:;"></a>\
                            </div>\
                            <a class="cart-remove" href="javascript:removeFromCart(\'' + product.skuId + '\')">删除</a>\
                            </div>';
					}
					else {
						str += '\
                        <div class="cart-goods cart-disable">\
                            <a><img src="'+ product.imgUrl + '" /></a>\
                                <p><a href="/'+ areaName + '/product/detail/' + product.id + '">' + product.name + '</a></p><h5>'+product.skuDetails+'</h5><span id="price">¥' + product.price.toFixed(2) + '</span>\
                            <div class="wrap-num">\
                            <a class="glyphicon glyphicon-minus" sku="' + product.skuId + '" href="javascript:;"></a>\
                                <input name="count" type="text" onkeyup="(this.v=function(){this.value=this.value.replace(/[^0-9-]+/,\'\');}).call(this)" onblur="this.v()" data-cartid="' + product.cartItemId + '" value="' + product.count + '" name="count" sku="' + product.skuId + '"/>\
                            <a class="glyphicon glyphicon-plus" sku="' + product.skuId + '" href="javascript:;"></a>\
                            </div> &nbsp; 失效\
                            <a class="cart-remove" href="javascript:removeFromCart(\''+ product.skuId + '\')">删除</a>\
                            </div>';
					}
				});
			});
		}
		$('.list-group.cart').html(str);
		$('#totalSkuPrice').html('¥' + 0);
		$('#selectedCount').html('(' + 0 + ')');
		bindSelectAll();
		bindAddAndReduceBtn();

		bindBatchRemove();
		ShowCheckState();
	});
}
function removeFromCart(skuId) {
    $.post('/m-wap/Cart/RemoveFromCart', { skuId: skuId }, function (result) {
        if (result.success)
            loadCartInfo();
        else
            alert(result.msg);
    });
}
function bindSelectAll() {
    $('#checkAll').unbind('click').click(function () {
        var checked = $(this).hasClass('active');
        if (checked) {
            $('.check-custom').each(function (i, e) {
                if ($(this).hasClass('active')) {
                    $(this).removeClass('active');
                }
            });
           
        } else {            
            $('.check-custom').each(function (i, e) {
                if (!$(this).hasClass('active')) {
                    $(this).addClass('active');
                }
            });
        }
        ComputeShowCartPrice();
    });
    
    $('.cart-shop .check-custom').unbind('click').click(function () {
        var checked = $(this).hasClass('active');
        if (checked) {
            $(this).parent().siblings().find('.check-custom').each(function(){
                if ($(this).hasClass('active')) {
                    $(this).removeClass('active');
                }
            });
            $(this).removeClass('active');
        } else {
            $(this).parent().siblings().find('.check-custom').each(function () {
                if (!$(this).hasClass('active')) {
                    $(this).addClass('active');
                }
            });
            $(this).addClass('active');
        }
        ShowCheckState();
        ComputeShowCartPrice();
    });

    $('.cart-goods .check-custom').unbind('click').click(function () {
        var checked = $(this).hasClass('active');
        if (checked) {
            $(this).removeClass('active');
            if ($(this).parent().siblings().not(".cart-goods").find('.check-custom').hasClass('active')) {
                $(this).parent().siblings().not(".cart-goods").find('.check-custom').removeClass('active');
            }
        } else {
            $(this).addClass('active');
            var allGoodsCheck = true;
            $(this).parent().parent().find(".cart-goods .check-custom").each(function () {
                if(!$(this).hasClass('active'))
                {
                    allGoodsCheck = false;
                }
            });
            if (allGoodsCheck)
            {
                $(this).parent().siblings().not(".cart-goods").find('.check-custom').addClass('active');
                
            }
        }
        ShowCheckState();
        ComputeShowCartPrice();
    });
}


function bindAddAndReduceBtn() {
    $('a.glyphicon-minus').click(function () {
        var skuId = $(this).attr('sku');
        var textBox = $(this).parent().find('input[name="count"]');
        var count = parseInt(textBox.val());
        if (count > 1) {
            count -= 1;
            textBox.val(count);
            updateCartItem(skuId, count);
            if ($(this).parent().parent().find('.check-custom').hasClass('active'))
            {
                var totalPrice = getCheckProductPrice().toFixed(2);
                var count = getCheckProductCount();
                $('#totalSkuPrice').html('¥' + totalPrice);
                $('#selectedCount').html('(' + count + ')');
            }
        }
    });

    $('a.glyphicon-plus').click(function () {
        var skuId = $(this).attr('sku');
        var textBox = $(this).parent().find('input[name="count"]');
        var count = parseInt(textBox.val());

        if (count > 0) {
            count += 1;
            textBox.val(count);
            updateCartItem(skuId, count);
            if ($(this).parent().parent().find('.check-custom').hasClass('active')){
                $('#totalSkuPrice').html('¥' + getCheckProductPrice().toFixed(2));
                $('#selectedCount').html('(' + getCheckProductCount() + ')');
            }
        }
    });

    $('input[name="count"]').change(function () {
        var skuId = $(this).attr('sku');
        var count = parseInt($(this).val());
        var textBox = $(this).parent().find('input[name="count"]');
        if (count > 0) {
            textBox.val(count);
            updateCartItem(skuId, count);
        }else
            $(this).val('1');
        if($(this).parent().parent().find('.check-custom').hasClass('active')){
            $('#totalSkuPrice').html('¥' + getCheckProductPrice().toFixed(2));
            $('#selectedCount').html('(' + getCheckProductCount() + ')');
        }
        
    });
}

function updateCartItem(skuId, count) {
    $.post('/'+areaName+'/cart/UpdateCartItem', { skuId: skuId, count: count }, function (result) {
        if (result.success) {
            ;
        }
        else
            alert(result.msg);
    });
}

function bindBatchRemove() {
    $('#deleteProducts').click(function () {
        if ($('.cart-goods .check-custom.active').length == 0)
        {
            $.dialog.errorTips('请选择删除的商品！');
            return;
        }
        $.dialog.confirm('确定要删除这些商品吗？', function () {
            var skuIds = [];
            $('.cart-goods .check-custom').each(function () {
                if ($(this).hasClass('active')) {
                    skuIds.push($(this).parent().find('input[name="count"]').attr('sku'));
                }
            });

            $.post('/m-wap/cart/BatchRemoveFromCart', { skuIds: skuIds.toString() }, function (result) {
                if (result.success)
                    $.dialog.succeedTips("删除成功", function () {
                        window.location.reload();
                    })
                else
                    alert(result.msg);
            });
        });
    });
}

//计算并显示购物车价格
//By DZY[150707]
function ComputeShowCartPrice() {
    var PriceTotal = 0;
    $(".list-group").find('.cart-goods .check-custom').each(function () {
        var _t = $(this);
        var _p=$(this).parent();
        if (_t.hasClass('active')) {
            var a = _p.find('#price').html(),
                b = a.replace('¥', ''),
                c = _p.find('input[name="count"]').val(),
                d = (+b) * (+c);
            PriceTotal += d;
        }
    });
    PriceTotal = parseInt(PriceTotal * 100) / 100;
    $('#totalSkuPrice').html('¥' + PriceTotal);
    $('#selectedCount').html('(' + getCheckProductCount() + ')');
}

//显示全选状态
function ShowCheckState() {
    var allShopCheck = true;
    $('.cart-shop .check-custom').each(function () {
        if (!$(this).hasClass('active')) {
            allShopCheck = false;
        }
    });
    if (allShopCheck) {
        $('#checkAll').addClass('active');
    } else {
        $('#checkAll').removeClass('active');
    }
}

function getCheckProductPrice()
{
    var t=0;
    $('.cart-goods .check-custom').each(function()
    {
        if($(this).hasClass('active')){
            var a= $(this).parent().find('#price').html(),
                b=a.replace('¥',''),
                c = $(this).parent().find('input[name="count"]').val(),
                d = (+b) * (+c);
            t+=d;
        }
    });
    return t;
}

function getCheckProductCount()
{
    var t = 0;
    $('.cart-goods .check-custom').each(function () {
        if ($(this).hasClass('active')) {
            var c = $(this).parent().find('input[name="count"]').val();
            d = parseInt(c);
            t += d;
        }
    });
    return t;
}

function bindToSettlement() {
    var memberId = $.cookie('Himall-User');
    var arr = [], str = '';
    $('.cart-goods .check-custom').each(function (i, e) {
        if ($(e).hasClass('active')) {
            arr.push($(e).parent().find('input[name="count"]').attr('data-cartid'));
        }
    });
    str = (arr && arr.join(','));
    if (str != '') {
        if (memberId) {
            location.href = '/' + areaName + '/Order/SubmiteByCart?' + 'cartItemIds=' + str;
        }
        else {
            $.fn.login({}, function () {
                location.href = '/' + areaName + '/Order/SubmiteByCart';
            }, '', '', '/' + areaName + '/Login/Login');
        }
    }
    else {
        $.dialog.errorTips('请选择结算的商品！');
    }
}
