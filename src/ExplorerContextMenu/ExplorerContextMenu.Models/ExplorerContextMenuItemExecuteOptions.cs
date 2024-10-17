namespace ExplorerContextMenu.Models
{
    public class ExplorerContextMenuItemExecuteOptions
    {
        /// <summary>
        /// 执行的命令, 当 Type 为 Check 或 RadioCheck 时, Command 可以为空
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// 多文件的操作
        /// </summary>
        public ExplorerContextMenuItemMultipleItemsOperation MultipleItemsOperation { get; set; }

        /// <summary>
        /// 当 Operation 为 AllFilesWithDelimiter 时, 
        /// 使用此参数指定多个文件路径拼接时使用的分隔符, 不能为空格。
        /// </summary>
        public string ItemPathDelimiter { get; set; }

        /// <summary>
        /// 当 Operation 为 SpaceSeparatedFilesWithPrefix 时, 
        /// 使用此参数指定文件参数的前缀。
        /// </summary>
        public string ItemPathArgumentPrefix { get; set; }
    }
}
