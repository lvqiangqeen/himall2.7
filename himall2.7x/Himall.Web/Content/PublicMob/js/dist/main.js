function initLoadMore(a) {
    var b = 1,
    c = {
        debug: !1,
        url: "",
        targetSelector: "",
        loadmoreSelector: ".j-loadmore",
        tpl: "",
        data: {
            p: b
        },
        callback: null
    },
    d = $.extend(!0, {},
    c, a);
    $(window).scroll(function () {
        var a = $(window).scrollTop() >= $(document).height() - $(window).height() - 5 ? !0 : !1;
        if (a) {
            var c = $(d.loadmoreSelector);
            return d.data.p = b = parseInt(b) + 1,
            $.ajax({
                url: d.url,
                type: "post",
                dataType: "json",
                data: d.data,
                beforeSend: function () {
                    c.find(".loadmore-icon").css("display", "inline-block")
                },
                error: function (a, b, c) { },
                success: function (a) {
                    if (d.debug && //onsole.log(a), a.length) {
                        var b = _.template(d.tpl, {
                            dataset: a,
                            is_display_original_price: $(':hidden[name="is_display_original_price"]').val()
                        }),
                        e = $(b);
                        $(d.targetSelector).append(e)
                    } else c.siblings(".j-noMoreData").css("display", "block"),
                    c.hide();
                    c.find(".loadmore-icon").hide(),
                    d.callback && d.callback(e)
                }
            }),
            !1
        }
    }),
    $(".members_nav1 .members_nav1_name").each(function () {
        0 == $(this).html() && $(this).hide()
    })
}
$(function () {
    var a = function (a) {
        var b, c, d, e = /vid\=([^\&]*)($|\&)+/g,
        f = /sid\/\w*.*?/g;
        return c = a.match(e),
        d = a.match(f),
        c && (c = c.toString(), b = "http://v.qq.com/iframe/player.html?" + c + "&tiny=0&auto=0"),
        d && (d = d.toString(), d = d.split("/v.swf"), d = d.toString(), d = d.replace("sid/", "").replace(",", ""), b = "http://player.youku.com/embed/" + d),
        b
    };
    $(".diyShowVideo").each(function (b, c) {
        var d = $(this),
        e = d.find("iframe.diy-video"),
        f = d.find("input:hidden[name=video]").val();
        f || console.error("视频地址错误");
        var g = a(f);
        e.attr("src", g)
    }),
    document.addEventListener("WeixinJSBridgeReady",
    function () {
        var a = document.getElementById("diy-audio"),
        b = function () {
            $(".diy-audio").bind("click",
            function (b) {
                var c = $(this);
                c.hasClass("play") ? (c.removeClass("play"), a.pause(), c.find(".j-audioText").html("播放暂停，点击继续..."), c.find("sub").show(), c.find("b").hide()) : (c.addClass("play"), a.play(), c.find(".j-audioText").html("正在播放中..."), c.find("sub").hide(), c.find("b").show())
            })
        };
        a.addEventListener("canplay",
        function (a) {
            var c = me.find('input[name="audio"]').val();
            "" != c ? me.find(".j-audioText").html("正在播放中...") : (me.find(".j-audioText").html("点击可播放!"), me.find("input[name=audio]").val(a.target.webkitAudioDecodedByteCount)),
            b()
        }),
        a.addEventListener("ended",
        function () {
            a.pause(),
            me.removeClass("play"),
            me.find(".j-audioText").html("播放完成,点击可重播！"),
            me.find("sub").show(),
            me.find("b").hide()
        })
    },
    !1),
    $(document).on("touchend", ".j-showNavSub dt",
    function () {
        $(this).siblings(".nav-item-sub").toggle(),
        $(this).parents(".nav-item").siblings(".nav-item").find(".nav-item-sub").hide()
    }),
    $(document).on("touchmove", document,
    function () {
        $(".nav-item-sub").hide()
    });
    var b = $("#order_type").val();
    b || $(".morder_con").eq(0).show(),
    $(".morder_nav section").click(function () {
        var a = $(".morder_nav section").index(this);
        $(this).addClass("cur").siblings().removeClass("cur"),
        $(".morder_con").eq(a).show().siblings(".morder_con").hide()
    }),
    $(".coupons_con ul li").eq(0).show(),
    $(".coupons_nav section").click(function () {
        var a = $(".coupons_nav section").index(this);
        $(this).addClass("cur").siblings().removeClass("cur"),
        $(".coupons_con ul li").eq(a).show().siblings().hide()
    }),
    $.aLert = function (a) {
        var b = {
            title: "是否继续",
            callback: null,
            clickOK: null,
            clickCancel: null
        },
        c = $.extend(!0, {},
        b, a),
        d = "<div class='J-hyd'><div class='title'>" + c.title + "</div><div class='button'><button type='button' class='J-okClk'>确定</button><button type='button' class='butcall J-noClk' >取消</button></div></div>";
        $("body").append(d);
        var e = $(".J-hyd").height(),
        f = $(window).height(),
        g = (f - e) / 2,
        h = ($(window).width() - 260) / 2;
        $(".J-hyd").css({
            top: g,
            left: h
        }),
        $("body").append('<div class="J-back"></div>').css({
            height: "100%",
            overflow: "hidden"
        }),
        $(".J-back").css("height", f);
        var i = function () {
            $(".J-hyd").remove(),
            $(".J-back").remove(),
            $("body").css({
                height: "auto",
                overflow: "auto"
            })
        };
        $(document).on("click", ".J-okClk",
        function () {
            c.callback && c.callback(),
            c.clickOK && c.clickOK(),
            i()
        }),
        $(document).on("click", ".J-noClk",
        function () {
            c.callback && c.callback(),
            c.clickCancel && c.clickCancel(),
            i()
        })
    },
    $.Error = function (a, b) {
        var b = b ? b : 1500,
        c = '<section class="Errormes">' + a + "</section>";
        $(".Errormes").length < 1 && $("body").append(c);
        var d = $(".Errormes").width(),
        e = $(".Errormes").height(),
        f = ($(window).width() - d) / 2,
        g = ($(window).height() - e) / 2;
        $(".Errormes").css({
            left: f,
            top: g
        }),
        $(".Errormes").animate({
            opacity: "1"
        },
        100,
        function () {
            var a = $(this);
            setTimeout(function () {
                a.fadeOut(300,
                function () {
                    a.remove()
                })
            },
            b)
        })
    },
    $.addone = function (a, b) {
        var c = $("#J-addone"),
        d = $("#J-addone-cartnum").text();
        c.text("+" + a).show().delay(400).fadeOut(b),
        $("#J-addone-cartnum").text(parseInt(d) + parseInt(a))
    }
});
var setCountDown = {
    timer: null,
    init: function (a) {
        var b = this;
        this.setShowTime(a.endtime, a.done),
        this.timer = setInterval(function () {
            b.setShowTime(a.endtime, a.done, a.callback)
        },
        1e3)
    },
    getCountdown: function (a) {
        var b = this.getSecond(a) - this.getSecond();
        if (0 > b) return [0, "00", "00", "00"];
        var c = parseInt(b % 60),
        d = parseInt(b / 3600 / 24),
        e = parseInt(b / 3600) - 24 * d,
        f = parseInt((b - 3600 * parseInt(b / 3600)) / 60);
        return e = e > 9 ? e : "0" + e,
        c = c > 9 ? c : "0" + c,
        f = f > 9 ? f : "0" + f,
        [d, e, f, c]
    },
    getSecond: function (a) {
        if (a) {
            var b = parseInt(a.slice(0, 4)),
            c = parseInt(a.match(/-\d*/gi)[0].replace("-", "") - 1),
            d = parseInt(a.match(/-\d*/gi)[1].replace("-", "")),
            e = parseInt(a.match(/\d*:/)[0].replace(":", "")),
            f = parseInt(a.match(/:\d*/)[0].replace(":", ""));
            return new Date(b, c, d, e, f, 0).getTime() / 1e3
        }
        return (new Date).getTime() / 1e3
    },
    setShowTime: function (a, b, c) {
        var d = this,
        e = this.getCountdown(a)[0],
        f = this.getCountdown(a)[1],
        g = this.getCountdown(a)[2],
        h = this.getCountdown(a)[3];
        b([e, f, g, h]),
        0 == e && "00" == f && "00" == g && "00" == h && c && c(d.timer)
    }
};