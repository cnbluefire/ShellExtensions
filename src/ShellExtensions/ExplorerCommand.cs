using ShellExtensions.ComInterfaces;
using ShellExtensions.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com;

namespace ShellExtensions
{
    public abstract class ExplorerCommand : IShellExtensions
    {
        public IServiceProvider ServiceProvider { get; } = new ServiceFactory();

        public virtual string? CommandName { get; internal set; }

        public virtual Guid? CanonicalName { get; }

        public virtual ExplorerCommandFlags Flags
        {
            get
            {
                if (Children != null && Children.Count != 0) return ExplorerCommandFlags.ECF_HASSUBCOMMANDS;
                return ExplorerCommandFlags.ECF_DEFAULT;
            }
        }

        public virtual string? GetIcon(ShellItemArray shellItems) => null;

        public virtual string? GetTitle(ShellItemArray shellItems) => null;

        public virtual string? GetToolTip(ShellItemArray shellItems) => null;

        public virtual ExplorerCommandState GetState(ShellItemArray shellItems, bool fOkToBeSlow, out bool pending)
        {
            pending = false;
            return ExplorerCommandState.ECS_ENABLED;
        }

        public virtual void Invoke(ExplorerCommandInvokeEventArgs args) { }

        public virtual IReadOnlyList<ExplorerCommand>? Children { get; } = null;
    }

    public class ExplorerCommandInvokeEventArgs
    {
        internal ExplorerCommandInvokeEventArgs(ShellItemArray shellItems, ShellFolder? folder)
        {
            ShellItems = shellItems;
            Folder = folder;
        }

        public ShellItemArray ShellItems { get; }

        public ShellFolder? Folder { get; }
    }
}
