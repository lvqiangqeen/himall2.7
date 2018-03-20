using System;
using System.Windows.Forms;
using Himall.Entity;
using System.Linq;
using System.Collections.Generic;
using Himall.Model;
using Himall.Search.Service.Model;
using Himall.Search.Service;
using System.Threading;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using System.Configuration;

namespace Indexer
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// 工作线程
        /// </summary>
        private Thread workThread;

        /// <summary>
        /// 定时工作线程
        /// </summary>
        private Thread timeThread;
        public Form1()
        {
            InitializeComponent();
            string str = ConfigurationManager.AppSettings["Count"];
            Count = int.Parse(str);
            str = ConfigurationManager.AppSettings["Interval"];
            Hours = int.Parse(str);
            this.MaximizeBox = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Control.CheckForIllegalCrossThreadCalls = false;
            string DictPath = Path.Combine(Environment.CurrentDirectory, "Dict") + Path.DirectorySeparatorChar;
            search = new Search(DictPath);
        }
        Search search;
        Entities db = new Entities();
        /// <summary>
        /// 一次建多少商品索引
        /// </summary>
        int Count = 100;
        /// <summary>
        /// 是否有余数
        /// </summary>
        bool hasmod = false;
        /// <summary>
        /// 索引创建间隔时间
        /// </summary>
        int Hours = 0;
        /// <summary>
        /// 最后一页商品数量
        /// </summary>
        int last = 0;

        private const string _start = "启动索引线程";
        private const string _stop = "停止索引线程";
        private static bool _isStart = false;
        /// <summary>
        /// 立即更新索引
        /// </summary>
        private void index_Click(object sender, EventArgs e)
        {
            workThread = new Thread(CreateIndexFromDb);
            workThread.Start();
        }

        private void thread_Click(object sender, EventArgs e)
        {
            if (!_isStart)
            {
                timeThread = new Thread(Timer);
                timeThread.Start();
                threadBtn.Text = _stop;
                _isStart = true;
            }
            else
            {
                timeThread.Abort();
                timeThread = null;
                threadBtn.Text = _start;
                _isStart = false;
            }
        }
        /// <summary>
        /// 从数据库读取数据并创建索引
        /// </summary>
        private void CreateIndexFromDb()
        {
            createIndexBtn.Enabled = false;
            if (isEmptyData.Checked)
            {
                msg.Text = "正在清空索引";
                search.EmptyIndex();
            }
            List<ProductInfo> list = db.ProductInfo.Where(p => p.AuditStatus == ProductInfo.ProductAuditStatus.Audited
        && p.SaleStatus == ProductInfo.ProductSaleStatus.OnSale).ToList();
            int max = list.Count / Count;
            last = list.Count % Count;
            if (last > 0)
            {
                hasmod = true;
                max++;
            }
            progressBar1.Maximum = max;
            msg.Text = "正在创建索引";
            for (int pagecount = 0; pagecount < progressBar1.Maximum; pagecount++)
            {
                List<ProductInfo> templist = list.Skip(pagecount * Count).Take(Count).ToList();
                CreateIndex(pagecount, templist);
                this.BeginInvoke(new System.EventHandler(UpdateProgress), pagecount + 1);
            }
            msg.Text = "索引创建完成";
            createIndexBtn.Enabled = true;
        }

        /// <summary>
        /// 定时创建索引
        /// </summary>
        private void Timer()
        {
            while (true)
            {
                Thread.Sleep(Hours * 1000 * 60 * 60);
                CreateIndexFromDb();
            }
        }

        /// <summary>
        /// 批量创建索引
        /// </summary>
        /// <param name="pagecount"></param>
        private void CreateIndex(int pagecount, List<ProductInfo> templist)
        {
            List<Index> result = new List<Index>();
            for (int i = 0; i < templist.Count(); i++)
            {
                Index temp = new Index();
                temp.Id = templist[i].Id;
                temp.ImagePath = templist[i].RelativePath;
                temp.Price = templist[i].MinSalePrice;
                temp.AddDate = templist[i].AddedDate;
                temp.BrandId = templist[i].BrandId;
                temp.CategoryId = templist[i].CategoryId;
                long productid = templist[i].Id;
                temp.Comments = db.ProductCommentInfo.Where(t => t.ProductId == productid).Count();
                temp.ProductName = templist[i].ProductName;
                temp.SaleCount = templist[i].SaleCounts;
                temp.ShopId = templist[i].ShopId;
                temp.TypeId = templist[i].TypeId;
                long sid = templist[i].ShopId;
                ShopInfo shop = db.ShopInfo.Where(e => e.Id == sid).FirstOrDefault();
                if (shop != null)
                    temp.ShopName = shop.ShopName;
                temp.ShopName = templist[i].Himall_Shops.ShopName;
                temp.CategoryPath = templist[i].CategoryPath;
                temp.EndDate = templist[i].Himall_Shops.EndDate.Value;
                long fid = templist[i].FreightTemplateId;
                FreightTemplateInfo ftl = db.FreightTemplateInfo.Where(e => e.Id == fid).FirstOrDefault();
                if (ftl != null && ftl.SourceAddress != null)
                {
                    long regionId = ftl.SourceAddress.Value;
                    temp.Address = GetRegionShortName(regionId);
                }
                var attrs = templist[i].ProductAttributeInfo.Where(t => t.ProductId == templist[i].Id).ToList();
                foreach (var t in attrs)
                    temp.AttrValueIds.Add(t.AttributeId + "_" + t.ValueId);
                result.Add(temp);
            }
            search.CreateIndex(result);
        }

        /// <summary>
        /// 更新进度条
        /// </summary>
        private void UpdateProgress(object o, System.EventArgs e)
        {
            int v = (int)o;
            progressBar1.Value = v;
            if (v == progressBar1.Maximum && hasmod)
            {
                if (progressBar1.Maximum == 1)
                    show.Text = ((progressBar1.Maximum - 1) * Count + last) + "/" + ((progressBar1.Maximum - 1) * Count + last);
                else
                    show.Text = v * Count + "/" + ((progressBar1.Maximum - 1) * Count + last);
            }
            else
                show.Text = v * Count + "/" + progressBar1.Maximum * Count;
        }

        #region 获取地址辅助方法
        IEnumerable<ProvinceMode> _cache = null;
        IEnumerable<ProvinceMode> GetRegions()
        {
            if (_cache != null)
                return _cache;
            string regionString = string.Empty;
            using (FileStream fs = new FileStream("Region.js", FileMode.Open))
            {
                using (StreamReader streamReader = new StreamReader(fs))
                {
                    regionString = streamReader.ReadToEnd();
                }
            }

            regionString = regionString.Replace("var province=", "");
            _cache = JsonConvert.DeserializeObject<IEnumerable<ProvinceMode>>(regionString);
            return _cache;
        }
        string GetRegionShortName(long regionId)
        {
            string address = string.Empty;
            IEnumerable<ProvinceMode> provinces = GetRegions();
            foreach (ProvinceMode p in provinces.ToList())
            {
                if (p.Id == regionId)
                    address = p.Name;

                foreach (CityMode c in p.City.ToList())
                {
                    if (c.Id == regionId)
                        address = p.Name + " " + c.Name;
                    if (c.County == null) continue;
                    foreach (CountyMode county in c.County.ToList())
                        if (county.Id == regionId)
                            address = p.Name + " " + c.Name + " " + county.Name;
                }
            }
            return GetShortAddress(address);
        }

        string GetShortAddress(string regionFullName)
        {
            string address = string.Empty;
            string[] addr = regionFullName.Split(' ');
            if (addr[0] == "北京" || addr[0] == "上海" || addr[0] == "天津" || addr[0] == "重庆")
            {
                address = addr[0];
            }
            else
            {
                if (addr[0].Contains("特别行政区"))
                {
                    address = addr[0].Replace("特别行政区", "");
                }
                else
                {
                    StringBuilder province = new StringBuilder();
                    province.Append(addr[0]);
                    province = province.Replace("省", "");
                    province = province.Replace("维吾尔", "");
                    province = province.Replace("回族", "");
                    province = province.Replace("壮族", "");
                    province = province.Replace("自治区", "");

                    StringBuilder city = new StringBuilder();
                    if (addr.Length > 1)
                    {
                        city.Append(addr[1]);
                        city = city.Replace("市", "");
                        city = city.Replace("盟", "");
                        city = city.Replace("林区", "");
                        city = city.Replace("地区", "");
                        city = city.Replace("土家族", "");
                        city = city.Replace("苗族", "");
                        city = city.Replace("回族", "");
                        city = city.Replace("黎族", "");
                        city = city.Replace("藏族", "");
                        city = city.Replace("傣族", "");
                        city = city.Replace("彝族", "");
                        city = city.Replace("哈尼族", "");
                        city = city.Replace("壮族", "");
                        city = city.Replace("白族", "");
                        city = city.Replace("景颇族", "");
                        city = city.Replace("傈僳族", "");
                        city = city.Replace("朝鲜族", "");
                        city = city.Replace("蒙古", "");
                        city = city.Replace("哈萨克", "");
                        city = city.Replace("柯尔克孜", "");
                        city = city.Replace("自治州", "");
                        city = city.Replace("自治县", "");
                        city = city.Replace("县", "");
                    }

                    address = province.ToString() + city.ToString();
                }
            }
            return address;
        }

        #endregion

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.Visible)
            {
                this.WindowState = FormWindowState.Minimized;
                this.notifyIcon1.Visible = true;
                this.Hide();
            }
            else
            {
                this.Visible = true;
                this.WindowState = FormWindowState.Normal;
                this.Activate();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                //取消"关闭窗口"事件
                e.Cancel = true;
                //使关闭时窗口向右下角缩小的效果
                this.WindowState = FormWindowState.Minimized;
                this.notifyIcon1.Visible = true;
                this.Hide();
                return;
            }
        }

        private void menu_exit_Click(object sender, EventArgs e)
        {
            this.notifyIcon1.Visible = false;
            this.Close();
            if (timeThread != null)
                timeThread.Abort();
            this.Dispose();
            System.Environment.Exit(0);
        }
    }
}
