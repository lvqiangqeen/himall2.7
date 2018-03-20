/*!
 * jQuery.mouseDelay.js v1.2
 * http://www.planeart.cn/?p=1073
 * Copyright 2011, TangBin
 * Dual licensed under the MIT or GPL Version 2 licenses.
 */
(function($,g){var h={},id=1,etid=g+'ETID';$.fn[g]=function(e,f){id++;f=f||this.data(etid)||id;e=e||150;if(f===id)this.data(etid,f);this._hover=this.hover;this.hover=function(c,d){c=c||$.noop;d=d||$.noop;this._hover(function(a){var b=this;clearTimeout(h[f]);h[f]=setTimeout(function(){c.call(b,a)},e)},function(a){var b=this;clearTimeout(h[f]);h[f]=setTimeout(function(){d.call(b,a)},e)});return this};return this};$.fn[g+'Pause']=function(){clearTimeout(this.data(etid));return this};$[g]={get:function(){return id++},pause:function(a){clearTimeout(h[a])}}})(jQuery,'mouseDelay');