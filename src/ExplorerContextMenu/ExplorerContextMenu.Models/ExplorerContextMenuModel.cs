using System.Collections.Generic;

namespace ExplorerContextMenu.Models
{
    public class ExplorerContextMenuModel
    {
        public const string DefaultConfigRegistryKey = "Software\\{pfn}\\ExplorerContextMenu";

        /// <summary>
        /// 存放设置的注册表
        /// </summary>
        public string ConfigRegistryKey { get; set; } = DefaultConfigRegistryKey;

        public List<ExplorerContextMenuItemModel> MenuItems { get; set; }
    }
}
