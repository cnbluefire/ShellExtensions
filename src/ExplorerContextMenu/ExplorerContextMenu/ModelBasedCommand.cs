using ExplorerContextMenu.ComObjects;
using ExplorerContextMenu.Helpers;
using ExplorerContextMenu.Models;
using ShellExtensions;
using ShellExtensions.Helpers;
using System.Diagnostics;

namespace ExplorerContextMenu;

internal class ModelBasedCommand : ShellExtensions.ExplorerCommand
{
    private readonly ModelFactory factory;
    private readonly ExplorerContextMenuItemModel model;
    private readonly Guid guid;
    private IReadOnlyList<ShellExtensions.ExplorerCommand>? children;

    internal ModelBasedCommand(ModelFactory factory, Models.ExplorerContextMenuItemModel model)
    {
        this.factory = factory;
        this.model = model;
        this.guid = Guid.Parse(model.Guid);

        var subItems = factory.GetSubItems(guid);

        if (subItems != null && subItems.Count > 0)
        {
            var children = new List<ShellExtensions.ExplorerCommand>();
            this.children = children;
            for (var i = 0; i < subItems.Count; i++)
            {
                if (subItems[i].Flags != null && subItems[i].Flags.IsSeparator)
                {
                    children.Add(new SeparatorCommand(Guid.Empty));
                }
                else
                {
                    children.Add(new ModelBasedCommand(factory, subItems[i]));
                }
            }
        }
    }

    public override Guid? CanonicalName => this.guid;

    public override ExplorerCommandFlags Flags => base.Flags | (ExplorerCommandFlags)(model.Flags?.GetFlags() ?? 0u);

