namespace ShellExtensions
{
    public enum ContextMenuType
    {
        Unknown = 0,

        ClassicContextMenu = 1 << 0,

        ModernContextMenu = 1 << 1,

        TreeView = 1 << 8,

        ClassicContextMenuInTreeView = ClassicContextMenu | TreeView,

        ModernContextMenuInTreeView = ModernContextMenu | TreeView,
    }
}
