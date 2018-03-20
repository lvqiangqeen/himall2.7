namespace System.Web.Mvc
{
    using System;
    using System.Text;
    using System.Web.Mvc;
    using Himall.IServices;
    using Himall.Web.Framework;

    public static class MVCHelpers
    {
        /// <summary>
        /// 上图分页右边的 页数信息
        /// </summary>
        /// <param name="html"></param>
        /// <param name="pagingInfo"></param>
        /// <param name="pageUrl"></param>
        /// <returns></returns>
        public static MvcHtmlString PageLinks(this System.Web.Mvc.HtmlHelper html, PagingInfo pagingInfo, Func<int, string> pageUrl)
        {
            //新建StringBuilder实例result       将他作为最终的html字符串生成相应的标签 
            StringBuilder result = new StringBuilder();

            if (pagingInfo.TotalPages != 0)
            {
               
                //首页 前页
                TagBuilder tagPrior = new TagBuilder("a");
                tagPrior.InnerHtml = "上一页";
                //将标签tostring后 追加到StringBuilder
                if (pagingInfo.CurrentPage != 1)
                {
                    tagPrior.MergeAttribute("href", pageUrl(pagingInfo.CurrentPage - 1));
                }
                else
                {
                    tagPrior.MergeAttribute("class", "prev-disabled");
                }
                result.Append(tagPrior.ToString());


                //1
                if (pagingInfo.TotalPages >= 1)
                {
                    TagBuilder one = new TagBuilder("a");
                    one.InnerHtml = "1";
                    one.MergeAttribute("href", pageUrl(1));
                    if (pagingInfo.CurrentPage == 1)
                    {
                        one.MergeAttribute("class", "current");
                    }
                    result.Append(one.ToString());
                }

                //2
                if (pagingInfo.TotalPages >= 2)
                {
                    TagBuilder two = new TagBuilder("a");
                    two.InnerHtml = "2";
                    two.MergeAttribute("href", pageUrl(2));
                    if (pagingInfo.CurrentPage == 2)
                    {
                        two.MergeAttribute("class", "current");
                    }
                    result.Append(two.ToString());
                }


                if (pagingInfo.CurrentPage > 5&& pagingInfo.TotalPages!=6)
                {
                    TagBuilder span = new TagBuilder("span");
                    span.InnerHtml = "...";
                    span.MergeAttribute("class", "text");
                    result.Append(span.ToString());
                }

                int index = pagingInfo.CurrentPage > 2 ?
                    pagingInfo.CurrentPage :
                    3;

                //1,2,3,4,5的时候
                if (pagingInfo.CurrentPage <= 5)
                {
                    int start = 3;
                    for (int i = start; i < 8 && i <= pagingInfo.TotalPages; i++)
                    {
                        TagBuilder temp = new TagBuilder("a");
                        temp.InnerHtml = i.ToString();
                        temp.MergeAttribute("href", pageUrl(i));
                        if (i == pagingInfo.CurrentPage)
                            temp.MergeAttribute("class", "current");
                        result.Append(temp.ToString());
                    }

                    if (pagingInfo.TotalPages > 7)
                    {
                        TagBuilder span = new TagBuilder("span");
                        span.InnerHtml = "...";
                        span.MergeAttribute("class", "text");
                        result.Append(span.ToString());
                    }
                }

                //后面不足5个
                if (pagingInfo.CurrentPage > 5 && pagingInfo.CurrentPage + 5 > pagingInfo.TotalPages)
                {
                    int start = pagingInfo.TotalPages - 4;
                    if (start == 2) start += 1;
                    for (int i = start; i <= pagingInfo.TotalPages; i++)
                    {
                        TagBuilder temp = new TagBuilder("a");
                        temp.InnerHtml = i.ToString();
                        temp.MergeAttribute("href", pageUrl(i));
                        if (i == pagingInfo.CurrentPage)
                            temp.MergeAttribute("class", "current");
                        result.Append(temp.ToString());
                    }
                }

                //大于5的时候
                if (pagingInfo.CurrentPage > 5 && pagingInfo.CurrentPage + 5 <= pagingInfo.TotalPages)
                {
                    //index = pagingInfo.CurrentPage % 5;
                    for (int i = pagingInfo.CurrentPage; i < pagingInfo.CurrentPage + 5; i++)
                    {
                        TagBuilder temp = new TagBuilder("a");
                        temp.InnerHtml = (i - 2).ToString();
                        temp.MergeAttribute("href", pageUrl(i - 2));
                        if (i == pagingInfo.CurrentPage + 2)
                            temp.MergeAttribute("class", "current");
                        result.Append(temp.ToString());
                    }
                    TagBuilder span = new TagBuilder("span");
                    span.InnerHtml = "...";
                    span.MergeAttribute("class", "text");
                    result.Append(span.ToString());
                }


                //下一页
                TagBuilder tagAfter = new TagBuilder("a");
                tagAfter.InnerHtml = "下一页";
                if (pagingInfo.CurrentPage != pagingInfo.TotalPages)
                {
                    tagAfter.MergeAttribute("href", pageUrl(pagingInfo.CurrentPage + 1));
                }
                else
                {
                    tagAfter.MergeAttribute("class", "next-disabled");
                }
                result.Append(tagAfter.ToString());
            }

            //创建html标签 返回！！！
            return MvcHtmlString.Create(result.ToString());
        }

        public static MvcHtmlString CategoryPath(string path, string pName)
        {
                StringBuilder sb = new StringBuilder("<div class=\"breadcrumb\">");
            try
            {
                pName = pName.Length > 40 ? pName.Substring(0, 40) + " ..." : pName;
                string mian = "", second = "", third = "";
                var catepath = path.Split('|');
                if (catepath.Length > 0)
                    mian = ServiceHelper.Create<ICategoryService>().GetCategory(long.Parse(catepath[0])).Name;
                if (catepath.Length > 1)
                    second = ServiceHelper.Create<ICategoryService>().GetCategory(long.Parse(catepath[1])).Name;
                if (catepath.Length > 2)
                    third = ServiceHelper.Create<ICategoryService>().GetCategory(long.Parse(catepath[2])).Name;
                sb.AppendFormat("<strong><a href=\"/search/searchAd?cid={0}\">{1}</a></strong>", long.Parse(catepath[0]), mian);
                sb.AppendFormat("<span>{0}{1}&nbsp;&gt;&nbsp;<a href=\"\">{2}</a></span>",
                    string.IsNullOrWhiteSpace(second) ? "" : string.Format("&nbsp;&gt;&nbsp;<a href=\"/search/searchAd?cid={0}\">{1}</a>", long.Parse(catepath[1]), second),
                    string.IsNullOrWhiteSpace(third) ? "" : string.Format("&nbsp;&gt;&nbsp;<a href=\"/search/searchAd?cid={0}\">{1}</a>", long.Parse(catepath[2]), third), pName);

                sb.AppendFormat("</div>");
            }
            catch
            {
                sb.AppendFormat("</div>");
            }
            return MvcHtmlString.Create(sb.ToString());
        }


        /// <summary>
        /// 上图分页左边的 页面信息显示
        /// </summary>
        /// <param name="html"></param>
        /// <param name="pagingInfo"></param>
        /// <returns></returns>
        public static MvcHtmlString PageShow(this System.Web.Mvc.HtmlHelper html, PagingInfo pagingInfo)
        {
            //新建StringBuilder实例result 将他作为最终的html字符串 生成相应的 标签 
            StringBuilder result = new StringBuilder();
            if (pagingInfo.TotalPages != 0)
            {
                result.Append("<font color='#ff6600'><b>" + pagingInfo.CurrentPage + "</b></font> / " + pagingInfo.TotalPages + " 页，每页<font color='#ff6600'><b>" + pagingInfo.ItemsPerPage + "</b></font> 条，共 <font color='#ff6600'><b>" + pagingInfo.TotalItems + "</b></font> 条");
            }
            return MvcHtmlString.Create(result.ToString());
        }
    }
    public class PagingInfo
    {
        public int TotalItems { get; set; }
        public int ItemsPerPage { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages
        {
            get { return (int)Math.Ceiling((decimal)TotalItems / ItemsPerPage); }
        }
    }


}