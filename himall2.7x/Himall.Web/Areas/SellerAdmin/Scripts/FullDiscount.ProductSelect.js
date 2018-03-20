//营销活动商品选择控件
$.FDProductSelector = {
	params: { },
	url: '/SellerAdmin/FullDiscount/GetCanJoinProducts',
	selected: [],
	selectedIds: [],
	conflict: [],
	html: '<div id="_FDProductSelector" class="goods-choose clearfix">\
            <div class="choose-search">\
                <div class="form-group">\
                    <label class="label-inline" for="">商品货号</label>\
                    <input class="form-control input-ssm" type="text" name="scode" style="width:120px;">\
                </div>\
                <div class="form-group">\
                    <label class="label-inline" for="">商品名称</label>\
                    <input class="form-control input-ssm" type="text" name="sname" style="width:120px;">\
                </div>\
                <button type="button" class="btn btn-primary btn-ssm">搜索</button>\
            </div>\
            <table class="table table-bordered table-choose"></table>\
            <div class="tabel-operate" id="_FDProductSelector_operate" style="display:none;">\
                <a class="btn btn-default btn-ssm btn-addAll" onclick="$.FDProductSelector.selectALL()">全部参加</a>\
                <a class="btn btn-default btn-ssm btn-addAll" onclick="$.FDProductSelector.removeALL()">取消选择</a>\
		    </div>\
            <div class="conflict hide" style="font-size:12px;margin-bottom:10px;">已选商品有（<em></em>个）商品已参与其他活动，<a onclick="$.FDProductSelector.showConflict()" >点击查看详情</a></div>\
        </div>',

	reload: function (selectedIds, activeId) {
		$.FDProductSelector.selectedIds = selectedIds;
		var columns = [
            [
            {
            	field: "state", title: "选择", width: 30, align: "center",
            	formatter: function (value, row, index) {
            		var flag = false;
            		if ($.FDProductSelector.selectedIds.indexOf(row.Id) > -1)
            			flag = true;
            		return '<input type="checkbox" tag="check-item" value="' + row.Id + '" ' + (flag ? "checked" : "") + ' />';
            	}
            },
            {
            	field: "Name", title: '商品', width: 366, align: "left",
            	formatter: function (value, row, index) {
            		var html = '<img src="' + row.Image + '"/><span class="overflow-ellipsis" style="width:400px">' + row.Name + '</span>';
            		return html;
            	}
            },
            {
            	field: "Price", title: "售价", width: 100,
            	formatter: function (value, row, index) {
            		var html = '￥' + row.Price.toFixed(2);
            		return html;
            	}
            }]
		];
		var rowFormatter = function (row, index) {
			var conflict = $.FDProductSelector.conflict;
			var item = null;
			for (var i = 0; i < conflict.length; i++)
				if (conflict[i].prouctId == row.Id || conflict[i].prouctId == 0) {
					item = conflict[i];
					break;
				}

			if (item == null) return;
			var tr = [];
			tr.push('style="background-color:#e8baba"');
			tr.push('title="此商品已参与门店[' + item.shopName + ']的活动[' + item.activityName + ']"');
			return tr.join(" ");
		}
		var url = this.url;
		this.params.selectedProductId = "";
		$.FDProductSelector.selected = [];
		this.params.activeId = activeId;
		if (selectedIds && selectedIds.length>0) {
			this.params.selectedProductId = $.FDProductSelector.selectedIds.join(",");
		}
		$("#_FDProductSelector table").hiMallDatagrid({
			url: url,
			nowrap: false,
			rownumbers: true,
			NoDataMsg: '没有找到符合条件的数据',
			border: false,
			fit: true,
			fitColumns: true,
			pagination: true,
			hasCheckbox: true,
			idField: "id",
			pageSize: 10,
			pagePosition: 'bottom',
			pageNumber: 1,
			operationButtons: "#_FDProductSelector_operate",
			queryParams: this.params,
			columns: columns,
			rowFormatter: rowFormatter
		});
		$("#_FDProductSelector").on("change", "[type=checkbox]", function () {
			var checked = $(this).is(":checked");
			if ($(this).hasClass("check-all")) {//全选
				if (checked) {
					$.FDProductSelector.selectAll();
				} else {
					$.FDProductSelector.removeAll();
				}
			} else {
				var val = $(this).val();
				if (checked)
					$.FDProductSelector.select(val);
				else
					$.FDProductSelector.remove(val);
			}

		});
		$("#_FDProductSelector .choose-search button").unbind('click').click(function () {
			$.FDProductSelector.params.name = $("#_FDProductSelector input[name=sname]").val();
			$.FDProductSelector.params.code = $("#_FDProductSelector input[name=scode]").val();
			$("#_FDProductSelector table").hiMallDatagrid('reload', $.FDProductSelector.params);
		});
	},
	getData: function () {
		return $("#_FDProductSelector table").hiMallDatagrid('getRows');
	},
	getDataById: function (id) {
		var data = $.FDProductSelector.getData();
		for (var i = 0; i < data.length; i++) {
			if (data[i].Id == id)
				return data[i];
		}
		return null;
	},
	select: function (id,pro) {
		if ($.FDProductSelector.selectedIds.indexOf(id) >= 0) return;
		if (!pro) {
			pro = $.FDProductSelector.getDataById(id);
		}
		if (pro != null)
			$.FDProductSelector.selected.push(pro);
		$.FDProductSelector.selectedIds.push(id);
		$("#_FDProductSelector [tag=check-item][value=" + id + "]").prop("checked", true);

	},
	selectAll: function () {
		var selecteds = $.FDProductSelector.getData();
		for (var i = 0; i < selecteds.length; i++) {
			$.FDProductSelector.select(selecteds[i].Id);
		}
	},
	selectALL: function () {
		var total = $("#_FDProductSelector table").hiMallDatagrid("data").total;
		var url = $.FDProductSelector.url;
		$.FDProductSelector.params.page = 1,
        $.FDProductSelector.params.rows = total,
        $.FDProductSelector.params.onlyId = true;
		$.post(url, $.FDProductSelector.params,
            function (result) {
				var data=result.rows
            	for (var i = 0; i < data.length; i++) {
            		$.FDProductSelector.select(data[i].Id,data[i]);
            	}
            });
		$.FDProductSelector.params.onlyId = false;
	},
	remove: function (id,pro) {
		if (!pro) {
			pro = $.FDProductSelector.getDataById(id);
		}
		if (pro != null)
			$.FDProductSelector.selected.remove(pro);
		$.FDProductSelector.selectedIds.remove(id);
		$("#_FDProductSelector [tag=check-item][value=" + id + "]").prop("checked", false);
	},
	removeAll: function () {
		var selecteds = $.FDProductSelector.getData();
		for (var i = 0; i < selecteds.length; i++) {
			$.FDProductSelector.remove(selecteds[i].Id);
		}
	},
	removeALL: function () {
		var total = $("#_FDProductSelector table").hiMallDatagrid("data").total;
		var url = $.FDProductSelector.url;
		$.FDProductSelector.params.page = 1,
        $.FDProductSelector.params.rows = total,
        $.FDProductSelector.params.onlyId = true;
		$.post(url, $.FDProductSelector.params,
			function (result) {
				var data=result.rows
				for (var i = 0; i < data.length; i++) {
					$.FDProductSelector.remove(data[i].Id,data[i]);
				}
			});
		$.FDProductSelector.params.onlyId = false;
		$(".check-all", "#_FDProductSelector").prop("checked",false);
	},
	removeConflict: function () {
		debugger
		var conflict = $.FDProductSelector.conflict;
		for (var i = 0; i < conflict.length; i++)
			$.FDProductSelector.remove(conflict[i].prouctId);
	},
	showConflict: function () {
		var conflict = $.FDProductSelector.conflict;
		var confilctText = ["<div style='position:relative; height:400px; overflow:auto'><table class='table'>"];
		confilctText.push("<tr><th width='300px'>商品</th><th width='150px'>门店</th><th width='100px'>活动类型</th><th width='100px'>活动名称</th></tr>");
		for (var i = 0; i < conflict.length; i++) {
			var item = conflict[i];
			confilctText.push()
			confilctText.push("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td></tr>".format(item.productName, item.shopName, item.activityTypeName, item.activityName));
		}
		confilctText.push("</table></div>")
		$.dialog({
			title: '活动冲突详情',
			lock: true,
			content: confilctText.join(""),
			width: "600px",
			height: "450px"
		});
	},
	clear: function () {
		$.FDProductSelector.selectedProducts = [];
		$("#_FDProductSelector ul").empty();
		$.FDProductSelector.params = { auditStatus: 2, saleStatus: 1 };
	},
	getSelected: function () {
		var products = [];
		if (!$.FDProductSelector.singleSelect) {
			var selecteds = $("#_FDProductSelector table").hiMallDatagrid('getSelections');
			$.each(selecteds, function (i, product) {
				if (product)
					products.push(product);
			});
		}
		else {
			products.push($("#_FDProductSelector table").hiMallDatagrid('getSelected'));
		}
		return products;
	},
	show: function (selectedProductIds, conflict, activeId, onSelectFinishedCallBack) {
		if (conflict == null) this.conflict = [];
		else this.conflict = conflict;
		$.dialog({
			title: '商品选择',
			lock: true,
			content: this.html,
			padding: '0',
			okVal: '保存',
			cancelVal: "关闭",
			//height:"550px",
			ok: function () {
				var products = $.FDProductSelector.selected;
				if (products == null || products.length == 0) {
					products = $("#_FDProductSelector table").hiMallDatagrid('getSelections');
				}
				var ids = $.FDProductSelector.selectedIds;
				var result = onSelectFinishedCallBack && onSelectFinishedCallBack(products, ids);
				if (result != false) {
					$.FDProductSelector.clear();
					$('#_FDProductSelector').remove();
				}
				else {
					return false;
				}
			},
			close: function () {
				$.FDProductSelector.clear();
				$('#_FDProductSelector').remove();
			}
		});
		this.reload(selectedProductIds, activeId);
		if (this.conflict.length > 0) {
			debugger
			var products = conflict.select(function (i) { return i.prouctId; }).distinct();
			$("#_FDProductSelector .conflict").removeClass("hide");

			$("#_FDProductSelector .conflict em").text(products.length);
		}
	}

};