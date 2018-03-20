$(function () {
    $("#productsNumberDiv").text(productsNumberIng);
    $("#useSpaceDiv").text(useSpace+"M");
    require.config({
        paths: {
            echarts: '/Scripts'
        }
    });
    require(
        [
            'echarts',
            'echarts/chart/pie'
        ],
        load
    );
});

function load(echarts) {

    var labelTop = {
        normal: {
            label: {
                show: true,
                position: 'bottom',
                formatter: '{b}',
                textStyle: {
                    baseline: 'top'
                }
            },
            labelLine: {
                show: false
            }
        }
    };
    var labelFromatter = {
        normal: {
            color: 'red',
            label: {
                formatter: function (params) {
                    return productsPercentage + '%'
                    // return 500 - params.value + '%'
                },
                textStyle: {
                    baseline: 'top'
                }
            }
        },
    }

    var labelFromatterImage = {
        normal: {
            color: 'red',
            label: {
                formatter: function (params) {
                    return useSpaceingPercentage + '%'
                    // return 500 - params.value + '%'
                },
                textStyle: {
                    baseline: 'top'
                }
            }
        },
    }

    var labelBottom = {
        normal: {
            color: '#fff',
            label: {
                show: true,
                position: 'center'
            },
            labelLine: {
                show: false
            }
        },
        emphasis: {
            color: '#fff'
        }
    };
    var radius = [43, 44];
    var shopProductPieOption = {
        legend: {
            x: 'right',
            y: 'center',
            data: [
               
            ]
        },
        title: {
            text: '',
            subtext: '',
            x: 'center'
        },
        toolbox: {
            show: false,
            feature: {
                dataView: { show: false, readOnly: false },
                magicType: {
                    show: true,
                    type: ['pie', 'funnel'],
                    option: {
                        funnel: {
                            width: '20%',
                            height: '30%',
                            itemStyle: {
                                normal: {
                                    label: {
                                        formatter: function (params) {
                                            return 'other\n' + params.value + '%\n'
                                        },
                                        textStyle: {
                                            baseline: 'middle'
                                        }
                                    }
                                },
                            }
                        }
                    }
                },
                restore: { show: false },
                saveAsImage: { show: false }
            }
        },
        series: [
            {
                pieNumber: productsNumberIng,
                type: 'pie',
                center: ['50%', '45%'],
                radius: radius,
                x: '0%', // for funnel
                itemStyle: labelFromatter,
                data: [
                    { name: 'productsNumberIng', value: (productsNumberIng - productsNumber), itemStyle: labelBottom },
                    { name: 'ProductsNumber', value: productsNumber, itemStyle: labelTop }

                ]
            }
        ]
    };





    var shopImagePieOption = {
        legend: {
            x: 'right',
            y: 'center',
            data: [

            ]
        },
        title: {
            text: '',
            subtext: '',
            x: 'center'
        },
        toolbox: {
            show: false,
            feature: {
                dataView: { show: false, readOnly: false },
                magicType: {
                    show: true,
                    type: ['pie', 'funnel'],
                    option: {
                        funnel: {
                            width: '20%',
                            height: '30%',
                            itemStyle: {
                                normal: {
                                    label: {
                                        formatter: function (params) {
                                            return 'other\n' + params.value + '%\n'
                                        },
                                        textStyle: {
                                            baseline: 'middle'
                                        }
                                    }
                                },
                            }
                        }
                    }
                },
                restore: { show: false },
                saveAsImage: { show: false }
            }
        },
        series: [
            {
                pieNumber: useSpace,
                type: 'pie',
                center: ['50%', '45%'],
                radius: radius,
                x: '0%', // for funnel
                itemStyle: labelFromatterImage,
                data: [
                    { name: 'useSpaceing    ', value: (useSpace - useSpaceing), itemStyle: labelBottom },
                    { name: 'useSpace', value: useSpaceing, itemStyle: labelTop }

                ]
            }
        ]
    };

    //店铺效果分析
    var shopProductPie = echarts.init(document.getElementById('shopProductPie'));
    var shopImagePie = echarts.init(document.getElementById('shopImagePie'));
    shopProductPie.showLoading({
        text: '正在加载图表...',
        effect: 'bubble',
        textStyle: {
            fontSize: 20
        }
    });
    shopImagePie.showLoading({
        text: '正在加载图表...',
        effect: 'bubble',
        textStyle: {
            fontSize: 20
        }
    });

    shopProductPie.hideLoading();
    shopImagePie.hideLoading();
    shopProductPie.setOption(shopProductPieOption);
    shopImagePie.setOption(shopImagePieOption);



}
