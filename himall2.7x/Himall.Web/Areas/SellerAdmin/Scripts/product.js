var onGetAttributeed, onGetSpecificationed, lastTr, colorAlias, sizeAlias, versionAlias;
var freightType = "";
function BindfreightTemplate() {
    $.ajax({
        type: 'get'
       , url: '/selleradmin/product/GetFreightTemplate'
       , datatype: 'json'
       , success: function (result) {
           if (result.success) {
               for (var i = 0 ; i < result.model.length; i++) {
                   var opt = $('#FreightTemplateId').find('option[value=' + result.model[i].Value + ']');
                   if (opt.length == 0) {
                       $('#FreightTemplateId').append('<option value="' + result.model[i].Value + '">' + result.model[i].Text + '</option>');
                   }
               }
           }
       }
    });
}

function SetFreightType() {
    if (freightType == "1") {
        $('#divWeight').show();
        $('#divVolume').hide();
    }
    else {
        if (freightType == "2") {
            $('#divWeight').hide();
            $('#divVolume').show();
        }
        else {
            $('#divWeight').hide();
            $('#divVolume').hide();
        }
    }
};

(function ($) {
    CategoryChange();
    $("select[name='GoodsCategory']").change();
    $("#addFreightTemplate").click(function () {
        $("#addFreightTemplate").attr("target", "_blank");
        $("#addFreightTemplate").attr("href", "/SellerAdmin/FreightTemplate/Add?displayUrl=/SellerAdmin/FreightTemplate/Index&tar=freighttemplate");
        ///SellerAdmin/freighttemplate/add?displayUrl=/SellerAdmin/FreightTemplate/Index
    })
    freightType = $("#CurFTVM").val();
    SetFreightType();
    $('#FreightTemplateId').bind('change', function () {
        var selectText = $('#FreightTemplateId').find('option:selected').text();
        if (selectText.indexOf('【按体积】') > 0) {
            freightType = '2';
        }
        else {
            if (selectText.indexOf('【按重量】') > 0) {
                freightType = '1';
            }
            else {
                freightType = '0';
            }
        }
        SetFreightType();
    });

    //添加自定义验证方法
    $.validator.addMethod('picrequired', function (value, element, param) {
        var result = false;

        $(element).parent().find('.upload_pic').each(function () {
            if ($.notNullOrEmpty($('input', this).val()))
                result = true;
        });

        return result;
    }, '请至少上传一张商品图片');

    $.validator.addMethod('desrequired', function (value, element, param) {
        var des = $('textarea[name="Description.Description"]').val();
        var mdes = $('textarea[name="Description.MobileDescription"]').val();

        if ($.isNullOrEmpty(des))
            return false;

        if ($.isNullOrEmpty(mdes)) {
            $(element).closest('.des').find('.nav-tabs .mdes').click();
            return false;
        }

        return true;
    }, '请输入商品描述');

    window.productInit = function () {
        var txtCategoryPath = $('#txtCategoryPath');
        var txtTypeId = $('#txtTypeId');
        var categoryId = $('#categoryId');
        var categoryDiv = $('#categoryDiv');
        var selectCategoryPath = $('#selectCategoryPath');
        var specificationsDiv = $('#specificationsDiv');

        //初始化富文本框
        UE.getEditor('desContainer');
        UE.getEditor('mdesContainer');

        var $form = $('form:first');
        //绑定属性
        onGetAttributeed = function (categoryId, data) {
            if (categoryId != defaultCategoryId)
                return;

            for (var i = 0; i < data.json.length; i++) {
                var item = data.json[i];
                for (var j = 0; j < item.AttrValues.length; j++) {
                    var attr = item.AttrValues[j];
                    if (selectAttributes.any(function (p) { return p.attributeId == item.AttrId && p.valueId == attr.Id; }))
                        attr.selected = true;
                }
            }
        };

        var skus;
        //绑定规格
        onGetSpecificationed = function (categoryId, data) {
            if (categoryId != defaultCategoryId)
                return;

            for (var i = 0; i < data.length; i++) {
                var item = data[i];
                var ids = selectSKUInfos[item.Specification.Name.toLowerCase() + 'Id'];
                for (var j = 0; j < item.Values.length; j++) {
                    var spec = item.Values[j];
                    if (ids.contains(spec.Id))
                        spec.checked = true;
                }
            }
        };

        //更改错误显示
        var validate = $form.validate();
        //覆盖默认的验证成功和验证失败事件
        if (validate) {
            validate.settings.success = function (label, element) {
                $(element).parent().removeClass('has-error');
                label.remove();
            };

            validate.settings.errorPlacement = function (error, element) {
                element.parent().addClass('has-error');
            };
        }
        $('#createCategory').click(function () {
            var a = $(this);
            var tbody = a.prev().find('tbody');
            var tr = tbody.find('tr:last').clone();
            if (tr.length == 0) {
                tr = lastTr;
            }
            if (tr.find("option[value!=0]:not(:disabled)").length == 1) {
                $.dialog.alertTips('没有可选的分类！');
                return;
            }
            tr.find('select').val('');
            tr.find("select option:selected").removeAttr("selected"); //移除属性selected
            tbody.append(tr);
            CategoryChange();//每次创建新项后绑定事件
            $("select[name='GoodsCategory']").change();
        });

        $('#tableCategory').on('click', 'tr td .del', function () {
            lastTr = $('#cateBody').find('tr:last').clone();
            $(this).closest('tr').remove();
            CategoryChange();
            $("select[name='GoodsCategory']").change();
        });

        //上一步或下一步
        $('.btn.step').click(function () {
            var step = $(this).attr('step');
            var li = $('li.' + step);
            if (li.hasClass('active'))
                return;

            var categoryIdValue = categoryId.val();
            if ($.isNullOrEmpty(categoryIdValue) || categoryIdValue == '0') {
                $.dialog.alertTips('请选择平台分类！');
                return;
            }

            if ($(this).hasClass('needValid')) {
                if (!$form.valid()) {
                    $.dialog.alertTips('必填项没有填写完成，请检查数据是否填写完整！');
                    return;
                }
            }

            $('li.step.active').removeClass('active');
            $('li.step.' + step).addClass('active');

            $(this).closest('.form-horizontal.step').addClass('hidden');
            $('.form-horizontal.step.' + step).removeClass('hidden');
        });

        //详情描术选项卡切换
        $('.nav.nav-tabs li').click(function () {
            var li = $(this);
            if (li.hasClass('active'))
                return;
            li.parent().children('li').toggleClass('active');
            li.parent().next('.tab-content').children('.tab-pane').toggleClass('active');
        });

        //当图片更改时sku表格动态生成对应的隐藏控件
        $('#skusDiv').on('change.usesku', 'table.img tbody .upload_pic input', skuPicChange);

        //开启规格复选框更改时
        $('#ckbSpecifications').change(function () {
            $('#changeSpecification').toggleClass('hidden');
            if (this.checked == true) {
                $('#txtMallPrice').attr('readonly', 'readonly');
                $('#txtStock').attr('readonly', 'readonly');
                $('#changeSpecification').click();
                $('.form-group.safestock').addClass('hidden');
            }
            else {
                $(skusDiv).html('');
                $('#txtMallPrice').removeAttr('readonly');
                $('#txtStock').removeAttr('readonly');
                $('.form-group.safestock').removeClass('hidden');
                $(".j_spec").hide();
            }
        });

        //开启规格后，弹出层的规格复选框更改时
        $('#specificationsDiv').on('change', 'input:checkbox', function () {
            if (this.checked)
                $(this).next().removeAttr('disabled');
            else
                $(this).next().attr('disabled', 'disabled');
        });

        //第一步选择分类时
        $('.pub-inline ul li', categoryDiv).click(function () {
            var li = $(this);
            if (li.hasClass('selected'))
                return;

            var div = li.closest('.pub-inline');
            $('li.selected', div).removeClass('selected');
            li.addClass('selected');

            var id = li.attr('id');
            div = div.next();
            if (div.length == 0) {//表示选择最后一个分类
                $('input[name="CategoryId"]').val(id);
                txtCategoryPath.val(li.attr('path'));
                var txtTypeIdValue = li.attr('typeId');
                txtTypeId.val(txtTypeIdValue);

                getSpecification(id, txtTypeIdValue);
                generateBrand(id);
                generateAttr(id, txtTypeIdValue);

                selectCategoryPath.html('');
                $('.pub-inline ul li.selected', categoryDiv).each(function (i) {
                    if (i > 0)
                        selectCategoryPath.append('>');
                    selectCategoryPath.append($(this).text().trim());
                });

                return;
            }

            $('#brand').html('<option value="0">请选择品牌</option>');
            $('#attrDiv').html('');

            $('input[name="CategoryId"]').val('');
            txtCategoryPath.val('');
            txtTypeId.val('');

            $('li:not(.hidden),li[pid={0}]'.format(id), div).toggleClass('hidden');
            $('li.selected', div).removeClass('selected');

            while (div.next().length > 0) {
                div = div.next();
                $('li:not(.hidden)'.format(id), div).toggleClass('hidden');
                $('li.selected', div).removeClass('selected');
            }
        });

        //点击‘修改规格’
        $('#changeSpecification').click(function () {
            $.dialog({
                title: '开启规格',
                id: 'addSpec',
                width: 620,
                content: specificationsDiv.get(0),
                lock: true,
                padding: '20px 10px 15px',
                okVal: '确认生成',
                ok: function () {//确认时生成表格
                    var spes = [];
                    var changeSpecs = [];
                    var haserror = false;
                    $('input:checkbox:checked', specificationsDiv).each(function () {
                        var me = this;
                        var $me = $(me);
                        var _p = $me.parent();
                        _p.removeClass("v-error");
                        var group = $me.closest('.group');
                        var specification = group.data('spec');
                        var value = { id: $me.attr('id'), value: $me.next().val(), oldValue: $me.val() };

                        if (value.value.length < 1) {
                            _p.addClass("v-error");
                            haserror = true;
                        } else {
                            if (value.value != value.oldValue)
                                changeSpecs.push({ id: value.id, specification: specification.Value, value: value.value });

                            var item = spes.first(function (p) { return p.specification.Value == specification.Value; })
                            if ($.isNullOrEmpty(item))
                                spes.push({ specification: specification, values: [value] });
                            else
                                item.values.push(value);
                        }
                    });
                    if (haserror) {
                        $.dialog.errorTips('规格值不可以为空！');
                        return false;
                    }
                    if (spes.length == 0) {
                        $(".j_spec").hide();
                        $(skusDiv).html('');
                        $('#txtMallPrice').removeAttr('readonly');
                        $('#txtStock').removeAttr('readonly');
                        $('.form-group.safestock').removeClass('hidden');
                        return;
                    }
                    //生成SKU设置表格
                    generateSKUTable(spes);
                    //保存商家修改后的规格
                    generateSellerSpec(changeSpecs);
                    $(".j_spec").show();
                }
            });
        });

        //上传图片按钮
        $('body').on('click', '.upload.pic', function () {
            var me = $(this);
            var div = me.parent().find('.upload_pic');
            var resetBtn = me.parent().find('.reset.pic');
            var delBtn = me.parent().find('.del');

            $.uploadImage({
                url: '/common/publicOperation/UploadPic',
                maxSize: 2,
                success: function (result) {
                    if (result) {
                        resetBtn.removeClass('hidden');
                        delBtn.removeClass('hide');
                        div.css('background-image', 'url("' + result + '")').find('input:hidden').val(result).change();
                    }
                },
                error: function () {
                    $.dialog.alertTips('图片上传出错,请重新上传！');
                }
            }).upload();
        });

        $('body').on('click', '.reset.pic', function () {
            var me = $(this);
            var div = me.parent().find('.upload_pic');
            div.css('background-image', '').find('input:hidden').val('').change();
            me.addClass('hidden');
        });
        //快速设置功能
        $('body').on('click', '.batch-attr-setting .btn', function () {
            $(this).closest('.batch-attr-setting').find('input').each(function () {
                var $this = $(this);
                var forName = $this.attr('for');
                var value = $this.val();
                if ($.notNullOrEmpty(value))
                    $('table.sku [name$=".{0}"]'.format(forName)).val(value).change();
            });
        });

        //加入草稿箱
        $('#inDraft').click(function () {
            $(this).after('<input type="hidden" name="SaleStatus" value="InDraft"/>');
            $('#submit').click();
            return false;
        });

        $('#submit').click(function () {
            //生成属性hidden input
            var attrIndex = 0;
            var attrDiv = $('#attrDiv');
            $('input[name^=SelectAttributes]', attrDiv).remove();
            $('tr', attrDiv).each(function () {
                var tr = $(this);
                var attrValues = [];
                var dropdown = $('td:last .dropdown', tr);
                if (dropdown.length > 0) {
                    attrValues = dropdown.dropdown('get value');
                } else {
                    var checkedInputs = $('td:last input:checked', tr);
                    if (checkedInputs.length > 0) {
                        checkedInputs.each(function () {
                            attrValues.push(this.value);
                        });
                    }
                }
                if (attrValues.length > 0) {
                    var attrId = tr.data('id');
                    for (var i = 0; i < attrValues.length; i++) {
                        $('td:last', tr).append('<input name="SelectAttributes[{0}].AttributeId" type="hidden" value="{1}"/><input name="SelectAttributes[{0}].ValueId" type="hidden" value="{2}"/>'
							.format(attrIndex + i, attrId, attrValues[i]));
                    }
                    attrIndex += attrValues.length;
                }
            });
        });

        $form.on('submit', function () {
            if (!$form.valid())
                return false;

            //#region 去除UEditor自动添加的br和p 自动添加的br和p导致后台判断认为修改了描述，然后修改状态为需要审核
            var tempInput = $('[name="Description.Description"]', $form);
            var oldValue = $('#oldDes').val();
            var regexStr1 = /(<p><br\/><\/p>)?<p>\t*<\/p>$/, regexStr2 = /<p>\t*<\/p>$/;
            if (tempInput.val().replace(regexStr1, '') == oldValue.replace(regexStr2, ''))
                tempInput.val(oldValue);
            else
                tempInput.val(tempInput.val().replace(regexStr1, ''));

            tempInput = $('[name="Description.MobileDescription"]', $form);
            oldValue = $('#oldMobileDes').val();
            if (tempInput.val().replace(regexStr1, '') == oldValue.replace(regexStr2, ''))
                tempInput.val(oldValue);
            else
                tempInput.val(tempInput.val().replace(regexStr1, ''));
            //#endregion

            var loading = showLoading();
            $form.ajaxSubmit({
                success: function (data) {
                    loading.close();
                    if (data) {
                        if (data.success == true) {
                            $.dialog.succeedTips('操作成功！');
                            setTimeout(function () {
                                location.href = '/SellerAdmin/product/management';
                            }, 1500);
                        } else {
                            $.dialog.errorTips(data.message);
                        }
                    } else {
                        $.dialog.errorTips('操作失败！');
                    }
                },
                error: function (data) {
                    loading.close();
                    $.dialog.errorTips('操作失败！');
                }
            });

            return false;
        });

        //商品列表页点编辑跳转过来时有值
        if ($.notNullOrEmpty(txtCategoryPath.val())) {
            //绑定分类下拉框
            var categoryPath = txtCategoryPath.val().split('|');
            for (var i = 0; i < categoryPath.length; i++) {
                $('.pub-inline:eq({0}) ul li#{1}'.format(i, categoryPath[i]), categoryDiv).click();
            }
        }

        $('.product.pic .upload_pic input:hidden').each(function () {
            var url = $(this).val();
            if (url == null || url.length == 0)
                return;
            var img = $('<img src="{0}" style="margin-top:100%"/>'.format($(this).val()));
            img.load(function () {
                $(this).closest('.product.pic').find('.del').removeClass('hide');
            }).appendTo(this);
        });

        $('.product.pic .del').click(function () {
            var parent = $(this).addClass('hide').closest('.product.pic');
            parent.find('.upload_pic input:hidden').val('').parent().css('background-image', '');
        });
    }

    function getSpecification(categoryId) {
        $.ajax({
            url: '{0}?productId={1}&categoryId={2}'.format(url_Specifications, modelId, categoryId),
            context: { categoryId: categoryId },
            success: function (data) {
                if (data) {
                    if ($.isFunction(onGetSpecificationed))
                        onGetSpecificationed(this.categoryId, data);

                    //开始生成规格内容
                    $('#specificationsDiv').html('');
                    //alert(JSON.stringify(data));
                    for (var i = 0; i < data.length; i++) {
                        var str = '', item = data[i];
                        for (var j = 0; j < item.Values.length; j++) {
                            var spe = item.Values[j];
                            str += '<div class="inline"><input type="checkbox" id="{0}" value="{1}"{2}/><input type="text" value="{1}"{3}/></div>'.format(spe.Id, spe.Value, spe.checked == true ? ' checked' : '', spe.checked == true ? '' : ' disabled');
                        }
                        //var div = $('<div class="group sku-group clearfix"><label>{0}：</label><div class="sku-group-item">{1}</div></div>'
                        //	.format(item.Specification.Text, str));
                        var typeAlias = item.Specification.Alias;
                        if (item.Specification.Alias == "" || item.Specification.Alias == null) {
                            typeAlias = item.Specification.Text;
                        }
                        //$("#" + item.Specification.Name + "_Alias").val(typeAlias);
                        $("label[name^='" + item.Specification.Name + "_Alias']").html(typeAlias);
                        var div = $('<div class="group sku-group clearfix"><label>{0}：</label><div class="sku-group-item">{1}</div></div>'
					    	.format(typeAlias, str));
                        div.data('spec', item.Specification);

                        $('#specificationsDiv').append(div);
                    }

                    if (data.length > 0)
                        $('.form-group.spec.hidden').removeClass('hidden');
                }
            }
        });
    }

    function skuPicChange() {
        var path = this.value;
        var tr = $(this).closest('tr');
        $('table.sku tr[{0}="{1}"]'.format(tr.data('name'), tr.data('value'))).each(function () {
            var lastTd = $('td:last', this);
            var showPicInput = $('input[name$=ShowPic]', lastTd);
            if (showPicInput.length == 0)
                lastTd.append('<input name="SKUExs[{0}].ShowPic" type="hidden" value="{1}"/>'.format($(this).index(), path));
            else
                showPicInput.val(path);
        });
    }

    function generateSKU(spes)//spes:[{specification:{value,text,name},values:[{id,value}]}]
    {
        var skus = [];
        function each(dones, specifications) {
            var item = spes[dones.length];
            specifications.push(item.specification);
            for (var i = 0; i < item.values.length; i++) {
                dones.push(item.values[i]);
                if (dones.length < spes.length) {
                    arguments.callee(dones, specifications);
                }
                else {
                    var sku = {
                        attrs: specifications.newitem(function (p, j) { return '{0}="{1}"'.format(p.Name, dones[j].oldValue); }),//因为页面绑定时取不到id，所以统一用文本 最后生成的tr为<tr version="100" color="白色" size="爱神的箭"></tr>
                        tds: '<td>' + dones.newitem(function (p, j) {
                            var html = '<label>' + p.value + '</label>';
                            var specItem = specifications[j];
                            //specItem.name为SpecificationType枚举项名称：如Color,Size,Vision
                            html += '<input name="SKUExs[{0}].{1}" type="hidden" value="{2}"/>'.format('{0}', specItem.Name, p.value);
                            html += '<input name="SKUExs[{0}].{1}Id" type="hidden" value="{2}"/>'.format('{0}', specItem.Name, p.id);
                            return html;
                        }).join('</td><td>') + '</td>',
                        tdCount: dones.length
                    };

                    skus.push(sku);
                }

                dones.removeAt(dones.length - 1);
            }
            specifications.removeAt(specifications.length - 1);
        }

        each([], []);

        return skus;
    }

    function generateSKUTable(spes) {
        var skus = generateSKU(spes);
        var skusDiv = $('#skusDiv');
        var skuTable = skusDiv.find('table.sku');
        var quickSettingTable = skusDiv.find('.batch-attr-setting');
        var imgTable = skusDiv.find('table.img tbody');

        //判断是否存在需要设置图片的规格
        var needPic = false;
        if (spes.any(function (p) { return p.specification.NeedPic; })) {
            if (imgTable.length == 0) {
                var first = spes.first(function (p) { return p.specification.NeedPic });//如果需要设置图片的规格有多个，需另外处理，暂不支持
                var typeAlias = first.specification.Alias;
                if (first.specification.Alias == "" || first.specification.Alias == null) {
                    typeAlias = first.specification.Text;
                }
                imgTable = $('<table class="table table-bordered img"><thead><tr><th>{0}</th><th>图片（可不上传，建议尺寸700*700）</th></tr></thead><tbody></tbody></table>'.format(typeAlias));

                if (skuTable.length > 0)
                    skuTable.before(imgTable);
                else
                    skusDiv.append(imgTable);

                imgTable = imgTable.find('tbody');
            }
        } else if (imgTable.length > 0)
            imgTable.closest('table').remove();

        if (quickSettingTable.length > 0)
            quickSettingTable.remove();

        //生成快速设置表格
        quickSettingTable = $('<div class="form-inline batch-attr-setting text-right">' +
            '<div class="form-group">' +
                '<label class="label-inline w75">批量设置:</label>' +
                '<input class="form-control input-sm w95" placeholder="商城价" for="SalePrice">' +
			'</div>' +
			'<div class="form-group">' +
    			'<input class="form-control input-sm w95" placeholder="库存" for="Stock">' +
			'</div>' +
			'<div class="form-group">' +
    			'<input class="form-control input-sm w95" placeholder="警戒库存" for="SafeStock">' +
			'</div>' +
			'<div class="form-group">' +
    			'<span class="btn btn-default">确定</span>' +
			'</div>' +
		'</div>');

        if (skuTable.length > 0)
            skuTable.before(quickSettingTable);
        else
            skusDiv.append(quickSettingTable);

        var ths = '', tbodyTrs = '';
        for (var i = 0; i < spes.length; i++) {
            var item = spes[i];
            var specification = item.specification;
            var typeAlias = specification.Alias;
            if (specification.Alias == "" || specification.Alias == null) {
                typeAlias = specification.Text;
            }
            ths += '<th>' + typeAlias + '</th>';//生成sku表格的头

            if (specification.NeedPic == true) {
                for (var j = 0; j < item.values.length; j++) {
                    var value = item.values[j];
                    var tr = imgTable.find('tr#{0}_{1}'.format(specification.Value, value.id));

                    if (tr.length == 0) {
                        tr = $(('<tr id="{0}_{1}" data-name="{2}" data-value="{3}"><td>{3}</td><td class="table-img-file"><div class="upload_pic"><input type="hidden"/></div>' +
							'<a class="hidden reset pic">删除</a><input class="upload pic" type="button" value="选择文件"/></td></tr>')
							.format(specification.Value, value.id, specification.Name, value.value));
                    }
                    if (j == 0)
                        imgTable.prepend(tr);
                    else
                        $('tr:eq({0})'.format(j - 1), imgTable).after(tr);
                }
                $('tr:gt({0})'.format(item.values.length - 1), imgTable).remove();
            }
        }

        ths += '<th><span class="red">*</span>商城价</th><th><span class="red">*</span>库存</th><th>警戒库存</th><th>货号</th>';

        var txtMallPrice = $('#txtMallPrice').val();
        var txtStock = $('#txtStock').val();
        var txtSafeStock = $('#txtSafeStock').val();
        var v1 = $.isNullOrEmpty(txtMallPrice) ? '0.01' : txtMallPrice;
        var v2 = $.isNullOrEmpty(txtStock) ? '0' : txtStock;
        var v3 = $.isNullOrEmpty(txtSafeStock) ? '0' : txtSafeStock;
        var v4 = '';
        for (var i = 0; i < skus.length; i++) {
            var item = skus[i];
            var oldTr = $('tr[{0}]'.format(item.attrs.join('][')), skuTable);
            if (oldTr.length > 0) {
                v1 = $('input[name$=".SalePrice"]', oldTr).val();
                v2 = $('input[name$=".Stock"]', oldTr).val();
                v3 = $('input[name$=".SafeStock"]', oldTr).val();
                v4 = $('input[name$=".Sku"]', oldTr).val();
            }
            //如果要显示错误就添加 data-msg-number="请输入正确的价格" 等Attribute
            tbodyTrs += ('<tr {0}>{1}<td><input number="true" min="0.01" required name="SKUExs[{6}].SalePrice" value="{2}" onchange="SKUSalePriceChange(this)" class="valid form-control"/></td>' +
					'<td><input data-val="true" number="true" min="0" required name="SKUExs[{6}].Stock" value="{3}" onchange="SKUStockChange(this)" class="valid form-control"/></td>' +
					'<td><input data-val="true" number="true" min="0" name="SKUExs[{6}].SafeStock" value="{4}" class="valid form-control"/></td>' +
					'<td><input class="form-control" name="SKUExs[{6}].Sku" value="{5}"/></td></tr>')
					.format(item.attrs.join(' '), item.tds.format(i), v1, v2, v3, v4, i);
        }

        if (skuTable.length > 0)
            skuTable.remove();
        skusDiv.append('<table class="table table-bordered sku mt20"><thead><tr>{0}</tr></thead><tbody>{1}</tbody></table>'.format(ths, tbodyTrs));

        //生成图片隐藏控件
        $('.upload_pic input', imgTable).change();
    }

    window.SKUSalePriceChange = function (obj) {
        var txtMallPrice = $('#txtMallPrice');

        var min = 0;
        $(obj).closest('.sku.table').find('input[name$=".SalePrice"]').each(function () {
            var value = $(this).val();
            value = parseFloat(value);
            if (isNaN(value) || value < 0.01)
                return;
            if (min == 0 || value < min)
                min = value;
        });

        if (min >= 0.01)
            txtMallPrice.val(min);
    }

    window.SKUStockChange = function (obj) {
        var txtStock = $('#txtStock');

        var min = 0;
        $(obj).closest('.sku.table').find('input[name$=".Stock"]').each(function () {
            var value = $(this).val();
            value = parseInt(value);
            if (isNaN(value) || value < 1)
                return;
            min += value;
        });

        if (min > 0)
            txtStock.val(min);
    }

    //生成商家自定义规格隐藏控件
    function generateSellerSpec(changeSpecs) {
        var sellerSpecDiv = $('#skusDiv div.sellerSpec');
        if (sellerSpecDiv.length == 0) {
            sellerSpecDiv = $('<div class="sellerSpec"></div>');
            $('#skusDiv').append(sellerSpecDiv);
        } else
            sellerSpecDiv.html('');

        var str = '';
        for (var i = 0; i < changeSpecs.length; i++) {
            var item = changeSpecs[i];
            str += '<input name="UpdateSpecs[{0}].Id" type="hidden" value="{1}"/>'.format(i, item.id) +
				'<input name="UpdateSpecs[{0}].Specification" type="hidden" value="{1}"/>'.format(i, item.specification) +
				'<input name="UpdateSpecs[{0}].Value" type="hidden" value="{1}"/>'.format(i, item.value);
        }
        sellerSpecDiv.append(str);
    }

    function generateBrand(categoryId) {
        $.get(url_CategoryBrands + '?categoryId=' + categoryId, function (data) {
            var brand = $('#brand');
            brand.html('<option value="0">请选择品牌</option>');

            if (data) {
                for (var i = 0; i < data.length; i++) {
                    var item = data[i];
                    brand.append('<option value="{0}"{1}>{2}</option>'.format(item.Id, item.Id == brandId ? " selected" : "", item.Name));
                }
            }
        });
    }

    function generateAttr(categoryId) {
        $.ajax({
            url: url_GetAttributes + '?isCategoryId=1&categoryId=' + categoryId,
            context: { categoryId: categoryId },
            success: function (data) {
                if (data) {
                    if ($.isFunction(onGetAttributeed))
                        onGetAttributeed(this.categoryId, data);

                    var context = this;
                    var table = $('<table class="table attr-table"><tbody></tbody></table>');
                    var tbody = table.children();

                    for (var i = 0; i < data.json.length; i++) {
                        var item = data.json[i];
                        var tr = $('<tr></tr>');
                        tr.append('<td><label class="attr-label">' + item.Name + '：</label></td>');
                        if (item.IsMulti == true) {
                            tr.append('<td>' + item.AttrValues.newitem(function (p) {
                                return '<label class="checkbox-inline" style="margin-right:40px;"><input type="checkbox" value="{0}" {1}/>{2}</label>'
									.format(p.Id, p.selected ? 'checked' : '', p.Name);
                            }).join('') + '</td>');
                        }
                        else {
                            tr.append($('<td></td>').appendDropdown({
                                defaultText: '请选择' + item.Name,
                                items: item.AttrValues.newitem(function (p) {
                                    return {
                                        value: p.Id,
                                        text: p.Name,
                                        selected: p.selected
                                    };
                                }),
                                isMultiple: item.IsMulti
                            }));
                        }
                        tbody.append(tr);
                        tr.data('id', item.AttrId);
                    }

                    $(attrDiv).html('').append(table);
                }
            }
        });
    }
    if (parseInt($("#HasSKU").val()) == 0) {
        $(".j_spec").hide();
    }
}(jQuery));
function CategoryChange() {
    $("select[name='GoodsCategory']").change(function () {
        var selectedCates = [];
        $("select[name='GoodsCategory'] option:selected").each(function () {
            selectedCates.push($(this).val());
        });
        var index = $("select[name='GoodsCategory']").index(this);
        $("select[name='GoodsCategory']").filter(':not(:eq(' + index + '))').find("option").removeAttr("disabled");
        for (var i = 0; i < selectedCates.length; i++) {
            if (selectedCates[i] > 0) {
                $("select[name='GoodsCategory']").filter(':not(:eq(' + index + '))').find("option[value=" + selectedCates[i] + "]:not(:selected)").attr("disabled", "disabled");
            }
        }
    });
}