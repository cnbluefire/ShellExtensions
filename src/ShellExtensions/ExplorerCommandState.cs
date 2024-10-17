namespace ShellExtensions
{
    [Flags]
    public enum ExplorerCommandState : uint
    {
        /// <summary>The item is enabled.</summary>
        ECS_ENABLED = 0,
        /// <summary>The item is unavailable. It might be displayed to the user as a dimmed, inaccessible item.</summary>
        ECS_DISABLED = 1,
        /// <summary>The item is hidden.</summary>
        ECS_HIDDEN = 2,
        /// <summary>The item is displayed with a check box and that check box is not checked.</summary>
        ECS_CHECKBOX = 4,
        /// <summary>The item is displayed with a check box and that check box is checked. <b>ECS_CHECKED</b> is always returned with ECS_CHECKBOX.</summary>
        ECS_CHECKED = 8,
        /// <summary><b>Windows 7 and later</b>. The item is one of a group of mutually exclusive options selected through a radio button. ECS_RADIOCHECK does not imply that the item is the selected option, though it might be.</summary>
        ECS_RADIOCHECK = 16,
    }
}
