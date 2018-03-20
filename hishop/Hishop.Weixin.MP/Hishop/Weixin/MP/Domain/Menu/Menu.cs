namespace Hishop.Weixin.MP.Domain.Menu
{
    using System;
    using System.Runtime.CompilerServices;

    public class Menu
    {
        public Menu()
        {
            this.menu = new ButtonGroup();
        }

        public ButtonGroup menu { get; set; }
    }
}

