
//排序
$("#otherProducts").on("click", '.glyphicon-circle-arrow-up', function () {
    var p = $(this).parents('tr');
    var index = p.parent().find('tr').index(p);
    if (index == 0)
        return false;
    else
        p.prev().before(p);

});
$("#otherProducts").on("click", '.glyphicon-circle-arrow-down', function () {
    var p = $(this).parents('tr');
    var count = p.parent().find('tr').length;
    var index = p.parent().find('tr').index(p);
    if (index == (count - 1))
        return false;
    else
        p.next().after(p);
});
$("#otherProducts").on("click", '.glyphicon', function () {
    $(this).parents('tbody').find('.glyphicon').removeClass('disabled');
    $(this).parents('tbody').find('tr').first().find('.glyphicon-circle-arrow-up').addClass('disabled');
    $(this).parents('tbody').find('tr').last().find('.glyphicon-circle-arrow-down').addClass('disabled');
});
var siblingsVal;
//$('.skutd').each(function () {
//    $('input',this).eq(1).keyup(function () {
        
//    });
//})

$(document).on('keyup', '.skutd input', function () {
    siblingsVal = $(this).siblings().val();
    this.value = this.value.replace(/[^0-9.]/g, '');
    var arr = this.value.split('.');
    if (arr.length > 2) {
        this.value = arr[0] + "." + arr[1];
    } else if (arr.length == 2) {
        this.value = arr[0] + "." + arr[1].substring(0,2);
    }
    
    if (this.value > parseFloat(siblingsVal)) {
        this.value = siblingsVal;
    }
})

$(document).on('change', '.skutd input', function () {
    if (this.value == '' || this.value <= 0) {
        this.value = '1';
    }
})


function Remove(obj, proid) {
    $(obj).parents("tr").remove();
    if (proid) {
        otherIds.remove(proid);
    }
    else {
        $("#MainProductId").val("");
    }
}
var center = "<span class='glyphicon glyphicon-circle-arrow-up'></span> <span class='glyphicon glyphicon-circle-arrow-down'></span>";
function CreateMainSkuTable(selectedProducts) {

    var t = selectedProducts[0].id;

    var other = $("#OtherTable tr[data-pid='" + t + "']");
    if (other.length > 0)
    {
        $.dialog.errorTips('主商品不能和附商品相同！');
        return;
    }

    var table = "<table id='MainTable' class='table  table-bordered tabel-list'>";
    var title = "<thead><tr><th>主商品</th><th>规格</th><th>原价格/组合价格</th></tr></thead><tbody>";
    var td = "<tr><td><a href='/product/detail/" + selectedProducts[0].id + "' target='_blank' title=" + selectedProducts[0].name + "><img src='" + selectedProducts[0].imgUrl + "'></img></a></td><td>";
    $(selectedProducts[0].skus).each(function (index, item) {
        td += "<p>" + (item.Color == null ? "" : item.Color) + " " + (item.Size == null ? "" : item.Size) + " " + (item.Version == null ? "" : item.Version) + "</p>";
    });
    td += "</td>";
    td += "<td>";
    $(selectedProducts[0].skus).each(function (index, item) {
        td += "<p class='skutd' data-pid='" + selectedProducts[0].id + "' data-skuid=" + item.Id + "><input type='text' readonly value=" + item.SalePrice + "> <input type='text'   value=" + item.SalePrice + "></p>";
    });
    td += "</td></tr></tbody></table>";
    var htmlTable = table + title + td; //<td><span data=" + selectedProducts[0].id + " onclick='Remove(this)'>删除</span></td>一个就重新选不需要删除
    $("#mainProducts").html(htmlTable);
}

function CreateOtherSkuTable(selectedProducts) {
    otherIds = [];
    var mainpid = $("#MainProductId").val();
    if (selectedProducts.length == 0) { $("#otherProducts").html(""); return; };
    var table = "<table id='OtherTable' class='table table-bordered tabel-list'>";
    var title = "<thead><tr><th>搭配商品</th><th>规格</th><th>原价格/组合价格</th><th>排序</th><th>操作</th></tr></thead><tbody>";
    var td = "";
    $(selectedProducts).each(function (i, pro) {
        if (pro.id == mainpid) { return true; }
        otherIds.push(pro.id);
        td += "<tr  class='otherTR' data-pid=" + pro.id + "><td><a href='/product/detail/" + pro.id + "' target='_blank' title=" + pro.name + "><img src='" + pro.imgUrl + "'></img></a></td><td>";
        $(pro.skus).each(function (index, item) {
            td += "<p>" + (item.Color == null ? "" : item.Color) + " " + (item.Size == null ? "" : item.Size) + " " + (item.Version == null ? "" : item.Version) + "</p>";
        });
        td += "</td>";
        td += "<td>";
        $(pro.skus).each(function (index, item) {
            td += "<p class='skutd' data-skuid=" + item.Id + "><input type='text' value=" + item.SalePrice + " readonly> <input type='text' value=" + item.SalePrice + "></p>";
        });
        td += "<td class='swaptd'>" + center + "</td></td><td><span class='btn-a'><a onclick='Remove(this," + pro.id + ")'>删除</a></span></td></tr>";
    });
    td += "</tbody></table>";
    var htmlTable = table + title + td;


    $("#otherProducts").html(htmlTable);
    $("#OtherTable tbody tr").first().find('.glyphicon-circle-arrow-up').addClass('disabled');
    $("#OtherTable tbody tr").last().find('.glyphicon-circle-arrow-down').addClass('disabled');
}

