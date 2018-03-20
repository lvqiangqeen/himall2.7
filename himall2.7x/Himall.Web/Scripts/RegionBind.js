


var setProvince = function ($provinceDiv, $cityDiv, $countyDiv,callback) {
    var provinceDiv = $provinceDiv || $('#provinceDiv');
    var cityDiv = $cityDiv || $('#cityDiv');
    var countyDiv = $countyDiv || $('#countyDiv');
    var areaName = $('#areaName');
    var createElem = function (data, elem, id) {
        if (!data) { return; }
        for (var i = 0, e; e = data[i++];) {
            if (id) {
                elem.append('<option value="' + e.id + '" data="' + id + '">' + e.name + '</option>');
            } else {
                elem.append('<option value="' + e.id + '">' + e.name + '</option>');
            }
        }
    };
    createElem(province, provinceDiv);

    var fnSelect = function (data, val, tag) {
        if (!data) { return; }
        for (var i = 0, e; e = data[i++];) {
            if (e.id == val) {
                return e[tag];
            }
        }
    };
    provinceDiv.change(function (e) {
        var t = e.target,
            id = $(this).val(),
            city;
        if (id != 0) {
            city = fnSelect(province, id, 'city');
            cityDiv.html('<option value="0">请选择</option>');
            countyDiv.html('<option value="0">请选择</option>');
            createElem(city, cityDiv, id);
            areaName.find('span').eq(0).html($(this).find("option:selected").html() + '&nbsp;');
            areaName.find('span').eq(1).html('');
            areaName.find('span').eq(2).html('');
        }
        return false
    });
    cityDiv.change(function (e) {
        var t = e.target,
            id = $(this).val(),
            tag,
            city,
            county;
        if (id != 0) {
            tag = $(this).find("option:selected").attr('data');
            city = fnSelect(province, tag, 'city');
            county = fnSelect(city, id, 'county');
            countyDiv.html('<option value="0">请选择</option>');
            createElem(county, countyDiv);
            areaName.find('span').eq(1).html($(this).find("option:selected").html() + '&nbsp;');
            areaName.find('span').eq(2).html('');
        }
    });
    countyDiv.change(function (e) {
        var id = $(this).val();
        if (id != 0) {
            areaName.find('span').eq(2).html($(this).find("option:selected").html());
        }
    });
};
//判断是否选择地区
var isSelectAddr = function (p, c, t) {
    if (!p || !c || !t)
        return false;
    var haveProvince = false;
    var haveCity = false;
    var haveTown = false;
    p.children().each(function (i, e) {
        s = e.selected;
        if (s == true && i > 0) {
            haveProvince = true;
            return;
        }
    });
    if (haveProvince) {
        c.children().each(function (i, e) {
            s = e.selected;
            if (s == true && i > 0) {
                haveCity = true;
                return;
            }
        });
        if (haveCity) {
            var idx = 0;
            t.children().each(function (i, e) {
                s = e.selected;
                idx = i;
                if (s == true && i > 0) {
                    haveTown = true;
                    return;
                }
            });
            haveTown = idx > 0 ? haveTown : true;
        }
    }
    return haveProvince && haveCity && haveTown;
};
var InitRegion = function ($provinceDiv, $cityDiv, $countyDiv,path) {
    var regionPath = path.split(',');
    $provinceDiv.val(regionPath[0]);
    $provinceDiv.trigger('change');
    $cityDiv.val(regionPath[1]);
    $cityDiv.trigger('change');
    if (regionPath.length == 3) {
        $countyDiv.val(regionPath[2]);
        $countyDiv.trigger('change');
    }
}
