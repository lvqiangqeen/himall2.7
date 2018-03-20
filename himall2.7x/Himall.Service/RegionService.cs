using Himall.CommonModel;
using Himall.Core;
using Himall.IServices;
using Himall.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Himall.Service
{
    public class RegionService : ServiceBase, IRegionService
    {

        private IEnumerable<Region> regions;
        private const string REGION_FILE_PATH = "/Scripts/region.json";
        private const string REGION_BAK_PATH = "/Scripts/regionbak.json";

        /// <summary>
        /// 横向平铺的地区数据
        /// </summary>
        private IEnumerable<Region> RegionSource
        {
            get
            {
                if (regions == null)
                {
                    regions = Cache.Get(CacheKeyCollection.Region) as IEnumerable<Region>;
                    if (regions == null)
                    {
                        regions = LoadRegionData();
                        Cache.Insert(CacheKeyCollection.Region, regions);
                    }
                }
                return regions;
            }
        }



        /// <summary>
        /// 从JS加载地区数据
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Region> LoadRegionData()
        {
            IEnumerable<Region> region;
            // string regionString = string.Empty;
            //using (FileStream fs = new FileStream(Core.Helper.IOHelper.GetMapPath("/Scripts/Region.json"), FileMode.Open))
            //{
            //    using (StreamReader streamReader = new StreamReader(fs))
            //    {
            //        regionString = streamReader.ReadToEnd();
            //    }
            //}

            var regionBytes = HimallIO.GetFileContent(REGION_FILE_PATH);
            var regionString = System.Text.Encoding.UTF8.GetString(regionBytes);
            List<Region> source = new List<Region>();
            region = JsonConvert.DeserializeObject<IEnumerable<Region>>(regionString);
            foreach (var item in region)
            {
                FillRegion(source, item, 1);
            }
            return source;
        }

        /// <summary>
        /// 把子地区平铺
        /// </summary>
        /// <param name="list"></param>
        /// <param name="parent"></param>
        /// <param name="level"></param>
        private void FillRegion(List<Region> list, Region parent, int level)
        {
            list.Add(parent);
            parent.Level = (Region.RegionLevel)level;
            level++;
            if (parent.Sub == null)
                return;
            if (level > 4) { parent.Sub = null; return; }//清空等级大于4的行政区域

            foreach (var sub in parent.Sub)
            {
                sub.Parent = parent;
                FillRegion(list, sub, level);
            }
        }

        /// <summary>
        /// 检查同名区域
        /// </summary>
        /// <param name="regionName"></param>
        /// <param name="region"></param>
        private void CheckRegionName(string regionName, List<Region> region)
        {
            if (region.Any(a => a.Name == regionName || a.ShortName == regionName))
            {
                throw new HimallException("已存在相同区域名称");
            }
        }


        public void EditRegion(string regionName, int regionId)
        {
            var region = GetRegion(regionId);

            List<Region> regs = new List<Region>();
            //检查重名
            if (region.Level == Region.RegionLevel.Province)
            {
                regs = RegionSource.Where(a => a.Level == Region.RegionLevel.Province & a.Id != regionId).ToList();
            }
            else
            {
                regs = region.Parent.Sub.Where(a => a.Id != regionId).ToList();
            }
            CheckRegionName(regionName, regs);
            var provinces = RegionSource.Where(a => a.Level == Region.RegionLevel.Province).ToList();
            region.Name = regionName;
            region.ShortName = GetShortAddressName(regionName);
            //var level = region.Level;
            //var regionPath = region.GetIdPath().Split(',').Select(a => int.Parse(a)).ToList();
            //if (region.Level== Region.RegionLevel.Province)
            //{
            //    CheckRegionName(regionName, provinces.Where(a=>a.Id!=regionId).ToList());
            //    var topRegion = provinces.Where(a => a.Id == regionId).FirstOrDefault();
            //    topRegion.Name = regionName;
            //    topRegion.ShortName = GetShortAddressName(regionName);
            //}
            //else if (level == Region.RegionLevel.City)
            //{
            //    var topRegion = region.Parent;
            //    CheckRegionName(regionName, topRegion.Sub.Where(a => a.Id != regionId).ToList());
            //    var secondRegion = topRegion.Sub.Where(a => a.Id == regionPath[1]).FirstOrDefault();
            //    secondRegion.Name = regionName;
            //    secondRegion.ShortName = GetShortAddressName(regionName);
            //}
            //else if (level == Region.RegionLevel.County)
            //{
            //    var topRegion = provinces.Where(a => a.Id == regionPath[0]).FirstOrDefault();
            //    var secondRegion = topRegion.Sub.Where(a => a.Id == regionPath[1]).FirstOrDefault();
            //    CheckRegionName(regionName, secondRegion.Sub.Where(a => a.Id != regionId).ToList());
            //    var thirdRegion = secondRegion.Sub.Where(a => a.Id == regionPath[2]).FirstOrDefault();
            //    thirdRegion.Name = regionName;
            //    thirdRegion.ShortName = GetShortAddressName(regionName);
            //}
            //else if (level == Region.RegionLevel.Town)
            //{
            //    var topRegion = provinces.Where(a => a.Id == regionPath[0]).FirstOrDefault();
            //    var secondRegion = topRegion.Sub.Where(a => a.Id == regionPath[1]).FirstOrDefault();
            //    var thirdRegion = secondRegion.Sub.Where(a => a.Id == regionPath[2]).FirstOrDefault();
            //    CheckRegionName(regionName, thirdRegion.Sub.Where(a => a.Id != regionId).ToList());
            //    var fourthRegion = thirdRegion.Sub.Where(a => a.Id == regionPath[3]).FirstOrDefault();
            //    fourthRegion.Name = regionName;
            //    fourthRegion.ShortName = GetShortAddressName(regionName);
            //}

            // var provinces = RegionSource.Where(p => p.Level == Region.RegionLevel.Province);
            var json = JsonConvert.SerializeObject(provinces);
            //using (StreamWriter sw = System.IO.File.CreateText(Core.Helper.IOHelper.GetMapPath("/Scripts/Region.json")))
            //{
            //    sw.WriteLine(json);
            //    sw.Flush();
            //    sw.Close();
            //}
            Core.HimallIO.CreateFile(REGION_FILE_PATH, json, FileCreateType.Create);
            Cache.Remove(CacheKeyCollection.Region);
            region = null;
        }

        public long AddRegion(string regionName, long parentid)
        {
            var region = new Region();
            var provinces = RegionSource.ToList();
            region.Id = RegionSource.Max(a => a.Id) + 1;
            if (parentid == 0)
            {//检查重名
                CheckRegionName(regionName, RegionSource.Where(a => a.Level == Region.RegionLevel.Province).ToList());
                region.Level = Region.RegionLevel.Province;
                region.Name = regionName;
                region.ShortName = GetShortAddressName(regionName);
                provinces.Add(region);
            }
            else
            {
                var parent = RegionSource.FirstOrDefault(p => p.Id == parentid);
                if (parent.Sub != null)
                {
                    //检查重名
                    CheckRegionName(regionName, parent.Sub.ToList());
                }
                else
                {
                    parent.Sub = new List<Region>();
                }

                region.Level = parent.Level + 1;
                region.Name = regionName;
                region.ShortName = GetShortAddressName(regionName);
                region.Parent = parent;
                parent.Sub.Add(region);
                provinces = RegionSource.ToList();
            }
            provinces = provinces.Where(p => p.Level == Region.RegionLevel.Province).ToList();
            var json = JsonConvert.SerializeObject(provinces);
            Core.HimallIO.CreateFile(REGION_FILE_PATH, json, FileCreateType.Create);
            //using (StreamWriter sw = System.IO.File.CreateText(Core.Helper.IOHelper.GetMapPath("/Scripts/Region.json")))
            //{
            //    sw.WriteLine(json);
            //    sw.Flush();
            //    sw.Close();
            //}
            Cache.Remove(CacheKeyCollection.Region);
            regions = null;
            return region.Id;
        }


        [Obsolete("请使用AddRegion有返回值的方法")]
        public void AddRegion(string regionName, Region.RegionLevel level, string path)
        {
            var maxId = RegionSource.Max(a => a.Id) + 1;
            var provinces = RegionSource.Where(a => a.Level == Region.RegionLevel.Province).ToList();
            var regionPath = path.Split(',').Select(a => int.Parse(a)).ToList();
            if (level == Region.RegionLevel.Province)
            {
                provinces.Add(new Region() { Name = regionName, Id = maxId, ShortName = GetShortAddressName(regionName) });
            }
            else if (level == Region.RegionLevel.City)
            {
                var topRegion = provinces.Where(a => a.Id == regionPath[0]).FirstOrDefault();
                SetSub(topRegion, regionName, maxId, regionName);
            }
            else if (level == Region.RegionLevel.County)
            {
                var topRegion = provinces.Where(a => a.Id == regionPath[0]).FirstOrDefault();
                var secondRegion = topRegion.Sub.Where(a => a.Id == regionPath[1]).FirstOrDefault();
                SetSub(secondRegion, regionName, maxId, regionName);
            }
            else if (level == Region.RegionLevel.Town)
            {
                var topRegion = provinces.Where(a => a.Id == regionPath[0]).FirstOrDefault();
                var secondRegion = topRegion.Sub.Where(a => a.Id == regionPath[1]).FirstOrDefault();
                var thirdRegion = secondRegion.Sub.Where(a => a.Id == regionPath[2]).FirstOrDefault();
                SetSub(thirdRegion, regionName, maxId, regionName);
            }
            var json = JsonConvert.SerializeObject(provinces);
            using (StreamWriter sw = System.IO.File.CreateText(Core.Helper.IOHelper.GetMapPath(REGION_FILE_PATH)))
            {
                sw.WriteLine(json);
                sw.Flush();
                sw.Close();
            }
            Cache.Remove(CacheKeyCollection.Region);
            regions = null;
        }
        /// <summary>
        /// 获取横向平铺的地区数据
        /// </summary>
        public IEnumerable<Region> GetAllRegions()
        {
            return RegionSource;
        }

        /// <summary>
        /// 重置地区数据 
        /// </summary>
        public void ResetRegions()
        {
            Core.HimallIO.CopyFile(REGION_BAK_PATH, REGION_FILE_PATH, true);
            Cache.Remove(CacheKeyCollection.Region);
            regions = null;
        }

        /// <summary>
        /// 根据ID获取某个地区的信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Region GetRegion(long id)
        {
            return RegionSource.FirstOrDefault(p => p.Id == id);
        }

        public Region GetRegion(long id, Region.RegionLevel level)
        {
            var region = RegionSource.FirstOrDefault(p => p.Id == id);
            while (region != null && region.Level > level)
            {
                region = region.Parent;
            }
            if (region != null && region.Level == level)
                return region;
            return null;
        }

        public Region GetRegionByName(string name)
        {
            return RegionSource.FirstOrDefault(p => p.Name.Contains(name) || name.Contains(p.Name));
        }

        public Region GetRegionByName(string name, Region.RegionLevel level)
        {
            return RegionSource.FirstOrDefault(p => (p.Name.Contains(name) || name.Contains(p.Name)) && p.Level == level);
        }

        public IEnumerable<Region> GetSubs(long parent, bool trace = false)
        {
            if (parent == 0)
                return RegionSource.Where(p => p.ParentId == 0);

            var region = RegionSource.FirstOrDefault(p => p.Id == parent);
            if (trace)
            {
                List<Region> sub = new List<Region>();
                FillSubRegion(sub, region);
                return sub;
            }
            return region.Sub ?? new List<Region>();//下属区域
        }


        /// <summary>
        /// 获取三级子类
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public IEnumerable<Region> GetThridSubs(long parent)
        {
            if (parent == 0)
                return RegionSource.Where(p => p.ParentId == 0);

            var region = RegionSource.FirstOrDefault(p => p.Id == parent);
            List<Region> sub = new List<Region>();
            FillSubRegion(sub, region, Region.RegionLevel.County);
            return sub;
        }



        /// <summary>
        /// 填充追溯所有下属子集
        /// </summary>
        /// <param name="list">填充列表</param>
        /// <param name="model"></param>
        private void FillSubRegion(List<Region> list, Region model, Region.RegionLevel level = Region.RegionLevel.Village)
        {
            if (model.Sub == null || model.Sub.Count == 0 || model.Level == level)
                return;
            foreach (var item in model.Sub)//市级
            {
                if (item.Sub != null && item.Sub.Count > 0 && item.Level != level)
                {
                    list.AddRange(item.Sub);
                    FillSubRegion(list, item, level);
                }
                else
                {
                    item.Sub = new List<Region>();
                }
            }
        }

        public string GetFullName(long id, string seperator = " ")
        {
            var region = RegionSource.FirstOrDefault(p => p.Id == id);
            if (region == null)
                return string.Empty;
            var name = region.Name;
            //追溯上级区域,最多追溯5次 防循环溢出
            for (int i = 0; i < 5 && region.Parent != null; i++)
            {
                region = region.Parent;
                name = region.Name + seperator + name;
            }
            return name;
        }

        public string GetRegionPath(long id, string seperator = ",")
        {
            var region = RegionSource.FirstOrDefault(p => p.Id == id);
            if (region == null)
                return string.Empty;
            var path = id.ToString();

            //追溯上级区域,最多追溯5次 防循环溢出
            for (int i = 0; i < 5 && region.Parent != null; i++)
            {
                region = region.Parent;
                path = region.Id + "," + path;
            }
            return path;
        }
        /// <summary>
        /// 根据地址名称反查地址全路径
        /// </summary>
        /// <param name="city">城市名</param>
        /// <param name="district">区名</param>
        /// <param name="street">街道名</param>
        /// <returns></returns>
        public string GetAddress_Components(string city, string district, string street,out string newStreet)
        {
            Region myregion = null;
            var cityData = RegionSource.FirstOrDefault(p => p.Name == city.Trim() && p.Level == Region.RegionLevel.City);//城市
            if (cityData != null)
            {
                var parent = RegionSource.FirstOrDefault(p => p.Name == district.Trim() && p.Level == Region.RegionLevel.County && p.ParentId == cityData.Id);//区域
                if (parent != null)
                {
                    myregion = RegionSource.Where(p => p.Name == street.Trim() && p.ParentId == (parent.Id)).FirstOrDefault();//优先街道
                    if (myregion == null)
                    {
                        street = street.Replace("街道","").Replace("镇","").Replace("街","");//地区库第四级可能不包含“街道、镇等文字”
                        myregion=RegionSource.Where(p => p.Name == street.Trim() && p.ParentId == (parent.Id)).FirstOrDefault();//特殊替换处理
                    }
                }
                if (myregion == null && parent != null)//街空取区
                {
                    myregion = parent;
                }
            }

            newStreet = street;

            if (myregion == null)
                return string.Empty;
            

            var path = myregion.Id.ToString();
            //追溯上级区域,最多追溯5次 防循环溢出
            for (int i = 0; i < 5 && myregion.Parent != null; i++)
            {
                myregion = myregion.Parent;
                path = myregion.Id + "," + path;
            }
            return path;
        }
        /// <summary>
        /// 通过IP取地区信息
        /// <para>(数据来源：淘宝)</para>
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public long GetRegionByIPInTaobao(string ip)
        {
            if (Core.Cache.Exists(ip))
                return long.Parse(Cache.Get(ip).ToString());
            string RequestUrl = "http://ip.taobao.com/service/getIpInfo.php?ip={0}";
            long result = 0;
            RequestUrl = string.Format(RequestUrl, ip);
            try
            {
                string requestdata = Himall.Core.Helper.WebHelper.GetRequestData(RequestUrl, "");
                TaobaoIpDataModel tbipdata = JsonConvert.DeserializeObject<TaobaoIpDataModel>(requestdata);
                if (tbipdata != null && tbipdata.code == 0)
                {
                    if (!string.IsNullOrWhiteSpace(tbipdata.data.city))
                    {
                        var city = GetRegionByName(tbipdata.data.city, Region.RegionLevel.City);
                        if (city != null)
                            return city.Id;
                    }
                    if (!string.IsNullOrWhiteSpace(tbipdata.data.region))
                    {
                        var province = GetRegionByName(tbipdata.data.region, Region.RegionLevel.Province);
                        if (province != null)
                            return province.Id;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            Cache.Insert(ip, result);
            return result;
        }

        #region 私有方法

        /// <summary>
        /// 获取短地址   
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string GetShortAddressName(string str)
        {
            string result = str;
            result = result.Replace("特别行政区", "");
            result = result.Replace("省", "");
            result = result.Replace("维吾尔", "");
            result = result.Replace("回族", "");
            result = result.Replace("壮族", "");
            result = result.Replace("自治区", "");
            result = result.Replace("市", "");
            result = result.Replace("盟", "");
            result = result.Replace("林区", "");
            result = result.Replace("地区", "");
            result = result.Replace("土家族", "");
            result = result.Replace("苗族", "");
            result = result.Replace("回族", "");
            result = result.Replace("黎族", "");
            result = result.Replace("藏族", "");
            result = result.Replace("傣族", "");
            result = result.Replace("彝族", "");
            result = result.Replace("哈尼族", "");
            result = result.Replace("壮族", "");
            result = result.Replace("白族", "");
            result = result.Replace("景颇族", "");
            result = result.Replace("傈僳族", "");
            result = result.Replace("朝鲜族", "");
            result = result.Replace("蒙古", "");
            result = result.Replace("哈萨克", "");
            result = result.Replace("柯尔克孜", "");
            result = result.Replace("自治州", "");
            result = result.Replace("自治县", "");
            result = result.Replace("县", "");
            return result;
        }
        private void SetSub(Region topRegion, string regionName, int Id, string shortName)
        {
            var sub = topRegion.Sub;
            if (sub == null)
            {
                sub = new List<Region>();
                sub.Add(new Region() { Name = regionName, Id = Id, ShortName = GetShortAddressName(regionName) });
                topRegion.Sub = sub;
            }
            else
            {
                CheckRegionName(regionName, sub);
                sub.Add(new Region() { Name = regionName, Id = Id, ShortName = GetShortAddressName(regionName) });
            }
        }

        #endregion
    }






    #region 淘宝Ip数据
    public class TaobaoIpDataModel
    {
        public int code { get; set; }
        public TaobaoIpData data { get; set; }
    }

    public class TaobaoIpData
    {
        public string country { get; set; }
        public string country_id { get; set; }
        public string area { get; set; }
        public string area_id { get; set; }
        public string region { get; set; }
        public string region_id { get; set; }
        public string city { get; set; }
        public string city_id { get; set; }
        public string county { get; set; }
        public string county_id { get; set; }
        public string isp { get; set; }
        public string isp_id { get; set; }
        public string ip { get; set; }
    }

    #endregion
}



