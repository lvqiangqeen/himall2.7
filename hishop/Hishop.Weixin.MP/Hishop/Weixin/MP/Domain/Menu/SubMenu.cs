namespace Hishop.Weixin.MP.Domain.Menu
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class SubMenu : BaseButton
    {
        public SubMenu()
        {
            this.sub_button = new List<SingleButton>();
        }

        public SubMenu(string name) : this()
        {
            base.name = name;
        }

        public List<SingleButton> sub_button { get; set; }
    }
}

