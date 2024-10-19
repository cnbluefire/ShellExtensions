using System.Collections.Generic;

namespace ExplorerContextMenu.Models
{
    /*
     支持的环境变量：
     dllPath        : 菜单 dll 的完整路径

     dllFolder      : 菜单 dll 所在目录，包含尾随的'\'

     packageFolder  :
     appFolder      : 打包模式下 app 的安装目录，包含尾随的'\'

     packageFamilyName :
     pfn            : package family name

     currentFolder  :
     currentDirectory :
     currentDir     : 当前所在文件夹，包含尾随的'\'

     items          : 选中的文件或文件夹路径，仅在 MultipleFilesOperation 为 Join 时可用，使用 PathDelimiter 分隔
     item           : 选中的文件或文件夹路径，MultipleFilesOperation 为 OneByOne 时返回正在处理的文件，Join 时返回选中文件中的第一个
     itemName       : 选中的文件或文件夹名
     
     本地化字符串字典的 key 不处理大小写
     */

    public class ExplorerContextMenuItemModel
    {
        /// <summary>
        /// 唯一 id, 不允许重复
        /// </summary>
        public string Guid { get; set; }

        /// <summary>
        /// 工具提示, 支持本地化, 如果无法适配任何语言则使用 default/en/en-US 获取值
        /// </summary>
        public Dictionary<string, string> ToolTip { get; set; }

        /// <summary>
        /// 标题, 支持本地化, 如果无法适配任何语言则使用 default/en/en-US 获取值
        /// </summary>
        public Dictionary<string, string> Title { get; set; }

        public ExplorerContextMenuItemCheckOptions CheckOptions { get; set; }
        /// <summary>
        /// 设置菜单的 flags, 当显示在 modern 菜单中时, IsSeparator 为 true 的菜单将会隐藏。<br />
        /// 当 HasLuaShield 为 true 时, 将使用管理员权限执行 command。<br />
        /// <see href="https://learn.microsoft.com/zh-cn/windows/win32/api/shobjidl_core/nf-shobjidl_core-iexplorercommand-getflags"/>
        /// </summary>
        public ExplorerContextMenuItemFlags Flags { get; set; } = new ExplorerContextMenuItemFlags();

        /// <summary>
        /// 默认图标
        /// </summary>
        public ExplorerContextMenuItemIconModel Icon { get; set; }

        /// <summary>
        /// 暗色模式图标
        /// </summary>
        public ExplorerContextMenuItemIconModel DarkIcon { get; set; }

        /// <summary>
        /// 子菜单的 Guid。如果值为三个以上的 '*', '-', '_', 或所有字符都一致的 Guid, 则认为是分隔线, 且只在经典菜单中显示。
        /// </summary>
        public List<string> SubMenuItems { get; set; }

        /// <summary>
        /// 多文件处理的配置
        /// </summary>
        public ExplorerContextMenuItemExecuteOptions ExecuteOptions { get; set; } = new ExplorerContextMenuItemExecuteOptions()
        {
            MultipleItemsOperation = ExplorerContextMenuItemMultipleItemsOperation.OneByOne,
            ItemPathDelimiter = ","
        };

        public ExplorerContextMenuItemVisibilityOptions VisibilityOptions { get; set; }
    }
}
