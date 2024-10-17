namespace ExplorerContextMenu.Models
{
    public class ExplorerContextMenuItemFlags
    {
        private uint flags;

        //public bool HasSplitButton
        //{
        //    get => (flags & 0x2u) == 0x2u;
        //    set
        //    {
        //        if (value) flags |= 0x2u;
        //        else flags &= ~unchecked((uint)0x2u);
        //    }
        //}

        //public bool HideLabel
        //{
        //    get => (flags & 0x4u) == 0x4u;
        //    set
        //    {
        //        if (value) flags |= 0x4u;
        //        else flags &= ~0x4u;
        //    }
        //}

        /// <summary>
        /// 仅在经典菜单中可用
        /// </summary>
        public bool IsSeparator
        {
            get => (flags & 0x8u) == 0x8u;
            set
            {
                if (value) flags |= 0x8u;
                else flags &= ~0x8u;
            }
        }

        /// <summary>
        /// UAC 盾牌标志
        /// </summary>
        public bool HasLuaShield
        {
            get => (flags & 0x10u) == 0x10u;
            set
            {
                if (value) flags |= 0x10u;
                else flags &= ~0x10u;
            }
        }

        /// <summary>
        /// 仅在经典菜单的子菜单中可用
        /// </summary>
        public bool SeparatorBefore
        {
            get => (flags & 0x20u) == 0x20u;
            set
            {
                if (value) flags |= 0x20u;
                else flags &= ~0x20u;
            }
        }

        /// <summary>
        /// 仅在经典菜单的子菜单中可用
        /// </summary>
        public bool SeparatorAfter
        {
            get => (flags & 0x40u) == 0x40u;
            set
            {
                if (value) flags |= 0x40u;
                else flags &= ~0x40u;
            }
        }

        //public bool IsDropDown
        //{
        //    get => (flags & 0x80u) == 0x80u;
        //    set
        //    {
        //        if (value) flags |= 0x80u;
        //        else flags &= ~0x80u;
        //    }
        //}

        //public bool Toggleable
        //{
        //    get => (flags & 0x100u) == 0x100u;
        //    set
        //    {
        //        if (value) flags |= 0x100u;
        //        else flags &= ~0x100u;
        //    }
        //}

        //public bool AutoMenuIcons
        //{
        //    get => (flags & 0x200u) == 0x200u;
        //    set
        //    {
        //        if (value) flags |= 0x200u;
        //        else flags &= ~0x200u;
        //    }
        //}

        public uint GetFlags() => flags;
    }
}
