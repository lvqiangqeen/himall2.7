$(function () {  
    initUpdateQuantity();
    bindAddProductsBtn();
    clear();
});

function bindAddProductsBtn() {
    $('#AddProduct').click(function () {
        var shopId = $("#hdSellerId").val();
        $.post('/UserPurchase/GetItemsId', {}, function (data) {
            $.shopProductSelector.show(data, function (selectedProducts) {
                //var ids = [];
                //$.each(selectedProducts, function () {
                //    ids.push(this.id);
                //});

                var ids = selectedProducts;
                var loading = showLoading();
                $.post('/UserPurchase/AddItems', { productIds: ids.toString() }, function (data) {
                    loading.close();
                    if (data.success) {
                        initGrid();
                    }
                    else
                        $.dialog.errorTips(data.msg);
                });
            }, shopId, true);
        });

    });
}

function initGrid() {
    $.ajax({
        type: 'post',
        url: '/UserPurchase/GetSelectedItems',
        data: null,
        dataType: "json",
        success: function (data) {
            var html = [];
            if (data.rows == null) {
                html.push('<tr><td colspan="6"><div class="empty"><b></b>暂时没有选择任何产品</div></td></tr>');
            }
            else {               
                           
                $.each(data.rows, function (i, product) {
                    html.push('<tr class="tr-td">');                  
                    html.push('<td align="left"><a target="_blank" href="/product/Detail/' + product.ProductId + '"> <img width="50" height="50" title="" src="' + product.ThumbnailsUrl + '"/>' + product.ProductName + '</a></td>');
                    html.push('<td class="ftx-04">￥' + product.SalePrice + '</td>');
                    var spec = GetSkus(product.Id, product.ProductId, product.SkuId, 1);
                    html.push('<td>' + spec + '</td>');
                    html.push('<td><input class="itxt" type="text" style=" width:40px" name="quentity" hpId="' + product.Id + '" value="' + product.Quantity + '"></td>');
                    var annex = "";
                    if (product.Annex == "") {
                        annex += '<input class="hiddenAttaSrc" type="hidden" value="">';
                        annex += '<span class="spanAttaSrc"></span>';
                        annex += '<input class="file uploadFilebtn" type="file" name="_file" onchange="uploadfile(this,' + product.Id + ')" />';
                    }
                    else {
                        var index = product.Annex.lastIndexOf('/');
                        var attname = product.Annex.substring(index + 1);
                        annex += '<input class="hiddenAttaSrc" type="hidden" value="' + product.Annex + '">';
                        annex += '<span class="spanAttaSrc">' + attname + '<img src="/Images/del.png" onclick="delatta(this,' + product.Id + ');" /></span>';
                        annex += '<input class="file uploadFilebtn" type="file" name="_file" onchange="uploadfile(this,' + product.Id + ')" style="display:none;" />';
                    }
                    html.push('<td>' + annex + '</td>');
                    html.push('<td><span class="btn-a"><a class="good-check" onclick="del(' + product.Id + ')">删除</a></span></td>');
                    html.push('</tr>');
                });
            }
            $("#productList tbody").empty();
            $("#productList tbody").append(html.join(''));
        },
        error: function () {

        }
    });
}

function del(id) {
    $.dialog.confirm('确定要删除该商品吗?', function () {
        var loading = showLoading();
        $.post('/UserPurchase/DeleteItem', { id: id }, function (result) {
            loading.close();
            if (result.success) {
                initGrid();
            }
            else
                $.dialog.errorTips('删除失败！' + result.msg);
        });
    });
}

function clear()
{
    var loading = showLoading();
    $.post('/UserPurchase/ClearItems', {}, function (result) {
        loading.close();
        if (result.success) {
            initGrid();
       }

    });
}

function initUpdateQuantity() {
    $('#productList').on('blur', 'input[name="quentity"]', function () {
        var id = $(this).attr('hpId');
        var quantity = $(this).val();
        var quantity = parseInt(quantity);
        if (isNaN(quantity)) {
            $.dialog.errorTips('数字格式不正确');
            $(this).val(1);
        }
        else {
            var loading = showLoading();
            $.post('/UserPurchase/UpdateQuantity', { id: id, quantity: quantity }, function (data) {
                loading.close();
                if (data.success) {
                    // $.dialog.tips('更新数量成功');
                }
                else
                    $.dialog.errorTips('更新数量失败！' + result.msg);
            });
        }
    });
}

function uploadfile(target,id) {
    var attaupfile = $(target); //上传控件
    var AttaHideFile = $(target).parent().find("input:hidden").eq(0); //附件隐藏域
    var SpanAtta = $(target).parent().find(".spanAttaSrc").eq(0); //文件名显示span
    if (attaupfile.val() == "") {
        $.dialog.errorTips("请选择要上传的文件！");
        return false;
    }
    else {
        if (!checkImgType(attaupfile.val())) {
            $.dialog.errorTips("上传格式为gif、jpeg、jpg、png、bmp、txt、doc、docx、xls、xlsx、ppt、pptx、zip、rar", '', 3);
            return false;
        }
    }

    //准备表单
    var myform = document.createElement("form");
    myform.action = '/common/PublicOperation/UploadPic';
    myform.method = "post";
    myform.enctype = "multipart/form-data";
    myform.style.display = "none";
    //将表单加当document上，
    document.body.appendChild(myform);  //重点
    var form = $(myform);

    var fu = attaupfile.clone(true).val(""); //先备份自身,用于提交成功后，再次附加到span中。
    var fu1 = attaupfile.appendTo(form); //然后将自身加到form中。此时form中已经有了file元素。

    //开始模拟提交表当。
    form.ajaxSubmit({
        success: function (data) {
            if (data == "NoFile" || data == "Error" || data == "格式不正确！") {
                $.dialog.errorTips(data);

                $(fu1).insertAfter($("span.spanAttaSrc", $(AttaHideFile).parent()));
                $(fu1).show();
                form.remove();
            }
            else {
                //文件上传成功，返回图片的路径。将路经赋给隐藏域
                AttaHideFile.val(data);

                var index = data.lastIndexOf('/');
                var attname = data.substring(index + 1);
                SpanAtta.html(attname + '<img src="/Images/del.png" onclick="delatta(this,' + id + ');" />');
                updateAnnex(id, data);

                $(fu1).insertAfter($("span.spanAttaSrc", $(AttaHideFile).parent()));
                $(fu1).hide();
                form.remove();
            }
            
        }
    });
}

