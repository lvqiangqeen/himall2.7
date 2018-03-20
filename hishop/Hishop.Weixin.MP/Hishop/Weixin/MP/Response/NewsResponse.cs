namespace Hishop.Weixin.MP.Response
{
    using Hishop.Weixin.MP;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class NewsResponse : AbstractResponse
    {
        public int ArticleCount
        {
            get
            {
                return ((this.Articles == null) ? 0 : this.Articles.Count);
            }
        }

        public IList<Article> Articles { get; set; }

        public override ResponseMsgType MsgType
        {
            get
            {
                return ResponseMsgType.News;
            }
        }
    }
}

