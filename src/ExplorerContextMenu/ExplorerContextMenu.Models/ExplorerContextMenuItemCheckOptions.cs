using System;
using System.Collections.Generic;
using System.Text;

namespace ExplorerContextMenu.Models
{
    public class ExplorerContextMenuItemCheckOptions
    {
        public bool IsCheckable { get; set; } = false;

        public ExplorerContextMenuItemCheckType CheckType { get; set; } = ExplorerContextMenuItemCheckType.Check;

        public string GroupName { get; set; }

        public bool DefaultChecked { get; set; } = false;
    }

    public enum ExplorerContextMenuItemCheckType
    {
        Check,

        // 顶级菜单中不支持此类型
        RadioCheck
    }
}
