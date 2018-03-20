/*
 * QW.Repeater.js
 * Repeat data display component
 * Author:Dzy [www.daiziyi.com]
 * Url:https://github.com/lovepoco/QW.Repeater
 * Thanks my wife ChenQianhua,Thanks my two son CongCong and ZhangZhang
 */

(function ($) {
    'use strict';

    var QWREPEATER_DATA_NAME = "QWRepeater";

    $.QWRepeater = function (el, options) {
        if (!(this instanceof $.QWRepeater)) {
            return new $.QWRepeater(el, options);
        }

        options = $.extend({}, $.QWRepeater.defaultOptions, options);

        var self = this;
        self.container = $(el);
        self.container.data(QWREPEATER_DATA_NAME, self);
        self.options = options;

        self.init = function () {
            var opts = self.options;
            if (opts.tmplId && opts.tmplId.length > 0) {
                opts.tmplContent = $(opts.tmplId).html();
            }
            self.verify();
        }

        self.loadData = function () {
            var opts = self.options;
            if (opts.needEmpty) {
                self.container.empty();
            }
            self.loading.show();
            self.dataNull.hide();
            if (typeof opts.onBeforeLoadData == 'function') {
                opts.onBeforeLoadData(self.options);
            }
            if (opts.url && opts.url.length > 0) {
                opts.data = null;
                self.loading.show();
                $.ajax({
                    type: opts.urlMethod,
                    url: opts.url,
                    data: opts.urlParameter,
                    dataType: "json",
                    success: function (data) {
                        self.loading.hide();						
						if (typeof opts.onLoadDataed == 'function') {
							opts.onLoadDataed(self.options);
						}
                        if (data) {
                            opts.data = data;
                            self.render();
                        }
                    },
                    error: function () {
                        self.loading.hide();
                    }
                });
            } else {
				if (typeof opts.onLoadDataed == 'function') {
					opts.onLoadDataed(self.options);
				}
                self.loading.hide();
                self.render();
            }
        }

        self.render = function () {
            self.loading.hide();

            var opts = self.options;
            if (typeof opts.onBeforeDraw == 'function') {
                opts.onBeforeDraw(self.container);
            }
            if (opts.onDataNotNullCheck(opts.data)) {
                var _render = _.template(opts.tmplContent);
                var _html = _render(opts.data);
                self.container.append(_html);
            } else {
                self.dataNull.show();
            }

            if (typeof opts.onDrawed == 'function') {
                opts.onDrawed(self.container);
            }
        }

        self.reload = function (param) {
            var opts = self.options;
            if (param) {
                self.options = $.extend({}, self.options, param);
                self.init();
                opts = self.options;
            }
            self.loadData();
        }

        self.verify = function () {
            var opts = self.options;
			opts.tmplContent=opts.tmplContent || null;
            if (opts.tmplContent == null || opts.tmplContent.length < 1) {
                throw new Error('[QWRepeater] error:template is null');
            }
        }

        self.loading = {
            LOADING_MSG: "<div class='qw_r_loading'><img src='/Images/dg_loading.gif'></div>",
            wrap: null,
            get: function () {
                if (this.wrap.is("tbody")) {
                    this.wrap = this.wrap.parent().parent();
                }
                var target = $(".qw_r_loading", this.wrap);
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
		
        self.dataNull = {
            NO_DATA_MSG: "<div class='qw_r_empty'><p>无数据！</p></div>",
            wrap: null,
            get: function () {
                if (this.wrap.is("tbody")) {
                    this.wrap = this.wrap.parent().parent();
                }
                var target = $(".qw_r_empty", this.wrap);
                if (target.length < 1) {
                    target = $(this.NO_DATA_MSG);
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

        self.callMethod = {
            option: function (options) {
                self.options = $.extend({}, self.options, options);
                self.init();
                return self.options;
            },
            destroy: function () {
                self.container.empty();
                self.container.removeData(QWREPEATER_DATA_NAME);
            },
            reload: function (param) {
                self.reload(param);
            },
            reloadData: function (param) {
                self.reload({ data: param });
            },
            reloadUrl: function (param) {
				var urlp = $.extend({}, self.options.urlParameter, param);
                self.reload({ urlParameter: urlp });
            },
            loading: function (isEmpty) {
                if (isEmpty) {
                    self.container.empty();
                    self.dataNull.hide();
                }
                self.loading.show();
            },
            loaded: function () {
                self.loading.hide();
            },
            external: function (param) {
                if (typeof (param) != "undefined") {
                    self.options = $.extend({}, self.options, { external:param});
                }
                return self.options.external;
            }
        };

        self.init();
        self.loadData();
        return self.container;
    };


    $.QWRepeater.defaultOptions = {
        tmplId: "#datatmpl",   
        tmplContent: "",  
        data: null,  
        url: null,   
        urlParameter: null,
        urlMethod: "get",  
        needEmpty: true,  
		external:null,
        onDataNotNullCheck: function (d) {
            var result = false;
            if (d) {
				result = true;
            }
            return result;
        },
        onBeforeLoadData:null,
        onLoadDataed:null,
        onBeforeDraw: null,   
        onDrawed: null
    };

    $.fn.QWRepeater = function () {
        var _self = this,
            args = Array.prototype.slice.call(arguments);

        if (typeof args[0] === 'string') {
            var $instance = $(_self).data(QWREPEATER_DATA_NAME);
            if (args[0] == "isExist") {
                return !!$instance;
            }
            if (!$instance) {
                throw new Error('[QWRepeater] the element is not instantiated');
            } else {
                return $instance.callMethod[args[0]](args[1]);
            }
        } else {
            return new $.QWRepeater(this, args[0]);
        }
    };
})(jQuery);