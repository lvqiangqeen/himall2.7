//分销
var d_PromoterStatusId = "upromstatus";
$(function () {
    var shareguide = $(".share-guide");
    if (shareguide.length < 1) {
        $("body").append("<div class=\"share-guide\"></div>");
    }
    $(".share-guide").click(function () {
        $(this).hide();
    });

    var pid = getProductId();
    var disbom = $(".dis-info-a");
    disbom.click(function () {
        var s = parseInt(getPromoterStatus());
        switch (s) {
            case 1:
                $(".share-guide").show();
                break;
            case 0:
                $.dialog.tips('分销员申请待平台审核，请耐心等待！');
                break;
            case 2:
                $.dialog.errorTips('你的申请被拒绝，请重新提交资料！', function () {
                    ApplyDistribution(pid);
                });
                break;
            case 3:
                $.dialog.confirm('您已被平台从销售员中清退，您分享的商品不再计算个人业绩，您确定要分享吗？', function () {
                    $(".share-guide").show();
                });
                break;
            default:
                ApplyDistribution(pid);
                break;
        }
    });
    initDistribution();
});

function initDistribution(showshare) {
    var url = '/' + areaName + '/product/GetDistributionInfo';
    var pid = getProductId();
    $.post(url, { id: pid }, function (data) {
        var disbom = $("#dis-brokerage-box");
        if (data) {
            var winxinShareArgs = window.winxinShareArgs;

            if (winxinShareArgs) {
                winxinShareArgs = $.extend({
                    appId: data.WeiXinShareArgs.AppId,
                    timestamp: data.WeiXinShareArgs.Timestamp,
                    noncestr: data.WeiXinShareArgs.NonceStr,
                    signature: data.WeiXinShareArgs.Signature
                }, winxinShareArgs);
                winxinShareArgs.share.link = data.ShareUrl;
                if (winxinShareArgs.share.imgUrl == null || winxinShareArgs.share.imgUrl == '')
                    winxinShareArgs.share.imgUrl = location.origin + '/Areas/Mobile/Templates/Default/Images/default.png';

                //初始化微信分享
                initWinxin(winxinShareArgs);
            }

            if (data.IsDistribution) {
                disbom.attr("href", "###");
                if (data.IsPromoter) {
                    setPromoterStatus(data.PromoterStatus);
                } else {
                    setPromoterStatus(-1);
                }
                $("#dis-brokerage").html(data.Brokerage.toFixed(2));
                disbom.show();
            }

            if (showshare) {
                $(".share-guide").show();
            }
        }
    });
}

function getPromoterStatus() {
    var result = -1;
    var _d = $("#" + d_PromoterStatusId);
    if (_d.length > 0) {
        result = _d.val();
    }
    return result;
}

function setPromoterStatus(s) {
    var _d = $("#" + d_PromoterStatusId);
    if (_d.length > 0) {
        _d.val(s);
    } else {
        $("<input type='hidden' id='" + d_PromoterStatusId + "' name='" + d_PromoterStatusId + "' value='" + s + "'>").appendTo("body");
    }
}

function ApplyDistribution(productId) {
    var returnurl = "/" + areaName + "/Product/Detail/" + productId;
    checkLogin(returnurl, function () {
        var applylink = '/' + areaName + '/Distribution/Apply/?productId=' + productId;
        var url = '/' + areaName + '/Product/ApplyDistribution/';
        $.post(url, { id: productId }, function (data) {
            if (data.success) {
                initDistribution(true);
            } else {
                if (data.status == -1) {
                    window.location.href = applylink;
                    return;
                }
                if (data.status == 2) {
                    setPromoterStatus(0);
                    $.dialog.tips('分销员申请待平台审核，请耐心等待！');
                    return;
                }
                $.dialog.tips(data.msg);
            }
        });
    }, shopId);
}