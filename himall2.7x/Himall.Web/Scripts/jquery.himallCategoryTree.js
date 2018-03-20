(function ($) {

    var container;
    var table;

    function initTable()
    {
        container.html('');
        table = $('<table class="table table-bordered category_table"><thead>\
                          <tr>\
                          <th class="td-choose">选择</th>\
                          <th>分类名称/排序</th>\
                          <th class="td-operate">操作</th>\
                          </tr>\
                     </thead>\
                     <tbody>\
                     </tbody></table>');
        container.append(table);
        container.append('<div class="table-bt clearfix">\
               <div class="tabel-operate">\
                  <label><input class="check-all" type="checkbox" name="" />全选</label>\
                  <button type="submit" id="deleteBatch" class="btn btn-danger btn-ssm">批量删除</button>\
              </div>\
              </div>');
    }


 

    function reload() {



    }


    function initLoad() {
        var html = '';
        initTable();
        $.post($.fn.himallCategoryTree.options.dataUrl, { parentId: 0 }, function (data) {
            if (!data || data.length == 0) {
                html = ' <tr>\
                       <td style="text-align:center;" colspan="3"><h2>没有任何分类</h2></td>\
                   </tr>';
            }
            else {
                $.each(data, function (i, category) {
                    html += '<tr class="level-1">\
                           <td class="td-choose"><input type="checkbox" name="" /></td>\
                           <td>\
                                <span class="glyphicon glyphicon-plus-sign"></span>\
                                <input type="hidden" class="hidden_id" value="'+ category.id + '" />\
                                <input class="text-name" type="text" value="' + category.name + '" />\
                                <input class="text-order" type="text" value="' + category.displaySequence + '" />\
                           </td>\
                           <td class="td-operate">\
                               <span class="btn-a">\
                                    <a href="./AddByParent?Id=' + category.id + '">新增下级</a><a href="./Edit?Id=' + category.id + '">编辑</a><a class="delete-classify">删除</a>\
                               </span>\
                           </td>\
                        </tr>';
                });
            }
            var tbody = table.find('tbody');
            tbody.html(html);
        })
    }

    

    $.fn.himallCategoryTree = function (options) {
        container = $(this);
        $.fn.himallCategoryTree.options = $.extend({}, $.fn.himallCategoryTree.options, options);

        initLoad();
    }





    $.fn.himallCategoryTree.options = {
        dataUrl: null,
        updateUrl: null,
        deleteUrl: null,
        batchDeleteUrl: null,
        addUrl: null,
        

    };




    
})(jQuery);