//检查上传的附件格式
function checkImgType(filename) {
    var pos = filename.lastIndexOf(".");
    var str = filename.substring(pos, filename.length)
    var str1 = str.toLowerCase();
    if (!/\.(gif|jpg|jpeg|png|bmp|txt|doc|docx|xls|xlsx|ppt|pptx|zip|rar)$/.test(str1)) {
        return false;
    }
    return true;
}

function delatta(obj, id) {
    $(obj).parent().parent().find("input:hidden").eq(0).val("");
    $(obj).parent().next().show();
    $(obj).parent().next().val("");
    $(obj).parent().html("");
    updateAnnex(id, "");
}

function updateAnnex(id,url)
{
    var loading = showLoading();
    $.post('/UserPurchase/UpdateAnnex', { id: id, url: url }, function (data) {
        loading.close();
        if (data.success) {
            // $.dialog.tips('更新数量成功');
        }
        else
            $.dialog.errorTips('更新数量失败！' + result.msg);
    });
}

function GetSkus(id,pid, value,t)
{
    var html = "";
    var selected = "";
    $.ajax({
        url: '/UserPurchase/GetSkus',
        type: 'post',
        async: false,
        data: { pid: pid },
        success: function (data) {
            if (data != null && data != undefined && data.length > 0) {
                var sel = [];
                sel.push('<select class="itxt" id="sel' + id + '" onchange="updateSkuid(' + id + ')">');
                sel.push('<option value="">请选择</option>');
                var skus = "";
                for (var i = 0; i < data.length; i++) {
                    var sku = "";
                    if (data[i].Color != "") {
                        sku += data[i].Color + ",";
                    }
                    if (data[i].Size != "") {
                        sku += data[i].Size + ",";
                    }
                    if (data[i].Version != "") {
                        sku += data[i].Version + ",";
                    }
                    skus += sku;
                    if (sku.length > 0) {
                        sku = sku.substring(0, sku.length - 1);
                        if (data[i].Id == value) {
                            sel.push('<option value="' + data[i].Id + '" stock="' + data[i].Stock + '" price="' + data[i].SalePrice + '" selected>' + sku + '</option>');
                            selected = sku;
                        }
                        else {
                            sel.push('<option value="' + data[i].Id + '" stock="' + data[i].Stock + '" price="' + data[i].SalePrice + '">' + sku + '</option>');
                        }
                    }
                    else
                        selected = "";
                }
                sel.push('</select>');
                if (t == 1) {
                    if (skus != "") {
                        html = sel.join('');
                    }
                    else {
                        html = "";
                        var loading = showLoading();
                        $.post('/UserPurchase/UpdateSkuId', { id: id, skuid: data[0].Id }, function (data) {
                            loading.close();
                        });
                    }
                    
                }
                else
                {
                    html = selected;
                }

            }
        }
    });
    return html;
}

function updateSkuid(id)
{
    var skuid = $("#sel" + id).val();
    var loading = showLoading();
    $.post('/UserPurchase/UpdateSkuId', { id: id, skuid: skuid }, function (data) {
        loading.close();
        if (data.success) {
            //initGrid();
            // $.dialog.tips('更新规格成功');
        }
        else
            $.dialog.alert('更新规格失败！' + result.msg);
    });
}

function initList() {
    $.ajax({
        type: 'post',
        url: '/UserPurchase/GetSelectedItems', //找不到调用的地方
        data: null,
        dataType: "json",
        success: function (data) {
            var html = [];
            if (data.rows == null) {
                html.push('<tr><td colspan="5"><div class="empty"><b></b>暂时没有选择任何产品</div></td></tr>');
            }
            else {
                $.each(data.rows, function (i, product) {
                    html.push('<tr class="tr-td">');
                    html.push('<td align="left"><a target="_blank" href="/product/Detail/' + product.ProductId + '"> <img width="50" height="50" title="" src="' + product.ThumbnailsUrl + '"/>' + product.ProductName + '</a></td>');
                    html.push('<td class="ftx-04">￥' + product.SalePrice + '</td>');
                    var spec = GetSkus(product.Id, product.ProductId, product.SkuId, 2);
                    html.push('<td>' + spec + '</td>');
                    html.push('<td>' + product.Quantity + '</td>');
                    var annex = "";
                    if (product.Annex != "") {
                        var index = product.Annex.lastIndexOf('/');
                        annex = product.Annex.substring(index + 1);
                    }

                    html.push('<td>' + annex + '</td>');

                    html.push('</tr>');
                });
            }
            $("#List tbody").empty();
            $("#List tbody").append(html.join(''));
        },
        error: function () {

        }
    });
    
}

function calculate(addressId)
{
    var receipt = $("input[name='receiptMode']:checked").val();
    $.post('/UserPurchase/CalcMoney', { type: receipt, addressId: addressId }, function (data) {
        $("#totalPrice").text(data[0]);
        $("#totalFreight").text(data[1]);
        $("#totalMoney").text(data[2]);
    });
}