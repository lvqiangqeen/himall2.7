//本js是仿require.js动态加载js文件，因为使用require.js历史包袱太重，所以仿照它写一个简单版的
//工作原理是，判断页面文档中是否有需要的js文件，如果没有则添加到head元素中

(function () {
	var groups = [], jsGroupList = [], scriptsList = [];//申明组名列表，脚本分组列表，文档中已存在的脚本列表
	jsGroupList.push({ group: '', fileNames: [{nickName:'',fileName:'',depends:['']}] });//绝对路径分组
	groups['common'] = '/Scripts';
	groups['admin'] = '/Areas/Admin/Scripts';
	groups['mobile'] = '/Areas/Mobile/Templates/Default/Scripts';
	groups['sellerAdmin'] = '/Areas/SellerAdmin/Scripts';
	groups['web'] = '/Areas/Web/Scripts';

	window.imitateRequireJS = new function () {
		//#region 私有方法
		var getNameInfo = function (name) {
			var temp = name.split(':');
			var group, nickName, fileName;
			if (temp.length > 1)
			{
				group = temp[0];
				name = temp[1];
			}

			temp = name.split('.');

			if (temp.length > 1 && temp[temp.length - 1] == 'js')
				fileName = name;
			else
				nickName = name;

			for (var i = 0; i < jsGroupList.length; i++) {
				var item = jsGroupList[i];
				if (group == null || group.toLowerCase() == item.group.toLowerCase()) {
					var result = getNameInfoFromFileNames(item.fileNames, nickName, fileName);
					if (result == null) {
						if (group)
							return null;
						else
							continue;
					}
					var path = groups[group ? group : item.group];
					return { depends: result.depends, path: combinPath(path, result.fileName) };
				}
			}
		};

		var getNameInfoFromFileNames = function (fileNames, nickName, fileName) {
			for (var i = 0; i < fileNames.length; i++) {
				var item = fileNames[i];

				if (nickName && item.nickName == nickName)
					return item;
				if (fileName && item.fileName == fileName)
					return item;
			}

			return null;
		}

		var combinPath = function (path1, path2) {
			if (path1[path1.length - 1] != '/')
				path1 += '/';

			if (path2[0] == '/')
				path2 = path2.substr(1);

			return path1 + path2;
		}

		var initScriptsList = function () {
			var elements = document.getElementsByTagName('script');
			for (var i = 0; i < elements.length; i++) {
				var item = elements[i];
				if (item.src.length > 0)
					scriptsList.push(item);
			}
		};
		//#endregion

		//#region 公共方法
		this.groups = groups;

		//添加脚本文件路径
		//jsGroupList:[{group:string,fileNames:[{nickName:string,fileName:string,depends:[]}]}]
		//例:imitateRequire.config([{group:'common',fileNames:[{nickName:"js1",fileName:"javascript1.js"},{nickName:"js2",fileName:"javascript2",depends:['js1']}]},{...}]);
		this.addJsFile = function (p) {
			if (p.group)
			{
				this.addJsFile([p]);
				return;
			}

			for (var i = 0; i < p.length; i++) {
				var item = p[i];
				if (item.group) {
					var existItem;
					for (var j = 0; j < jsGroupList.length; j++) {
						var tempItem = jsGroupList[j];
						if (tempItem.group.toLowerCase() == item.group.toLowerCase()) {
							existItem = tempItem;
							break;
						}
					}
					if (existItem) {
						for (var j = 0; j < item.fileNames.length; j++) {
							existItem.fileNames.push(item.fileNames[j]);
						}
					} else
						jsGroupList.push(item);
				} else if (typeof item == 'string')
					jsGroupList[0].fileNames.push({ fileName: item });
			}
		};

		//加载脚本文件
		//name格式:[group:](nickName|fileName.js),如果别名和文件名唯一可以不带group 如：common:js1或common:javascript1.js或js1或javascript1.js
		//脚本文件加载完成回调
		this.load = function (name,onload) {
			if (scriptsList.length == 0)
				initScriptsList();

			var nameInfo = getNameInfo(name);
			if (nameInfo == null)
				return;

			var exist = false;
			var path = location.origin+nameInfo.path;
			for (var i = 0; i < scriptsList.length; i++) {
				var item = scriptsList[i];
				if (item.src.toLowerCase() == path.toLowerCase()) {
					exist = true;
					break;
				}
			}

			if (exist == true) {
				if (typeof onload == 'function')
					onload();
				return;
			}

			var dependCount = 0;

			if (nameInfo.depends && nameInfo.depends.length > 0) {
				for (var i = 0; i < nameInfo.depends.length; i++) {
					dependCount++;
					this.load(nameInfo.depends[i], function () {
						dependCount--;
						if (dependCount == 0)
							createScriptElement(path, onload);
					});
				}
			}
			else
				createScriptElement(path, onload);
		};
		//#endregion

		//#region 私有方法
		function createScriptElement(path,onload) {
			var element = document.createElement('script');
			element.async = true;
			element.src = path;
			element.onload = onload;
			scriptsList[0].parentNode.appendChild(element);
			scriptsList.push(element);
		}
		//#endregion
	};
})();