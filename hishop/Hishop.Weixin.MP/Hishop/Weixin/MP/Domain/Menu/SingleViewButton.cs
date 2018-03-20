namespace Hishop.Weixin.MP.Domain.Menu
{
    using System;
    using System.Runtime.CompilerServices;

    public class SingleViewButton : SingleButton
    {
        public SingleViewButton() : base(ButtonType.view.ToString())
        {
        }

        public string url { get; set; }
    }
}

