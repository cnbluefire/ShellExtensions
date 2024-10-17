using ExplorerContextMenu.Models;
using System.Diagnostics.CodeAnalysis;

namespace ExplorerContextMenu.Helpers
{
    internal class CheckBoxHelper
    {
        private readonly ModelFactory factory;

        private string registryKey;

        public CheckBoxHelper(ModelFactory factory)
        {
            this.factory = factory;

            registryKey = string.Empty;

            var tmpKey = factory.RootModel.ConfigRegistryKey;
            if (!string.IsNullOrEmpty(tmpKey))
            {
                tmpKey = tmpKey.Replace('/', '\\');
                var context = new ParameterContext();
                if (context.TryFormat(tmpKey, out var value))
                {
                    registryKey = value;
                }
            }
        }

        internal bool? IsChecked(Guid guid)
        {
            if (string.IsNullOrEmpty(registryKey)) return null;

            if (IsCheckedCore(guid, out var item, out var value, out var isDefault))
            {
                // 手动设置过值或默认值为 false 时直接返回
                if (!isDefault || !value) return value;

                var groupFlag = string.IsNullOrEmpty(item.CheckOptions.GroupName);
                if (groupFlag)
                {
                    var groupIds = factory.GetGroupIds(item.CheckOptions.GroupName);
                    if (groupIds.Count > 1)
                    {
                        bool hasCheckedItemInGroup = false;
                        foreach (var id in groupIds)
                        {
                            if (id != guid)
                            {
                                if (IsCheckedCore(id, out var item2, out var value2, out var isDefaultValue2))
                                {
                                    hasCheckedItemInGroup |= value2;
                                }
                            }
                        }
                        if (hasCheckedItemInGroup) return false;
                    }
                }

                return true;
            }

            return null;
        }

        private bool IsCheckedCore(Guid guid, [NotNullWhen(true)] out ExplorerContextMenuItemModel? item, out bool value, out bool isDefaultValue)
        {
            item = null;
            value = false;
            isDefaultValue = false;

            if (string.IsNullOrEmpty(registryKey)) return false;

            item = factory.GetItem(guid);
            if (item?.CheckOptions != null && item.CheckOptions.IsCheckable)
            {
                // HKCU\\Software\\xxx\\ExplorerContextMenu\\{xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx}\\Checked
                if (SettingsHelper.TryGetValue(Path.Combine(registryKey, guid.ToString("B")), "Checked", out value))
                {
                    isDefaultValue = false;
                    return true;
                }
                value = item.CheckOptions.DefaultChecked;
                isDefaultValue = true;
                return true;
            }
            return false;
        }

        internal bool? SetCheckState(Guid guid, bool? state)
        {
            if (string.IsNullOrEmpty(registryKey)) return null;

            var item = factory.GetItem(guid);
            if (item?.CheckOptions != null && item.CheckOptions.IsCheckable)
            {
                if (!state.HasValue)
                {
                    if (SettingsHelper.UnsetValue(Path.Combine(registryKey, guid.ToString("B")), "Checked"))
                    {
                        return item.CheckOptions.DefaultChecked;
                    }
                    return null;
                }

                if (!state.Value || string.IsNullOrEmpty(item.CheckOptions.GroupName))
                {
                    if (SettingsHelper.SetValue(Path.Combine(registryKey, guid.ToString("B")), "Checked", state.Value))
                    {
                        return state;
                    }
                    return null;
                }
                else
                {
                    if (SettingsHelper.SetValue(Path.Combine(registryKey, guid.ToString("B")), "Checked", true))
                    {
                        var groupIds = factory.GetGroupIds(item.CheckOptions.GroupName);
                        if (groupIds.Count > 1)
                        {
                            foreach (var id in groupIds)
                            {
                                if (id != guid)
                                {
                                    SettingsHelper.SetValue(Path.Combine(registryKey, id.ToString("B")), "Checked", false);
                                }
                            }
                        }
                        return true;
                    }
                }
            }
            return null;
        }
    }
}
