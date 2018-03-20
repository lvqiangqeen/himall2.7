$(function () {
    var loadingTicket;
    var myChart;
    var mapName;
    option = {
        tooltip: {
            trigger: 'axis',
            formatter: function (params, ticket, callback) {
                var type = $("button.active").val();
                var html = '';
                var t1 = '<span style="text-align:left;">'+ params[0][1]+'销售额：<b style="color:yellow;font-size:14px;">' + params[0][2] + '元</b></span>';
                html = ['<div style="text-align:left;">', t1, '</div>'];
                return html.join('');
            }
        },
        legend: {
            data: ['店铺销售额']
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
                type: 'line',
                data: [],
                smooth: true,
                symbol: 'emptyCircle',
                markPoint: {
                    data: [
                        { type: 'max', name: '最高' },
                        { type: 'min', name: '最低' }
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
        load
    );


    function load(echarts) {
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
            var dimension = $("#dimensionType .active").attr("data");

            var myurl = "/SellerAdmin/Billing/GetSevenDaysTradeChart";

            if (dimension == 1) {
                myurl = "/SellerAdmin/Billing/GetSevenDaysTradeChart";
            }
            else if (dimension == 2) {
                myurl = "/SellerAdmin/Billing/GetThirdtyDaysTradeChart";
            }
            else
                if (dimension == 3) {
                    myurl = "/SellerAdmin/Billing/GetTradeChartMonthChart";
                }

            var loading = showLoading();
            ajaxRequest({
                type: 'Post',
                url:myurl,
                param: {},
                dataType: "json",
                success: function (data) {
                    loading.close();
                    if (data.successful == true) {
                        option.series[0].data = [];
                        option.xAxis[0].data = data.chart.XAxisData;
                        option.series[0].data = data.chart.SeriesData[0].Data;
                        option.series[0].name = data.chart.SeriesData[0].Name;
                        // option.legend.data[0] = data.chart.SeriesData[0].Name;
                        mapName = data.chart.ExpandProp;
                        myChart.hideLoading();
                        myChart.setOption(option);
                    }
                }, error: function () {
                    loading.close();
                }
            });
        }, 500);



        $("#SearchBtn").click(function () {
            //(new function (editor.doc.getValue()))();
            if (myChart && myChart.dispose) {
                myChart.dispose();
            }
            load(echarts);
        });

        $("#dimensionType li").click(function () {
            $("#dimensionType li").each(function () {
                $(this).removeClass('active');
            });
            $(this).addClass('active');
            load(echarts);
        });

    }



    //$(".start_datetime").click(function () {
    //    $('.end_datetime').datetimepicker('show');
    //});



});

