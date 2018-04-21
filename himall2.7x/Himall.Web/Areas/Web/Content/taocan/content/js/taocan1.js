$(function () {
    //banner images
    (function () {
        var bannerImgArr = $(".banner_img>img");
        var bannerItemArr = $(".banner_img_item>span");
        var len = bannerImgArr.length,
            index = 0;
        setInterval(switchBannerImg, 1000 * 2);

        function switchBannerImg() {
            for (var i = 0; i < len; i++) {
                $(bannerImgArr[i]).hide();
                $(bannerItemArr[i]).removeClass('cur')
            }
            $(bannerImgArr[index]).show();
            $(bannerItemArr[index]).addClass('cur');

            if (index < len - 1) {
                index++;
            } else {
                index = 0;
            }
        }
    })();

    (function () {
        var zhuCaiImgArr = $("#taocan_zhucai>div.taocan2_imgs>img");
        var len = zhuCaiImgArr.length, index = 0;
        $("#taocan_zhucai>div.next").click(function () {
            index = Math.min(++index, len - 1);
            for (var i = 0; i < len; i++) {
                $(zhuCaiImgArr[i]).hide();
            }
            $(zhuCaiImgArr[index]).show();

        });
        $("#taocan_zhucai>div.prev").click(function () {
            index = Math.max(--index, 0);
            for (var i = 0; i < len; i++) {
                $(zhuCaiImgArr[i]).hide();
            }
            $(zhuCaiImgArr[index]).show();
        });
    })();

    //single
    (function () {
        var singleImgArr = $("#taocan_single>div.taocan2_imgs>img");
        var len = singleImgArr.length, index = 0;
        $("#taocan_single>div.next").click(function () {
            index = Math.min(++index, len - 1);
            for (var i = 0; i < len; i++) {
                $(singleImgArr[i]).hide();
            }
            $(singleImgArr[index]).show();

        });
        $("#taocan_single>div.prev").click(function () {
            index = Math.max(--index, 0);
            for (var i = 0; i < len; i++) {
                $(singleImgArr[i]).hide();
            }
            $(singleImgArr[index]).show();
        });
    })();

    //brand
    (function () {
        var linkImgArr = $("#link_brand>div.taocan2_links>div");
        var len = linkImgArr.length, index = 0;
        $("#link_brand>div.next").click(function () {
            index = Math.min(++index, len - 1);
            for (var i = 0; i < len; i++) {
                $(linkImgArr[i]).hide();
            }
            $(linkImgArr[index]).show();

        });
        $("#link_brand>div.prev").click(function () {
            index = Math.max(--index, 0);
            for (var i = 0; i < len; i++) {
                $(linkImgArr[i]).hide();
            }
            $(linkImgArr[index]).show();
        });
    })();
})