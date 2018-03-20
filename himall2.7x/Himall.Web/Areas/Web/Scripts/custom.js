// JavaScript Document

$(window).load(function() {
	var flag=getCookie('resizeWeb');
	
	if(flag==1){
		setFull()
	}
});
$(document).ready(function() {
	
    //头部导航下拉
	$('.navbar-nav .dropdown').hover(function(){
		$(this).addClass('open');
	},function(){
		$(this).removeClass('open');
	});
	
	
	
	//左侧菜单切换
	$('.navbar-nav .dropdown').each(function(){$(this).find('li').eq(0).addClass('current')})
	
	$('.navbar-nav .dropdown').click(function() {
        var str=$(this).find('.dropdown-menu').html();
		$(this).addClass('active').siblings().removeClass('active').end().find('li').first().addClass('current').siblings().removeClass();;
		$('.aside-list').html(str);
		
		//默认跳转至第一个二级菜单的链接
		$("iframe").attr("src", $(this).find('li').first().children().attr("href"));
    });
	
	
	$('.aside-list,.navbar-nav .dropdown').on('click','li',function() {
      //  $(this).addClass('current').siblings().removeClass();
    });
	
	
	
	//iframe高度自适应
	$('.content').height($(window).height()-64);
	$(window).resize(function() {
        $('.content').height($(window).height()-64);
    });
	
	//初始化模拟点击添加左菜单
	$(".nav li").first().click();
	
	//全屏切换
	var flag=0;
	//var classFull='glyphicon-resize-full';
	//var classSmall='glyphicon-resize-small';
	//addCookie('resizeWeb',flag,24);
	$('#resize-web').click(function() {
		
        if($(this).hasClass('glyphicon-resize-full')){
			flag=1;
			setFull();
		}else{
			flag=0;
			$('.container').css({'width':'1170px'});
			$(this).removeClass('glyphicon-resize-small').addClass('glyphicon-resize-full');
			$('.content').css({width:'82.5%'});
			$('.aside').show();
		}
		addCookie('resizeWeb',flag,24)
    });
	

	
});

function setFull(){
	$('.container').css({width:'100%'});
	$('#resize-web').removeClass('glyphicon-resize-full').addClass('glyphicon-resize-small');
	$('.content').css({width:'100%'});
	$('.aside').hide();
}


function addCookie(name,value,expiresHours){ 
	var cookieString=name+"="+escape(value); 
	if(expiresHours>0){ 
		var date=new Date(); 
		date.setTime(date.getTime+expiresHours*3600*1000); 
		cookieString=cookieString+"; expires="+date.toGMTString(); 
	} 
	document.cookie=cookieString; 
}


function getCookie(name){ 
	var strCookie=document.cookie; 
	var arrCookie=strCookie.split("; "); 
	for(var i=0;i<arrCookie.length;i++){ 
		var arr=arrCookie[i].split("="); 
		if(arr[0]==name)return arr[1]; 
	} 
	return ""; 
}

function deleteCookie(name){ 
	var date=new Date(); 
	date.setTime(date.getTime()-10000); 
	document.cookie=name+"=v; expires="+date.toGMTString(); 
} 