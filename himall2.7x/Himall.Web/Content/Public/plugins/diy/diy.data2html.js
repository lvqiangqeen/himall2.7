/*!
 * HiShop Template Edit Config 
 * version: 1.0
 * build: Tue Aug 11 2015 11:16
 * author: CJZhao
 */

$(function () {
	HiShop.Convert = HiShop.Convert ? HiShop.Convert : {}; //Convert 命名空间
	/*将数据转换为html文本*/
	HiShop.Convert.ToHtml = function () {
		var a = HiShop.DIY.Unit.getData();
		var head = $("#tpl_diy_con_typeHeader_style1").html();
		var con = _.template(head, a.PModules[0]);
		_.each(a.LModules,
		  function (a) {
		  	con += _.template($("#tpl_diy_con_type" + a.type).html(), a);
		  });
		console.log(con);
	}
});