    public override ExplorerCommandState GetState(ShellItemArray shellItems, bool fOkToBeSlow, out bool pending)
    {
        pending = false;

        #region Visibility Properties

        var visibilityOptions = model.VisibilityOptions;

        if (visibilityOptions != null)
        {
            if (visibilityOptions.Show
                && visibilityOptions.ShowInModernContextMenu
                && visibilityOptions.ShowInClassicContextMenu
                && visibilityOptions.ShowWithFile
                && visibilityOptions.ShowWithFolder
                && visibilityOptions.ShowWithMultipleItems
                && !visibilityOptions.RegistryOverrideSupport)
            {
                visibilityOptions = null;
            }
        }

        if (visibilityOptions != null)
        {
            if (visibilityOptions.RegistryOverrideSupport)
            {
                if (!fOkToBeSlow)
                {
                    pending = true;
                    return ExplorerCommandState.ECS_ENABLED;
                }

                if (factory.VisibilityHelper.TryGetValue(guid, nameof(visibilityOptions.Show), out var _show))
                {
                    visibilityOptions.Show = _show;
                }

                if (visibilityOptions.Show)
                {
                    if (factory.VisibilityHelper.TryGetValue(guid, nameof(visibilityOptions.ShowInModernContextMenu), out var _showInModernContextMenu))
                    {
                        visibilityOptions.ShowInModernContextMenu = _showInModernContextMenu;
                    }
                    if (factory.VisibilityHelper.TryGetValue(guid, nameof(visibilityOptions.ShowInClassicContextMenu), out var _showInClassicContextMenu))
                    {
                        visibilityOptions.ShowInClassicContextMenu = _showInClassicContextMenu;
                    }
                    if (factory.VisibilityHelper.TryGetValue(guid, nameof(visibilityOptions.ShowWithFile), out var _showWithFile))
                    {
                        visibilityOptions.ShowWithFile = _showWithFile;
                    }
                    if (factory.VisibilityHelper.TryGetValue(guid, nameof(visibilityOptions.ShowWithFolder), out var _showWithFolder))
                    {
                        visibilityOptions.ShowWithFolder = _showWithFolder;
                    }
                    if (factory.VisibilityHelper.TryGetValue(guid, nameof(visibilityOptions.ShowWithMultipleItems), out var _showWithMultipleItems))
                    {
                        visibilityOptions.ShowWithMultipleItems = _showWithMultipleItems;
                    }
                }
            }

            if (!visibilityOptions.Show)
            {
                return ExplorerCommandState.ECS_HIDDEN;
            }

            if (!visibilityOptions.ShowInModernContextMenu || !visibilityOptions.ShowInClassicContextMenu)
            {
                var contextMenuTypeAccessor = ServiceProvider.GetService<IContextMenuTypeAccessor>();
                if (contextMenuTypeAccessor != null && contextMenuTypeAccessor.ContextMenuType != ContextMenuType.Unknown)
                {
                    var type = contextMenuTypeAccessor.ContextMenuType;
                    if (!visibilityOptions.ShowInModernContextMenu && (type & ContextMenuType.ModernContextMenu) == 0)
                    {
                        return ExplorerCommandState.ECS_HIDDEN;
                    }
                    if (!visibilityOptions.ShowInClassicContextMenu && (type & ContextMenuType.ClassicContextMenu) == 0)
                    {
                        return ExplorerCommandState.ECS_HIDDEN;
                    }
                }
            }

            int count = shellItems.Count;
            bool hasFile = false;
            bool hasFolder = false;

            for (int i = 0; i < count; i++)
            {
                var item = shellItems[i];
                if (item.Attributes.Folder)
                {
                    if (!hasFolder)
                    {
                        if (!item.Attributes.FileSystem && !item.Attributes.Stream)
                        {
                            hasFolder = true;
                        }
                    }
                }
                else
                {
                    hasFile = true;
                }

                if (hasFile && hasFolder) break;
            }

            if (!visibilityOptions.ShowWithMultipleItems && count > 1)
            {
                return ExplorerCommandState.ECS_HIDDEN;
            }

            if (!visibilityOptions.ShowWithFile && hasFile)
            {
                return ExplorerCommandState.ECS_HIDDEN;
            }

            if (!visibilityOptions.ShowWithFolder && hasFolder)
            {
                return ExplorerCommandState.ECS_HIDDEN;
            }
        }

        #endregion Visibility Properties

        var state = ExplorerCommandState.ECS_ENABLED;

        if (model.CheckOptions != null && model.CheckOptions.IsCheckable)
        {
            if (model.CheckOptions.CheckType == ExplorerContextMenuItemCheckType.Check)
            {
                state |= ExplorerCommandState.ECS_CHECKBOX;
            }
            var check = factory.CheckBoxHelper.IsChecked(guid);
            if (check is true)
            {
                if (model.CheckOptions.CheckType == ExplorerContextMenuItemCheckType.Check)
                {
                    state |= ExplorerCommandState.ECS_CHECKED;
                }
                else if (model.CheckOptions.CheckType == ExplorerContextMenuItemCheckType.RadioCheck)
                {
                    state |= ExplorerCommandState.ECS_RADIOCHECK;
                }
            }
        }

        return state;
    }

    public override string? GetTitle(ShellItemArray shellItems)
    {
        var title = factory.LocalizedString.Get(model.Title);
        if (!string.IsNullOrEmpty(title))
        {
            var context = new ParameterContext(
                model,
                shellItems,
                () => this.ServiceProvider.GetService<IDirectoryBackgroundAccessor>()?.ShellFolder,
                false);
            if (context.TryFormat(title, out var result))
            {
                return result;
            }
        }

        return base.GetTitle(shellItems);
    }

    public override string? GetToolTip(ShellItemArray shellItems)
    {
        var toolTip = factory.LocalizedString.Get(model.ToolTip);
        if (!string.IsNullOrEmpty(toolTip))
        {
            var context = new ParameterContext(
                model,
                shellItems,
                () => this.ServiceProvider.GetService<IDirectoryBackgroundAccessor>()?.ShellFolder,
                false);
            if (context.TryFormat(toolTip, out var result))
            {
                return result;
            }
        }

        return base.GetToolTip(shellItems);
    }

