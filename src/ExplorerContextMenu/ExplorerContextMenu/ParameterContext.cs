using ExplorerContextMenu.Models;
using ShellExtensions;
using ShellExtensions.Helpers;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace ExplorerContextMenu;

internal partial struct ParameterContext
{
    private readonly ExplorerContextMenuItemModel? model;
    private readonly ShellItemArray? shellItems;
    private readonly int index;
    private readonly Func<ShellFolder?>? folderGetter;

    public ParameterContext(
        ExplorerContextMenuItemModel? model,
        ShellItemArray? shellItems,
        int index,
        Func<ShellFolder?>? folderGetter)
    {
        this.model = model;
        this.shellItems = shellItems;
        this.index = index;
        this.folderGetter = folderGetter;
    }

    public readonly bool TryFormat(string text, [NotNullWhen(true)] out string? output)
    {
        output = null;

        var context = this;

        string? folderPath = null;
        bool flag = true;
        IReadOnlyList<ShellItem>? _shellItems = null;

        var result = VariableRegex().Replace(text, match =>
        {
            var variable = match.Groups[1].Value;
            if (string.Equals(variable, "dllPath", StringComparison.OrdinalIgnoreCase))
            {
                return DllModule.Location;
            }
            else if (string.Equals(variable, "dllFolder", StringComparison.OrdinalIgnoreCase))
            {
                return DllModule.BaseDirectory;
            }
            else if (string.Equals(variable, "packageFolder", StringComparison.OrdinalIgnoreCase)
                || string.Equals(variable, "appFolder", StringComparison.OrdinalIgnoreCase))
            {
                if (PackageProperties.Current != null)
                {
                    return PackageProperties.Current.PackageInstallLocation;
                }
            }
            else if (string.Equals(variable, "pfn", StringComparison.OrdinalIgnoreCase)
                || string.Equals(variable, "packageFamilyName", StringComparison.OrdinalIgnoreCase))
            {
                if (PackageProperties.Current != null)
                {
                    return PackageProperties.Current.PackageFamilyName;
                }
            }
            else if (string.Equals(variable, "currentFolder", StringComparison.OrdinalIgnoreCase)
                || string.Equals(variable, "currentDirectory", StringComparison.OrdinalIgnoreCase)
                || string.Equals(variable, "currentDir", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    if (folderPath == null)
                    {
                        try
                        {
                            var item = context.folderGetter?.Invoke();
                            if (item != null && item.IsNormalFolder)
                            {
                                folderPath = item.FullPath;
                                if (string.IsNullOrEmpty(folderPath)) folderPath = null;
                                else if (folderPath[^1] != '\\') folderPath = $"{folderPath}\\";
                            }
                        }
                        catch { }
                        if (folderPath == null)
                        {
                            flag = false;
                            folderPath = string.Empty;
                        }
                    }
                }
                catch { }
                if (!string.IsNullOrEmpty(folderPath)) return folderPath;
            }
            else if (string.Equals(variable, "item", StringComparison.OrdinalIgnoreCase))
            {
                if (context.shellItems != null
                    && context.index >= 0
                    && context.index < context.shellItems.Count)
                {
                    return context.shellItems[context.index].FullPath;
                }
            }
            else if (string.Equals(variable, "items", StringComparison.OrdinalIgnoreCase))
            {
                if (context.model?.ExecuteOptions != null)
                {
                    var shellItems = GetShellItems();

                    if (shellItems != null)
                    {
                        if (context.model.ExecuteOptions.MultipleItemsOperation == ExplorerContextMenuItemMultipleItemsOperation.SpaceSeparatedItemsWithPrefix)
                        {
                            if (shellItems != null)
                            {
                                var sb = new StringBuilder(300 * shellItems.Count);

                                for (int i = 0; i < shellItems.Count; i++)
                                {
                                    if (i > 0) sb.Append(' ');
                                    if (!string.IsNullOrEmpty(context.model.ExecuteOptions.ItemPathArgumentPrefix))
                                    {
                                        sb.Append(context.model.ExecuteOptions.ItemPathArgumentPrefix);
                                        if (context.model.ExecuteOptions.ItemPathArgumentPrefix[^1] != ' ')
                                        {
                                            sb.Append(' ');
                                        }
                                    }
                                    sb.Append('"')
                                        .Append(shellItems[i].FullPath)
                                        .Append('"');
                                }

                                return sb.ToString();
                            }
                        }
                        else if (context.model.ExecuteOptions.MultipleItemsOperation == ExplorerContextMenuItemMultipleItemsOperation.AllItemsWithDelimiter)
                        {
                            var sb = new StringBuilder('"', 255 * shellItems.Count);

                            bool started = false;
                            for (int i = 0; i < shellItems.Count; i++)
                            {
                                if (!started)
                                {
                                    started = true;
                                }
                                else if (!string.IsNullOrEmpty(context.model.ExecuteOptions.ItemPathDelimiter))
                                {
                                    sb.Append(context.model.ExecuteOptions.ItemPathDelimiter);
                                }
                                sb.Append(shellItems[i].FullPath);
                            }

                            sb.Append('"');

                            return sb.ToString();
                        }
                    }
                }
            }

            flag = false;
            return string.Empty;
        });

        if (flag)
        {
            output = result;
            return true;
        }

        return false;



        IReadOnlyList<ShellItem>? GetShellItems()
        {
            if (_shellItems != null) return _shellItems;

            if (context.model != null && context.shellItems != null)
            {
                var count = context.shellItems.Count;
                for (int i = 0; i < count; i++)
                {
                    var shellItem = context.shellItems[i];
                    var isFolder = (shellItem.Attributes.Folder && shellItem.Attributes.Stream);

                    var showWithFile = context.model.VisibilityOptions?.ShowWithFile ?? true;
                    var showWithFolder = context.model.VisibilityOptions?.ShowWithFolder ?? true;

                    if ((isFolder && showWithFolder)
                        || (!isFolder && showWithFile))
                    {
                        if (_shellItems is List<ShellItem> list) list.Add(shellItem);
                        else _shellItems = new List<ShellItem> { shellItem };
                    }
                }
            }
            if (_shellItems == null) _shellItems = [];
            return _shellItems;
        }

    }

    [GeneratedRegex("\\{([a-zA-Z0-9_]{0,15})\\}")]
    private static partial Regex VariableRegex();
}