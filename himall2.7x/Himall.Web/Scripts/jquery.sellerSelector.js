$.sellerSelector = {
    params: { selectedShopIds: [], isShopCategory: false },
    serviceType: 'admin',
    multiSelect: true,
    selectedShops: {},
    html: '<div id="_shopSelector" class="goods-choose clearfix">\
            <div class="choose-left">\
                <div class="choose-search">\
                	<div class="form-group">\
                        <label for="">所在地区：</label>\
                        <span id="regionSeller"></span>\
						 &nbsp; 关键词：<input type="text" id="shopname">\
						<button type="button">搜索</button>\
                    </div>\
                </div>\
                <table class="tb-void tb-dialog"></table>\
            </div>\
            <div class="choose-right">\
                \
                <ul class="clearfix">\
                    <li>\
                        <a href="javascript:;"></a>\
                        <i class="glyphicon glyphicon-remove"></i>\
                    </li>\
                </ul>\
            </div>\
            \
        </div>\
',

    loadedShops: null,
    reload: function (selectedShopIds) {
        this.loadedShops = [];
        var url = '/UserInquirySheet/GetShopList';
        $.sellerSelector.params.selectedShopIds = selectedShopIds;

        var columns = [
            { checkbox: true, width: 50 },
            { field: "ShopName", title: '供应商', align: "left" },
            { field: "CompanyRegionAddress", title: "所在地区", align: "left" },
            { field: "BusinessSphere", title: "主营产品", align: "left", width:250 },
            {
                field: "s", title: "操作", align: "center",
                formatter: function (value, row, index) {

                    $.sellerSelector.loadedShops[row.Id] = row;
                    var html = '<span class="btn-a" shopId="' + row.Id + '">';
                    console.log($.sellerSelector.loadedShops);
                    //alert(selectedShopIds);alert(row.Id);
                    if ($.sellerSelector.params.selectedShopIds && $.sellerSelector.params.selectedShopIds.indexOf(row.Id) > -1)
                        html += '已选择'
                    else
                        html += '<a shopId="' + row.Id + '" href="javascript:;" onclick="$.sellerSelector.select(' + row.Id + ',this)" class="active" >选择';
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


        $("#_shopSelector table").hiMallDatagrid({
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
            idField: "Id",
            pageSize: 7,
            pagePosition: 'bottom',
            pageNumber: 1,
            queryParams: this.params,
            columns: [columns]
        });

        if (selectedShopIds) {
            $.post(url, { ids: selectedShopIds.toString(), page: 1, rows: selectedShopIds.length }, function (shops) {
                $.each(shops.rows, function (i, shop) {
                    $.sellerSelector.selectedShops[shop.Id] = shop;
                    var li = '<li shopId="' + shop.Id + '">\
                        <a href="javascript:;" >' + shop.ShopName + '</a>\
                        <i type="del">×</i>\
                         </li>';
                    $("#_shopSelector ul").append(li);
                });
            }, "json");
        }

        $("#_shopSelector .choose-search button").unbind('click').click(function () {
            var keyWords = $("#shopname").val();
            keyWords = $.trim(keyWords);

            var provinceRegionId = $('#regionSeller').regionSelector('value', 'province');
            var cityRegionId = $('#regionSeller').regionSelector('value', 'city');
            var areaRegionId = $('#regionSeller').regionSelector('value');
            if (provinceRegionId == '请选择省') {
                $.sellerSelector.params.regionId = null;
                $.sellerSelector.params.nextRegionId = null;
            }
            if (provinceRegionId != '请选择省' && cityRegionId == '请选择市') {
                $.sellerSelector.params.regionId = $('#regionSeller').regionSelector('value', 'province');
                $.sellerSelector.params.nextRegionId = $('#regionSeller').regionSelector('getNextId', 'province');
                if ($.sellerSelector.params.nextRegionId == null)
                {
                    $.sellerSelector.params.nextRegionId = '3802';
                }
                //alert($.sellerSelector.params.nextRegionId);
            }
            if (cityRegionId != '请选择市' && areaRegionId.length == 0) {
                $.sellerSelector.params.regionId = $('#regionSeller').regionSelector('value', 'city');
                $.sellerSelector.params.nextRegionId = $('#regionSeller').regionSelector('getNextId', 'city');
                if ($.sellerSelector.params.nextRegionId == null)
                {
                    $.sellerSelector.params.nextRegionId = $('#regionSeller').regionSelector('getNextId', 'province');
                }
            }
            if (areaRegionId.length > 0) {
                $.sellerSelector.params.regionId = $('#regionSeller').regionSelector('value');
                $.sellerSelector.params.nextRegionId = $('#regionSeller').regionSelector('getNextId');
            }


            $.sellerSelector.params.shopName = keyWords;
            $("#_shopSelector table").hiMallDatagrid('reload', $.sellerSelector.params);
        });

    },
    select: function (shopId, sender) {

        var shop = this.loadedShops[shopId];
        this.selectedShops[shopId] = shop;
        if (!this.params.selectedShopIds)
            this.params.selectedShopIds = [];
        this.params.selectedShopIds.push(shopId);
        var li = '<li shopId="' + shopId + '">\
                        <a href="javascript:;">' + shop.ShopName + '</a>\
                        <i type="del">×</i>\
                  </li>';
        $("#_shopSelector ul").append(li);
        var span = $(sender).parent();
        $(sender).remove();
        span.html('已选择');
        $('.choose-right').scrollTop($('.choose-right ul').height() - $('.choose-right').height());

    },
    removeShop: function (shopId) {
        $('#_shopSelector ul li[shopId="' + shopId + '"]').remove();
        var removedShops = {};
        var shopIds = [];
        $.each(this.selectedShops, function (i, shop) {
            if (shop && i!= shopId) {
                removedShops[i] = shop;
                shopIds.push(i);
            }
        });
        this.selectedShops = removedShops;
        this.params.selectedShopIds = shopIds;
        var btn = $('span[shopId="' + shopId + '"]');
        if (btn) {
            btn.html(
            '<a shopId="' + shopId + '" href="javascript:;" onclick="$.sellerSelector.select(' + shopId + ',this)" class="active" >选择 ');
            btn.addClass('active');
        }
    },
    clear: function () {
        this.selectedShops = {};
        $("#_shopSelector ul").empty();
        this.params = { auditStatus: 2, saleStatus: 1, selectedShopIds: [] };
    },
    getSelected: function () {
        var shops = [];
        if (this.multiSelect) {
            $.each(this.selectedShops, function (i, shop) {
                if (shop)
                    shops.push(shop);
            });
        }
        else {
            shops.push($("#_shopSelector table").hiMallDatagrid('getSelected'));
        }
        return shops;
    },
    show: function (selectedShopIds, onSelectFinishedCallBack, multiSelect) {
        /// <param name="serviceType" type="String">平台：admin,商家：selleradmin,默认为平台</param>
        /// <param name="multiSelect" type="Bool">是否多选，默认为True</param>

        if (multiSelect != null)
            this.multiSelect = multiSelect;

        var dialog=$.dialog({
			id:'sellerChoose',
            title: '供应商选择',
            lock: true,
            content: this.html,
            width: 660,
            padding: '15px',
            okVal: '确认选择',
            ok: function () {
                onSelectFinishedCallBack($.sellerSelector.getSelected());
                $.sellerSelector.clear();
                $('#_shopSelector').remove();
            },
            close: function () {
                $.sellerSelector.clear();
                $('#_shopSelector').remove();
            }
        });
		
		dialog.position(null,50);
		
        $('#regionSeller').regionSelector();

        if (!this.multiSelect) {
            $('.choose-right').hide();
            $('.choose-left').css('width', '100%');
        }

        //注册删除事件
        $('#_shopSelector').on('click', 'i[type="del"]', function () {
            var parent = $(this).parent();
            $.sellerSelector.removeShop(parent.attr('shopId'));

        });

        this.clear();
        this.reload(selectedShopIds);

        /*if (+[1, ]) {
            $(".choose-right").niceScroll({
                styler: 'fb',
                cursorcolor: "#7B7C7E",
                cursorwidth: 6,
            });
        }*/
    }
};