    public override string? GetIcon(ShellItemArray shellItems)
    {
        var icon = model.Icon?.IconFile;
        if (!string.IsNullOrEmpty(model.DarkIcon?.IconFile)
            && ThemeHelper.GetAppTheme() == ThemeHelper.AppTheme.Dark)
        {
            icon = model.DarkIcon?.IconFile;
        }

        if (!string.IsNullOrEmpty(icon))
        {
            var context = new ParameterContext(
                model,
                shellItems,
                () => this.ServiceProvider.GetService<IDirectoryBackgroundAccessor>()?.ShellFolder,
                false);
            if (context.TryFormat(icon, out var result))
            {
                return result;
            }
        }

        return base.GetIcon(shellItems);
    }

    public override IReadOnlyList<ExplorerCommand>? Children => children;

    public override void Invoke(ExplorerCommandInvokeEventArgs args)
    {
        try
        {
            if (model.CheckOptions != null && model.CheckOptions.IsCheckable)
            {
                if (model.CheckOptions.CheckType == ExplorerContextMenuItemCheckType.RadioCheck)
                {
                    factory.CheckBoxHelper.SetCheckState(guid, true);
                }
                else if (model.CheckOptions.CheckType == ExplorerContextMenuItemCheckType.Check)
                {
                    var check = factory.CheckBoxHelper.IsChecked(guid);
                    if (check is true) factory.CheckBoxHelper.SetCheckState(guid, false);
                    else if (check is false) factory.CheckBoxHelper.SetCheckState(guid, true);
                }
            }
        }
        catch { }

        var command = model.ExecuteOptions?.Command?.Trim();

        if (!string.IsNullOrEmpty(command))
        {
            var context = new ParameterContext(model, args.ShellItems, () => args.Folder, model.ExecuteOptions!.CommandIsUri);
            if (model.ExecuteOptions!.MultipleItemsOperation == ExplorerContextMenuItemMultipleItemsOperation.OneByOne)
            {
                for (var i = 0; i < context.ShellItemsCount; i++)
                {
                    context.Index = i;
                    if (context.TryFormat(command, out var result))
                    {
                        StartProcess(result, model.Flags?.HasLuaShield ?? false, factory);
                    }
                }
            }
            else
            {
                if (context.TryFormat(command, out var result))
                {
                    StartProcess(result, model.Flags?.HasLuaShield ?? false, factory);
                }
            }
        }

        unsafe static bool StartProcess(string _command, bool _hasLuaShield, ModelFactory _factory)
        {
            if (CommandHelper.TryParseCommand(_command, out var _fileName, out var _parameters))
            {
                var _verb = _hasLuaShield switch
                {
                    true => "runas",
                    _ => "open"
                };

                fixed (char* pVerb = _verb, pFileName = _fileName, pParameters = _parameters)
                {
                    var shellExecuteInfo = new Windows.Win32.UI.Shell.SHELLEXECUTEINFOW();
                    shellExecuteInfo.cbSize = (uint)sizeof(Windows.Win32.UI.Shell.SHELLEXECUTEINFOW);
                    shellExecuteInfo.nShow = 1;
                    shellExecuteInfo.lpVerb = pVerb;
                    shellExecuteInfo.lpFile = pFileName;
                    shellExecuteInfo.lpParameters = pParameters;
                    shellExecuteInfo.fMask =
                        Windows.Win32.PInvoke.SEE_MASK_NOCLOSEPROCESS
                        | Windows.Win32.PInvoke.SEE_MASK_FLAG_DDEWAIT;

                    Dictionary<string, string>? environmentVariables = null;
                    _factory.CheckBoxHelper.AppendEnvironmentVariables(ref environmentVariables);

                    if (environmentVariables != null && environmentVariables.Count > 0)
                    {
                        shellExecuteInfo.fMask |= Windows.Win32.PInvoke.SEE_MASK_FLAG_HINST_IS_SITE;
                        shellExecuteInfo.hInstApp = (Windows.Win32.Foundation.HINSTANCE)CCreatingProcess.CreateSite(out var creatingProcess);

                        creatingProcess.ProcessCreating += (s, a) =>
                        {
                            foreach (var (k, v) in environmentVariables)
                            {
                                a.CreateProcessInputs->SetEnvironmentVariable(k, v);
                            }
                        };
                    }

                    return Windows.Win32.PInvoke.ShellExecuteEx(&shellExecuteInfo);
                }
            }
            return false;
        }
    }
}
