using ShellExtensions;
using ShellExtensions.Helpers;

namespace ExplorerContextMenu;

internal class SeparatorCommand : ShellExtensions.ExplorerCommand
{
    private readonly Guid guid;

    internal SeparatorCommand(Guid guid)
    {
        this.guid = guid;
    }

    public override Guid? CanonicalName => guid;

    public override ExplorerCommandFlags Flags => ExplorerCommandFlags.ECF_ISSEPARATOR;

    public override ExplorerCommandState GetState(ShellItemArray shellItems, bool fOkToBeSlow, out bool pending)
    {
        pending = false;

        var contextMenuTypeAccessor = ServiceProvider.GetService<IContextMenuTypeAccessor>();

        if (contextMenuTypeAccessor == null) return ExplorerCommandState.ECS_HIDDEN;
        else if (contextMenuTypeAccessor.ContextMenuType == ContextMenuType.Unknown) return ExplorerCommandState.ECS_HIDDEN;
        else if ((contextMenuTypeAccessor.ContextMenuType & ContextMenuType.ModernContextMenu) != 0) return ExplorerCommandState.ECS_HIDDEN;

        return ExplorerCommandState.ECS_ENABLED;
    }
}
