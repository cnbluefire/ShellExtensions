using System.Collections.Generic;

namespace ExplorerContextMenu.Models
{
    public class ExplorerContextMenuItemIconModel
    {
        /// <summary>
        /// 设置外部图标资源文件, 可以是 .ico 文件, 也可以是 .dll 或 .exe 文件内的资源<br />
        /// <see href="https://learn.microsoft.com/zh-cn/windows/win32/api/shobjidl_core/nf-shobjidl_core-iexplorercommand-geticon"/>
        /// </summary>
        public string IconFile { get; set; }

        /// <summary>
        /// 设置嵌入的 .ico 文件，文件内可以有多个图标
        /// </summary>
        public string EmbeddedIconFile { get; set; }

        /// <summary>
        /// 需要嵌入 dll 内的 png 资源, key 为尺寸, value 为 png 图片路径。<br />
        /// 生成内嵌 ico 时, png 将自动缩放至 key 指定的尺寸。<br />
        /// 如 ["16"] = "Assets\16x16.png", ["32"] = "Assets\32x32.png"<br />
        /// 支持的尺寸: 256, 64, 48, 40, 32, 24, 20, 16 
        /// </summary>
        public Dictionary<string, string> EmbeddedPngFiles { get; set; }
    }
}
