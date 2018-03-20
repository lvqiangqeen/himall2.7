/*
author: Five(673921852)
description: 市级地区选择（适用于运费计算，收货地址）
2015.6.1
*/

(function($) {
	$.fn.himallDistrict = function(options) {
		var defaults = {
			id : $(this).attr("id"),// 设置id
			renderTo : $(this).parent(),// 指定绑定源
			items : new Array(),// 地区数据源
			select : '',// 默认选中项,优先从this的data-select属性获取默认选中值
			ajaxUrl:'',
			closeFn : function() {// 关闭时回调
			}
		};

		var params = $.extend(defaults, options);
		params.renderTo = (typeof params.renderTo == 'string' ? $(params.renderTo) : params.renderTo);
		
		var getData=function(id){
			var result={};
			$.ajax({
				url: params.ajaxUrl,
				async:false,
				cache:true,
				data:{parent:id},
				success: function (data) {
					result=data;
				}
			});
			return result;
		}
		
		/**
		 * 遍历
		 */
		this.each(function() {
			
			var _this = $(this);
			var district=_this.siblings('.himall-district');
			
			if (!(params.id.length > 0)) {
				return false;
			}
			var thisId = 'himallDistrict-' + params.id;
			var arrSelect = params.select.split(',');
			
			var ids=_this.data("select")+'';
			if (ids) {
				if(ids.indexOf(',')>-1){
					arrSelect = ids.split(',');
				}else{
					arrSelect = [ids];
				}
			}
			
			//初始化省
			var provinceData=getData(0);
				provinceStr='<ul class="district-ul province-ul cl">';
			
			for(var i=0; i<provinceData.length;i++){
				provinceStr+='<li><a data-id="'+provinceData[i].Id+'" >'+provinceData[i].Name+'</a></li>';
			}
			provinceStr+='</ul>';
			
			var himallDistrict = '<div class="himall-district" id="'+thisId+'"><div class="district-hd"><span>请选择省</span></div>'+provinceStr+'<ul class="district-ul city-ul cl"></ul><ul class="district-ul county-ul cl"></ul><ul class="district-ul street-ul cl"></ul></div>';
			
			
			if(params.renderTo.find("#"+thisId).length>0){
				if($("#"+thisId).is(':visible')){
					$("#"+thisId).hide();
					_this.removeClass('active');
					return;
				}else{
					$("#"+thisId).show();
					_this.addClass('active');
				}
				
			}else{
				params.renderTo.append(himallDistrict);
				_this.addClass('active');
			}
			
			
			var headSelect=params.renderTo.find('.district-hd');
			
			
			//省级点击
			var provinceId,
				cityId,
				countyId;
				
			params.renderTo.off('click','.province-ul a');
			params.renderTo.on('click','.province-ul a',function() {
				var cityStr='';
				provinceId=$(this).data('id');
				var parent=getData(provinceId);
				$(this).parent().addClass('cur').siblings().removeClass();
				if(parent.length>0){
					$(this).parents('.district-ul').hide().siblings('ul.city-ul').show();
					headSelect.html('<span>'+$(this).text()+'</span> <span class="cur">请选择市</span>')
					for(var i=0; i<parent.length;i++){
						cityStr+='<li><a data-id="'+parent[i].Id+'">'+parent[i].Name+'</a></li>';
					}
					params.renderTo.find('.city-ul').html(cityStr);
				}else{
					headSelect.html('<span class="cur">'+$(this).text()+'</span>');
					_this.removeClass('active').data('select',provinceId).html(headSelect.text());
					$(this).parents('.himall-district').hide();
					params.closeFn();
				}
			});
			
			//市级点击
			params.renderTo.off('click','.city-ul a');
			params.renderTo.on('click','.city-ul a',function() {
				var countyStr='';
				cityId=$(this).data('id');
				var parent=getData(cityId);
				$(this).parent().addClass('cur').siblings().removeClass();
				if(parent.length>0){
					$(this).parents('.district-ul').hide().siblings('ul.county-ul').show();
					headSelect.html('<span>'+headSelect.children().eq(0).text()+'</span> <span>'+$(this).text()+'</span> <span class="cur">请选择区</span>');
					for(var i=0; i<parent.length;i++){
						countyStr+='<li><a data-id="'+parent[i].Id+'">'+parent[i].Name+'</a></li>';
					}
					params.renderTo.find('.county-ul').html(countyStr);
				}else{
					headSelect.html('<span>'+headSelect.children().eq(0).text()+'</span> <span class="cur">'+$(this).text()+'</span>');
					_this.removeClass('active').data('select',provinceId+','+cityId).html(headSelect.text());
					$(this).parents('.himall-district').hide();
					params.closeFn();
				}
			});
			
			//区级点击
			params.renderTo.off('click','.county-ul a');
			params.renderTo.on('click','.county-ul a',function() {
				var streetStr='';
				countyId=$(this).data('id');
				var parent=getData(countyId);
				$(this).parent().addClass('cur').siblings().removeClass();
				if(parent.length>0){
					$(this).parents('.district-ul').hide().siblings('ul.street-ul').show();
					headSelect.html('<span>'+headSelect.children().eq(0).text()+'</span> <span>'+headSelect.children().eq(1).text()+'</span> <span>'+$(this).text()+'</span> <span class="cur">请选择街道</span>');
					for(var i=0; i<parent.length;i++){
						streetStr+='<li><a data-id="'+parent[i].Id+'">'+parent[i].Name+'</a></li>';
					}
					params.renderTo.find('.street-ul').html(streetStr);
				}else{
					headSelect.html('<span>'+headSelect.children().eq(0).text()+'</span> <span>'+headSelect.children().eq(1).text()+'</span> <span class="cur">'+$(this).text()+'</span>');
					_this.removeClass('active').data('select',provinceId+','+cityId+','+countyId).html(headSelect.text());
					$(this).parents('.himall-district').hide();
					params.closeFn();
				}
			});
			
			//街级点击
			params.renderTo.off('click','.street-ul a');
			params.renderTo.on('click','.street-ul a',function() {
				headSelect.children().eq(3).text($(this).text().substring(0,6));
				_this.removeClass('active').data('select',provinceId+','+cityId+','+countyId+','+$(this).data('id')).html(headSelect.text());
				$(this).parent().addClass('cur').siblings().removeClass();
				$(this).parents('.himall-district').hide();
				params.closeFn();
				
			} );
			
			//切换卡点击
			params.renderTo.on('click','.district-hd span',function() {
				$(this).addClass('cur').siblings().removeClass().parent().siblings('ul').hide().eq($(this).index()).show();
			});
			
			//初始化数据
			if(arrSelect.length>0){
				var arrSet = _this.text();
				if(arrSet.indexOf(' ')>-1){
					arrSet = arrSet.split(' ');
				}else{
					arrSet=[arrSet];
				}
				
				var strH='';
				for(var i=0; i<arrSet.length; i++){
					if(i<arrSet.length-1){
						params.renderTo.find('ul').eq(i).find('a[data-id="'+arrSelect[i]+'"]').click();
					}else{
						params.renderTo.find('ul').eq(i).find('a[data-id="'+arrSelect[i]+'"]').parent().addClass('cur');
					}
					strH+=(i>0?' ':'')+'<span>'+arrSet[i]+'</span>';
				}
				headSelect.html(strH);
				
				headSelect.children().first().click();
				params.closeFn();
			}
		});
	};
})(jQuery);