using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerContextMenu.Models
{
    public class ExplorerContextMenuItemVisibilityOptions
    {
        public bool Show { get; set; } = true;

        /// <summary>
        /// 显示在 win11 的 modern context menu 中
        /// </summary>
        public bool ShowInModernContextMenu { get; set; } = true;

        /// <summary>
        /// 显示在经典 context menu 中
        /// </summary>
        public bool ShowInClassicContextMenu { get; set; } = true;

        /// <summary>
        /// 选中文件时显示
        /// </summary>
        public bool ShowWithFile { get; set; } = true;

        /// <summary>
        /// 选中文件夹时显示
        /// </summary>
        public bool ShowWithFolder { get; set; } = true;

        /// <summary>
        /// 选中多个文件时显示
        /// </summary>
        public bool ShowWithMultipleItems { get; set; } = true;

        /// <summary>
        /// 支持使用注册表值重载
        /// </summary>
        public bool RegistryOverrideSupport { get; set; } = false;
    }
}
