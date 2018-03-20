$(function(){
var loadingTicketShop;
var myChartShop;
var mapNameShop;
optionShop = {
    tooltip: {
        trigger: 'axis',
        formatter: function (params, ticket, callback) {
            var type = 1;
            var html = '';
            if (1 == type) {
                var t1 = '<span style="text-align:left;">店铺：<b style="color:yellow;font-size:14px;">' + mapNameShop[params[0][1] - 1] + '</b></span>';
                var t2 = '<span style="text-align:left;">订单量：<b style="color:yellow;font-size:14px;">' + params[0][2] + '</b>个</span>';
                html = ['<div style="text-align:left;">', t1, '<br />', t2, '</div>'];
            } else {
                var t1 = '<span style="text-align:left;">店铺：<b style="color:yellow;font-size:14px;">' + mapNameShop[params[0][1] - 1] + '</b></span>';
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
        show: true,
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
            type: 'bar',
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
		'echarts/chart/map'
	],
	function (echarts) {
		myChartShop = echarts.init(document.getElementById('main2'));
		myChartShop.showLoading({
			text: '正在加载图表...',
			effect: 'bubble',
			textStyle: {
				fontSize: 20
			}
		});
		clearTimeout(loadingTicketShop);
		loadingTicketShop = setTimeout(function () {
			ajaxRequest({
				type: 'GET',
				url: "./GetRecentMonthShopSaleRankChart",
				param: {},
				dataType: "json",
				success: function (data) {
					if (data.successful == true) {
						optionShop.series[0].data = [];
						optionShop.xAxis[0].data = data.chart.XAxisData;
						optionShop.series[0].data = data.chart.SeriesData[0].Data;
						optionShop.series[0].name = data.chart.SeriesData[0].Name;
						optionShop.legend.data[0] = data.chart.SeriesData[0].Name;
						mapNameShop = data.chart.ExpandProp;
	
						myChartShop.hideLoading();
						myChartShop.setOption(optionShop);
					}
				}, error: function () { }
			});
		}, 500);
		
	}
);
});