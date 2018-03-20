namespace Hishop.Weixin.MP.Domain.Menu
{
    using System;
    using System.Runtime.CompilerServices;

    public abstract class SingleButton : BaseButton
    {
        public SingleButton(string theType)
        {
            this.type = theType;
        }

        public string type { get; set; }
    }
}

