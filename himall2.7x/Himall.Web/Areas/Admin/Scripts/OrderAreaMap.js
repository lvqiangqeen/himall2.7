
$(function(){
// --- 地图 ---
var myChart;
var loadingTicket;

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
	openLoading
);
	

function openLoading(echarts) {


    myChart = echarts.init(document.getElementById('mainMap'));
    myChart.showLoading({
        text: '正在加载图表...',
        effect: 'bubble',
        textStyle: {
            fontSize: 20
        }
    });
    clearTimeout(loadingTicket);
    loadingTicket = setTimeout(function () {
        myChart.hideLoading();
        myChart.setOption(option);
    }, 1200);
	
	
	function toDecimal2(x) {
		var f = parseFloat(x);
		if (isNaN(f)) {
			return false;
		}
		var f = Math.round(x * 100) / 100;
		var s = f.toString();
		var rs = s.indexOf('.');
		if (rs < 0) {
			rs = s.length;
			s += '.';
		}
		while (s.length <= rs + 2) {
			s += '0';
		}
		return s;
	}
	function LoadChart() {
	    var loading = showLoading();
		ajaxRequest({
			type: 'GET',
			url: "./GetAreaMapBySearch",
			param: { dimension: $("button.active").val(), year: $("#year option:selected").val(), month: $("#month option:selected").val() },
			dataType: "json",
			success: function (data) {
			    loading.close();
				if (data.successful == true) {
					option.dataRange.min = data.chart.RangeMin == 0 ? 0 : data.chart.RangeMin - 1;
					option.dataRange.max = data.chart.RangeMax;
					option.series[0].data = [];
					if (null != data.chart.Series) {
	
						option.series[0].name = data.chart.Series.Name;
						for (var i = 0; i < data.chart.Series.Data.length; i++) {
						    var name = data.chart.Series.Data[i].name;
							//for (var j = 0; j < province.length; j++) {
							//	if (province[j].id == data.chart.Series.Data[i].name) {
							//		name = province[j].name;
							//		var reg = /省$/gi;
							//		name = name.replace(reg, "");
							//	}
							//}
							var serData = { name: name, value: data.chart.Series.Data[i].value };
							option.series[0].data.push(serData);
						}
					}
					var type = parseInt($("button.active").val());
					var sum = 0;
					for (var i = 0; i < option.series[0].data.length; i++) {
						sum += option.series[0].data[i].value;
					}
					switch (type) {
						case 1: $("#diaplayName").text('下单客户数: '); $("#displayValue").text(sum + ' 个'); break;
						case 2: $("#diaplayName").text('下单量: '); $("#displayValue").text(sum + ' 个'); break;
						case 3: $("#diaplayName").text('下单金额: '); $("#displayValue").text(toDecimal2(sum )+ ' 元'); break;
					}
	
				}
			}, error: function () {
			    loading.close(); }
		});
	}
	
	LoadChart();
	$("#SearchBtn").click(function () {
		if (myChart && myChart.dispose) {
			myChart.dispose();
		}
		LoadChart();
		openLoading(echarts);
	});

	$(".btn-group button").click(function () {
		$(".btn-default").each(function () {
			$(this).removeClass('active');
		});
		$(this).addClass('active');
		LoadChart();
		openLoading(echarts);
	});
	

}
var option = {
    tooltip: {
        trigger: 'item', formatter: function (params, ticket, callback) {
            var type = parseInt($("button.active").val());
            var t1 = params[0].length == 0 ? (type == 3 ? "下单金额" : (type == 2 ? "下单量" : "下单客户数")) : params[0];
            var t2 = params[1] + '  :  <b style="color:yellow;font-size:14px;">' + params[2] + '</b>  ' + (type == 3 ? "元" : "个");
            var html = ['<div style="text-align:left;">', t1, '<br />', t2, '</div>'];

            return html.join('');
        }
    },
    dataRange: {
        min: 0,
        max: 1,
        x: 'left',
        y: 'bottom',
        text: ['高', '低'],           // 文本，默认为数值文本
        calculable: true
    },
    series: [
        {
            type: 'map',
            mapType: 'china',
            roam: false,
            itemStyle: {
                normal: {
                    borderWidth: 1,
                    borderColor: '#FFFFFF',
                    color: '#DEA6ED',
                    label: {
                        show: false
                    }
                }
            },
            data: [
            ]
        }
    ]
};

});


function ExportExeclByArea() {
    var year = $("#year option:selected").val();
    var month = $("#month option:selected").val();
    var href = "/Admin/Statistics/ExportAreaMap?year=" + year + "&month=" + month;
    $("#aAreaMapExport").attr("href", href);
}