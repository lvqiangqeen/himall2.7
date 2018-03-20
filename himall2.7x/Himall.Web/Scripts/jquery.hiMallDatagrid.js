/**
 * jQuery hiMallDatagrid 1.1.0
 *
 * 作者：廖日龙
 * 说明：基于JQuery的插件
 *
 * 
 */
(function ($) {
    $.extend(Array.prototype, {
        indexOf: function (o) {
            for (var i = 0, len = this.length; i < len; i++) {
                if (this[i] == o) {
                    return i;
                }
            }
            return -1;
        }, remove: function (o) {
            var index = this.indexOf(o);
            if (index != -1) {
                this.splice(index, 1);
            }
            return this;
        }, removeById: function (filed, id) {
            for (var i = 0, len = this.length; i < len; i++) {
                if (this[i][filed] == id) {
                    this.splice(i, 1);
                    return this;
                }
            }
            return this;
        }
    });
    //内部方法
    //请求Ajax方法
    function request(target, param) {
        var opts = $.data(target, "hiMallDatagrid").options;
        if (param) {
            opts.queryParams = $.extend({}, opts.queryParams, param);
        }
        if (!opts.url) {
            return;
        }
        var queryObj = $.extend({}, opts.queryParams, { page: opts.pageNumber, rows: opts.pageSize });

        //显示加载信息
        $(target).hiMallDatagrid("loading");

        //Ajax获取数据
        setTimeout(function () {
            ajaxRequest();
        }, 0);



        (function initialOperationBtn() {
            //初始化操作按钮
            var operationButtons = $(opts.operationButtons);
            if (operationButtons.length) {
                opts.operationButtonsInnerHTML = operationButtons.html();
                if (operationButtons.hasClass('keep') == false)
                    operationButtons.remove();
            }

            //初始化Toolbar
            if ($(opts.toolbar).length) {
                var inner = $(opts.toolbar).html();
                $(opts.toolbar).remove();
                opts.toolbarInnerHTML = inner;
                $(target).prev("form").remove();
                $(opts.toolbarInnerHTML).insertBefore($(target));
            }
        })();
        function ajaxRequest() {
            $.ajax({
                type: opts.method,
                url: opts.url,
                data: queryObj,
                dataType: "json",
                success: function (data) {
                    setTimeout(function () {
                        $(target).hiMallDatagrid("loaded");
                    }, 0);

                    loadData(target, data);

                },
                error: function () {
                    setTimeout(function () {
                        $(target).hiMallDatagrid("loaded");
                    }, 0);

                    if (opts.onLoadError) {
                        opts.onLoadError.apply(target, arguments);
                    }
                }
            });
        };
    };

    function getSelectRows(target) {
        var opts = $.data(target, "hiMallDatagrid").options;
        var data = $.data(target, "hiMallDatagrid").data;
        var tBody = $(target).find("tbody");
        if (opts.idField) {
            return $.data(target, "hiMallDatagrid").selectedRows;
        } else {
            var rowAry = [];
            $("tr.hiMallDatagrid-row-selected", tBody).each(function () {
                var row_index = parseInt($(this).attr("hiMallDatagrid-row-index"));
                rowAry.push(data.rows[row_index]);
            });
            return rowAry;
        }
    };

    function selectAll(target) {
        var opts = $.data(target, "hiMallDatagrid").options;
        var data = $.data(target, "hiMallDatagrid").data;
        var selectRows = $.data(target, "hiMallDatagrid").selectedRows;
        var tBody = $(target).find("tbody");
        var rows = data.rows;
        selectRows = [];
        $("div.hiMallDatagrid-cell-check input[type=checkbox]:enabled", tBody)
            .prop("checked", true).parents('tr').addClass("hiMallDatagrid-row-selected");
        for (var i = 0; i < rows.length; i++) {
            if (opts.idField) {
                var row = rows[i];
                if ($("div.hiMallDatagrid-cell-check input[type=checkbox]").eq(i).is(':checked')) {
                    selectRows.push(row);
                }
            }
        }
        $.data(target, "hiMallDatagrid").selectedRows = selectRows;
    };
    function clearSelectRows(target) {
        var opts = $.data(target, "hiMallDatagrid").options;
        var data = $.data(target, "hiMallDatagrid").data;
        var selectRows = $.data(target, "hiMallDatagrid").selectedRows;
        var tBody = $(target).find("tbody");
        $("tr", tBody).removeClass("hiMallDatagrid-row-selected");
        $("div.hiMallDatagrid-cell-check input[type=checkbox]", tBody).prop("checked", false);
        if (opts.idField && data.rows != null) {
            for (var i = 0; i < data.rows.length; i++) {
                selectRows.removeById(opts.idField, data.rows[i][opts.idField]);
            }
        }
    };

    function clearSelections(target) {
        clearSelectRows(target);
        var selectRows = $.data(target, "hiMallDatagrid").selectedRows;
        selectRows.splice(0, selectRows.length);
    };

    function getRowByIndex(target, index) {
        var data = $.data(target, "hiMallDatagrid").data;
        if (index < 0 || index >= data.rows.length || isNaN(index)) return;
        return data.rows[index];
    }

    function selectRow(target, index) {
        var opts = $.data(target, "hiMallDatagrid").options;
        var data = $.data(target, "hiMallDatagrid").data;
        var tBody = $(target).find("tbody");
        var selectRows = $.data(target, "hiMallDatagrid").selectedRows;
        if (index < 0 || index >= data.rows.length) {
            return;
        }
        if (opts.singleSelect == true) {
            clearSelections(target);
        }
        var tr = $("tr[hiMallDatagrid-row-index=" + index + "]", tBody);
        if (!tr.hasClass("hiMallDatagrid-row-selected")) {
            tr.addClass("hiMallDatagrid-row-selected");
            var ck = $("div.hiMallDatagrid-cell-check input[type=checkbox]", tr);
            ck.attr("checked", true);
            if (opts.idField) {
                var row = data.rows[index];
                (function () {
                    for (var i = 0; i < selectRows.length; i++) {
                        if (selectRows[i][opts.idField] == row[opts.idField]) {
                            return;
                        }
                    }
                    selectRows.push(row);
                    //console.log(selectRows);
                })();
            }
        }
    };


    function unSelectRow(target, index) {
        var opts = $.data(target, "hiMallDatagrid").options;
        var data = $.data(target, "hiMallDatagrid").data;
        var selectRows = $.data(target, "hiMallDatagrid").selectedRows;
        if (index < 0 || index >= data.rows.length) {
            return;
        }
        var tBody = $(target).find("tbody");
        var tr = $("tr[hiMallDatagrid-row-index=" + index + "]", tBody);
        var ck = $("tr[hiMallDatagrid-row-index=" + index + "] div.hiMallDatagrid-cell-check input[type=checkbox]", tBody);
        tr.removeClass("hiMallDatagrid-row-selected");
        ck.prop("checked", false);
        var row = data.rows[index];
        if (opts.idField) {
            selectRows.removeById(opts.idField, row[opts.idField]);
        }
    };

    //绑定事件预留方法
    function bindCellsEvents(target) {
        var opts = $.data(target, "hiMallDatagrid").options;
        var total = $.data(target, "hiMallDatagrid").data.total;
        var pageTarget = $(target).next();
        var before = $("a.beforePageBtn", pageTarget);
        var after = $("a.afterPageBtn", pageTarget);
        var pageCount = parseInt(total / opts.pageSize) + (total % opts.pageSize == 0 ? 0 : 1);


        //排序事件-xielingxiao
        $(target).off('click', 'th[data-sort]');
        $(target).on('click', 'th[data-sort]', function () {
            var IsAsc = false;
            $(this).siblings().removeClass('state_switch select');
            if (!$(this).hasClass('select')) {
                $(this).addClass('select');
            } else {
                if ($(this).hasClass('state_switch')) {
                    $(this).removeClass('state_switch');
                    IsAsc = false;
                } else {
                    $(this).addClass('state_switch');
                    IsAsc = true;
                }
            }
            request(target, { Sort: $(this).data('sort'), IsAsc: IsAsc });
        });


        //翻页跳转弹框
        $('#jump-to,.tipic-dialog', pageTarget).click(function (e) {
            e ? e.stopPropagation() : event.cancelBubble = true;
        });

        $('#jump-to', pageTarget).click(function () {
            $('.tipic-dialog', pageTarget).fadeToggle(200).find('input').first().focus();
        });
        $(document).click(function () {
            $('.tipic-dialog', pageTarget).hide();

        });

        if (opts.pageNumber > 1) {
            $(before).removeAttr('disabled');
            $(before).unbind("click").bind("click", function () {
                if (opts.pageNumber > 1) {
                    opts.pageNumber = parseInt(opts.pageNumber) - 1;
                    request(target);
                } else {
                    $.dialog.errorTips('已经是第一页了');
                }

            });
        } else {
            $(before).attr('disabled', 'disabled');
        }
        if (opts.pageNumber < pageCount) {
            $(after).removeAttr('disabled');
            $(after).unbind("click").bind("click", function () {
                if (opts.pageNumber < pageCount) {
                    opts.pageNumber = parseInt(opts.pageNumber) + 1;
                    request(target);
                } else {
                    $.dialog.errorTips('已经是最后一页了');
                }

            });
        } else {
            $(after).attr('disabled', 'disabled');
        }

        $("input.pageJumpBtn", pageTarget).unbind("click").bind("click", function () {
            var jump = $("input.pageNumberTxt", pageTarget).val();
            jump = jump.replace(/\b(0+)/gi, "");//去掉前置0
            if (isNaN(jump) || jump > pageCount || jump <= 0) {
                $.dialog.errorTips('请您输入有效的页码');
                $("input.pageNumberTxt", pageTarget).val("");
            }
            else {
                opts.pageNumber = jump;
                request(target);
            }
        });
        $("input.check-all", pageTarget).unbind("click").bind("click", function () {
            if (opts.singleSelect) {
                return false;
            }
            if ($(this).is(":checked")) {
                selectAll(target);
            } else {
                clearSelectRows(target);
            }
        });

        $("div.hiMallDatagrid-cell-check input[type=checkbox]", $(target)).unbind("click").bind("click", function (e) {
            var findIndex = $(this).parent().parent().parent().attr("hiMallDatagrid-row-index");
            if (opts.singleSelect) {
                clearSelections(target);
                selectRow(target, findIndex);
            } else {
                if ($(this).is(":checked")) {
                    selectRow(target, findIndex);
                } else {
                    unSelectRow(target, findIndex);
                }
            }
            e.stopPropagation();
        });
    }

    //加载数据到列表
    function loadData(target, _data) {


        var opts = $.data(target, "hiMallDatagrid").options;

        var selectRows = $.data(target, "hiMallDatagrid").selectedRows;

        $.data(target, "hiMallDatagrid").data = _data;

        //开始渲染列表
        opts.view.render.call(opts.view, target, $(target));

        initialPager(target);
        bindCellsEvents(target);

        clearSelections(target);
        opts.onLoadSuccess();

    };

    //获取所有列表的列集合
    function getColumnFiles(target) {
        var opts = $.data(target, "hiMallDatagrid").options;
        var cols = opts.columns;
        if (cols.length == 0) {
            return [];
        }
        var columnFileds = [];
        function findLocation(index) {
            var c = 0;
            var i = 0;
            while (true) {
                if (columnFileds[i] == undefined) {
                    if (c == index) {
                        return i;
                    }
                    c++;
                }
                i++;
            }
        };
        function getColumnFileds(r) {
            var ff = [];
            var c = 0;
            for (var i = 0; i < cols[r].length; i++) {
                var col = cols[r][i];
                if (col == null || col == undefined) continue;
                if (col.field) {
                    ff.push([c, col.field]);
                }
                c += parseInt(col.colspan || "1");
            }
            for (var i = 0; i < ff.length; i++) {
                ff[i][0] = findLocation(ff[i][0]);
            }
            for (var i = 0; i < ff.length; i++) {
                var f = ff[i];
                columnFileds[f[0]] = f[1];
            }
        };
        for (var i = 0; i < cols.length; i++) {
            getColumnFileds(i);
        }
        return columnFileds;
    };

    function getColOptions(target, filed) {
        var opts = $.data(target, "hiMallDatagrid").options;
        if (opts.columns) {
            for (var i = 0; i < opts.columns.length; i++) {
                var colOpts = opts.columns[i];
                for (var j = 0; j < colOpts.length; j++) {
                    var col = colOpts[j];
                    if (col.field == filed) {
                        return col;
                    }
                }
            }
        }
        return null;
    };

    function initialPager(target) {

        var total = $.data(target, "hiMallDatagrid").data.total;
        var opts = $.data(target, "hiMallDatagrid").options;
        $(target).siblings(".table-bt").remove();
        if (total > 0) {
            var pageCount = parseInt(total / opts.pageSize) + (total % opts.pageSize == 0 ? 0 : 1);
            if (isNaN(pageCount)) pageCount = 1;
            if (pageCount < 1) pageCount = 1;
            if (total == 0) pageCount = 0;
            var currentPage = opts.pageNumber;
            if (opts.pagination) {
                var htmlArray = ["<div class=\"table-bt clearfix\">"];
                htmlArray.push("<div class=\"tabel-operate\">");
                if (opts.hasCheckbox && !opts.singleSelect) {
                    htmlArray.push("<div class='checkbox inline-block'><label><input name=\"\" class=\"check-all\" type=\"checkbox\"/>全选</label></div>");

                }
                htmlArray.push(opts.operationButtonsInnerHTML);
                htmlArray.push("</div>");
                htmlArray.push("<div class=\"pager\">");
                htmlArray.push("<span class=\"active-page\">" + currentPage.toString() + "/" + pageCount.toString() + " 页，共" + total.toString() + "条记录 </span>");
                htmlArray.push("<a class=\"btn btn-default btn-ssm beforePageBtn\" href=\"javascript:void(0);\">上一页</a>");
                htmlArray.push("<a class=\"btn btn-default btn-ssm afterPageBtn\" href=\"javascript:void(0);\">下一页</a>");
                htmlArray.push("<a id=\"jump-to\" class=\"btn btn-default btn-ssm\">指定跳转</a>");
                htmlArray.push("<div class=\"tipic-dialog\">");
                htmlArray.push("<div class=\"page-jump\">");
                htmlArray.push("<span>跳转到第<input class=\"pageNumberTxt input-int-num\" type=\"text\" name=\"\">页</span>");
                htmlArray.push("<input type=\"button\" class=\"btn btn-default btn-ssm pageJumpBtn\" value=\"确定\"></input>");
                htmlArray.push("</div></div></div></div>");

                $(htmlArray.join("")).insertAfter($(target));
                $('.input-int-num').keyup(function () {
                    (this.v = function () { this.value = this.value.replace(/[^0-9-]+/, ''); if (this.value == '') { this.value = '1' } }).call(this);
                }).blur(function () {
                    this.v();
                });

            }
        }
    }



    //插件构造函数
    $.fn.hiMallDatagrid = function (options, param) {
        if (typeof options == "string") {
            return $.fn.hiMallDatagrid.methods[options](this, param);
        }

        options = options || {};
        return this.each(function () {
            var state = $.data(this, "hiMallDatagrid");
            var opts;
            if (state) {
                opts = $.extend(state.options, options);
                state.options = opts;
            } else {
                opts = $.extend({}, $.fn.hiMallDatagrid.defaults, options);
                //console.log(opts);
                $.data(this, "hiMallDatagrid", {
                    options: opts,
                    selectedRows: [],
                    data: {
                        total: 0,
                        rows: []
                    }
                });
            }
            opts.view.renderHead.call(opts.view, this, $(this));

            //Ajax获取数据
            if (opts.url) {
                request(this);
            }
        });
    };

    //插件对外公开的所有方法集合
    $.fn.hiMallDatagrid.methods = {
        options: function (jq) {
            var opts = $.data(jq[0], "hiMallDatagrid").options;
            return opts;
        },
        data: function (jq) {
            var data = $.data(jq[0], "hiMallDatagrid").data;
            return data;
        },
        reload: function (jq, param) {
            return jq.each(function () {
                if (!param.pageNumber) param.pageNumber = 1;
                $.data(this, "hiMallDatagrid").options.pageNumber = param.pageNumber;
                request(this, param);
            });
        },
        clearReload: function (jq, param) {
            return jq.each(function () {
                var opts = $.data(jq[0], "hiMallDatagrid").options;
                opts.pageNumber = 1;
                opts.queryParams = param || {};
                request(this);
            });
        },
        //显示正在加载提示信息
        loading: function (jq) {
            return jq.each(function () {
                var opts = $.data(this, "hiMallDatagrid").options;

                if (opts.loadMsg) {
                    /*  显示正在加载提示信息
                    */
                    var html = ["<tbody><tr style='display:none;'><td colspan=" + opts.columns[0].length + "></td></tr></tbody>"];
                    if ($("#loadAjax").length === 0) {
                        html.push("<tr id='loadAjax'><td style='height:100px;text-align:center;' colspan='" + opts.columns[0].length.toString() + "'><img src='/Images/dg_loading.gif'></td></tr>");
                    }
                    $(jq).find("tbody").remove();
                    $(jq).append(html.join(""));
                }
            });
        },
        //清除加载提示信息
        loaded: function (jq) {
            return jq.each(function () {
                /* 清除加载提示信息
                */
                $("#loadDiv", $(jq)).remove();
            });
        },

        loadData: function (jq, data) {
            return jq.each(function () {
                loadData(this, data);
                //cacheRows(this);
            });
        },
        getSelected: function (jq) {
            var rows = getSelectRows(jq[0]);
            return rows.length > 0 ? rows[0] : null;
        },
        getRows: function (jq) {
            return $.data(jq[0], "hiMallDatagrid").data.rows;
        },
        getSelections: function (jq) {
            return getSelectRows(jq[0]);
        },

        clearSelections: function (jq) {
            return jq.each(function () {
                clearSelections(this);
            });
        },
        getRowByIndex: function (jq, index) {
            return getRowByIndex(jq[0], index)
        },
        selectRow: function (jq, index) {
            return jq.each(function () {
                selectRow(this, index);
            });
        }, getColumnFields: function (jq) {
            return getColumnFiles(jq[0]);
        }, getColumnOption: function (jq, filed) {
            return getColOptions(jq[0], filed);
        }
    };

    var renderView = {
        render: function (target, container) {
            var opts = $.data(target, "hiMallDatagrid").options;
            var rows = $.data(target, "hiMallDatagrid").data.rows;
            var closFileds = $(target).hiMallDatagrid("getColumnFields");

            if (rows == null || rows.length <= 0) {
                $("#loadAjax").remove();
                var html = ["<tbody><tr><td style='text-align:center;' colspan=" + opts.columns[0].length + "><h2 style='font-size: 18px;margin-top:30px;margin-bottom:40px;color:#8e8f92;'>" + opts.NoDataMsg + "</h2></td></tr></tbody>"];
                $(container).find("tbody").remove();
                $(container).append(html.join(""));
            }
            else {
                $("#loadAjax").remove();
                $(container).find("tbody tr").show();
                var htmlAry = ["<tbody>"];
                var rowData = {};
                for (var i = 0; i < rows.length; i++) {
                    var cls = (i % 2 && opts.striped) ? "class=\"hiMallDatagrid-row-alt\"" : "";
                    var rowStyle = opts.rowStyler ? opts.rowStyler.call(target, i, rows[i]) : "";
                    var strStyle = rowStyle ? "style=\"" + rowStyle + "\"" : "";
                    var rowHead = opts.rowHeadFormatter ? opts.rowHeadFormatter.call(this, target, i, rows[i]) : "";
                    var rowFoot = opts.rowFootFormatter ? opts.rowFootFormatter.call(this, target, i, rows[i]) : "";
                    htmlAry.push(rowHead);
                    if (opts.childField && rows[i][opts.childField].length > 0) {
                        for (var j = 0; j < rows[i][opts.childField].length ; j++) {
                            var childData = $.extend({}, rows[i], rows[i][opts.childField][j]);
                            htmlAry.push("<tr hiMallDatagrid-row-index=\"" + i + "\" " + cls + " " + strStyle + " ");
                            if (opts.rowFormatter)
                                htmlAry.push(opts.rowFormatter(childData, i));
                            htmlAry.push(" >");
                            htmlAry.push(this.renderRow.call(this, target, closFileds, i, childData));
                            htmlAry.push("</tr>");
                        }
                    }
                    else {
                        htmlAry.push("<tr hiMallDatagrid-row-index=\"" + i + "\" " + cls + " " + strStyle + " ");
                        if (opts.rowFormatter)
                            htmlAry.push(opts.rowFormatter(rows[i], i));
                        htmlAry.push(" >");
                        htmlAry.push(this.renderRow.call(this, target, closFileds, i, rows[i]));
                        htmlAry.push("</tr>");
                    }
                    htmlAry.push(rowFoot);
                }
                htmlAry.push("</tbody>");
                $(container).find("tbody").remove();
                $(container).append(htmlAry.join(""));
            }
        },
        renderHead: function (target, container) {
            var opts = $.data(target, "hiMallDatagrid").options;
            var closFileds = $(target).hiMallDatagrid("getColumnFields");
            var htmlAry = ["<thead><tr>"];

            for (var i = 0; i < closFileds.length; i++) {
                if (closFileds[i] == "门店未授权") {
                    continue;
                }
                var content = "",
                	className = "",
                	findFiled = closFileds[i],
                	col = $(target).hiMallDatagrid("getColumnOption", findFiled),
                	sort = '',
                	sortHtml = '',
                	widthStyle = col.width === undefined ? "width:auto;" : "width:" + (col.width) + "px;";
                if (col.checkbox) {
                    content = "选择";
                    className = " class=\"td-choose\"";
                    opts.hasCheckbox = true;
                } else {
                    content = col.title;
                }
                if (col.operation) {
                    content = "操作";
                    className = " class=\"td-operate\"";
                }

                //添加排序功能-xielingxiao
                if (col.sort) {
                    sort = ' data-sort="' + col.field + '"';
                    content += '<i class="icon_sort"></i>';
                }

                var style = col.hidden ? "style=\"display:none;\"" : "style=\"text-align:center;padding-left:0px;padding-right:0px;" + widthStyle + "\"";
                htmlAry.push('<th ' + className + style + sort + '>');
                htmlAry.push(content);
                htmlAry.push("</th>");
            }
            htmlAry.push("<tr id='loadAjax'><th style='height:100px;text-align:center;' colspan='" + closFileds.length.toString() + "'><img src='/Images/dg_loading.gif'></th></tr>");
            htmlAry.push("</tr></thead>");
            if (!$(container).find("thead").length)
                $(container).html(htmlAry.join(""));
        },
        renderRow: function (target, fileds, rowIndex, rowData) {
            var opts = $.data(target, "hiMallDatagrid").options;
            var cc = [];
            for (var i = 0; i < fileds.length; i++) {
                var findFiled = fileds[i];
                if (findFiled == "门店未授权") {
                    continue;
                }
                var col = $(target).hiMallDatagrid("getColumnOption", findFiled);
                if (col) {
                    var className = "";
                    var findStyler = col.styler ? (col.styler(rowData[findFiled], rowData, rowIndex) || "") : "";
                    var width = col.width === undefined ? "width:auto;" : "width:" + (col.width) + "px;";
                    var strFindStyler = col.hidden ? "style=\"display:none;" + findStyler + "\"" : (findStyler ? "style=\"" + findStyler + width + "\"" : "style=\"" + width + "\"");

                    if (col.checkbox) {
                        className = " class=\"td-choose\"";
                    } else if (col.operation) {
                        className = " class=\"td-operate td-lg\"";
                    }
                    cc.push("<td " + className + "  field=\"" + findFiled + "\" " + strFindStyler + ">");

                    //var strFindStyler = col.width === undefined ? "width:auto;" : "width:" + (col.width) + "px;";
                    strFindStyler = "text-align:" + (col.align || "center") + ";";
                    cc.push("<div style=\"" + strFindStyler + "\" ");
                    if (col.checkbox) {
                        cc.push(" class=\"hiMallDatagrid-cell-check ");
                    } else {
                        cc.push(" class=\"hiMallDatagrid-cell ");
                    }
                    cc.push("\">");
                    if (col.checkbox) {
                        if (col.formatter) {
                            cc.push(col.formatter(rowData[findFiled], rowData, rowIndex));
                        } else {
                            cc.push("<input type=\"checkbox\"/>");
                        }
                    } else {
                        if (col.formatter) {
                            cc.push(col.formatter(rowData[findFiled], rowData, rowIndex));
                        } else {
                            cc.push(rowData[findFiled]);
                        }
                    }
                    cc.push("</div>");
                    cc.push("</td>");
                }
            }
            return cc.join("");
        }
    };
    $.fn.hiMallDatagrid.defaults = {
        rowFormatter: null,
        rowHeadFormatter: null,
        rowFootFormatter: null,
        columns: null,
        toolbar: null,
        toolbarInnerHTML: '',
        operationButtons: null,
        operationButtonsInnerHTML: null,
        method: "post",
        hasCheckbox: false,
        idField: null,
        url: null,
        loadMsg: "正在加载,请稍等 ...",
        NoDataMsg: '没有找到符合条件的数据',
        singleSelect: false,
        pagination: false,
        pageNumber: 1,
        pageSize: 20,
        queryParams: {},
        sortOrder: "asc",
        view: renderView,
        onLoadSuccess: function () { },
        onLoadError: function () { },
        childField: ''
    };

})(jQuery);