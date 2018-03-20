var orderStatus = QueryString("orderStatus");
var ShowRefundStats = ["", "待商家审核", "待买家寄货", "待商家收货", "商家拒绝", "待平台确认", "退款成功"];

$(function () {
    if (orderStatus == 1)
        $('.clearfix li').eq(1).addClass("active");
    else if (orderStatus == 2)
        $('.clearfix li').eq(2).addClass("active");
    else if (orderStatus == 3)
        $('.clearfix li').eq(3).addClass("active");
    else if (orderStatus == 5)
        $('.clearfix li').eq(4).addClass("active");
    else
        $('.clearfix li').eq(0).addClass("active");
    loadOrders(1, orderStatus);
});

function userOrders(status) {
    if (status == 0 || status == 1)//全部订单或待支付订单
        location.href = '/common/site/pay?area=mobile&platform=' + areaName.replace('m-', '') + '&controller=member&action=orders&orderStatus=' + status;
    else
        location.href = '/' + areaName + '/Member/Orders?orderStatus=' + status;
}

var page = 1;

$(window).scroll(function () {
    var scrollTop = $(this).scrollTop();
    var scrollHeight = $(document).height();
    var windowHeight = $(this).height();

    if (scrollTop + windowHeight >= scrollHeight) {
        $('#autoLoad').removeClass('hide');
        loadOrders(++page);
    }
});

function loadOrders(page, status) {
    if (page == 1)
        var loading = showLoading();
    var url = '/' + areaName + '/Member/GetUserOrders?orderStatus=' + orderStatus;
    $.post(url, { orderStatus: status, pageNo: page, pageSize: 4 }, function (result) {
        $('#autoLoad').addClass('hide');
        var htmls = [];
        var vshopurl = '';
        if (result.length > 0) {
            $.each(result, function (i, item) {
                if (!(status == 5 && item.commentCount != 0)) {
                    if (item.itemInfo.length > 0 && item.itemInfo[0].vshopid > 0)
                        vshopurl = '/' + areaName + '/vshop/detail/' + item.itemInfo[0].vshopid;
                    else
                        vshopurl = '/' + areaName + '/vshop';
                    var html = '<a href="' + vshopurl + '"><h6>' + item.shopname + '<i class="glyphicon glyphicon-menu-right"></i>' + '<span class="pull-right">' + item.status + '</span></h6></a>';
                    $.each(item.itemInfo, function (j, orderItem) {//图片已在后台处理
                        var detailUrl;
                        if (item.orderStatus == 1)
                            detailUrl = '/common/site/pay?area=mobile&platform=' + areaName.replace('m-', '') + '&controller=order&action=detail&id=' + item.id;
                        else
                            detailUrl = '/{0}/Order/Detail?id={1}'.format(areaName, item.id);

                        html += '<a href="{0}"><div class="order-goods clearfix">'.format(detailUrl);
                        html += '<img src="' + orderItem.image + '" /><p class="name"><span class="pd-name">' + orderItem.productName + '</span><span class="red-p">¥' + orderItem.price + '</span></p>';
                        html += '<p class="p-r"><span class="gray">' + (orderItem.color == null ? '' : orderItem.color) + ' ' + (orderItem.size == null ? '' : orderItem.size) + ' ' + (orderItem.version == null ? '' : orderItem.version) + '</span><em><i style="font-style: normal;padding:0 5px;">x</i>' + orderItem.count + '</em></p>';
                        if (orderItem.OrderRefundId > 0 || item.OrderRefundId > 0) {
                            var refundid = item.OrderRefundId;
                            var refundstate = item.RefundStats;
                            if (orderItem.OrderRefundId > 0) {
                                refundid = orderItem.OrderRefundId;
                                refundstate = orderItem.RefundStats;
                            }

                            html += '<span style="color:#f9b739;position:absolute;right:11px;bottom:6px;">';
                            switch (refundstate) {
                                case 4:
                                    html += "退款拒绝";
                                    break;
                                case 7:
                                    html += "退款成功";
                                    break;
                                default:
                                    html += "退款中";
                                    break;
                            }
                            html += '</span>';
                        }
                        html += '</div></a>';
                    });
                    html += '<p class="order-text"><a href="/' + areaName + '/Order/Detail?id=' + item.id + '"><span>共 ' + item.productCount + ' 件商品</span> <span>总价： <em>¥' + item.orderTotalAmount + '</em></span></a></p>';

                    var orderBtns = [];
                    //晒单链接
                    var orderShare = '/' + areaName + '/Order/OrderShare?orderids=' + item.id;;
                    switch (item.orderStatus) {
                        case 1:
                            orderBtns.push('<a class="btn-cnf" pay orderTotal="{0}" orderId="{1}">去付款</a>'.format(item.orderTotalAmount, item.id));
                            if (item.OrderType != 3)
                                orderBtns.push('<a class="btn-del" onclick="CancelOrder({0})">取消订单</a>'.format(item.id));
                            break;
                        case 2:
                            orderBtns.push('<a class="btn-del" href="' + orderShare + '">我要晒单</a>');//待发货晒单
                            if (item.PaymentType == 3 && item.OrderType != 3)
                                orderBtns.push('<a class="btn-del" onclick="CancelOrder({0})">取消订单</a>'.format(item.id));
                            break;
                        case 3:
                            orderBtns.push('<a class="btn-del" href="' + orderShare + '">我要晒单</a>');//待收获晒单
                            if (item.ShipOrderNumber != "" && item.ShipOrderNumber != null)
                                orderBtns.push('<a class="btn-cnf" onclick="ConfirmOrder({0})">确认收货</a><a class="btn-del" href="/{1}/order/expressInfo?orderId={2}">查看物流</a>'.format(item.id, areaName, item.id));
                            else
                                orderBtns.push('<a class="btn-cnf" onclick="ConfirmOrder({0})">确认收货</a>'.format(item.id));
                            break;
                        case 5:
                            orderBtns.push('<a class="btn-del" href="' + orderShare + '">我要晒单</a>');//已完成晒单
                            if (item.ShipOrderNumber != "" && item.ShipOrderNumber != null)
                                orderBtns.push('<a class="btn-del"  href="/{0}/order/expressInfo?orderId={1}">查看物流</a>'.format(areaName, item.id));
                            if (item.commentCount == 0)
                                orderBtns.push('<a class="btn-cnf" href="/{0}/comment?orderId={1}">评论</a>'.format(areaName, item.id));
                            else if (item.commentCount >= 1 && !item.HasAppendComment)
                                orderBtns.push('<a class="btn-cnf" href="/{0}/comment/appendcomment?orderId={1}">追加评论</a>'.format(areaName, item.id));
                            break;
                        case 6:
                            orderBtns.push('<a class="btn-del" href="' + orderShare + '">我要晒单</a>');//待自提晒单
                            if (item.PickUp && item.PickUp.length > 0) {
                                orderBtns.push('<a class="btn-del" href="/{0}/Member/PickupGoods?id={1}">提货码</a>'.format(areaName, item.id));
                            }
                            break;
                        default:
                            break;
                    }
                    if ((item.CanRefund == true || item.RefundStats == 4) && item.EnabledRefundAmount > 0) {
                        var _html = '<a class="btn-cnf" href="/{0}/OrderRefund/RefundApply/?orderid={1}'.format(areaName, item.id);
                        if (item.OrderRefundId && item.OrderRefundId > 0) {
                            _html += '&refundid=' + item.OrderRefundId;
                        }
                        _html += '">退款</a>';
                        orderBtns.push(_html);
                    }
                    if (orderBtns.length > 0) {
                        html += '<p class="order-btn">{0}</p>'.format(orderBtns.join(''));
                    }

                    htmls.push(html);
                }
            });
            $('.order-list').append('<li>{0}</li>'.format(htmls.join('</li><li>')));
        }
        else {
            $('#autoLoad').html('没有更多订单了').removeClass('hide');
        }
        if (page == 1)
            loading.close();
    });
}

