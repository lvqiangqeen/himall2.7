/*
 * Description:原生图片延迟加载
 * 2015.10.14--five(673921852)
 */
window.delayimg = (function(window, document, undefined) {
	'use strict';
	var store = [],
		nodes,
		offset,
		throttle,
		content,
		poll,
		_inView = function(el) {
			var coords = el.getBoundingClientRect();
			return (((coords.top >= 0 && coords.left >= 0 && coords.top) <= (window.innerHeight || document.documentElement.clientHeight) + parseInt(offset))&&coords.bottom+40>0&&coords.width+coords.height>0);
		},
		_pollImages = function() {
			for (var i = store.length; i--;) {
				var curObj = store[i];
				if (_inView(curObj)) {
					if (curObj.getAttribute('data-delay-background') !== null) {
						curObj.style.opacity=1;
						curObj.style.backgroundImage = "url(" + curObj.getAttribute('data-delay-background') + ")";
						curObj.removeAttribute('data-delay-background');
			        }
			        else if (curObj.getAttribute('data-delay') !== null){
						curObj.src = curObj.getAttribute('data-delay');
						curObj.onload = function() {
							this.style.opacity=1;
							this.removeAttribute('data-delay');
							this.removeAttribute('height');
							this.removeAttribute('width');
							this.parentNode.style.background='transparent';
				        }
			        }
					store.splice(i, 1);
				}
			}
		},
		_throttle = function() {
			clearTimeout(poll);
			poll = setTimeout(_pollImages, throttle);
			if(store.length<=0)
				content.removeEventListener('scroll', _throttle);
		},
		init = function(opts) {
			store=[];
			nodes = document.querySelectorAll('img[data-delay],[data-delay-background]');
			var opts = opts || {};
			offset = opts.offset || 0;
			throttle = opts.throttle || 300;
			content = opts.content || window;
	
			for (var i = 0; i < nodes.length; i++) {
				store.unshift(nodes[i]);
			}
			_throttle();
			content.addEventListener('scroll', _throttle, false);
		},
		render = function(){
			store = [];
			nodes = document.querySelectorAll('img[data-delay],[data-delay-background]');
			for (var i = 0; i < nodes.length; i++) {
				store.unshift(nodes[i]);
			}
			_throttle();
			content.addEventListener('scroll', _throttle, false);
		};

	return {
		init: init,
		render: render
	};

})(window, document);