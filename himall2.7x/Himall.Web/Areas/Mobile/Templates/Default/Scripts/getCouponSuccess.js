$(function() {
    loadingobj = showLoading("数据装载中...");
    var recordVal = $("#couponRecord").val();
    var geturl= '/' + areaName + '/vshop/GetWXCardData/';
    $.post(geturl, {id:recordVal}, function(data,status) {
        loadingobj.close();
        if(data.cardId){
            if(data.cardId != "0") {
                $("#addCard").show();
                $('#addCard').click(function() {
                    wx.addCard({
                        cardList: [{
                            cardId: data.cardId,
                            cardExt: jQuery.toJSON(data.cardExt)
                        }],
                        success: function(res) {
                            $("#addCard").hide();
                        }
                    });
                });
            }
        }
    });

});