using ShellExtensions.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShellExtensions
{
    public interface IDirectoryBackgroundAccessor
    {
        ShellFolder? ShellFolder { get; }
    }

    public interface IContextMenuTypeAccessor
    {
        /// <summary>
        /// 使用此属性时, ThreadingModel 必须为 STA
        /// </summary>
        ContextMenuType ContextMenuType { get; }
    }

    internal class DirectoryBackgroundAccessor : IDirectoryBackgroundAccessor
    {
        private readonly WeakReference weakRef;
        private readonly Func<object, ShellFolder?> func;

        internal DirectoryBackgroundAccessor(object target, Func<object, ShellFolder?> func)
        {
            weakRef = new WeakReference(target);
            this.func = func;
        }

        public ShellFolder? ShellFolder => weakRef.Target is object target ?
            func.Invoke(target) : null;
    }

    internal class ContextMenuTypeAccessor : IContextMenuTypeAccessor
    {
        public ContextMenuType ContextMenuType { get; set; }
    }
}