function CollotionInfo() {
    var collocation = {
        Id: 0,
        Title: "测试",
        StartTime: "2015-09-02T11:31:49",
        EndTime: "2015-09-30T00:00:00",
        ShortDesc: "描述",
        ShopId: 1,
        CreateTime: "2015-09-02",
        CollocationPoruducts: []
    };
    function collocationProduct() {
        this.ProductId = 123;
        this.IsMain = true;
        this.DisplaySequence = 1;
        this.CollocationSkus = [];
    }
    function Sku() {
        this.ProductId = 123,
          this.SkuID = "123_0_73_76",
        //this.ColloProductId =0,
             this.Price = 1,
           this.SkuPirce = 0.5
    }
    collocation.Title = $("#Title").val();
    collocation.Id = $("#collocationId").val();
    collocation.StartTime = $("#StartTime").val();
    collocation.EndTime = $("#EndTime").val();
    collocation.ShortDesc = $("#ShortDesc").val();
    var mainProduct = new collocationProduct();

    mainProduct.ProductId = $("#MainTable").find(".skutd").eq(0).data("pid");
    mainProduct.IsMain = true;
    mainProduct.DisplaySequence = 0;
    $("#MainTable").find(".skutd").each(function (index, item) {
        var mainSku = new Sku();
        mainSku.ProductId = $(item).data("pid");
        mainSku.SkuID = $(item).data("skuid");
        mainSku.Price = $(item).find("input").eq(1).val();
        mainSku.SkuPirce = $(item).find("input").eq(0).val();
        mainProduct.CollocationSkus.push(mainSku);
    });
    collocation.CollocationPoruducts.push(mainProduct);
    $(".otherTR").each(function (index, item) {
        var otherProduct = new collocationProduct();
        otherProduct.ProductId = $(item).data("pid");
        otherProduct.IsMain = false;
        otherProduct.DisplaySequence = ++index;
        $(item).find(".skutd").each(function (i, skuItem) {
            var otherSku = new Sku();
            otherSku.ProductId = otherProduct.ProductId
            otherSku.SkuID = $(skuItem).data("skuid");
            otherSku.Price = $(skuItem).find("input").eq(1).val();
            otherSku.SkuPirce = $(skuItem).find("input").eq(0).val();
            otherProduct.CollocationSkus.push(otherSku);
        });
        collocation.CollocationPoruducts.push(otherProduct);
    });
    return collocation;
}

function CheckCollocation() {
    var title = $("#Title").val().trim();
    var startTime = $("#StartTime").val();
    var endTime = $("#EndTime").val();
    var regDate = /^\d{4}(\-|\.)\d{2}(\-|\.)\d{2}$/;
    if (title.length <=0) {
        $.dialog.errorTips('组合购标题不能为空！');
        return false;
    }
    else if ($("#MainTable .skutd").length == 0) {
        $.dialog.errorTips('没有选择主商品！');
        return false;
    }
    else if ($(".otherTR").length == 0) {
        $.dialog.errorTips('没有选择附加商品！');
        return false;
    }
    else if ($(".otherTR").length > 9) {
        $.dialog.errorTips('附加商品最多允许9个！');
        return false;
    }
    else if (!regDate.test(startTime) || !regDate.test(endTime)) {
        $.dialog.errorTips('开始日期或者结束日期格式错误！');
        return false;
    }
    else if ($("#StartTime").val() > $("#EndTime").val()) {
        $.dialog.errorTips('开始时间不能大于结束时间！');
        return false;
    }
    return true;
}


function PostCollocation() {
    if (!CheckCollocation()) return;
    var collocationjson = CollotionInfo();
    if (collocationjson == null) return;
    var objectString = JSON.stringify(collocationjson);
   
    $.post('./AddCollocation', { collocationjson: objectString }, function (result) {

        if (result.success) {
            $.dialog.tips('保存成功', function () { location.href = './management'; });
        }
        else
            $.dialog.tips('保存失败！' + result.msg);
    }, "json");
}
function EditCollocation() {
    if (!CheckCollocation()) return;
    var collocationjson = CollotionInfo();
    if (collocationjson == null) return;
    var objectString = JSON.stringify(collocationjson);

    $.post('/SellerAdmin/Collocation/EditCollocation', { collocationjson: objectString }, function (result) {

        if (result.success) {
            $.dialog.tips('保存成功', function () { location.href = '/SellerAdmin/Collocation/management'; });
        }
        else
            $.dialog.tips('保存失败！' + result.msg);
    }, "json");
}