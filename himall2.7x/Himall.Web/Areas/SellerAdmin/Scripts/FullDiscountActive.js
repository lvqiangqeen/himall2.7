
var rdallProduct, a_selpro, s_selnum, hidProductIds, proselbox, prodatabox, showpage;
var btsubmit, loading, isposting = false;
var stimebox, etimebox;
var selectProducts = new Array();
var pageAllData = new Array();
var pageAllTotal = 0;
var d_rule;

$(function () {
    stimebox = $("#StartTime");
    etimebox = $('#EndTime');
    rdallProduct = $("[name=IsAllProduct]");
    a_selpro = $("#a-selpro");
    s_selnum = $("#s-selnum");
    hidProductIds = $("#ProductIds");
    proselbox = $("#proselbox");
    prodatabox = $("#prodatabox");
    showpage = $("#showpage");

    etimebox.datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd hh:ii:ss',
        autoclose: true,
        minView: 0
    });
    etimebox.datetimepicker('setStartDate', stimebox.val());
    etimebox.datetimepicker('setEndDate', $("#endsertime").val());
    d_rule = $("#rule");
    d_rule.initRule();
    $("#btnAddRule").click(function () {
        d_rule.addRule();
    });

    rdallProduct.click(function () {
        var _t = $(this);
        if (_t.val() == "True") {
            proselbox.hide();
            a_selpro.hide();
        } else {
            a_selpro.show();
            proselbox.show();
        }
    });

    a_selpro.click(function () {
        var selected = [];
        var hpval = hidProductIds.val();
        if (hpval.length > 0) {
            selected = hpval.split(",");
        }
        $.FDProductSelector.show(selected, null, $("#Id").val(), function (product, selected) {
            if (selectProducts.length < 1) {
                selectProducts = product;
            } else {
                selectProducts = selectProducts.concat(product);
            }
            ShowSelectedStatus();
        });
    });

    $(".txt-search").keydown(function (e) {
        if (e.keyCode == 13) {
            ShowSelectedStatus();
            return false;
        }
    });

    $("#searchButton").click(function () {
        ShowSelectedStatus();
    });

    $(".check-all").click(function () {
        var _chk = $(this).prop("checked");
        $("[name=procheck]").prop("checked", _chk);
    });
    $("#batchDelete").click(function () {
        $("[name=procheck]:checked").each(function () {
            var _id = $(this).val();
            RemoveSelectProduct(_id);
        });
        ShowSelectedStatus();
    });


    prodatabox.QWRepeater({
        tmplId: "#datatmp", data: { rows: [] },
        onDataNotNullCheck: function (d) {
            var result = false;
            if (d && d.rows) {
                if (d.rows.length > 0) {
                    result = true;
                }
            }
            return result;
        }
    });

    showpage.QWPaginator({
        pageSize: 10, currentPage: 1, first: '', last: '',
        prev: '<a class="btn btn-default btn-ssm beforePageBtn" href="javascript:void(0);">上一页</a>',
        next: '<a class="btn btn-default btn-ssm afterPageBtn" href="javascript:void(0);">下一页</a>',
        page: '',
        pageinfo: '<span class="active-page">{{page}}/{{totalPages}} 页，共{{totalCounts}}条记录 </span>',
        visiblePages: 5, disableClass: 'disabled', activeClass: 'active', showInputPage: true,
        onPageChange: function (pageindex, type) {
            var pagedata = [];
            var opts = showpage.QWPaginator("option");
            var psize = opts.pageSize;
            var pstart = (pageindex - 1) * psize;
            var pend = pstart + psize;
            if (pend > pageAllTotal) {
                pend = pageAllTotal;
            }
            for (var i = pstart; i < pend; i++) {
                pagedata.push(pageAllData[i]);
            }
            $(".check-all").prop("checked", false);
            prodatabox.QWRepeater("reloadData", { rows: pagedata });
        }
    });

    //处理初始数据
    var selproids = hidProductIds.val();
    if (selproids.length > 0 && selproids != -1) {
        prodatabox.QWRepeater("loading", true);
        $.post($("#getprourl").val(), { productids: selproids }, function (result) {
            prodatabox.QWRepeater("loaded");
            if (result) {
                if (selectProducts.length < 1) {
                    selectProducts = result;
                } else {
                    selectProducts = selectProducts.concat(result);
                }
                ShowSelectedStatus();
            } else {
                prodatabox.QWRepeater("reloadData");
            }
        });
    }
});

btsubmit = $("#btsubmit");