function ConfirmOrder(orderId) {

    $.dialog.confirm("你确定收到货了吗？", function () { Confirm(orderId); });
}

function Confirm(orderId) {
    $.ajax({
        type: 'post',
        url: '/' + areaName + '/Order/ConfirmOrder',
        dataType: 'json',
        data: { orderId: orderId },
        success: function (d) {
            if (d.success) {
                $.dialog.succeedTips("确认成功！", function () {
                    window.location.href = window.location.href;
                }, 1);
            }
            else {
                $.dialog.errorTips("确认失败！", '', 2);
            }
        }
    });
}

function CancelOrder(orderId) {
    $.dialog.confirm("确定取消该订单吗？", function () { Cancel(orderId); });
}

function Cancel(orderId) {
    $.ajax({
        type: 'post',
        url: '/' + areaName + '/Order/CloseOrder',
        dataType: 'json',
        data: { orderId: orderId },
        success: function (d) {
            if (d.success) {
                $.dialog.succeedTips("取消成功！", function () {
                    window.location.href = window.location.href;
                }, 1);
            }
            else {
                $.dialog.errorTips("取消失败！", '', 2);
            }
        }
    });
}

$('.order-list').on('click', 'a[pay]', function () {
    var loading = showLoading();
    var orderId = $(this).attr('orderId');
    if ($(this).attr('orderTotal') == 0) {
        loading && loading.close();
        $.dialog.confirm('您确定用积分抵扣全部金额吗？', function () {
            ajaxRequest({
                type: 'POST',
                url: '/' + areaName + '/Order/PayOrderByIntegral',
                param: { orderIds: orderId },
                dataType: 'json',
                success: function (data) {
                    if (data.success == true) {
                        $.dialog.succeedTips('支付成功！', function () {
                            location.href = '/' + areaName + '/Member/Orders';
                        }, 0.5);
                    }
                },
                error: function (data) { $.dialog.tips('支付失败,请稍候尝试.', null, 0.5); }
            });
        });
    }
    else {
        loading && loading.close();
        GetPayment(orderId, window.location.href);//lly 传入跳转页面url
    }

    $(document).on('click', '.cover,.custom-dialog-header .close', function () {
        if ($('.custom-dialog').is(':visible')) {
            $('.cover,.custom-dialog').fadeOut();
        }
    });
});