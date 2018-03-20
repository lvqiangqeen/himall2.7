(function () {
	var templateId, ulAreaData, freightDetailData, liDataKey = 'listItem';

	$(function () {
		//需引用vue.js CDN地址:http://cdn.jsdelivr.net/vue/1.0.7/vue.min.js
		//vue.js 是属性发生改变就会更新文档，无需手动刷新
		ulAreaData = new Vue({
			el: '#ulArea',
			data: {
				leftProvince: getListData(0),
				editRowIndex: 0,//当前编辑区域的行序号
				rightProvice: []
			}, computed: {
				// a computed getter
				currentRightProvice: function () {
					while (this.rightProvice.length <= this.editRowIndex)
						this.rightProvice.push([]);
					return this.rightProvice[this.editRowIndex];
				}
			}
		});
		freightDetailData = new Vue({
			el: 'table.table-area-freight>tbody',
			data: {
				freightDetails: []
			},
			methods: {
				edit: function (index) {
					var _this = this;
					ulAreaData.editRowIndex = index;

					//弹窗
					$.dialog({
						title: '选择区域',
						lock: true,
						width: 700,
						id: 'logoArea',
						content: $("#ulArea")[0],
						padding: '20px 30px',
						okVal: '保存',
						ok: function () {
							_this.freightDetails[index].showSelectData = showSelect(ulAreaData.currentRightProvice);
						}
					});
				},
				remove: function (index) {
					ulAreaData.editRowIndex = index;
					var list = ulAreaData.currentRightProvice;
					while (list.length > 0) {
						var data = list[0];
						removeChildren(list, data);

						var parent = createParent(ulAreaData.leftProvince, data);
						addChildren(parent, data);
					}

					ulAreaData.rightProvice.removeAt(index);
					this.freightDetails.removeAt(index);
				}
			},
			created: function () {
				$('table.table-area-freight>tbody').show();
			}
		});

		templateId = $('#Id').val();
		if ($.notNullOrEmpty(templateId) && templateId > 0) {
			$.get('GetFreightAreaDetail?templateId={0}'.format(templateId), function (data) {
				if (data && data.length > 0) {
					data = data.where(function (p) { return p.isDefault == 0; });

					var selects = [];
					for (var i = 0; i < data.length; i++) {
						var item = data[i];

						var list = [];

						for (var j = 0; j < item.freightAreaDetail.length; j++) {
							var area = item.freightAreaDetail[j];
							var province = ulAreaData.leftProvince.first(function (p) { return p.Id == area.provinceId; });
							if (area.cityId) {
								getListData(area.provinceId, province);

								var city = province.childrens.first(function (p) { return p.Id == area.cityId; });
								if (area.countyId) {
									getListData(area.cityId, city);

									var county = city.childrens.first(function (p) { return p.Id == area.countyId; });
									if (area.townIds) {
										getListData(area.countyId, county);

										var townIds = area.townIds.split(',');
										for (var k = 0; k < townIds.length; k++) {
											var town = county.childrens.first(function (p) { return p.Id == townIds[k]; });
											list.push(town);
										}
									} else
										list.push(county);
								} else
									list.push(city);
							} else
								list.push(province);
						}

						selects.push(list);
					}

					for (var i = 0; i < data.length; i++) {
						ulAreaData.editRowIndex = i;
						var list = selects[i];

						for (var j = 0; j < list.length; j++) {
							var listItem = list[j];
							removeChildren(ulAreaData.leftProvince, listItem);
							var parent = createParent(ulAreaData.currentRightProvice, listItem);
							addChildren(parent, listItem);
						}

						var detail = $.extend(data[i], { showSelectData: showSelect(ulAreaData.currentRightProvice) });
						freightDetailData.freightDetails.push(detail);
					}
				}
			});
		}

		//初始化页面
		$('#inputDefFirstUnit').val(initDefFirst);
		$('#inputDefFirstUnitMonry').val(initDefFirstMoney);
		$('#inputDefAccumulationUnit').val(initDefAccumulationUnit);
		$('#inputDefAccumulationUnitMoney').val(initDefAccumulationUnitMoney);

		$("#regionSelector").RegionSelector({
			selectClass: "form-control input-sm select-sort",
			valueHidden: "#SourceAddress"
		});

		$('#radioSelfDef,#radioSellerDef').click(function () {
			SetFreeStatus();
		});
		$('#radioPiece,#radioWeight,#radioBulk').click(function () {
			setUnit();
		});
		//根据计价方式设置单位
		setUnit();
		//根据是否包邮隐藏地区运费
		SetFreeStatus();

		$('#btnSave').click(function () {
			SaveData();
		});

		//新增行
		$('#addCityFreight').click(function () {
			freightDetailData.freightDetails.push({ showSelectData: [] });
		});

		$('a[name="delContent"]').click(function () {
			$(this).parent().parent().parent().remove();
		});

		//弹框显示市级
		$(document).on('click', '.operate', function () {
			var cityDiv = $(this).siblings('div');
			if (cityDiv.is(':hidden')) {
				$('.city-box').hide();
				cityDiv.show();
			} else {
				cityDiv.hide();
			}

		});
		if (IsUsed == 1) {
			$('input[name="valuationMethod"]').attr('disabled', 'disabled');
			$('#valuationMethodTip').text('已使用，不能修改');
		}
		//弹框关闭市级
		$(document).on('click', '.city .colse', function () {
			$(this).parents('.city-box').hide();
		});
	});

	function getListData(id, parent) {
		if (parent && parent.initChildren == true)
			return;

		var listData = [];
		$.ajax({
			url: '/common/RegionAPI/GetSubRegion',
			type: 'POST', //GET
			async: false,    //或false,是否异步
			data: {
				parent: id
			},
			timeout: 5000,    //超时时间
			dataType: 'json',    //返回的数据格式：json/xml/html/script/jsonp/text
			success: function (data) {
				sortList(data);
				for (var i = 0; i < data.length; i++) {
					var item = data[i];
					item.showChildren = false;
					item.initChildren = false;
					item.childrens = [];
					item.childrensLength = 0;
					item.isFull = function () {//判断是否包含所有下级
						var fullChildrens = this.childrens.sum(function (p) { return p.isFull() ? 1 : 0; });//sum方法是申明在CommonJS.js里面的Array的扩展方法
						return fullChildrens == this.childrensLength;
					};
					if (parent) {
						item.parent = parent;
						parent.childrens.push(item);//属性发生改变自动刷新文档
						parent.childrensLength++;//此属性只能在getListData后赋值
						parent.initChildren = true;
					}
				}
				listData = data;
			}
		});

		return listData;
	}

	function openChildren(id) {
		var _this = $(this);
		var li = _this.parent();

		var item = findItemData(id, li);
		if (item.initChildren == false)
			getListData(id, item);
		item.showChildren = !item.showChildren;//属性发生改变自动刷新文档

		return false;
	}

	//找到文档节点对应的数据
	function findItemData(id, li) {
		var item = li.data(liDataKey);
		if (item == null) {
			var items;
			var parentLi = li.parent().closest('li');
			if (parentLi.length == 0)
				items = li.closest('ul.ul_first.unused').length > 0 ? ulAreaData.leftProvince : ulAreaData.currentRightProvice;
			else
				items = findItemData(parentLi.attr('id'), parentLi).childrens;

			for (var i = 0; i < items.length; i++) {
				var temp = items[i];
				if (temp.Id == id) {
					li.data(liDataKey, temp);
					return temp;
				}
			}
		}
		return item;
	}

	function selectItem(e, id) {
		e = e ? e : window.event;

		if (window.event) { // IE  
			e.cancelBubble = true;
		} else { // FF  
			//e.preventDefault();   
			e.stopPropagation();
		}

		$(this).parent().toggleClass("selected");
		return false;
	}

	//添加
	function addItem() {
		$('#ulArea ul.ul_first.unused li.selected').each(function () {
			var li = $(this);
			var data = findItemData(li.attr('id'), li);

			removeChildren(ulAreaData.leftProvince, data);
			var parent = createParent(ulAreaData.currentRightProvice, data);
			addChildren(parent, data);
		});
	}

	//移除
	function removeItem() {
		$('#ulArea ul.ul_first.used li.selected').each(function () {
			var li = $(this);
			var data = findItemData(li.attr('id'), li);

			removeChildren(ulAreaData.currentRightProvice, data);
			var parent = createParent(ulAreaData.leftProvince, data);
			addChildren(parent, data);
		});
	}

	function addChildren(parent, item) {
		for (var i = 0; i < item.childrens.length; i++) {
			var children = item.childrens[i];
			var exitChildrent = parent.childrens.first(function (p) { return p.Id == children.Id });//first方法是申明在CommonJS.js里面的Array的扩展方法
			if (exitChildrent) {
				addChildren(exitChildrent, children);
			}
			else {
				parent.childrens.push(children);
				children.parent = parent;
			}
		}
		sortList(parent.childrens);
	}

	function createParent(list, item) {
		var parent;
		if (item.parent) {
			parent = createParent(list, item.parent);//获取到新添加或已存在的项
			list = parent.childrens;
		}
		if (list == null)
			debugger;
		var children = list.first(function (p) { return p.Id == item.Id });//first方法是申明在CommonJS.js里面的Array的扩展方法
		if (children == null) {//判断list里是否已有这一项
			var newItem = {};
			for (var pn in item) {
				newItem[pn] = item[pn];
			}
			newItem.childrens = [];
			newItem.parent = parent;
			list.push(newItem);
			sortList(list);
			return newItem;//创建新项做为下一项的父项返回
		}

		return children;//将已有项做为下一项的父项返回
	}

	function removeChildren(list, item) {
		if (item.parent) {
			item.parent.childrens.remove(item);//remove方法是申明在CommonJS.js里面的Array的扩展方法
			if (item.parent.childrens.length == 0)
				removeChildren(list, item.parent);
		}
		else
			list.remove(item);
	}

	function sortList(list) {
		list.sort(function (a, b) {
			return a.Id - b.Id;
		});
	}

	function showSelect(list) {
		var array = [];
		for (var i = 0; i < list.length; i++) {
			var item = list[i];
			var temp = { name: item.Name, deep: 0 };
			if (item.isFull() == false) {
				temp.childrens = showSelect(item.childrens);
				var max = 0;
				for (var j = 0; j < temp.childrens.length; j++) {
					if (temp.childrens[j].deep > max)
						max = temp.childrens[j].deep;
				}
				temp.deep = max + 1;
			}
			array.push(temp);
		}
		return array;
	}

	function setUnit() {
		if ($('#radioPiece').attr('checked') == 'checked') {
			$('span[name="ValuationUnitDesc"]').text('件');
			$('span[name="ValuationUnit"]').html('件');
		}
		if ($('#radioWeight').attr('checked') == 'checked') {
			$('span[name="ValuationUnitDesc"]').text('重');
			$('span[name="ValuationUnit"]').html('kg');
		}
		if ($('#radioBulk').attr('checked') == 'checked') {
			$('span[name="ValuationUnitDesc"]').text('体积');
			$('span[name="ValuationUnit"]').html('m<sup>3</sup>');
		}
	}

	function SetFreeStatus() {
		if ($('#radioSelfDef').attr('checked') == 'checked') {
			$('#divContent').show();
		}
		else {
			$('#divContent').hide();
		}
	}

	function checkData(freightArea) {
		if ($('#inputTempName').val() == '') {
			$('#inputTempName').focus();
			$.dialog.errorTips('请输入运费模板名称');
			return false;
		}
		var sourceAddress = $('#SourceAddress').val();
		if (sourceAddress == '' || sourceAddress == '0') {
			$.dialog.errorTips('请选择商品地址');
			return false;
		}

		if ($('#radioSelfDef').attr('checked') == 'checked') {
			//默认运费检查
			var reg = /^[0-9]+([.]{1}[0-9]{1,3})?$/;
			var defFirstUnit = $('#inputDefFirstUnit').val(),
				defFirstUnitMonry = $('#inputDefFirstUnitMonry').val(),
				defAccumulationUnit = $('#inputDefAccumulationUnit').val(),
				defAccumulationUnitMoney = $('#inputDefAccumulationUnitMoney').val();
			if (!reg.test($('#inputDefFirstUnit').val()) || !reg.test($('#inputDefFirstUnitMonry').val()) || !reg.test($('#inputDefAccumulationUnit').val()) || !reg.test($('#inputDefAccumulationUnitMoney').val())) {
				$.dialog.errorTips('默认运费为空或不为数字，请检查');
				return false;
			}
			else {
				if (parseFloat(defFirstUnit) <= 0 || parseFloat(defFirstUnitMonry) < 0 || parseFloat(defAccumulationUnit) <= 0 || parseFloat(defAccumulationUnitMoney) < 0) {
					$.dialog.errorTips('默认运费不能小于零，请检查');
					return false;
				}
			}

			var hasError = false;
			function checkValue(tr, selector, name, value) {
				$(selector, tr).addClass('error');

				if ($.isNullOrEmpty(value)) {
					$.dialog.errorTips(name + '不能为空');
					return false;
				}

				var temp = parseFloat(value);

				if (isNaN(temp)) {
					$.dialog.errorTips(name + '不是数字');
					return false;
				}
			    //指定可配送区域的首费和续费都可以为 0
				if (name.indexOf("费")<0) {
				    if (temp <= 0) {
				        $.dialog.errorTips(name + '不能小于0');
				        return false;
				    }
				}

				$(selector, tr).removeClass('error');
				return true;
			}

			$('table.table-area-freight tbody tr').each(function (i) {
				if (hasError == true)
					return;

				var tr = $(this);
				var item = {
					firstUnit: $('.firstUnit', tr).val(),
					firstUnitMonry: $('.firstUnitMonry', tr).val(),
					accumulationUnit: $('.accumulationUnit', tr).val(),
					accumulationUnitMoney: $('.accumulationUnitMoney', tr).val(),
					isDefault: 0,
					freightAreaDetail: []
				};

				if (!checkValue(tr, '.firstUnit', '首件', item.firstUnit) ||
					!checkValue(tr, '.firstUnitMonry', '首费', item.firstUnitMonry) ||
					!checkValue(tr, '.accumulationUnit', '续件', item.accumulationUnit) ||
					!checkValue(tr, '.accumulationUnitMoney', '续费', item.accumulationUnitMoney)) {
					hasError = true;
					return;
				}

				var selectArea = ulAreaData.rightProvice[i];
				if (selectArea == null || selectArea.length == 0) {
					$.dialog.errorTips('运送地区不能为空');
					hasError = true;
					return;
				}

				for (var i = 0; i < selectArea.length; i++) {
					var province = selectArea[i];
					if (province.childrens.length == 0) {
						item.freightAreaDetail.push({ provinceId: province.Id });
						continue;
					}
					for (var j = 0; j < province.childrens.length; j++) {
						var city = province.childrens[j];
						if (city.childrens.length == 0) {
							item.freightAreaDetail.push({ provinceId: province.Id, cityId: city.Id });
							continue;
						}
						for (var k = 0; k < city.childrens.length; k++) {
							var county = city.childrens[k];
							item.freightAreaDetail.push({
								provinceId: province.Id,
								cityId: city.Id,
								countyId: county.Id,
								townIds: county.childrens.newitem(function (p) { return p.Id; }).join(',')
							});
						}
					}
				}

				freightArea.push(item);
			});

			if (hasError == false)
				return freightArea;
			return false;
		}
		return true;
	}

	function SaveData() {
		var freightArea = [{
			firstUnit: $('#inputDefFirstUnit').val(),
			firstUnitMonry: $('#inputDefFirstUnitMonry').val(),
			accumulationUnit: $('#inputDefAccumulationUnit').val(),
			accumulationUnitMoney: $('#inputDefAccumulationUnitMoney').val(),
			isDefault: 1,
			freightAreaDetail: []
		}];

		if (checkData(freightArea)) {
			var freightTemplate = {};
			freightTemplate.id = templateId;
			freightTemplate.Name = $('#inputTempName').val();
			freightTemplate.SourceAddress = $("#SourceAddress").val();
			freightTemplate.SendTime = $("#selsendtime").val();
			freightTemplate.IsFree = $('input[name="isfree"]:checked').val();
			freightTemplate.ValuationMethod = $('input[name="valuationMethod"]:checked').val();
			freightTemplate.FreightArea = freightArea;

			var loading = showLoading();
			$.post('SaveTemplate', { templateinfo: freightTemplate }, function (result) {
				loading.close();
				if (result.successful) {
					$.dialog.succeedTips('保存成功！');
					if (window.location.href.toLowerCase().indexOf('tar=freighttemplate') > 0) {
						if (window.opener && window.opener.BindfreightTemplate) {
							window.opener.BindfreightTemplate();
						}
						window.close();
					}
					window.location.href = "/SellerAdmin/FreightTemplate/Index";
				}
			});
		}
	}

	window.freightTemplateAddJs = {
		addItem: addItem,
		removeItem: removeItem,
		selectItem: selectItem,
		openChildren: openChildren
	};
})();