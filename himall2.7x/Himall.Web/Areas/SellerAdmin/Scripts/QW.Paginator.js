/*
 * QW.Paginator.js
 * Generic pagination component
 * Author:Dzy [www.daiziyi.com]
 * Note:jQuery >= 1.7
 * Url:https://github.com/lovepoco/QW.Paginator
 * Thanks my wife ChenQianhua,Thanks my two son CongCong and ZhangZhang
 */

(function ($) {
    'use strict';

    var QWPAGINATOR_DATA_NAME = "QWPaginator";
	var QWPAGINATOR_DOM_ROLE_NAME="qwp-role";
	var QWPAGINATOR_DOM_DATA_NAME="qwp-data";

    $.QWPaginator = function (el, options) {
        if (!(this instanceof $.QWPaginator)) {
            return new $.QWPaginator(el, options);
        }
        options = $.extend({}, $.QWPaginator.defaultOptions, options);
        var self = this;
        self.container = $(el);
        self.container.data(QWPAGINATOR_DATA_NAME, self);
        self.options = options;

        self.init = function (opt) {
            self.verify();
            self.render.draw();
            self.triggerEvent(this.options.currentPage, 'init');
        };

        self.verify = function () {
            var opts = self.options;
            if (!self.isNumber(opts.totalPages)) {
                opts.totalPages = $.QWPaginator.defaultOptions.totalPages;
            }
            if (!self.isNumber(opts.totalCounts)) {
                opts.totalCounts = $.QWPaginator.defaultOptions.totalCounts;
            }
            if (!self.isNumber(opts.pageSize)) {
                opts.pageSize = $.QWPaginator.defaultOptions.pageSize;
            }
            if (!self.isNumber(opts.currentPage)) {
                opts.currentPage = $.QWPaginator.defaultOptions.currentPage;
            }
            if (!self.isNumber(opts.visiblePages)) {
                opts.visiblePages = $.QWPaginator.defaultOptions.visiblePages;
            }
            if (!opts.totalPages) {
                throw new Error('[QWPaginator] totalPages is required');
            }
            if (!opts.totalPages && !opts.pageSize && opts.totalCounts) {
                opts.pageSize = $.QWPaginator.defaultOptions.pageSize;
            }
            if (opts.totalCounts && opts.pageSize) {
                opts.totalPages = Math.ceil(opts.totalCounts / opts.pageSize);
            } else {
                opts.totalPages = $.QWPaginator.defaultOptions.totalPages;
            }
            if (opts.currentPage < 1) {
                throw new Error('[QWPaginator] currentPage is incorrect');
            }
            if (opts.totalPages < 1) {
                throw new Error('[QWPaginator] totalPages cannot be less currentPage');
            }
            //if (opts.currentPage > opts.totalPages) { throw new Error('[QWPaginator] currentPage is incorrect'); }
        };
		
        self.render = {
			buildHtml:function(){
				var html = [];
				var pages = self.getPages();
				for (var i = 0, j = pages.length; i < j; i++) {
					html.push(this.buildItem('page', pages[i]));
				}

				self.isEnable('prev') && html.unshift(this.buildItem('prev', self.options.currentPage - 1));
				self.isEnable('first') && html.unshift(this.buildItem('first', 1));
				self.isEnable('pageinfo') && html.unshift(this.buildItem('pageinfo', self.options.currentPage));
				self.isEnable('next') && html.push(this.buildItem('next', self.options.currentPage + 1));
				self.isEnable('last') && html.push(this.buildItem('last', self.options.totalPages));
				if (self.options["showInputPage"]) {
				    html.push('<a id="jump-to" class="btn btn-default btn-ssm">指定跳转</a>');
				    html.push('<div class="tipic-dialog"><div class="page-jump"><span>跳转到第<input class="pageNumberTxt input-int-num qw_txt_go" type="text" value="' + self.options.currentPage + '">页</span><input type="button" class="btn btn-default btn-ssm pageJumpBtn qw_btn_go" value="确定"></div></div>');
				}
				
				self.container.html(html.join(''));
			},
			buildItem:function(type, pageData){
				var html = self.options[type]
					.replace(/{{page}}/g, pageData)
					.replace(/{{totalPages}}/g, self.options.totalPages)
					.replace(/{{totalCounts}}/g, self.options.totalCounts);
				var _d = $(html);
				if (type != "pageinfo") {
				    _d.attr(QWPAGINATOR_DOM_ROLE_NAME, type);
				    _d.attr(QWPAGINATOR_DOM_DATA_NAME, pageData);
				}
				return this.getOutHTML(_d);
			},
			setStatus:function(){
				var options = self.options;

				if (!self.isEnable('first') || options.currentPage === 1) {
					$('['+QWPAGINATOR_DOM_ROLE_NAME+'=first]', self.container).addClass(options.disableClass);
				}
				if (!self.isEnable('prev') || options.currentPage === 1) {
					$('['+QWPAGINATOR_DOM_ROLE_NAME+'=prev]', self.container).addClass(options.disableClass);
				}
				if (!self.isEnable('next') || options.currentPage >= options.totalPages) {
					$('['+QWPAGINATOR_DOM_ROLE_NAME+'=next]', self.container).addClass(options.disableClass);
				}
				if (!self.isEnable('last') || options.currentPage >= options.totalPages) {
					$('['+QWPAGINATOR_DOM_ROLE_NAME+'=last]', self.container).addClass(options.disableClass);
				}
				var _paged=$('['+QWPAGINATOR_DOM_ROLE_NAME+'=page]', self.container);
				_paged.each(function(){
					var _t=$(this);
					if(_t.attr(QWPAGINATOR_DOM_DATA_NAME)==options.currentPage){
						_t.addClass(options.activeClass);
					}else{
						_t.removeClass(options.activeClass);
					}
				})
			},
			bindEvents:function(){
				var opts = self.options;
				self.container.off();
				
				self.container.on('click', '['+QWPAGINATOR_DOM_ROLE_NAME+']', function () {
					var $el = $(this);
					if ($el.hasClass(opts.disableClass) || $el.hasClass(opts.activeClass)) {
						return;
					}
					var pageIndex = +$el.attr(QWPAGINATOR_DOM_DATA_NAME);
					//if (pageIndex > opts.totalPages) { throw new Error("The pageIndex cannot be greater than "+opts.totalPages+";"); }
					if (self.triggerEvent(pageIndex, 'go')) {
						self.goPage(pageIndex);
					}
				});
				$('#jump-to,.tipic-dialog', self.container).click(function (e) {
				    e ? e.stopPropagation() : event.cancelBubble = true;
				});
				$('#jump-to', self.container).click(function () {
				    $('.tipic-dialog', self.container).fadeToggle(200).find('input').first().focus();
				});
				$(document).click(function () {
				    $('.tipic-dialog', self.container).hide();
				});
				$(".qw_btn_go", self.container).on('click', function () {
					var $el = $(this);
					var $gonum = self.container.find(".qw_txt_go");
					var txtval = parseInt($gonum.val());
					if (txtval <= 0 || isNaN(txtval)) {
						$gonum.val("1");
						return;
					}
					var pageIndex = +txtval;
					if (self.triggerEvent(pageIndex, 'go')) {
						self.goPage(pageIndex);
					}
				});
				
				self.container.on('keypress', ".qw_txt_go", function (event) {
					if (event.keyCode == 13) {
					var $el = $(this);
					self.container.find(".qw_btn_go").trigger("click");
					event.returnValue = false;
					event.stopPropagation();
					event.preventDefault();
					return false;
					}
				});
			},
			getOutHTML:function(d){
				return $("<p>").append(d.eq(0).clone()).html();
			},
			draw:function(){
				this.buildHtml();
				this.setStatus();
				this.bindEvents();
			}
        };

        self.loading = {
            LOADING_MSG: "<div class='qw_p_loading'><p>Loading...</p></div>",
            wrap: null,
            get: function () {
                if (this.wrap.is("tbody")) {
                    this.wrap = this.wrap.parent().parent();
                }
                var target = $(".qw_p_loading", this.wrap);
                if (target.length < 1) {
                    target = $(this.LOADING_MSG);
                    target.hide();
                    this.wrap.append(target);
                }
                return target;
            },
            show: function () {
                this.wrap = self.container;
                var _d = this.get();
                _d.show();
            },
            hide: function () {
                this.wrap = self.container;
                var _d = this.get();
                _d.hide();
            }
        };

        self.getPages = function () {
            var pages = [],
                visiblePages = self.options.visiblePages,
                currentPage = self.options.currentPage,
                totalPages = self.options.totalPages;

            if (visiblePages > totalPages) {
                visiblePages = totalPages;
            }

            var half = Math.floor(visiblePages / 2);
            var start = currentPage - half + 1 - visiblePages % 2;
            var end = currentPage + half;

            if (start < 1) {
                start = 1;
                end = visiblePages;
            }
            if (end > totalPages) {
                end = totalPages;
                start = 1 + totalPages - visiblePages;
            }

            var itPage = start;
            while (itPage <= end) {
                pages.push(itPage);
                itPage++;
            }

            return pages;
        };

        self.isNumber = function (value) {
            var type = typeof value;
            return type === 'number' || type === 'undefined';
        };

        self.isEnable = function (type) {
            return self.options[type] && typeof self.options[type] === 'string';
        };

        self.goPage = function (pageIndex) {
            self.options.currentPage = pageIndex;
            self.render.draw();
        };

        self.triggerEvent = function (pageIndex, type) {
            return (typeof self.options.onPageChange !== 'function') || (self.options.onPageChange(pageIndex, type) !== false);
        };

        self.callMethod = {
            option: function (options) {
                self.options = $.extend({}, self.options, options);
                self.verify();
                self.render.draw();
                return self.options;
            },
            destroy: function () {
                self.container.empty();
                self.container.removeData(QWPAGINATOR_DATA_NAME);
            },
            redraw: function (param) {
                if (param) {
                    //设置选项值
                    self.options = $.extend({}, self.options, param);
                }
                self.init(self.options);
            },
            loading: function (isEmpty) {
                if (isEmpty) {
                    self.container.empty();
                }
                self.loading.show();
            },
            loaded: function () {
                self.loading.hide();
            },
            external: function (param) {
                if (typeof (param) != "undefined") {
                    self.options = $.extend({}, self.options, { external: param });
                }
                return self.options.external;
            }
        };

        self.init(options);

        return self.container;
    };

    $.QWPaginator.defaultOptions = {
        first: '<li class="page-first"><a href="javascript:;">First</a></li>',
        prev: '<li class="page-prev"><a href="javascript:;">&laquo;</a></li>',
        next: '<li class="page-next"><a href="javascript:;">&raquo;</a></li>',
        last: '<li class="page-last"><a href="javascript:;">Last</a></li>',
        page: '<li><a href="javascript:;">{{page}}</a></li>',
        pageinfo:'',
		showInputPage:false,
        totalPages: 1,
        totalCounts: 0,
        pageSize: 0,
        currentPage: 1,
        visiblePages: 7,
        disableClass: 'disabled',
        activeClass: 'page-cur',
        external: null,
        onPageChange: null
    };

    $.fn.QWPaginator = function () {
        var _self = this,
            args = Array.prototype.slice.call(arguments);

        if (typeof args[0] === 'string') {
            var $instance = $(_self).data(QWPAGINATOR_DATA_NAME);
            if (args[0] == "isExist") {
                return !!$instance;
            }
            if (!$instance) {
                throw new Error('[QWPaginator] the element is not instantiated');
            } else {
                return $instance.callMethod[args[0]](args[1]);
            }
        } else {
            return new $.QWPaginator(this, args[0]);
        }
    };

})(jQuery);