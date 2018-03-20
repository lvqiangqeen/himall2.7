$.shopProductSelector = {
    params: { auditStatus: 2, saleStatus: 1, selectedProductIds: [], shopId: 0, isShopCategory: false },
    shopId: 0,
    multiSelect: true,
    selectedProducts: [],
    html: '<div id="_shopProductSelector" class="goods-choose clearfix">\
            <div class="choose-left">\
                <div class="choose-search">\
                	<div class="form-group">\
                        <label for="">商品分类</label>\
                        <select></select>\
                        <select></select>\
                        <select></select>\
						&nbsp; 关键词：<input class="form-control input-ssm" type="text" id="">\
						<button type="button" style="vertical-align: bottom;">搜索</button>\
                    </div>\
                </div>\
                <table class="tb-void tb-dialog"></table>\
            </div>\
            <div class="choose-right">\
                <ul class="clearfix">\
                </ul>\
            </div>\
        </div>\
',

    loadedProducts: null,
    reload: function (shopId, selectedProductIds) {
        this.loadedProducts = [];
        var url = '/Product/Browse';
        $.shopProductSelector.params.selectedProductIds = selectedProductIds;
        $.shopProductSelector.params.shopId = shopId;

        var columns = [
             { checkbox: true, width: 50 },
                {
                    field: "name", title: '商品', width: 366, align: "left",
                    formatter: function (value, row, index) {
                        var html = '<img src="' + row.imgUrl + '"/><span class="overflow-ellipsis">' + row.name + '</span>';
                        return html;
                    }
                },
            {
                field: "price", title: "售价", width: 100, align: "right",
                formatter: function (value, row, index) {
                    var html = '￥' + row.price.toFixed(2);
                    return html;
                }
            },
            {
                field: "s", title: "操作", width: 86, align: "center",
                formatter: function (value, row, index) {
                    $.shopProductSelector.loadedProducts[row.id.toString()] = row;
                    var html = '<span class="btn-a" productId="' + row.id + '">';
                    //if ($.shopProductSelector.params.selectedProductIds && $.shopProductSelector.params.selectedProductIds.indexOf(row.id) > -1)
                    //    html += '已选择';
                    //else
                    //    html += '<a productId="' + row.id + '" href="javascript:;" onclick="$.shopProductSelector.select(' + row.id + ',this)" class="active" >选择 <i class="glyphicon glyphicon-chevron-right"></i>';
                    html += '<a productId="' + row.id + '" href="javascript:;" onclick="$.shopProductSelector.select(' + row.id + ',this)" class="active" >选择';
                    html += '</a></span>';
                    return html;
                },
                styler: function () {
                    return 'td-operate';
                }
            }
        ];

        var start, end;
        var newColumns = [];
        if (this.multiSelect) {
            start = 1;
            end = columns.length;
        }
        else {
            start = 0;
            end = columns.length - 1;
        }
        for (var i = start; i < end; i++) {
            newColumns.push(columns[i]);
        }
        columns = newColumns;


        $("#_shopProductSelector table").hiMallDatagrid({
            url: url,
            nowrap: false,
            rownumbers: true,
            NoDataMsg: '没有找到符合条件的数据',
            border: false,
            fit: true,
            fitColumns: true,
            pagination: true,
            hasCheckbox: !this.multiSelect,
            singleSelect: !this.multiSelect,
            idField: "id",
            pageSize: 7,
            pagePosition: 'bottom',
            pageNumber: 1,
            queryParams: this.params,
            columns: [columns]
        });

        if (selectedProductIds) {
            $.post(url, { shopId: shopId, Ids: selectedProductIds.toString(), page: 1, rows: selectedProductIds.length }, function (products) {
                $.each(products.rows, function (i, product) {
                    $.shopProductSelector.selectedProducts[product.id] = product;
                    for (var i = selectedProductIds.length - 1; i >= 0; i--) {
                        if (selectedProductIds[i] == product.id) {
                            var li = '<li productId="' + product.id + '">\
                            <a href="javascript:;" ><img src="' + product.imgUrl + '"/></a>\
                            <i type="del">×</i>\
                            </li>';
                            $("#_shopProductSelector ul").append(li);
                        }
                    }

                });
            }, "json");
        }

        $("#_shopProductSelector .choose-search button").unbind('click').click(function () {
            var keyWords = $("#_shopProductSelector .choose-search input").val();
            keyWords = $.trim(keyWords);
            $.shopProductSelector.params.keyWords = keyWords;
            $("#_shopProductSelector table").hiMallDatagrid('reload', $.shopProductSelector.params);
        });

    },
    select: function (productId, sender) {
        var product = this.loadedProducts[productId];
        this.selectedProducts[productId] = product;
        if (!this.params.selectedProductIds)
            this.params.selectedProductIds = [];
        this.params.selectedProductIds.push(productId);
        var li = '<li productId="' + productId + '">\
                        <a href="javascript:;"><img src="' + product.imgUrl + '"/></a>\
                        <i type="del">×</i>\
                  </li>';
        $("#_shopProductSelector ul").append(li);
        //var span = $(sender).parent();
        //$(sender).remove();
        //span.html('已选择');
        $('.choose-right').scrollTop($('.choose-right ul').height() - $('.choose-right').height());
    },
    removeProduct: function (productId) {
        $('#_shopProductSelector ul li[productId="' + productId + '"]').remove();
        var removedProducts = [];
        var productIds = [];
        $.each(this.selectedProducts, function (i, product) {
            if (product && product.id != productId) {
                removedProducts[product.id] = product;
                productIds.push(product.id);
            }
        });
        this.selectedProducts = removedProducts;
        this.params.selectedProductIds = productIds;
        var btn = $('span[productId="' + productId + '"]');
        if (btn) {
            btn.html(
            '<a productId="' + productId + '" href="javascript:;" onclick="$.shopProductSelector.select(' + productId + ',this)" class="active" >选择');
            btn.addClass('active');
        }
    },
    clear: function () {
        this.selectedProducts = [];
        $("#_shopProductSelector ul").empty();
        this.params = { auditStatus: 2, saleStatus: 1, selectedProductIds: [] };
    },
    getSelected: function () {
        var products = [];
        if (this.multiSelect) {
            //$.each(this.selectedProducts, function (i, product) {
            //    if (product)
            //        products.push(product);
            //});
            $.each($(".choose-right ul").find("li"), function (i, product) {
                if ($(product).attr("productid") != "")
                    products.push($(product).attr("productid"));
            });
        }
        else {
            products.push($("#_shopProductSelector table").hiMallDatagrid('getSelected'));
        }

        return products;
    },
    show: function (selectedProductIds, onSelectFinishedCallBack, shopId, multiSelect) {
        if (multiSelect != null)
            this.multiSelect = multiSelect;

        var dialog=$.dialog({
            title: '商品选择',
            lock: true,
            content: this.html,
            padding: '10px',
            okVal: '确认选择',
            ok: function () {
                onSelectFinishedCallBack && onSelectFinishedCallBack($.shopProductSelector.getSelected());
                $.shopProductSelector.clear();
                $('#_shopProductSelector').remove();
            },
            close: function () {
                $.shopProductSelector.clear();
                $('#_shopProductSelector').remove();
            }
        });
		
		dialog.position(null,50);

        if (!this.multiSelect) {
            $('.choose-right').hide();
            $('.choose-left').css('width', '100%');
        }

        $('#_shopProductSelector select').himallLinkage({
            url: "/category/GetAuthorizationCategory?shopId=" + shopId,
            //url: "/category/getCategory",
            enableDefaultItem: true,
            defaultItemsText: '全部',
            onChange: function (level, value) {
                var categoryId = value;
                if (level > 0 && !value) {
                    var parentLevel = level - 1;
                    categoryId = $('#_shopProductSelector select').himallLinkage('value', parentLevel);
                    if (parentLevel > 0 && !categoryId) {
                        parentLevel--;
                        categoryId = $('#_shopProductSelector select').himallLinkage('value', parentLevel);
                    }
                }
                $.shopProductSelector.params.categoryId = categoryId;
                $.shopProductSelector.params.shopId = shopId;
                $.shopProductSelector.params.isShopCategory = true;
            }
        });

        //注册删除事件
        $('#_shopProductSelector').on('click', 'i[type="del"]', function () {
            var index = $(this).parent().index();
            var parent = $(this).parent();
            //$.shopProductSelector.removeProduct(parent.attr('productId'));
            parent.remove();
        });

        this.clear();
        this.reload(shopId,selectedProductIds);

        /*if (+[1, ]) {
            $(".choose-right").niceScroll({
                styler: 'fb',
                cursorcolor: "#7B7C7E",
                cursorwidth: 6,
            });
        }*/
    }
};
