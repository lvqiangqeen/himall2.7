namespace Hishop.Weixin.MP.Domain.Menu
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class ButtonGroup
    {
        public ButtonGroup()
        {
            this.button = new List<BaseButton>();
        }

        public List<BaseButton> button { get; set; }
    }
}

