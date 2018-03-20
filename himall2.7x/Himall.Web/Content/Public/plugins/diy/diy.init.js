$(function () {
    //添加一个模块
    $(".j-diy-addModule").click(function () {
        var type = $(this).data("type"); //获取模块类型
        var strUrl = window.location.toLocaleString().toLocaleLowerCase();
        var clientName = "wapshop";
        if (strUrl.replace("/vshop/", "").length < strUrl.length) {
            clientName = "vshop";
        } else if (strUrl.replace("/wapshop/", "").length < strUrl.length) {
            clientName = "wapshop";
        } else if (strUrl.replace("/alioh/", "").length < strUrl.length) {
            clientName = "alioh";
        } else if (strUrl.replace("/wxsmallprogram/", "").length < strUrl.length) {
            clientName = "xcxshop";
        }
        //默认数据
        var moduleData = {
            id: HiShop.DIY.getTimestamp(), //模块ID
            type: type, //模块类型
            draggable: true, //是否可拖动
            sort: 0, //排序
            content: null //模块内容
        };

        //根据模块类型设置默认值
        switch (type) {
            //富文本
            case 1:
                moduleData.ue = null;
                moduleData.content = {
                    fulltext: "&lt;p&gt;『富文本编辑器』&lt;/p&gt;"
                };
                break;
                //标题
            case 2:
                moduleData.content = {
                    title: "标题名称",
                    subtitle: "『副标题』",
                    direction: "left",
                    space: 0,
                    style:0
                };
                break;
                //自定义模块
            case 3: break;
                //商品
            case 4:
                moduleData.content = {
                    layout: 1,
                    showPrice: true,
                    showIco: true,
                    showName: 1,
                    goodslist: []
                }
                break;
                //商品列表（分组标签）
            case 5:
                moduleData.content = {
                    layout: 1,
                    showPrice: true,
                    showIco: true,
                    showName: true,
                    group: null,
                    firstPriority: 1,
                    secondPriority: 3,
                    thirdPriority:5,
                    goodsize: 6,
                    goodslist: [
                        {
                            item_id: "1",
                            link: "#",
                            pic: "/Content/Public/images/diy/goodsView1.jpg",
                            price: "100.00",
                            title: "第一个商品"
                        },
                        {
                            item_id: "2",
                            link: "#",
                            pic: "/Content/Public/images/diy/goodsView2.jpg",
                            price: "200.00",
                            title: "第二个商品"
                        },
                        {
                            item_id: "3",
                            link: "#",
                            pic: "/Content/Public/images/diy/goodsView3.jpg",
                            price: "300.00",
                            title: "第三个商品"
                        },
                        {
                            item_id: "4",
                            link: "#",
                            pic: "/Content/Public/images/diy/goodsView4.jpg",
                            price: "400.00",
                            title: "第四个商品"
                        }
                    ]
                }
                break;
                //搜索
            case 6: break;
                //文本导航
            case 7:
                moduleData.content = {
                    dataset: [
                        {
                            linkType: 0,
                            link: "",
                            title: "",
                            showtitle: ""
                        }
                    ]
                }
                break;
                //图片导航
            case 8:
                moduleData.content = {
                    dataset: [
                        {
                            linkType: 0,
                            link: "#",
                            title: "导航名称",
                            showtitle: "导航名称",
                            pic: "/Content/Public/images/diy/waitupload.png"
                        },
                        {
                            linkType: 0,
                            link: "#",
                            title: "导航名称",
                            showtitle: "导航名称",
                            pic: "/Content/Public/images/diy/waitupload.png"
                        },
                        {
                            linkType: 0,
                            link: "#",
                            title: "导航名称",
                            showtitle: "导航名称",
                            pic: "/Content/Public/images/diy/waitupload.png"
                        },
                        {
                            linkType: 0,
                            link: "#",
                            title: "导航名称",
                            showtitle: "导航名称",
                            pic: "/Content/Public/images/diy/waitupload.png"
                        }
                    ]
                }
                break;
                //广告图片
            case 9:
                moduleData.content = {
                    showType: 1,
                    space: 0,
                    margin: 10,
                    dataset: []
                }
                break;
                //分割线
            case 10: break;
                //辅助空白
            case 11:
                moduleData.content = {
                    height: 10
                }
                break;
                // 顶部菜单
            case 12:
                moduleData.content = {
                    style: '0',
                    marginstyle: '0',
                    dataset: [
                        {
                            link: "/" + clientName + "/default.aspx",
                            linkType: 6,
                            showtitle: "首页",
                            title: "店铺主页",
                            pic: "/Content/Public/images/icon/style1/color0/icon_home.png",
                            bgColor: "#07a0e7",
                            cloPicker: '0',
                            fotColor: '#fff'
                        },
                        {
                            link: "/"+clientName+"/default.aspx",
                            linkType: 6,
                            showtitle: "新品",
                            title: "",
                            pic: "/Content/Public/images/icon/style1/color0/icon_newgoods.png",
                            bgColor: "#72c201",
                            cloPicker: '1',
                            fotColor: '#fff'
                        },
                        {
                            link: "/" + clientName + "/default.aspx",
                            linkType: 6,
                            showtitle: "热卖",
                            title: "",
                            pic: "/Content/Public/images/icon/style1/color0/icon_hotsale.png",
                            bgColor: "#ffa800",
                            cloPicker: '2',
                            fotColor: '#fff'
                        },
                        {
                            link: "/" + clientName + "/default.aspx",
                            linkType: 6,
                            showtitle: "会员主页",
                            title: "",
                            pic: "/Content/Public/images/icon/style1/color0/icon_user.png",
                            bgColor: "#d50303",
                            cloPicker: '3',
                            fotColor: '#fff'
                        }
                    ]
                }
                break;
                // 顶部菜单
            case 13:
                moduleData.content = {
                    layout: '1',
                    dataset: [
                        {
                            linkType: 0,
                            link: "#",
                            title: "导航名称",
                            pic: "/Content/Public/images/diy/waitupload.png"
                        },
                        {
                            linkType: 0,
                            link: "#",
                            title: "导航名称",
                            pic: "/Content/Public/images/diy/waitupload.png"
                        },
                        {
                            linkType: 0,
                            link: "#",
                            title: "导航名称",
                            pic: "/Content/Public/images/diy/waitupload.png"
                        }
                    ]
                }
                break;
                // 视频
            case 14:
                moduleData.content = {
                    website: ''
                }
                break;
                // 音频
            case 15:
                moduleData.content = {
                    direct: 0,
                    imgsrc: '',
                    audiosrc: ''
                }
                break;
                // 公告
            case 16:
                moduleData.content = {
                    linkType: 0,
                    title: "公告",
                    showtitle: "请填写内容，如果过长，将会滚动显示"
                }
                break;


            //APP模块1
            case 20:
                if (clientName == "appshop" || clientName == "xcxshop") {
                    moduleData.content = {
                        layout: '1',
                        dataset: [
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Content/Public/images/diy/app/bar01.png"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Content/Public/images/diy/app/group_1_2.png"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Content/Public/images/diy/app/group_1_3.png"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Content/Public/images/diy/app/group_1_4.png"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Content/Public/images/diy/app/group_1_5.png"
                            }
                        ]
                    }
                } else {
                    moduleData.content = {
                        layout: '1',
                        dataset: [
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Admin/images/wap/group_1_1.jpg"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Admin/images/wap/group_1_2.jpg"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Admin/images/wap/group_1_3.jpg"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Admin/images/wap/group_1_4.jpg"
                            }
                        ]
                    }
                }
                break;

            //APP模块2
            case 21:
                if (clientName == "appshop" || clientName == "xcxshop") {
                    moduleData.content = {
                        layout: '1',
                        dataset: [
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Content/Public/images/diy/app/bar01.png"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Content/Public/images/diy/app/group_2_2.png"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Content/Public/images/diy/app/group_2_3.png"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Content/Public/images/diy/app/group_2_4.png"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Content/Public/images/diy/app/group_2_5.png"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Content/Public/images/diy/app/group_2_6.png"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Content/Public/images/diy/app/group_2_7.png"
                            }
                        ]
                    }
                } else {
                    moduleData.content = {
                        layout: '1',
                        dataset: [
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Admin/images/wap/group_2_1.jpg"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Admin/images/wap/group_2_2.jpg"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Admin/images/wap/group_2_3.jpg"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Admin/images/wap/group_2_4.jpg"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Admin/images/wap/group_2_5.jpg"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Admin/images/wap/group_2_6.jpg"
                            }
                        ]
                    }
                }

                break;
            case 22:
                if (clientName == "appshop" || clientName == "xcxshop") {
                    moduleData.content = {
                        layout: '1',
                        dataset: [
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Content/Public/images/diy/app/bar01.png"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Content/Public/images/diy/app/group_3_2.png"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Content/Public/images/diy/app/group_3_3.png"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Content/Public/images/diy/app/group_3_4.png"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Content/Public/images/diy/app/group_3_5.png"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Content/Public/images/diy/app/group_3_6.png"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Content/Public/images/diy/app/group_3_7.png"
                            }
                        ]
                    }
                } else {
                    moduleData.content = {
                        layout: '1',
                        dataset: [
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Admin/images/wap/group_3_1.jpg"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Admin/images/wap/group_3_2.jpg"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Admin/images/wap/group_3_3.jpg"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Admin/images/wap/group_3_4.jpg"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Admin/images/wap/group_3_5.jpg"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Admin/images/wap/group_3_6.jpg"
                            }
                        ]
                    }
                }
                break;
            case 23:
                if (clientName == "appshop" || clientName == "xcxshop") {
                    moduleData.content = {
                        layout: '1',
                        dataset: [
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Content/Public/images/diy/app/bar01.png"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Content/Public/images/diy/app/group_4_2.png"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Content/Public/images/diy/app/group_4_3.png"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Content/Public/images/diy/app/group_4_4.png"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Content/Public/images/diy/app/group_4_5.png"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Content/Public/images/diy/app/group_4_6.png"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Content/Public/images/diy/app/group_4_7.png"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Content/Public/images/diy/app/group_4_8.png"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Content/Public/images/diy/app/group_4_9.png"
                            }
                        ]
                    }
                } else {
                    moduleData.content = {
                        layout: '1',
                        dataset: [
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Admin/images/wap/group_4_1.jpg"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Admin/images/wap/group_4_2.jpg"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Admin/images/wap/group_4_3.jpg"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Admin/images/wap/group_4_4.jpg"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Admin/images/wap/group_4_5.jpg"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Admin/images/wap/group_4_6.jpg"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Admin/images/wap/group_4_7.jpg"
                            },
                            {
                                linkType: 0,
                                link: "#",
                                title: "导航名称",
                                pic: "/Admin/images/wap/group_4_8.jpg"
                            }
                        ]
                    }
                }

                break;
            case 24:
                moduleData.content = {
                    layout: '1',
                    dataset: [
                        {
                            linkType: 0,
                            link: "#",
                            title: "导航名称",
                            pic: "/Content/Public/images/diy/app/ad.png"
                        },
                    ]
                }
                break;
        }

        //添加模块
        HiShop.DIY.add(moduleData, true);
    });

    //初始化布局拖动事件
    $("#diy-phone .drag").sortable({
        revert: true,
        placeholder: "drag-highlight",
        stop: function (event, ui) {
            HiShop.DIY.repositionCtrl(ui.item, $(".diy-ctrl-item[data-origin='item']")); //重置ctrl的位置
        }
    }).disableSelection();

    //编辑页面标题
    $(".j-pagetitle").click(function () {
        $(".diy-ctrl-item[data-origin='pagetitle']").show().siblings(".diy-ctrl-item[data-origin='item']").hide();
    });

    //编辑页面标题同步到手机视图中
    $(".j-pagetitle-ipt").change(function () {
        $(".j-pagetitle").text($(this).val());
    });
});