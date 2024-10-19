﻿using ExplorerContextMenu.Models;
using ShellExtensions;
using ShellExtensions.Helpers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace ExplorerContextMenu;

internal partial class ParameterContext
{
    private readonly ExplorerContextMenuItemModel? model;
    private readonly ShellItemArray? shellItems;
    private readonly Func<ShellFolder?>? folderGetter;
    private IReadOnlyList<ShellItem>? _shellItems = null;

    public ParameterContext() : this(null, null, null) { }

    public ParameterContext(
        ExplorerContextMenuItemModel? model,
        ShellItemArray? shellItems,
        Func<ShellFolder?>? folderGetter)
    {
        this.model = model;
        this.shellItems = shellItems;
        this.folderGetter = folderGetter;
        this.Index = 0;
    }

    public int Index { get; set; }

    public int ShellItemsCount => GetShellItems()?.Count ?? 0;

    public unsafe bool TryFormat(string text, [NotNullWhen(true)] out string? output)
    {
        LogHelper.Instance.Debug(text);
        foreach (var item in GetShellItems() ?? [])
        {
            LogHelper.Instance.Debug($"ShellItem: {item.GetDisplayName(ShellItem.SIGDN.SIGDN_NORMALDISPLAY)}");
        }

        output = null;

        string? folderPath = null;
        bool flag = true;

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
                if (folderGetter != null)
                {
                    try
                    {
                        if (folderPath == null)
                        {
                            try
                            {
                                var item = folderGetter?.Invoke();
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
            }
            else if (string.Equals(variable, "item", StringComparison.OrdinalIgnoreCase))
            {
                if (shellItems != null)
                {
                    var shellItems = GetShellItems();

                    if (shellItems != null
                        && Index >= 0
                        && Index < shellItems?.Count)
                    {

                        return shellItems[Index].FullPath;
                    }
                }
            }
            else if (string.Equals(variable, "items", StringComparison.OrdinalIgnoreCase))
            {
                if (shellItems != null)
                {
                    if (model?.ExecuteOptions != null)
                    {
                        var shellItems = GetShellItems();

                        if (shellItems != null)
                        {
                            if (model.ExecuteOptions.MultipleItemsOperation == ExplorerContextMenuItemMultipleItemsOperation.SpaceSeparatedItemsWithPrefix)
                            {
                                if (shellItems != null)
                                {
                                    var sb = new StringBuilder(300 * shellItems.Count);

                                    for (int i = 0; i < shellItems.Count; i++)
                                    {
                                        if (i > 0) sb.Append(' ');
                                        if (!string.IsNullOrEmpty(model.ExecuteOptions.ItemPathArgumentPrefix))
                                        {
                                            sb.Append(model.ExecuteOptions.ItemPathArgumentPrefix);
                                            if (model.ExecuteOptions.ItemPathArgumentPrefix[^1] != ' ')
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
                            else if (model.ExecuteOptions.MultipleItemsOperation == ExplorerContextMenuItemMultipleItemsOperation.AllItemsWithDelimiter)
                            {
                                var sb = new StringBuilder('"', 255 * shellItems.Count);

                                bool started = false;
                                for (int i = 0; i < shellItems.Count; i++)
                                {
                                    if (!started)
                                    {
                                        started = true;
                                    }
                                    else if (!string.IsNullOrEmpty(model.ExecuteOptions.ItemPathDelimiter))
                                    {
                                        sb.Append(model.ExecuteOptions.ItemPathDelimiter);
                                    }
                                    sb.Append(shellItems[i].FullPath);
                                }

                                sb.Append('"');

                                return sb.ToString();
                            }
                        }
                    }
                }
            }
            else if (string.Equals(variable, "itemName", StringComparison.OrdinalIgnoreCase))
            {
                if (shellItems != null)
                {
                    var index = Index;
                    var shellItems = GetShellItems();

                    if (shellItems != null)
                    {
                        if (index < 0 || index >= shellItems.Count)
                        {
                            index = 0;
                        }

                        var shellItem = shellItems[index];

                        return shellItem.GetDisplayName(ShellItem.SIGDN.SIGDN_NORMALDISPLAY);
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
    }

    private IReadOnlyList<ShellItem>? GetShellItems()
    {
        if (_shellItems != null) return _shellItems;

        if (model != null && shellItems != null)
        {
            var count = shellItems.Count;
            for (int i = 0; i < count; i++)
            {
                var shellItem = shellItems[i];
                var isFolder = (shellItem.Attributes.Folder && shellItem.Attributes.Stream);

                var showWithFile = model.VisibilityOptions?.ShowWithFile ?? true;
                var showWithFolder = model.VisibilityOptions?.ShowWithFolder ?? true;

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

    [GeneratedRegex("\\{([a-zA-Z0-9_]{0,15})\\}")]
    private static partial Regex VariableRegex();
}