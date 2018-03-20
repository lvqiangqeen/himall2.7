$(function () {
    HiShop.DIY = HiShop.DIY ? HiShop.DIY : {}; //DIY 命名空间
    HiShop.DIY.Unit = HiShop.DIY.Unit ? HiShop.DIY.Unit : {}; //unit
    HiShop.DIY.PModules = HiShop.DIY.PModules ? HiShop.DIY.PModules : []; //页面模块
    HiShop.DIY.LModules = HiShop.DIY.LModules ? HiShop.DIY.LModules : []; //自定义模块列表

    var $diy_contain = $("#diy-contain"), //Diy 内容显示区域
        $diy_ctrl = $("#diy-ctrl"); //Diy 控制器显示区域

    //DIY常量
    HiShop.DIY.constant = {
        diyoffset: $(".diy").offset()
    };

    //获取当前时间戳
    HiShop.DIY.getTimestamp = function () {
        var date = new Date();
        return "" + date.getFullYear() + parseInt(date.getMonth() + 1) + date.getDate() + date.getHours() + date.getMinutes() + date.getSeconds() + date.getMilliseconds();
    };

    /*
     * 添加模块
     * @param type 模块类型
     * @param data 模块数据
     * @param showctrl 是否添加完后立即显示控制内容
     */
    HiShop.DIY.add = function (data, showctrl) {
        //添加手机内容
        var html_con = _.template($("#tpl_diy_con_type" + data.type).html(), data), //内容
            html_conitem = _.template($("#tpl_diy_conitem").html(), {
                html: html_con
            }), //diy通用外层容器
            $render_conitem = $(html_conitem); //渲染模板

        data.dom_conitem = $render_conitem; //缓存手机内容dom

        var $actionPanel = $render_conitem.find(".diy-conitem-action"),
            $btn_edit = $actionPanel.find(".j-edit"),
            $btn_del = $actionPanel.find(".j-del");

        //绑定编辑模块事件
        $actionPanel.click(function () {
            $(".diy-conitem-action").removeClass("selected");
            $(this).addClass("selected");
            HiShop.DIY.edit(data);
        });

        //是否插入可拖拽区域
        var dragPanel = "";

        //只有可拖拽的模块才可以被删除
        if (data.draggable) {
            //绑定删除模块事件
            $btn_del.click(function () {
                HiShop.DIY.del(data);
                return false;
            });
            dragPanel = ".drag";
        } else {
            $btn_del.remove();
            dragPanel = ".nodrag";
        }

        $diy_contain.find(dragPanel).append($render_conitem); //插入文档

        //是否添加完后立即显示控制内容
        showctrl = showctrl ? showctrl : false;

        if (showctrl) {
            $actionPanel.click(); //触发一次编辑事件
        }

        //根据是否可拖动插入不同的缓存数组
        if (data.draggable) {
            HiShop.DIY.LModules.push(data);
        } else {
            HiShop.DIY.PModules.push(data);
        }

        return false;
    };

    /*
     * 编辑模块
     * @param data 模块数据
     */
    HiShop.DIY.edit = function (data) {
        //移除之前的模块控制内容
        $diy_ctrl.find(".diy-ctrl-item[data-origin='item']").remove();

        //渲染模板
        var html_ctrl_panel = $("#tpl_diy_ctrl").html(),
            html_ctrl_con = _.template($("#tpl_diy_ctrl_type" + data.type).html(), data),
            html_ctrl = _.template(html_ctrl_panel, { html: html_ctrl_con }),
            $render_ctrl = $(html_ctrl);

        $diy_ctrl.append($render_ctrl); //插入dom

        HiShop.DIY.repositionCtrl(data.dom_conitem, $render_ctrl); //设置控制内容的位置

        HiShop.DIY.bindEvents($render_ctrl, data); //绑定各种事件

        $render_ctrl.show().siblings(".diy-ctrl-item").hide(); //显示控制内容，并隐藏其它

        return false;
    };

    /*
     * 重设控制内容的位置
     * @param conitem 手机视图dom对象
     * @param ctrl 控制内容dom对象
     */
    HiShop.DIY.repositionCtrl = function (conitem, ctrl) {
        var top_conitem = conitem.offset().top,
            curPosTop = top_conitem - HiShop.DIY.constant.diyoffset.top;

        ctrl.css("marginTop", curPosTop);//设置位置

        $("html,body").animate({ scrollTop: curPosTop }, 300);//滚动页面
    };

    /*
     * 删除模块
     * @param data 模块数据
     */
    HiShop.DIY.del = function (data) {
        if (!data) return;
        //提示删除
        $.jBox.show({
            title: "提示",         
            content: _.template($("#tpl_jbox_simple").html(), {               
                content: "删除后将不可恢复，是否继续？"
            }),
            btnOK: {
                onBtnClick: function (jbox) {
                    $.jBox.close(jbox);
                    //从缓存数组中删除
                    var lists = HiShop.DIY.LModules,
                        lists_len = HiShop.DIY.LModules.length;

                    for (var i = 0; i < lists_len; i++) {
                        if (lists[i].id == data.id) {
                            lists.splice(i, 1);
                            break;
                        }
                    }
                    //从文档中删除
                    data.dom_conitem.remove();
                    $diy_ctrl.find(".diy-ctrl-item[data-origin='item']").remove();
                }
            }
        });
        return false;
    };

    /*
     * 绑定ctrl事件
     * @param ctrldom 空中内容dom
     * @param data 模块数据
     */
    HiShop.DIY.bindEvents = function (ctrldom, data) {
        //根据不同类型模块绑定相应事件
        // switch (data.type) {
        //     case 1:HiShop.DIY.Unit.event_type1(ctrldom, data);break;//富文本
        //     case 2:HiShop.DIY.Unit.event_type2(ctrldom, data);break;//标题
        //     case 3:HiShop.DIY.Unit.event_type3(ctrldom, data);break;//自定义模块
        //     case 4:HiShop.DIY.Unit.event_type4(ctrldom, data);break;//商品
        //     case 5:HiShop.DIY.Unit.event_type5(ctrldom, data);break;//商品列表（分组标签）
        //     // case 6:HiShop.DIY.Unit.event_type6(ctrldom, data);break;//商品搜索
        //     case 7:HiShop.DIY.Unit.event_type7(ctrldom, data);break;//文本导航
        //     case 8:HiShop.DIY.Unit.event_type8(ctrldom, data);break;//图片导航
        //     case 9:HiShop.DIY.Unit.event_type9(ctrldom, data);break;//图片广告
        //     // case 10:HiShop.DIY.Unit.event_type10(ctrldom, data);break;//分割线
        //     case 11:HiShop.DIY.Unit.event_type11(ctrldom, data);break;//辅助空白
        // }

        if (data.type == 6 || data.type == 10) return;
        HiShop.DIY.Unit["event_type" + data.type](ctrldom, data);
    };

    /*
     * 重新计算装修模块的排序
     */
    HiShop.DIY.reCalcPModulesSort = function () {
        _.each(HiShop.DIY.LModules, function (module, index) {
            module.sort = module.dom_conitem.index();
        });
    };

    /*
     * 获取装修数据
     */
    HiShop.DIY.Unit.getData = function () {
        HiShop.DIY.reCalcPModulesSort(); //重新计算模块的排序

        //数据格式
        var data = {
            page: {}, //页面信息
            PModules: {}, //页面模块
            LModules: {} //装修模块
        };

        data.page.title = $(".j-pagetitle-ipt").val(); //获取页面标题数据
        data.page.subtitle = $(".j-pagesubtitle-ipt").val(); //获取页面标题数据
        data.page.view_pic = $(".j-view_pic-ipt").prop('src');
        data.page.praise_num = $(".j-pagepraisenum").val();
        data.page.describe = $(".j-pagetitle-describe").val(); //获取页面描述数据
        data.page.tags = $(".j-pagetitle-tags").val(); //获取页面标签数据
        data.page.icon = $(".j-pagetitle-icon").val(); //获取页面图标数据
        data.PModules = HiShop.DIY.PModules; //获取页面模块数据
       


        data.page.goto_time = $('.j-gototime-ipt').val(); // 获取时间
        //缓存排序后的自定义模块数组
        var newsortarr = [];

        //重排序
        for (var i = 0; i < HiShop.DIY.LModules.length; i++) {
            var tmp = HiShop.DIY.LModules[i];
            if (tmp != '') {
                newsortarr[tmp.sort] = tmp;
            }
        }

        data.LModules = newsortarr;

        //移除数据里的dom参数和ue参数
        var tmp = $.extend(true, {}, data);

        _.each(tmp.LModules, function (item) {
            if (item.type == 5) {
                item.dom_conitem = HiShop.Base64.encode(HiShop.Base64.utf16to8(HiShop.Config.HiTempLatePath.GroupGoodTemp + item.content.layout + HiShop.Config.HiTempLatePath.TemplateExt));
            }
            else if (item.type == 9){
                item.dom_conitem = HiShop.Base64.encode(HiShop.Base64.utf16to8(_.template($("#tpl_diy_con_type" + item.type+"Phone").html(), item)));
            }
            else {
                item.dom_conitem = HiShop.Base64.encode(HiShop.Base64.utf16to8(_.template($("#tpl_diy_con_type" + item.type).html(), item)));
            }


           
            item.dom_ctrl = null;
            item.ue = null;
        });

        _.each(tmp.PModules, function (item) {
            var head = $("#tpl_diy_con_type" + item.type).html();
            if (item.type == 9) {
                head = $("#tpl_diy_con_type" + item.type+"Phone").html();
            } else if (item.type == "Header_style11" || item.type == "Header_style14") {
                head = $("#tpl_diy_con_type" + item.type + "Phone").html();
            }
            item.dom_conitem = HiShop.Base64.encode(HiShop.Base64.utf16to8(_.template(head, item) + HiShop.DIY.Unit.HStyleAddJs(item.content.bg)));
            
            
            item.dom_ctrl = null;
            item.ue = null;
        });

        return tmp;
    }


    HiShop.DIY.Unit.HStyleAddJs = function (bg)
    {
        //html = "  <script src=\"http://apps.bdimg.com/libs/jquery/2.1.4/jquery.min.js\" type=\"text/javascript\"></script>\n";
        html = " <script type=\"text/javascript\">\n\
                    $(function () {\n\
                        $('html').css({\n\
                            'height':'100%',\n\
                            'width':'100%'\n\
                        })\n\
                        $('body').css({\n\
                            'height':'100%',\n\
                            'width':'100%',\n\
                            'background':'url(" + bg + ") no-repeat',\n\
                            'backgroundSize':'cover'\n\
                        })\n\
                    })\n\
                </script>";
        return html;
    }


    HiShop.DIY.Unit.html_encode = function (str) {
        var s = "";

        if (str.length == 0) return "";
        s = str.replace(/&/g, "&amp;");
        s = s.replace(/</g, "&lt;");
        s = s.replace(/>/g, "&gt;");
        s = s.replace(/ /g, "&nbsp;");
        s = s.replace(/\'/g, "&#39;");
        s = s.replace(/\"/g, "&quot;");

        return s;
    }




    HiShop.DIY.Unit.html_decode = function (str) {
        var s = "";

        if (str.length == 0) return "";
        s = str.replace(/&amp;/g, "&");
        s = s.replace(/&lt;/g, "<");
        s = s.replace(/&gt;/g, ">");
        s = s.replace(/&nbsp;/g, " ");
        s = s.replace(/&#39;/g, "\'");
        s = s.replace(/&quot;/g, "\"");

        return s;
    }


});