$(function () {

    var loadingTicket;
    var myChart;
    var mapName;
    var shopChart;

    option = {
        tooltip: {
            trigger: 'axis',
            formatter: function (params, ticket, callback) {
                var type = 1;
                var html = '';
                if (1 == type) {
                    var t1 = '<span style="text-align:left;">商品：<b style="color:yellow;font-size:14px;">' + mapName[params[0][1] - 1] + '</b></span>';
                    var t2 = '<span style="text-align:left;">销售量：<b style="color:yellow;font-size:14px;">' + params[0][2] + '</b>个</span>';
                    html = ['<div style="text-align:left;">', t1, '<br />', t2, '</div>'];
                } else {
                    var t1 = '<span style="text-align:left;">商品：<b style="color:yellow;font-size:14px;">' + mapName[params[0][1] - 1] + '</b></span>';
                    var t2 = '<span style="text-align:left;">销售额：<b style="color:yellow;font-size:14px;">' + params[0][2] + '</b>元</span>';
                    html = ['<div style="text-align:left;">', t1, '<br />', t2, '</div>'];
                }

                return html.join('');
            }
        },
        legend: {
            data: ['2']
        },
        toolbox: {
            show: false,
            feature: {
                magicType: { show: true, type: ['line', 'bar'] },
                restore: { show: true },
                saveAsImage: { show: true }
            }
        },
        calculable: true,
        xAxis: [
            {
                type: 'category',
                data: []
            }
        ],
        yAxis: [
            {
                type: 'value',
                splitArea: { show: true }
            }
        ],
        series: [
            {
                name: '123',
                type: 'line',
                data: [],
                smooth: true,
                symbol: 'emptyCircle',
                markPoint: {
                    data: [
                        { type: 'max', name: '最多' },
                        { type: 'min', name: '最少' }
                    ]
                },
                markLine: {
                    data: [
                        { type: 'average', name: '平均值' }
                    ]
                }
            }
        ]
    };


    require.config({
        paths: {
            echarts: '/Scripts'
        }
    });
    require(
        [
            'echarts',
            'echarts/chart/bar',
            'echarts/chart/line',
            'echarts/chart/map',
            'echarts/chart/funnel'
        ],
        load
    );


    function load(echarts) {
        
        //商品销售
        myChart = echarts.init(document.getElementById('main'));

        myChart.showLoading({
            text: '正在加载图表...',
            effect: 'bubble',
            textStyle: {
                fontSize: 20
            }
        });

        clearTimeout(loadingTicket);

        loadingTicket = setTimeout(function () {
            //商品销售
            ajaxRequest({
                type: 'GET',
                url: "./ProductRecentMonthSaleRank",
                param: {},
                dataType: "json",
                success: function (data) {                    
                    if (data.successful == true) {
                        option.series[0].data = [];
                        option.xAxis[0].data = data.chart.XAxisData;
                        option.series[0].data = data.chart.SeriesData[0].Data;
                        option.series[0].name = data.chart.SeriesData[0].Name;
                        option.legend.data[0] = data.chart.SeriesData[0].Name;
                        mapName = data.chart.ExpandProp;

                        myChart.hideLoading();
                        myChart.setOption(option);
                    }
                }, error: function () { }
            });            

        }, 500);

    }

});