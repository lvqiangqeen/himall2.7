function Swipe(m, e) {
    function w() { h = c.children; p = Array(h.length); d = m.getBoundingClientRect().width || m.offsetWidth; c.style.width = h.length * d + "px"; m.style.width = d + "px"; for (var b = h.length; b--;) { var l = h[b]; l.style.width = d + "px"; l.setAttribute("data-index", b); q.transitions && (l.style.left = b * -d + "px", k(b, a > b ? -d : a < b ? d : 0, 0)) } q.transitions || (c.style.left = a * -d + "px"); m.style.visibility = "visible" } function x() { a < h.length - 1 ? u(a + 1) : e.continuous && u(0) } function u(b, l) {
        if (a != b) {
            if (q.transitions) {
                for (var c = Math.abs(a - b) -
                1, f = Math.abs(a - b) / (a - b) ; c--;) k((b > a ? b : a) - c - 1, d * f, 0); k(a, d * f, l || n); k(b, 0, l || n)
            } else F(a * -d, b * -d, l || n); a = b; c = e.callback && e.callback(a, h[a]); setTimeout(c || y, 0)
        }
    } function k(b, a, c) { v(b, a, c); p[b] = a } function v(b, a, c) { if (b = (b = h[b]) && b.style) b.webkitTransitionDuration = b.MozTransitionDuration = b.msTransitionDuration = b.OTransitionDuration = b.transitionDuration = c + "ms", b.webkitTransform = "translate(" + a + "px,0)translateZ(0)", b.msTransform = b.MozTransform = b.OTransform = "translateX(" + a + "px)" } function F(b, l, d) {
        if (d) var f =
        +new Date, g = setInterval(function () { var k = +new Date - f; k > d ? (c.style.left = l + "px", r && (z = setTimeout(x, r)), e.transitionEnd && e.transitionEnd.call(event, a, h[a]), clearInterval(g)) : c.style.left = Math.floor(k / d * 100) / 100 * (l - b) + b + "px" }, 4); else c.style.left = l + "px"
    } function A() { r = 0; clearTimeout(z) } var y = function () { }, q = {
        addEventListener: !!window.addEventListener, touch: "ontouchstart" in window || window.DocumentTouch && document instanceof DocumentTouch, transitions: function (b) {
            var a = ["transformProperty", "WebkitTransform",
            "MozTransform", "OTransform", "msTransform"], c; for (c in a) if (void 0 !== b.style[a[c]]) return !0; return !1
        }(document.createElement("swipe"))
    }; if (m) {
        var c = m.children[0], h, p, d; e = e || {}; var a = parseInt(e.startSlide, 10) || 0, n = e.speed || 300; e.continuous = e.continuous ? e.continuous : !0; var r = e.auto || 0, z, C, D, E, f, B, t, g = {
            handleEvent: function (b) {
                switch (b.type) {
                    case "touchstart": this.start(b); break; case "touchmove": this.move(b); break; case "touchend": var a = this.end(b); setTimeout(a || y, 0); break; case "webkitTransitionEnd": case "msTransitionEnd": case "oTransitionEnd": case "otransitionend": case "transitionend": a =
                    this.transitionEnd(b); setTimeout(a || y, 0); break; case "resize": a = w.call(), setTimeout(a || y, 0)
                } e.stopPropagation && b.stopPropagation()
            }, start: function (b) { b = b.touches[0]; C = b.pageX; D = b.pageY; E = +new Date; B = f = t = void 0; c.addEventListener("touchmove", this, !1); c.addEventListener("touchend", this, !1) }, move: function (b) {
                if (!(1 < b.touches.length || b.scale && 1 !== b.scale)) {
                    e.disableScroll && b.preventDefault(); var c = b.touches[0]; f = c.pageX - C; B = c.pageY - D; "undefined" == typeof t && (t = !!(t || Math.abs(f) < Math.abs(B))); t || (b.preventDefault(),
                    A(), f /= !a && 0 < f || a == h.length - 1 && 0 > f ? Math.abs(f) / d + 1 : 1, v(a - 1, f + p[a - 1], 0), v(a, f + p[a], 0), v(a + 1, f + p[a + 1], 0))
                }
            }, end: function (b) { b = 250 > Number(+new Date - E) && 20 < Math.abs(f) || Math.abs(f) > d / 2; var l = !a && 0 < f || a == h.length - 1 && 0 > f, m = 0 > f; t || (b && !l ? (m ? (k(a - 1, -d, 0), k(a, p[a] - d, n), k(a + 1, p[a + 1] - d, n), a += 1) : (k(a + 1, d, 0), k(a, p[a] + d, n), k(a - 1, p[a - 1] + d, n), a += -1), e.callback && e.callback(a, h[a])) : (k(a - 1, -d, n), k(a, 0, n), k(a + 1, d, n))); c.removeEventListener("touchmove", g, !1); c.removeEventListener("touchend", g, !1) }, transitionEnd: function (b) {
                parseInt(b.target.getAttribute("data-index"),
                10) == a && (r && (z = setTimeout(x, r)), e.transitionEnd && e.transitionEnd.call(b, a, h[a]))
            }
        }; w(); r && (z = setTimeout(x, r)); q.addEventListener ? (q.touch && c.addEventListener("touchstart", g, !1), q.transitions && (c.addEventListener("webkitTransitionEnd", g, !1), c.addEventListener("msTransitionEnd", g, !1), c.addEventListener("oTransitionEnd", g, !1), c.addEventListener("otransitionend", g, !1), c.addEventListener("transitionend", g, !1)), window.addEventListener("resize", g, !1)) : window.onresize = function () { w() }; return {
            setup: function () { w() },
            slide: function (b, a) { u(b, a) }, prev: function () { A(); a ? u(a - 1) : e.continuous && u(h.length - 1) }, next: function () { A(); x() }, getPos: function () { return a }, kill: function () {
                A(); c.style.width = "auto"; c.style.left = 0; for (var a = h.length; a--;) { var d = h[a]; d.style.width = "100%"; d.style.left = 0; q.transitions && v(a, 0, 0) } q.addEventListener ? (c.removeEventListener("touchstart", g, !1), c.removeEventListener("webkitTransitionEnd", g, !1), c.removeEventListener("msTransitionEnd", g, !1), c.removeEventListener("oTransitionEnd", g, !1), c.removeEventListener("otransitionend",
                g, !1), c.removeEventListener("transitionend", g, !1), window.removeEventListener("resize", g, !1)) : window.onresize = null
            }
        }
    }
} (window.jQuery || window.Zepto) && function (m) { m.fn.Swipe = function (e) { return this.each(function () { m(this).data("Swipe", new Swipe(m(this)[0], e)) }) } }(window.jQuery || window.Zepto);