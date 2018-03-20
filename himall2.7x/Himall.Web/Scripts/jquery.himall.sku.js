/*
 * Description:商品规格SKU处理
 * 2016.6.14--five(673921852)
 * SkuData格式:{"SkuId": "294_19_0_15", //可根据需求添加参数在callback中处理,SkuId、Price、Stock必须
			"Color": "黄色",
			"Stock": 10,
			"Price": 13，
			"Size": "",
			"Version": "100"}
 */

; (function ($) {
    $.fn.himallSku = function (options) {

        var defautls = {
            data: {},   // ajax请求data
            productId: null,   // 商品id,选填，默认为data第一个参数值
            spec: '.spec',       //sku组父级
            itemClass: '.itemSku',  //每个sku选项
            resultClass: {
                price: null,
                stock: null,
                chose: null
            },    //选择数据返回对象
            ajaxType: 'GET',   //ajax请求类型
            ajaxUrl: '',
            skuPosition: null,  //sku数据所处属性名
            callBack: function () { }  // 选择回调方法，已输出选中sku相关数据
        };
        var params = $.extend({}, defautls, options || {});

        if (!params.productId) {
            for (var k in params.data) {
                params.productId = params.data[k];
                break;
            }
        }
        var skuLen = $(params.spec, _this).length;
        var _this = $(this),
			SKUDATA = null,
			skuLen = $(params.spec, _this).length,
			skuId = [0, 0, 0],
			checkTodo = function (el, data, skuId,skus) {
			    el.each(function () {
			        skuId[parseInt($(this).attr('st'))] = $(this).attr('cid');
			        //判断是否是第一行的规格，如果是，都设为可选
			        if(parseInt($(this).attr('st'))!=0){
			        	 if (!data[params.productId + '_' + skuId.join('_')]) {
				            $(this).removeClass('enabled').addClass('disabled');
				            $(this).removeClass('selected');
				        } else {
				            $(this).removeClass('disabled').addClass('enabled');
				        }
			        }
			       
			    });
			    //找出每个规格中的可选项
			    skus.each(function(index,obj){
			    	var selectd = 0;
			    	$(".enabled",$(obj)).each(function(){
			    		if($(this).hasClass('selected')){
			    			selectd=1;
			    		}
			    	})
			    	if(selectd!=1){
			    		$(".enabled",$(obj)).eq(0).addClass('selected');
			    	}
			    });
			},
			getProperty = function (el, data) {
			    if (el) {
			        if ($(el).length > 1) {
			            $(el, _this).html(data);
			        } else {
			            $(el).html(data);
			        }
			    }
			};

        $.ajax({
            type: params.ajaxType,
            url: params.ajaxUrl,
            data: params.data,
            dataType: 'json',
            success: function (data) {
                if (!params.skuPosition) {
                    SKUDATA = data;
                } else {
                    SKUDATA = data[params.skuPosition]
                }

                //sku重组
                var skuArr = {};
                for (var i = 0; i < SKUDATA.length; i++) {
                    skuArr[SKUDATA[i].SkuId] = SKUDATA[i];
                }

                //一组规格处理
                if (skuLen == 1) {
                    _this.find('.enabled').each(function () {
                        skuId[parseInt($(this).attr('st'))] = $(this).attr('cid');
                        if (!skuArr[params.productId + '_' + skuId.join('_')]) {
                            $(this).removeClass('enabled').addClass('disabled');
                        }
                    });
                }

                //两组规格处理
                if (skuLen == 2) {
                    _this.on('click', '.enabled', function () {
                        skuId[parseInt($(this).attr('st'))] = $(this).attr('cid');
                        checkTodo($(this).parents(params.spec).siblings(params.spec).find(params.itemClass), skuArr, skuId,$(this).parents(params.spec).siblings(params.spec));
                    });
                }

                //三组规格处理
                if (skuLen == 3) {
                    _this.on('click', '.enabled', function () {
                        skuId[parseInt($(this).attr('st'))] = $(this).attr('cid');
                        var sibSku = $(this).parents(params.spec).siblings(params.spec),
							sibOne = sibSku.eq(0).find('.selected'),
							sibTwo = sibSku.eq(1).find('.selected');

                        //选中兄弟sku其中一个测试另外一个是否可选
                        if (sibOne.length > 0) {
                            skuId[parseInt(sibOne.attr('st'))] = sibOne.attr('cid');
                            checkTodo(sibSku.eq(1).find(params.itemClass), skuArr, skuId,sibSku);
                            sibTwo = sibSku.eq(1).find('.selected');
                        }

                        if (sibTwo.length > 0) {
                            skuId[parseInt(sibTwo.attr('st'))] = sibTwo.attr('cid');
                            checkTodo(sibSku.eq(0).find(params.itemClass), skuArr, skuId,sibSku);
                        }
                    });
                }

                //选择公用操作
                _this.on('click', '.enabled', function () {
                    if ($(this).hasClass('selected')) {
                        return;
                    }
                    $(this).addClass('selected').siblings().removeClass('selected');
                    var len = $('.selected', _this).length;
                    if (len === skuLen) {
                        for (var i = 0; i < len; i++) {
                            skuId[parseInt($('.selected', _this).eq(i).attr('st'))] = $('.selected', _this).eq(i).attr('cid');
                        }
                        var select = skuArr[params.productId + '_' + skuId.join('_')];
                       

                        getProperty(params.resultClass.price, select.Price.toFixed(2));
                        getProperty(params.resultClass.stock, select.Stock);
                        params.callBack(select, _this);  //回调方法，返回当前选中sku、 当前对象
                        //预处理了已选择规格显示，可自行在回调中处理
                        if (params.resultClass.chose) {
                            if ($(params.resultClass.chose).length > 1) {
                                $(params.resultClass.chose, _this).html('已选择：' +
									(select.Color != '' && select.Color != null ? '<em class="red">"' + select.Color + '"</em>&nbsp;&nbsp;' : '') +
									(select.Size != '' && select.Size != null ? '<em class="red">"' + select.Size + '"</em>&nbsp;&nbsp;' : '') +
									(select.Version != '' && select.Version != null ? '<em class="red">"' + select.Version + '"</em>' : '')
								);
                            } else {
                                $(params.resultClass.chose).html('已选择：' +
									(select.Color != '' && select.Color != null ? '<em class="red">"' + select.Color + '"</em>&nbsp;&nbsp;' : '') +
									(select.Size != '' && select.Size != null ? '<em class="red">"' + select.Size + '"</em>&nbsp;&nbsp;' : '') +
									(select.Version != '' && select.Version != null ? '<em class="red">"' + select.Version + '"</em>' : '')
								);
                            }
                        }
                    }
                });


                //加载初始化
                if (skuLen != 0) {
                    $(params.spec, _this).each(function () {
                        $(this).find('.enabled').first().trigger("click");
                    });
                } else if (skuLen == 0) {
                    getProperty(params.resultClass.price, SKUDATA[0].Price.toFixed(2));
                    getProperty(params.resultClass.stock, SKUDATA[0].Stock);
                }

            }
        });
    }
})(jQuery);







