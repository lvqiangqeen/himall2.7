namespace Hishop.Weixin.MP.Domain.Menu
{
    using System;
    using System.Runtime.CompilerServices;

    public class SingleClickButton : SingleButton
    {
        public SingleClickButton() : base(ButtonType.click.ToString())
        {
        }

        public string key { get; set; }
    }
}

