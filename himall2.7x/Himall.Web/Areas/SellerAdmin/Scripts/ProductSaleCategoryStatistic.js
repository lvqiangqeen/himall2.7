var loadingTicket;
var myCountChart, myAmountChart;
var countOption = {};
var amountOption = {};
var defaultDate = new Date();
defaultDate.setDate(defaultDate.getDate() - 1);
$(function () {
    var legendData = $('#categoryStr').val().split(',');
    var countData = $('#countStr').val().split(',');
    var amountData = $('#amountStr').val().split(',');
    var countSeriesData = [];
    var amountSeriesData = [];
    for (var i = 0; i < legendData.length; i++) {
        countSeriesData.push({ value: countData[i], name: legendData[i] });
        amountSeriesData.push({ value: amountData[i], name: legendData[i] });
    }
    countOption = {
        title: {
            text: '一级分类商品销售数                                                                           一级分类商品销售金额',
            x: 'center',
            textStyle: {
                fontWeight: 'normal',
                color: '#616161',
                fontSize: '16',
            }
        },
        tooltip: {
            trigger: 'item',
            formatter: "{a} <br/>{b} : {c} ({d}%)"
        },
        legend: {
            orient: 'horizontal',
            bottom: 'bottom',
            icon: 'circle',
            left: 'center',
            data: legendData
        },
        series: [
            {
                name: '访问来源',
                type: 'pie',
                radius: '50%',
                center: ['25%', '50%'],
                data: countSeriesData,
                itemStyle: {
                    emphasis: {
                        shadowBlur: 10,
                        shadowOffsetX: 0,
                        shadowColor: 'rgba(0, 0, 0, 0.5)'
                    }
                }
            }, {
                name: '访问来源',
                type: 'pie',
                radius: '50%',
                center: ['75%', '50%'],
                data: amountSeriesData,
                itemStyle: {
                    emphasis: {
                        shadowBlur: 10,
                        shadowOffsetX: 0,
                        shadowColor: 'rgba(0, 0, 0, 0.5)'
                    }
                }
            }
        ],
        color: ["#ff7878", "#68c1b8", "#fdbf74", "#a4adbd", "#686d78"],
    };

    //bindChart(countOption);
    $('#defaultBtn').click();

    $('#inputStartDate').daterangepicker(
        {
            format: 'YYYY-MM-DD',
            startDate: defaultDate, endDate: defaultDate
        }
    ).bind('apply.daterangepicker', function (event, obj) {
        if ($('#inputStartDate').val() == "" || $('#inputStartDate').val() == "YYYY/MM/DD — YYYY/MM/DD") {
            return false;
        }
        var strArr = $('#inputStartDate').val().split("-");
        var startDate = strArr[0] + "-" + strArr[1] + "-" + strArr[2];
        var endDate = strArr[3] + "-" + strArr[4] + "-" + strArr[5];
        if (startDate != '' && endDate != '')
            LoadChartData(startDate, endDate);
        else {
            $.dialog.tips('请选择时间范围');
        }
    });
});

function bindChart(countOption) {
    myCountChart = echarts.init(document.getElementById('mainSaleCount'));

    myCountChart.showLoading({
        text: '正在加载图表...',
        effect: 'bubble',
        textStyle: {
            fontSize: 20
        }
    });


    clearTimeout(loadingTicket);
    loadingTicket = setTimeout(function () {
        myCountChart.hideLoading();
        myCountChart.setOption(countOption);
    }, 500);
}
function LoadChartData(date1, date2) {
    //(new function (editor.doc.getValue()))();
    if (myCountChart && myCountChart.dispose) {
        myCountChart.dispose();
    }
    var para = {};
    para.startDate = date1;
    para.endDate = date2;
    var loading = showLoading();
    ajaxRequest({
        type: 'GET',
        url: "./GetProductSaleCategoryStatistic",
        param: para,
        dataType: "json",
        success: function (data) {
            loading.close();
            if (data.success) {
                var legendData = [];
                var countSeriesData = [];
                var amountSeriesData = [];
                var salecounts = 0;
                var saleamounts = 0;
                for (var i = 0 ; i < data.model.length; i++) {
                    item = data.model[i];
                    legendData.push(item.CategoryName);
                    salecounts += item.SaleCounts;
                    saleamounts += item.SaleAmounts;
                    countSeriesData.push({ value: item.SaleCounts, name: item.CategoryName });
                    amountSeriesData.push({ value: item.SaleAmounts, name: item.CategoryName });
                }
                if (salecounts <= 0) {
                    $('#mainSaleCount').hide();
                    $('#mainTips').show();
                    $('#mainTips').html('<h3 class="table-tips">没有相关数据</h3>');
                }
                else {
                    $('#mainSaleCount').show();
                    $('#mainTips').hide();
                }
                countOption.legend.data = legendData;
                countOption.series[0].data = countSeriesData;
                countOption.series[1].data = amountSeriesData;
                bindChart(countOption);
            }
        }, error: function () {
            loading.close();
        }
    });
}