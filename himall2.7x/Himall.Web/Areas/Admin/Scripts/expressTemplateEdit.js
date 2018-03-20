var t_img;
var isLoad = true;

isImgLoad(function () {
    ; (function (win, doc) {
        var $height = jQuery('#id_img').height();
        var $ = function (elem) { return doc.getElementById(elem); },// 取得元素操作
            //------- 为拖入编辑区域的控件绑定拖动事件
            widgetList = {},// 里面包含拖入到编辑区域的控件
            roundFun = function (numberRound, roundDigit) {//四舍五入，保留位数为roundDigit
                if (numberRound >= 0) {
                    var tempNumber = parseInt((numberRound * Math.pow(10, roundDigit) + 0.5)) / Math.pow(10, roundDigit);
                    return tempNumber;
                } else {
                    numberRound1 = -numberRound;
                    var tempNumber = parseInt((numberRound1 * Math.pow(10, roundDigit) + 0.5)) / Math.pow(10, roundDigit);
                    return -tempNumber;
                }
            },
            /*获取元素的纵坐标*/
            getY = function (dom) {
                var offset = dom.offsetTop;
                if (dom.offsetParent != null) {
                    offset += getY(dom.offsetParent);
                }
                return offset;
            },
            noSelectTxt = function () {
                doc.onselectstart = function () { return false; };
            },
            selectTxt = function () {
                doc.onselectstart = function () { return true; };
            },
            /*获取元素的横坐标*/
            getX = function (dom) {
                var offset = dom.offsetLeft;
                if (dom.offsetParent != null) {
                    offset += getX(dom.offsetParent);
                }
                return offset;
            },
            getCoordinate = function (dom, name) {// 取得元素相对编辑区域的坐标
                var t = dom.offsetTop,
                    l = dom.offsetLeft,
                    w = dom.offsetWidth,
                    h = dom.offsetHeight,
                    name,
                    reg = /(\d)+/ig;
                if (name) {
                    name = name.match(reg) || '';
                    //debugger;
                    //console.log(name);
                }
                var result = {
                    a: '[' + parseInt(((l / 868) * 10000), 10) + ',' + (parseInt(((t / (+$height)) * 10000), 10)) + ']',
                    b: '[' + parseInt((((l + w) / 868) * 10000), 10) + ',' + (parseInt((((t + h) / (+$height)) * 10000), 10)) + ']',
                    name: (name[0])
                };
                console.log(result.a + '  ' + result.b);
                return result;
            },
            // 拖动事件
            drag = function (dom, preventX, preventY, scopeX, scopeY) {
                var obj = dom,
                    scopeX = scopeX || [],// 拖动范围
                    scopeY = scopeY || [],
                    preventX = preventX || false,// 用来阻止拖动方向 true 为阻止
                    preventY = preventY || false,
                    left = null,// 初始化left的值
                    top = null,// 初始化top的值
                    startX = null,// 初始化时的坐标
                    startY = null,
                    init = function () {
                        obj.style.cursor = 'move';
                        obj.onmousedown = function (e) {
                            noSelectTxt();
                            var event = fixEvent((e || window.event));
                            start(event);
                            event.stopPropagation();
                            return false;
                        };
                    },
                    start = function (e) {
                        left = parseInt((obj.style.left || 0), 0);
                        top = parseInt((obj.style.top || 0), 10);
                        startX = e.pageX;
                        startY = e.pageY;
                        obj.onmouseover = function (e) {
                            obj.setAttribute('class', 'move');
                        };
                        obj.onmouseout = function (e) {
                            obj.setAttribute('class', 'box3');
                        }
                        obj.setAttribute('class', 'move');
                        obj.style.background = 'none';
                        doc.onmousemove = function (e) {
                            var event = fixEvent((e || window.event));
                            move(event);
                        };
                        doc.onmouseup = function () {
                            obj.style.background = '#fff';
                            end();
                        };
                    },
                    move = function (e) {
                        var l = left + e.pageX - startX,
                            t = top + e.pageY - startY;
                        if (scopeX[0] != undefined) {
                            l = Math.max(scopeX[0], l);
                        }
                        if (scopeX[1] != undefined) {
                            l = Math.min(scopeX[1], l);
                        }
                        if (scopeY[0] != undefined) {
                            t = Math.max(scopeY[0], t);
                        }
                        if (scopeY[1] != undefined) {
                            t = Math.min(scopeY[1], t);
                        }
                        if (preventX == false) {
                            obj.style.left = l + 'px';
                        }
                        if (preventY == false) {
                            obj.style.top = t + 'px';
                        }
                        return false;
                    },
                    end = function () {
                        selectTxt();
                        doc.onmousemove = null;
                        doc.onmouseup = null;
                    };
                init();
            },
            setSize = function (dom, preventX, preventY, scopeX, scopeY, key) {
                var obj = dom,
                    preventX = preventX || false,// 用来阻止拖动方向 true 为阻止
                    preventY = preventY || false,
                    scopeX = scopeX || [],// X轴拖动范围min,max
                    scopeY = scopeY || [],// Y轴拖动范围min,max
                    startX = null,
                    startY = null,
                    width = null,
                    height = null,
                    init = function () {
                        var sizeElem = doc.createElement('div'),
                            delElem = doc.createElement('div');
                        sizeElem.style.cssText = ";position:absolute;right:0;bottom:0;width:16px;height:16px;background:url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAA4AAAAPCAYAAADUFP50AAAAL0lEQVR42mNgGAUM8vLykkpKSv/J0gSiydJEtI3omoiyEZsmgjaSpQkEyNI0jAEAGmUcTE++Z2AAAAAASUVORK5CYII=) no-repeat;z-index:9999;";

                        delElem.style.cssText = ";position:absolute;right:0;top:0;width:16px;height:16px;background:url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAsAAAALCAYAAACprHcmAAAAQUlEQVR42mNgGFxAQUEhQUlJ6b+ioqI+iA+iQXyQOIZikAQMwzTCMIZimEnoGGYTTqcg24BVIUkmk+RmkkKDGAAAM1wzAjoB/IEAAAAASUVORK5CYII=) no-repeat;z-index:9999;cursor:pointer;";
                        delElem.onclick = function (e) {
                            delElem.parentNode.parentNode.removeChild(delElem.parentNode);
                            widgetList[key] = false;
                            var elem = doc.createElement('div'),
                                tag = delElem.parentNode.getAttribute('data-tag'),
                                str = delElem.parentNode.getAttribute('data-txt');
                            elem.style.cssText = "display:inline-block;padding:0 5px;height:30px;border: 1px dashed #666;text-align:center;line-height:28px;font-size:12px;cursor:move;margin-right:10px;float:left;margin-bottom: 10px;";
                            elem.setAttribute('id', tag);
                            elem.innerHTML = str;
                            elem.setAttribute('data-tag', key + ',' + str);
                            $('widgetBox').appendChild(elem);
                            createWidget($(tag), key, tag);
                        };
                        if (preventX) {
                            sizeElem.style.cursor = 'w-resize';
                        } else if (preventY) {
                            sizeElem.style.cursor = 'n-resize';
                        } else {
                            sizeElem.style.cursor = 'nw-resize';
                        }
                        obj.appendChild(sizeElem);
                        obj.appendChild(delElem);
                        sizeElem.onmousedown = function (e) {
                            noSelectTxt();
                            var event = fixEvent((e || window.event));
                            // 开始拖动
                            start(event);
                            event.stopPropagation();
                            return false;
                        }
                    },
                    start = function (e) {
                        startX = e.pageX;
                        startY = e.pageY;
                        width = obj.offsetWidth;
                        height = obj.offsetHeight;
                        doc.onmousemove = function (e) {
                            var event = fixEvent((e || window.event));
                            // 拖动中
                            move(event);
                        };
                        doc.onmouseup = function () {
                            // 结束拖动
                            end();
                        };
                    },
                    move = function (e) {
                        var w = width + e.pageX - startX,
                            h = height + e.pageY - startY;
                        if (scopeX[0] != undefined) {
                            w = Math.max(scopeX[0], w);
                        }
                        if (scopeX[1] != undefined) {
                            w = Math.min(scopeX[1], w);
                        }
                        if (scopeY[0] != undefined) {
                            h = Math.max(scopeY[0], h);
                        }
                        if (scopeY[1] != undefined) {
                            h = Math.min(scopeY[1], h);
                        }
                        if (preventX == false) {
                            obj.style.width = w + 'px';
                        }
                        if (preventY == false) {
                            obj.style.height = h + 'px';
                        }
                        return false;
                    },
                    end = function () {
                        doc.onmousemove = null;
                        doc.onmouseup = null;
                        selectTxt();
                    };
                init();
            },
            /* IE里面的事件修正 */
            fixEvent = function (e) {
                if (e.target) { return e; }
                var event = {}, name;
                event.target = (e.srcElement || document);
                event.preventDefault = function () {
                    e.returnValue = false;
                };
                event.stopPropagation = function () {
                    e.cancelBubble = true;
                };
                for (name in e) {
                    event[name] = e[name];
                }
                if ((event.pageX !== true) && event.clientX !== null) {
                    var doc = document.documentElement,
                    body = document.body;
                    event.pageX = event.clientX + (doc && doc.scrollLeft || body && body.scrollLeft || 0) - (doc && doc.clientLeft || body && body.clientLeft || 0);
                    event.pageY = event.clientY + (doc && doc.scrollTop || body && body.scrollTop || 0) - (doc && doc.clientTop || body && body.clientTop || 0);
                }
                return event;
            },// 检测控件是否已在编辑区域
            checkWidget = function (key) {
                if (widgetList[key]) {
                    return true;
                }
            },
            createWidget = function (dom, key, tag) {
                var obj = dom,
                    key = key,
                    tag = tag,
                    widgetStartX = null,
                    widgetEndX = null,
                    widgetStartY = null,
                    widgetEndY = null,
                    startX = null,
                    startY = null,
                    elem = null,
                    arr = dom.getAttribute('data-tag').split(','),
                    id = arr[0],
                    str = arr[1],
                    init = function () {
                        widgetStartX = getX($('canvas'));
                        widgetStartY = getY($('canvas'));
                        widgetEndX = widgetStartX + 868;
                        widgetEndY = widgetStartY + 474;
                        obj.onmousedown = function (e) {
                            noSelectTxt();// 禁止文本选中
                            doc.onselectstart = function () { return false; };
                            var event = fixEvent((e || window.event));
                            // 开始拖动
                            start(event, key, tag);
                            event.stopPropagation();
                            return false;
                        };
                    },
                    start = function (e, key, tag) {
                        startX = e.pageX;
                        startY = e.pageY;
                        elem = doc.createElement('div');
                        elem.style.cssText = 'display:none;';
                        elem.setAttribute('class', 'move');
                        elem.setAttribute('id', id);
                        elem.setAttribute('data-tag', tag);
                        elem.setAttribute('data-txt', str);
                        elem.innerHTML = str;
                        $('canvas').appendChild(elem);
                        doc.onmousemove = function (e) {
                            var event = fixEvent((e || window.event));
                            // 拖动中
                            move(event);
                        };
                        doc.onmouseup = function (e) {
                            var event = fixEvent((e || window.event));
                            // 结束拖动
                            end(event, key, tag);
                        };
                    },
                    move = function (e) {
                        if ((e.pageX > widgetStartX && e.pageX < widgetEndX) && (e.pageY > widgetStartY && e.pageY < widgetEndY)) {
                            elem.style.left = (e.pageX - widgetStartX - 50) + 'px';
                            elem.style.top = (e.pageY - widgetStartY - 15) + 'px';
                            elem.style.display = 'block';
                            elem.style.lineHeight = '32px';
                            elem.style.textIndent = '0.8em';
                            elem.style.background = 'none';
                        }
                    },
                    end = function (e, key, tag) {
                        selectTxt();// 恢复文本选中
                        doc.onmousemove = null;
                        doc.onmouseup = null;
                        if ((e.pageX > widgetStartX && e.pageX < widgetEndX) && (e.pageY > widgetStartY && e.pageY < widgetEndY)) {
                            // if(checkWidget(key)){
                            //   alert('控件已经存在编辑区域!');
                            //   elem.parentNode.removeChild(elem);// 如果重复拖动控件则对控件进行移除
                            //   return;
                            // }
                            $(tag).parentNode.removeChild($(tag));
                            elem.setAttribute('class', 'box3');
                            elem.style.background = '#fff';
                            setSize($(id), false, false, [60, 500], [24, 300], key);
                            drag($(id), false, false, [0, 860], [0, 474]);
                            widgetList[key] = true;
                        } else {
                            // 如果超出边界则移除
                            elem.parentNode.removeChild(elem);
                        }
                    };
                init();
            },
            createElem = function (id, tag, str) {
                var elem = doc.createElement('div');
                elem.style.cssText = 'display:inline-block;padding:0 5px;height:30px;border: 1px dashed #666;color:#999;cursor:move;text-align:center;line-height:28px;font-size:12px;margin-right:10px;float:left;margin-bottom: 10px;';
                elem.setAttribute('id', id);
                elem.setAttribute('data-tag', tag);
                elem.innerHTML = str;
                $('widgetBox').appendChild(elem);
            },
            // 默认
            defaultCreate = function (id, tag, str, width, height, left, top) {
                var elem = doc.createElement('div');
                elem.style.cssText = 'left:' + left + 'px;top:' + top + 'px;width:' + width + 'px;height:' + height + 'px;';
                elem.setAttribute('id', id);
                elem.setAttribute('data-tag', tag);
                elem.setAttribute('data-txt', str);
                elem.setAttribute('class', 'box3');
                elem.innerHTML = str;
                elem.style.lineHeight = '32px';
                elem.style.textIndent = '0.8em';
                $('canvas').appendChild(elem);
                setSize($(id), false, false, [60, 500], [24, 300], id);
                drag($(id), false, false, [0, 860], [0, 474]);
                widgetList[id] = true;
            },
            callback = function (data, selectedCount) {
                //var test=[{a:[5633,3319],b:[6566,3841]},{a:[7638,3256],b:[8536,3736]}]
                var i = 0, len = data.length, tagArr = ['a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'], temp, width, height, left, top;
                for (; i < len; i++) {
                    if (data[i].selected) {
                        console.log(data[i].a + ' ' + data[i].b);
                        data[i]['tag'] = tagArr[i];
                        temp = data[i].key;
                        data[i].key = tagArr[i] + data[i].key;
                        data[i].b[0] = parseInt((data[i].b[0] * 868) / 10000) + 1;
                        data[i].b[1] = parseInt((data[i].b[1] * $height) / 10000) + 1;
                        data[i].a[0] = parseInt((data[i].a[0] * 868) / 10000) + 1;
                        data[i].a[1] = parseInt((data[i].a[1] * $height) / 10000) + 1;
                        width = data[i].b[0] - data[i].a[0];
                        height = data[i].b[1] - data[i].a[1];
                        left = data[i].a[0];
                        top = data[i].a[1];
                        defaultCreate(tagArr[i], data[i].key, data[i].value, width, height, left, top);
                    } else {
                        data[i]['tag'] = tagArr[i];
                        temp = data[i].key;
                        data[i].key = tagArr[i] + data[i].key;
                        createElem(data[i].key, data[i]['tag'] + ',' + data[i].value, data[i].value);
                        createWidget($(data[i].key), data[i]['tag'], data[i].key);
                    }
                }
            },
            getAjax = function (callback) {
                if (0) {// 开启测试
                    var i = 0, len = data.length, tagArr = ['a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'], temp, width, height, left, top;;
                    for (; i < len; i++) {
                        if (data[i].selected) {
                            data[i]['tag'] = tagArr[i];
                            temp = data[i].key;
                            data[i].key = tagArr[i] + data[i].key;
                            width = data[i].b[0] - data[i].a[0];
                            height = data[i].b[1] - data[i].a[1];
                            left = data[i].a[0];
                            top = data[i].a[1];
                            //console.log(top,'-',data[i].a[1])
                            defaultCreate(tagArr[i], data[i].key, data[i].value, width, height, left, top);
                        } else {
                            data[i]['tag'] = tagArr[i];
                            temp = data[i].key;
                            data[i].key = tagArr[i] + data[i].key;
                            createElem(data[i].key, data[i]['tag'] + ',' + data[i].value, data[i].value);
                            createWidget($(data[i].key), data[i]['tag'], data[i].key);
                        }
                    }
                } else {
                    jQuery.ajax({
                        type: 'post',
                        url: 'getConfig',
                        data: { name: expressTemplateName },
                        dataType: "html",
                        success: function (data) {
                            var data = (new Function("", "return " + data))();
                            callback(data.data, data.selectedCount);
                        }
                    });
                }
            };
        $('canvas').style.height = $height + 'px';
        $('id_parent').style.height = 30 + $height + 'px';
        getAjax(callback);
        //createWidget($('addressTag'),'address','addressTag');
        //createWidget($('nameTag'),'name','nameTag');
        //------- 获取所有控件的坐标
        doc.getElementById('ids').onclick = function () {
            var i, arr = [], result, name;
            for (i in widgetList) {
                if (widgetList.hasOwnProperty(i)) {
                    if (widgetList[i]) {
                        name = $(i).getAttribute('data-tag');
                        result = getCoordinate($(i), name);
                        arr.push(result);
                    }
                }
            }
            //console.log(arr)
            //return;
            jQuery.ajax({
                type: 'POST',
                url: 'Save',
                data: { elements: JSON.stringify(arr), name: expressTemplateName },
                dataType: "json",
                success: function (data) {
                    if (data.success)
                        jQuery.dialog.tips('保存成功!', function () { location.href = "management"; });
                    else
                        jQuery.dialog.alert('保存失败！' + data.msg);
                }
            });

        };
    }(window, document));
});

// 判断图片加载的函数
function isImgLoad(callback) {
    if ($('#id_img').height() === 0) {
        isLoad = false;
    }
    // 为true，没有发现为0的。加载完毕
    if (isLoad) {
        clearTimeout(t_img);
        callback();
        // 为false，因为找到了没有加载完成的图，将调用定时器递归
    } else {
        isLoad = true;
        t_img = setTimeout(function () {
            isImgLoad(callback); // 递归扫描
        }, 500);
    }
}