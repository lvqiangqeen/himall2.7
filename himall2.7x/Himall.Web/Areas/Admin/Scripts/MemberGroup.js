$(function () {
    
    function memberGroupChart(el, option) {
        var myChart = echarts.init(document.getElementById(el));
        myChart.showLoading({
            text: '正在加载图表...',
            effect: 'bubble',
            textStyle: {
                fontSize: 20
            }
        });
        setTimeout(function () {
            myChart.hideLoading();
            myChart.setOption(option);
        }, 1200);
        myChart.on(config.EVENT.CLICK, eConsole);
    }
    
    function eConsole(param) {      
        if (typeof param.seriesIndex == 'undefined') {      
            return;      
        }      
        if (param.type == 'click') {    
            window.location.href = "/Admin/Member/ManagementPower?StatisticsType=" + param.data.type;
        }      
    }
    
    // 第一个
    memberGroupChart('product-rank', {
        series: [
            {
                name: '',
                type: 'pie',
                radius: '55%',
                center: ['50%', '60%'],
                data: [
                    { value: ActiveOne, name: ActiveOneProportion, itemStyle: { normal: { color: '#c13736' } },type:0 },
                    { value: ActiveThree, name: ActiveThreeProportion, itemStyle: { normal: { color: '#2f4553' } }, type: 1 },
                    { value: ActiveSix, name: ActiveSixProportion, itemStyle: { normal: { color: '#65a1a9' } }, type: 2 }
                ],
                itemStyle: {
                    emphasis: {
                        shadowBlur: 10,
                        shadowOffsetX: 0,
                        shadowColor: 'rgba(0, 0, 0, 0.5)'
                    }
                }
            }
        ]
    });
    
    // 第二个
    memberGroupChart('product-rank2', {
        series: [
            {
                name: '',
                type: 'pie',
                radius: '55%',
                center: ['50%', '60%'],
                data: [
                    { value: SleepingThree, name: SleepingThreeProportion, itemStyle: { normal: { color: '#c13736' } }, type: 10 },
                    { value: SleepingSix, name: SleepingSixProportion, itemStyle: { normal: { color: '#2f4553' } }, type: 11 },
                    { value: SleepingNine, name: SleepingNineProportion, itemStyle: { normal: { color: '#65a1a9' } }, type: 12 },
                    { value: SleepingTwelve, name: SleepingTwelveProportion, itemStyle: { normal: { color: '#d38267' } }, type: 13 },
                    { value: SleepingTwentyFour, name: SleepingTwentyFourProportion, itemStyle: { normal: { color: '#b7ce8d' } }, type: 14 }
                ],
                itemStyle: {
                    emphasis: {
                        shadowBlur: 10,
                        shadowOffsetX: 0,
                        shadowColor: 'rgba(0, 0, 0, 0.5)'
                    }
                }
            }
        ]
    });
    
    // 第三个
    memberGroupChart('product-rank3', {
        series: [
            {
                name: '',
                type: 'pie',
                radius: '55%',
                center: ['50%', '60%'],
                data: [
                    { value: BirthdayToday, name: BirthdayTodayProportion, itemStyle: { normal: { color: '#c13736' } }, type: 100 },
                    { value: BirthdayToMonth, name: BirthdayToMonthProportion, itemStyle: { normal: { color: '#2f4553' } }, type: 101 },
                    { value: BirthdayNextMonth, name: BirthdayNextMonthProportion, itemStyle: { normal: { color: '#65a1a9' } }, type: 102 }
                ],
                itemStyle: {
                    emphasis: {
                        shadowBlur: 10,
                        shadowOffsetX: 0,
                        shadowColor: 'rgba(0, 0, 0, 0.5)'
                    }
                }
            }
        ]
    });
    
    // 第四个
    memberGroupChart('product-rank4', {
        series: [
            {
                name: '',
                type: 'pie',
                radius: '55%',
                center: ['50%', '60%'],
                data: [
                    { value: RegisteredMember, name: '100%', type: 1000 }
                ],
                itemStyle: {
                    emphasis: {
                        shadowBlur: 10,
                        shadowOffsetX: 0,
                        shadowColor: 'rgba(0, 0, 0, 0.5)'
                    }
                }
            }
        ]
    });
    
});