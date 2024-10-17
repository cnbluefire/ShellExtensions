using ExplorerContextMenu.Helpers;
using ExplorerContextMenu.Models;
using System.Linq;

namespace ExplorerContextMenu;

internal class ModelFactory
{
    private readonly ExplorerContextMenuModel rootModel;

    private readonly Dictionary<Guid, Models.ExplorerContextMenuItemModel> items;
    private readonly Dictionary<string, List<Guid>> groupIds;
    private readonly IReadOnlyList<Guid> keys;

    internal ModelFactory(Models.ExplorerContextMenuModel rootModel)
    {
        this.rootModel = rootModel;

        items = new Dictionary<Guid, ExplorerContextMenuItemModel>();
        groupIds = new Dictionary<string, List<Guid>>();
        var keys = new List<Guid>();
        this.keys = keys;

        HashSet<Guid> separatorGuids = [
            new Guid("00000000-0000-0000-0000-000000000000"),
            new Guid("11111111-1111-1111-1111-111111111111"),
            new Guid("22222222-2222-2222-2222-222222222222"),
            new Guid("33333333-3333-3333-3333-333333333333"),
            new Guid("44444444-4444-4444-4444-444444444444"),
            new Guid("55555555-5555-5555-5555-555555555555"),
            new Guid("66666666-6666-6666-6666-666666666666"),
            new Guid("77777777-7777-7777-7777-777777777777"),
            new Guid("88888888-8888-8888-8888-888888888888"),
            new Guid("99999999-9999-9999-9999-999999999999"),
            new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
            new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
            new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
        ];

        SeparatorGuids = separatorGuids;

        if (rootModel?.MenuItems != null && rootModel.MenuItems.Count > 0)
        {
            for (var i = 0; i < rootModel.MenuItems.Count; i++)
            {
                if (Guid.TryParse(rootModel.MenuItems[i]?.Guid, out var guid)
                    && items.TryAdd(guid, rootModel.MenuItems[i]))
                {
                    keys.Add(guid);
                    separatorGuids.Remove(guid);
                    if (!string.IsNullOrEmpty(rootModel.MenuItems[i].CheckOptions?.GroupName))
                    {
                        if (!groupIds.TryGetValue(rootModel.MenuItems[i].CheckOptions.GroupName, out var list))
                        {
                            groupIds[rootModel.MenuItems[i].CheckOptions.GroupName] = list = new List<Guid>();
                        }
                        list.Add(guid);
                    }
                }
            }
        }

        CheckBoxHelper = new CheckBoxHelper(this);
        LocalizedString = new LocalizedString(this);
        VisibilityHelper = new VisibilityHelper(this);
    }

    internal ExplorerContextMenuModel RootModel => rootModel;

    internal CheckBoxHelper CheckBoxHelper { get; }

    internal LocalizedString LocalizedString { get; }

    internal VisibilityHelper VisibilityHelper { get; }

    internal IReadOnlyList<Guid> GetKeys() => keys;

    internal Models.ExplorerContextMenuItemModel? GetItem(Guid guid)
    {
        if (items.TryGetValue(guid, out var result)) return result;
        return null;
    }

    internal IReadOnlyList<Models.ExplorerContextMenuItemModel>? GetSubItems(Guid guid)
    {
        var item = GetItem(guid);
        if (item != null && item.SubMenuItems != null && item.SubMenuItems.Count > 0)
        {
            var list = new List<Models.ExplorerContextMenuItemModel>();
            if (item?.SubMenuItems != null && item.SubMenuItems.Count > 0)
            {
                for (int i = 0; i < item.SubMenuItems.Count; i++)
                {
                    var subItemGuid = item.SubMenuItems[i];
                    if (Guid.TryParse(subItemGuid, out var guid2))
                    {
                        var subItem = this.GetItem(guid2);
                        if (subItem != null) list.Add(subItem);
                    }
                    else if (IsSeparatorGuid(subItemGuid))
                    {
                        list.Add(Separator);
                    }
                }
            }

            return list;
        }
        return null;
    }

    internal IReadOnlyCollection<Guid> GetGroupIds(string groupName)
    {
        if (groupIds.TryGetValue(groupName, out var result)) return result;
        return [];
    }

    private bool IsSeparatorGuid(string guid)
    {
        if (guid != null && guid.Length >= 3)
        {
            var first = guid[0];
            bool flag = IsSeparatorChar(first);
            for (int j = 1; flag && j < guid.Length; j++)
            {
                flag = guid[j] == first && IsSeparatorChar(guid[j]);
            }

            if (flag) return flag;
        }

        if (Guid.TryParse(guid, out var guid2))
        {
            return SeparatorGuids.Contains(guid2);
        }

        return false;

        static bool IsSeparatorChar(char _ch) => _ch == '*' || _ch == '-' || _ch == '_';
    }

    internal IReadOnlyCollection<Guid> SeparatorGuids { get; private set; }

    private static Models.ExplorerContextMenuItemModel? separator;
    private static Models.ExplorerContextMenuItemModel Separator => separator ??= new ExplorerContextMenuItemModel()
    {
        Guid = default,
        Flags = new ExplorerContextMenuItemFlags()
        {
            IsSeparator = true
        },
        VisibilityOptions = new ExplorerContextMenuItemVisibilityOptions()
        {
            ShowInClassicContextMenu = true,
            ShowInModernContextMenu = false,
        }
    };
}
