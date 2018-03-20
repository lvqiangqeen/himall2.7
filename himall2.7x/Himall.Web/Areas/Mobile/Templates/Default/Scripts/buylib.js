//--------------------------------------
//购买有关功能
//--------------------------------------

var skuId = new Array(3);

//是否有规格
function IsHasSKU(id) {
    result = false;
    $.ajax({
        type: "post",
        url: '/' + areaName + '/Product/GetHasSku',
        data: { id: id },
        async: false,   //必须同步
        success: function (data) {
            if (data.hassku == true) {
                result = true;
            } else {
                result = false;
            }
        }
    });
    return result;
}

//显示规格
function ShowSKUInfo(id, func) {
    var boxstr = "<div id=\"J_shop_att\"></div>";
    var box = $("#J_shop_att");
    if (box.length == 0) {
        box = $(boxstr);
        $("body").append(box);
    }
    var html = $.ajax({
        type: "get",
        url: '/' + areaName + '/Product/ShowSkuInfo',
        data: { id: id },
        success: function (html) {
            box.html(html);
            if (func) {
                func();
            }
        }
    });
}

//显示规格 绑定规则
//function ShowSKUInfoBySKUS(id,groupSKUS, func) {
//    var boxstr = "<div id=\"J_shop_att\"></div>";
//    var box = $("#J_shop_att");
//    if (box.length == 0) {
//        box = $(boxstr);
//        $("body").append(box);
//    }
//    var html = $.ajax({
//        type: "get",
//        url: '/' + areaName + '/PortfolioBuy/ShowSkuInfo',
//        data: { id: id, groupSKUS: groupSKUS },
//        success: function (html) {
//            box.html(html);
//            if (func) {
//                func();
//            }
//        }
//    });
//}

function checkFirstSKUWhenHas() {
    $(".spec").each(function () {
       $(this).find('.enabled').first().click();
    });
}


function checkSkuBuyNum(dom, max) {
    var dom = $(dom);
    var num = 0;
    var result = true;
    try {
        num = parseInt(dom.val());
    } catch (ex) {
        num = 0;
    }
    if (num < 1) {
        $.dialog.errorTips('购买数量有误');
        dom.val(1);
        result = false;
    }

    try {
        max = parseInt(max);
    } catch (ex) {
        max = 0;
    }
    if (num > max) {
        $.dialog.errorTips('超出库存或限购量');
        dom.val(max);
        result = false;
    }
    return result;
}

function getskuidbypid(id) {
    chooseResult();
    var gid = id;
    var sku = '';
    for (var i = 0; i < skuId.length; i++) {
        sku += ((skuId[i] == undefined ? 0 : skuId[i]) + '_');
    }
    if (sku.length === 0) { sku = "0_0_0_"; }
    sku = gid + '_' + sku.substring(0, sku.length - 1);
    return sku;
}

function chooseResult() {
    //已选择显示
    var len = $('#choose .spec .selected').length;
    for (var i = 0; i < len; i++) {

        var index = parseInt($('#choose .spec .selected').eq(i).attr('st'));
        skuId[index] = $('#choose .spec .selected').eq(i).attr('cid');
    }
    
}

function addToCart(sku, num, successfunc, errorfunc) {
    $.post('/' + areaName + '/cart/AddProductToCart', {
        skuId: sku,
        count: num
    }, function (result) {
        if (result.success == true) {
            $.dialog.succeedTips("添加成功！");
            if (successfunc) {
                successfunc();
            }
        } else {
            $.dialog.errorTips(result.msg);
            if (errorfunc) {
                errorfunc();
            }
        }
    })

}

function InitCover() {
    var dom = $(".cover");
    if (!dom) {
        dom = $("<div class=\"cover\" style=\"display:none;\"></div>");
        $("body").append(dom);

        dom.click(function () {
            $('.cover').hide();
        });
    }
    return dom;
}

function IsOk(oids) {
    var result = true;
    $.ajax({
        type: 'post',
        url: '/' + areaName + '/Payment/PayByCapitalIsOk',
        data: { "ids": oids },
        async: false,
        dataType: "json",
        success: function (data) {
            result = data.success;
        }
    });
    return result;
}