//提交前检测
$("#addform").bind("submit", function (e) {
    var _t = $(this);
    var rule = d_rule.getRule();
    for (var i = 0; i < rule.length; i++) {
        var item = rule[i];
        item.Quota = parseFloat(item.Quota);
        item.Discount = parseFloat(item.Discount);
        var d_Quota = $("[name=Quota]", d_rule).eq(item.level - 1);
        var d_Discount = $("[name=Discount]", d_rule).eq(item.level - 1);
        d_rule.removeInputErrorClass(d_Quota);
        d_rule.removeInputErrorClass(d_Discount);
        if (!/^\d+?(\.\d{1,2})?$/.test(d_Discount.val())) {
            d_rule.addInputErrorClass(d_Discount);
            $.dialog.tips("层级" + item.level + " 优惠方式参数只可以是数字，且仅可以保留两位小数");
            return false;
        }
        if (!/^\d+?(\.\d{1,2})?$/.test(d_Quota.val())) {
            d_rule.addInputErrorClass(d_Quota);
            $.dialog.tips("层级" + item.level + " 优惠门槛参数只可以是数字，且仅可以保留两位小数");
            return false;
        }
        if (isNaN(item.Quota)) {
            d_rule.addInputErrorClass(d_Quota);
            $.dialog.tips("层级" + item.level + " 优惠条件参数设置错误");
            return false;
        }
        if (isNaN(item.Discount)) {
            d_rule.addInputErrorClass(d_Discount);
            $.dialog.tips("层级" + item.level + " 优惠条件参数设置错误");
            return false;
        }
        if (item.Quota <= 0) {
            $.dialog.tips("层级" + item.level + " 优惠门槛必须大于0");
            d_rule.addInputErrorClass(d_Quota);
            return false;
        }

        if (item.Discount < 0 || item.Discount > item.Quota) {
            $.dialog.tips("层级" + item.level + " 优惠方式不可以大于门槛");
            d_rule.addInputErrorClass(d_Quota);
            return false;
        }
        for (var j = 0; j < rule.length; j++) {
            if (item.Quota == rule[j].Quota && i != j) {
                $.dialog.tips("层级" + item.level + "和 层级" + rule[j].level + "门槛重复");
                d_rule.addInputErrorClass(d_Quota);
                d_rule.addInputErrorClass(d_Discount);
                return false;
            }
        }
    }
    $("input[name=RuleJSON]").val(JSON.stringify(rule));
    return true;
});

function DistinctFliter() {
    var _tmp = new Array();
    var _cn = selectProducts.length;
    for (var i = 0; i < _cn; i++) {
        var item = selectProducts[i];
        var isadd = true;
        var _tn = _tmp.length;
        for (var ti = 0; ti < _tn; ti++) {
            var _tmpitem = _tmp[ti];
            if (_tmpitem.Id == item.Id) {
                isadd = false;
            }
        }
        if (isadd) {
            _tmp.push(item);
        }
    }
    selectProducts = _tmp;
}

function ShowSelectedStatus(page) {
    DistinctFliter();
    hidProductIds.val(GetSelectedProductIds());
    s_selnum.html(selectProducts.length);

    pageAllData = selectProducts;
    pageAllTotal = pageAllData.length;
    var spname = $("#txtproductname").val();
    var spcode = $("#txtproductcode").val();
    var _tmparr = [];
    if (pageAllTotal > 0) {
        if (spname.length > 0) {
            spname = spname.trim();
            console.log(spname);
            for (var i = 0; i < pageAllTotal; i++) {
                var item = pageAllData[i];
                if (item.Name && item.Name.indexOf(spname) > -1) {
                    _tmparr.push(item);
                }
            }
            pageAllData = _tmparr;
            pageAllTotal = pageAllData.length;
        }
    }
    if (pageAllTotal > 0) {
        _tmparr = [];
        if (spcode.length > 0) {
            spcode = spcode.trim();
            for (var i = 0; i < pageAllTotal; i++) {
                var item = pageAllData[i];
                if (item.ProductCode && item.ProductCode.indexOf(spcode) > -1) {
                    _tmparr.push(item);
                }
            }
            pageAllData = _tmparr;
            pageAllTotal = pageAllData.length;
        }
    }

    if (pageAllTotal == 0) {
        prodatabox.QWRepeater("reloadData", { rows: [] });
        showpage.QWPaginator("redraw", { totalCounts: 0 });
        return;
    }
    var pageopts = showpage.QWPaginator("option");
    page = page || pageopts.currentPage;
    showpage.QWPaginator("redraw", { totalCounts: pageAllTotal, currentPage: page });
}

function GetSelectedProductIds() {
    var _tmp = new Array();
    var _cn = selectProducts.length;
    for (var i = 0; i < _cn; i++) {
        var item = selectProducts[i];
        //if (!item.IsException)
        _tmp.push(item.Id);
    }
    return _tmp.join(",");
}

function RemoveSelectProduct(id) {
    var arrlen = selectProducts.length;
    var isdel = false;
    for (var i = 0; i < arrlen; i++) {
        var item = selectProducts[i];
        if (item.Id == id) {
            isdel = true;
        }
        if (isdel) {
            if (i < arrlen - 1) {
                selectProducts[i] = selectProducts[i + 1];
            }
        }
    }
    if (isdel) {
        selectProducts.length -= 1;
    }
}

function beginpost() {

    if (isposting) {
        $.dialog.tips("数据提交中...");
        return false;
    }
    isposting = true;
    btsubmit.text("提交中...");
    loading = showLoading();
}

function successpost(data) {
    isposting = false;
    btsubmit.text("保 存");
    loading.close();
    if (data.success == true) {
        $.dialog.tips("活动信息操作成功", function () {
            window.location.href = $("#mgurl").val();//数据提交成功页面跳转
        });
    } else {
        switch (data.status) {
            case -2:
                var cantjoin = data.msg.split(",");
                for (var ji = 0; ji < cantjoin.length; ji++) {
                    for (var si = 0; si < selectProducts.length; si++) {
                        var item = selectProducts[si];
                        if (item.Id && item.Id == cantjoin[ji]) {
                            item.IsException = true;
                        }
                    }
                }
                selectProducts.sort(function (a, b) { return b.IsException - a.IsException });
                ShowSelectedStatus();
                $.dialog.errorTips("存在冲突商品或商品不在销售中");
                break;
            default:
                //清理数据冲突状态
                for (var si = 0; si < selectProducts.length; si++) {
                    var item = selectProducts[si];
                    item.IsException = false;
                }
                ShowSelectedStatus();
                $.dialog.errorTips(data.msg);
                break;
        }
    }
}

function failpost() {
    isposting = false;
    btsubmit.text("保 存");
    loading.close();
    $.dialog.errorTips("保存失败，请检查数据是否完整，且不可以有特殊字符");
}