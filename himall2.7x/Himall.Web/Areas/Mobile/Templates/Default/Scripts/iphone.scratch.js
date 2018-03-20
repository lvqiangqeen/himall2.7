//2015.11.16
(function($) {
	$.fn.extend({
		scratchOn: function(mask,options) {
			if(mask && mask!=''){
				var $this=$(this);
				var $canvas,$wd,$ht,$avb;
				var defaults = {left:0.36,line:40};
				var opts = $.extend(defaults,options);
				opts.line=$(window).width()/320*opts.line;
				init();
			}//end if
			
			function init(){
				$canvas=$('<canvas></canvas>').attr({width:$this.outerWidth(),height:$this.outerHeight()}).appendTo($this);
				$this.css({visibility:'visible'});
				$wd = $canvas.width();
				$ht = $canvas.height();
				$avb=$wd*$ht*opts.left;//剩余多少有效像素后触发刮刮卡中奖信息
				var ctx=$canvas[0].getContext('2d');
				var img = new Image();
				img.onload = function(){
					ctx.drawImage(img, 0, 0, $wd, $ht);
					ctx.globalCompositeOperation = 'destination-out';
					ctx.fillStyle = "rgba(255,255,255,1)";
					ctx.strokeStyle = "rgba(255, 255, 255, 1)";
					ctx.lineJoin = "round";
					ctx.lineCap = "round";
					ctx.lineWidth = opts.line;
                    ///判断是否可以操作
					if ($(".none-chance").length == 0) {
					    $canvas.on('touchstart', canvas_touchstart).on('touchmove', canvas_touchmove).on('touchend', canvas_touchend);
					    $this.on('off', this_off);
					}
				};
				img.src = mask;
				
			}//end func
			
			function this_off(e){
				$this.off('off',this_off);
				$canvas.off().remove();
				if(opts.onOff) opts.onOff($this);
			}//end func
					
			//----------------刮刮卡功能
					
			function canvas_touchstart(e){
				e.preventDefault();
				var x = e.originalEvent.touches[0].pageX - $(this).offset().left,
				y = e.originalEvent.touches[0].pageY - $(this).offset().top;
				var ctx=this.getContext('2d');
				ctx.beginPath();
				ctx.arc(x, y, opts.line/2, 0, 2 * Math.PI);
				ctx.closePath();
				ctx.fill();
				ctx.beginPath();
				ctx.moveTo(x, y);
			}//end func
			
						
			function canvas_touchmove(e){
				e.preventDefault();
				e.stopImmediatePropagation();
				var x = e.originalEvent.touches[0].pageX - $(this).offset().left,
				y = e.originalEvent.touches[0].pageY - $(this).offset().top;
				var ctx=this.getContext('2d');
				ctx.lineTo(x,y);
				ctx.stroke();
			}//end func    
					
			function canvas_touchend(e){
				e.preventDefault();
				var ctx=this.getContext('2d');
				ctx.closePath();
				// 得到中奖图片的像素数据（像素计算非常耗费CPU和内存，可能会导致浏览器崩溃）。
				var data = ctx.getImageData(0, 0, $wd, $ht).data;
				// 通过计算每一个像素，alpha不为0的为有效像素，得知还有多少有效像素,低于设定值则算作挂卡结束。
				var j=0;
				for (var i = 0; i < data.length; i+=8) if (data[i + 7]) j++;
				console.log(j);
				// 低于设定值则算作挂卡结束，弹出中奖信息，同时撤掉遮挡区域。
				if (j <= $avb) {
					ctx.clearRect(0, 0, $wd, $ht);
					$(this).off().remove();
					if(opts.onComplete) opts.onComplete();
				}//end if
			}//end func
			
		}//end fn
	});//end extend
})(jQuery);//闭包