//获取支付方式
//lly 增加跳转Url
function GetPayment(oids,backUrl) {
    var cover = InitCover();
    var isfgord = false;
    var isweixin = false
    backUrl = backUrl || '';//没传默认为空
    try{
        isfgord = isFightGroup || false;
    } catch (ex) {
        isfgord = false;
    }
    try{
        isweixin = isWeiXin || false;
    }catch(ex)
    {
        isweixin = false;
    }

    var pdlg = $("#paymentsChooser");
    if (!pdlg) {
        pdlg = $("<div class=\"custom-dialog\" id=\"paymentsChooser\"></div>");
        $("body").append(pdlg);
    }

    if (oids.length < 1) {
        $.dialog.tips('错误的订单编号');
        return false;
    }
    var loading = showLoading();

    $.post('/' + areaName + '/payment/get', { orderIds: oids },
        function (payments) {
            var totalAmount = payments.totalAmount.toFixed(2);
            loading && loading.close();
            /*if (payments.length > 0) {
                paymentShown = true;//标记已经显示支付方式
                var html = '';
                $.each(payments, function (i, payment) {
                    url = "/" + areaName + "/payment/Pay/?pmtid=" + payment.id + "&ids=" + oids;
                    html += '<a class="btn btn-success btn-block" href="' + url + '">' + payment.name + '</a>';
                });
                pdlg.html(html);

                cover.fadeIn();
                pdlg.show();
            }
            else {
                $.dialog.tips('没有可用的支付方式，请稍候再试', function () {
                    location.href = "/" + areaName + '/member/orders';
                });
            }*/
            //payments = [{ id: 0, name: '支付宝' }, { id: 1, name: '微信' }, { id: 9, name: '预存款支付' }]
            var hasAdv = IsOk(oids);//判断是否开通预付款和预付款余额是否充足
            var ypayment = { id: 9, name: '预存款支付' };
            var paymentType = payments.data;
            paymentType.push(ypayment);
            paymentShown = true;//标记已经显示支付方式
            var payHtml = '';
            $.each(paymentType, function (i, payment) {
                if (payment.id == 9) {
                    url = "/" + areaName + "/payment/PayByCapital/?pmtid=" + payment.id + "&ids=" + oids;
                }
                else {
                    url = "/" + areaName + "/payment/Pay/?pmtid=" + payment.id + "&ids=" + oids;
                }
                if (!hasAdv && payment.id == 9) {
                    payHtml += '<li class="disabled" >预付款支付（余额不足）</li>';
                } else {
                    payHtml += '<li data-Id="' + payment.id + '" data-url="' + url + '">' + payment.name + '</li>';
                }
                
            });
            var html = '';
            html += '<div class="custom-dialog-header"><div class="close"></div>';
            html += '<p class="p1 text-center">请选择支付方式</p>';
            html += '<p class="p2 text-center">付款金额<span>￥' + totalAmount + '</span></p>';
            html += '</div>';
            html += '<ul class="custom-dialog-body">' + payHtml + '</ul>';
            html += '<div class="custom-dialog-footer"><a class="btn btn-danger">提交</a></div>';
            html += '<a id="payJumpUrl" style="display:none;" href=""></a>';
            pdlg.html(html);
            cover.fadeIn();
            pdlg.show();

            if ($('#stepone').length < 1) {
                var htmlSetPad = "";
                htmlSetPad += '<div class="box1 lh24 steponeee" id="stepone" style="display:none">';
                htmlSetPad += '<span class="close" aria-hidden="true"></span><form>';
                htmlSetPad += '<h3 class="title_txt cur">请设置支付密码</h3>';
                htmlSetPad += '<div class="item"><div class="fl">';
                htmlSetPad += '<input type="password" placeholder="请输入支付密码" value="" id="firstPwd"  maxlength="20" class="form-control itxt fl">';
                htmlSetPad += '</div></div><div class="item"> <div class="fl">';
                htmlSetPad += '<input type="password" placeholder="请再次输入支付密码" value="" id="secondPwd"  maxlength="20" class="form-control itxt fl">';
                htmlSetPad += '</div></div><div class="item"> <div class="fl"><a id="submitPwd" class="btn btn-primary">提交</a></div></div></form></div>';
                $("body").append(htmlSetPad);
            }

            if ($('#payPwd').length < 1) {
                var htmlPayPwd = "";
                htmlPayPwd += '<div class="box1 lh24 steponeee" id="payPwd" style="display:none">';
                htmlPayPwd += '<span class="close" aria-hidden="true"></span><form>';
                htmlPayPwd += '<h3 class="title_txt cur">支付密码</h3>';
                htmlPayPwd += '<div class="item"><div>';
                htmlPayPwd += '<input type="password" placeholder="请输入支付密码" value="" id="inputPwd"  maxlength="20" class="form-control itxt fl">';
                htmlPayPwd += '</div></div><div class="item"> <div><a id="submitPay" class="btn btn-primary">确认</a></div></div></form></div>';
                $("body").append(htmlPayPwd);
            }

            if ($('#paymentsChooser')) {
                $('.steponeee .close').click(function () {
                    $(this).parent().hide();
                    $('#paymentsChooser').show();
                })
            }

            var stae1, stae2, stae3;
            var pwdErrMsg = '密码不能为空！';
            $('#firstPwd').blur(function () {
                var d = $(this).val();
                if (d.length < 6) {
                    $('#firstPwd').css({ borderColor: '#f60' });
                    pwdErrMsg = '密码长度不能少于6位';
                    stae2 = '';
                } else {
                    $('#firstPwd').css({ borderColor: '#ccc' });
                    stae2 = d;
                    if ($('#secondPwd').val() != '' && $('#secondPwd').val() == $('#firstPwd').val()) {
                        $('#secondPwd').css({ borderColor: '#ccc' });
                        stae3 = d;
                    }
                    else {
                        stae3 = '';
                        pwdErrMsg = '两次密码不一致！';
                    }
                }
            });
            $('#secondPwd').blur(function () {
                var d = $(this).val();
                if (d == $('#firstPwd').val()) {
                    $('#secondPwd').css({ borderColor: '#ccc' });
                    stae3 = d;
                } else {
                    $('#secondPwd').css({ borderColor: '#f60' });
                    pwdErrMsg = '两次密码不一致！';
                    stae3 = '';
                }
            });
            $('#submitPwd').on('click', function () {
                if (!stae2) {
                    $('#firstPwd').css({ borderColor: '#f60' });
                    $.dialog.alert(pwdErrMsg);
                }
                if (!stae3) {
                    $('#secondPwd').css({ borderColor: '#f60' });
                    $.dialog.alert(pwdErrMsg);
                }
                if (stae2 && stae3) {
                    var loading = showLoading();
                    $.ajax({
                        type: 'post',
                        url: '/' + areaName + '/Payment/SetPayPwd',
                        data: { "pwd": stae3 },
                        dataType: "json",
                        success: function (data) {
                            loading.close();
                            if (data.success) {
                                $.dialog.succeedTips('设置成功！');
                                pwdflag = 'true';
                                $('#stepone').hide();
                                $('#paymentsChooser').show();
                            }
                        }
                    });
                }
            });
            // 支付弹窗
            var jumpUrl = '';
            var type = '';
            $('.custom-dialog-body').on('click', 'li', function () {
                if ($(this).hasClass('disabled')) {
                    $.dialog.errorTips('余额不足，请选择其他支付方式');
                    return false;
                }
                $(this).addClass('selected').siblings('li').removeClass('selected');
                 jumpUrl = $(this).data('url');
                 type = $(this).data('id');
            });
            $('.custom-dialog-footer .btn').click(function () {
                if (jumpUrl != '') {
                    if (type == 9) {
                        $.ajax({
                            type: 'post',
                            url: '/' + areaName + '/Payment/GetPayPwd',
                            dataType: 'json',
                            data: {},
                            success: function (d) {
                                if (d.success) {
                                    $('#payPwd').show();
                                    $('#paymentsChooser').hide();
                                }
                                else {
                                    $('#stepone').show();
                                    $('#paymentsChooser').hide();
                                }
                            }
                        });
                        $('#submitPay').on('click', function () {
                            var pwd = $('#inputPwd').val();
                            if (pwd != null && pwd != "") {
                                $.ajax({
                                    type: 'post',
                                    url: '/' + areaName + '/Payment/PayByCapital',
                                    dataType: 'json',
                                    data: { ids: oids, payPwd: pwd },
                                    success: function (d) {
                                        $.dialog.tips(d.msg);
                                        if (d.success) {
                                            if (isfgord) {
                                                //拼团跳转
                                                window.location.href = '/' + areaName + "/FightGroup/GroupOrderOk?orderid=" + oids;
                                            } else {
                                                //window.location.href = '/' + areaName + "/Member/Orders?orderStatus=2v";
                                                location.href = '/' + areaName + '/Order/OrderShare?orderids=' + oids;
                                            }
                                        }
                                    }
                                });
                            }
                        });
                    }
                    else {
                        //lly 调整
                        loading = showLoading('');
                        $.post(jumpUrl, function (data) {
                            loading.close();
                            if (data.success) {
                                //通过A链接模拟跳转
                            	//jumpUrl在weixin中为一段js脚本，直接调用jsapi
                            	if ($('#payJumpUrl').length > 0) {
                            		$('#payJumpUrl').attr('href', data.jumpUrl);
                            	}
                            	else {
                            		$(pdlg).append('<a id="payJumpUrl" style="display:none;" href="' + data.jumpUrl + '"></a>');
                            	}
                            	document.getElementById("payJumpUrl").click();
                            }
                            else {
                                $.dialog.alert(data.msg);
                            }
                            //lly 去掉自动跳转，在支付方式中处理
                            /*if (backUrl == '') {
                                //请求一次后，自动跳转
                                if (isfgord) {
                                    //拼团跳转
                                    window.location.href = '/' + areaName + "/FightGroup/GroupOrderOk?orderid=" + oids;
                                } else {
                                    window.location.href = '/' + areaName + "/Member/Orders?orderStatus=2v";
                                }
                            }
                            else {
                                window.location.href = backUrl;
                            }*/
                            //window.location.href = jumpUrl;
                        });
                    }
                }
                else {
                    $.dialog.alert('请选择支付方式！');
                }
            });
        });
}
