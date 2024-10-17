namespace ExplorerContextMenu.Models
{
    public enum ExplorerContextMenuItemMultipleItemsOperation
    {
        /// <summary>
        /// 选中多个文件时, 分别对每个文件执行一次 Command
        /// </summary>
        OneByOne,

        /// <summary>
        /// 选中多个文件时, 将每个文件作为一个参数, 并且拼接前缀。<br />
        /// 如: --input "c:\file1" --input "c:\file2"
        /// </summary>
        SpaceSeparatedItemsWithPrefix,

        /// <summary>
        /// 选中多个文件时, 将所有文件拼接到一个参数中。分隔符不能使用空格。<br />
        /// 如: "c:\file1,c:\file2"
        /// </summary>
        AllItemsWithDelimiter,
    }
}
