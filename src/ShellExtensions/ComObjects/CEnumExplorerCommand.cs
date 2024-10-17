using ShellExtensions.ComInterfaces;
using ShellExtensions.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;

namespace ShellExtensions.ComObjects
{
    [GeneratedComClass]
    internal partial class CEnumExplorerCommand : IEnumExplorerCommand
    {
        private ExplorerCommand[] commands;
        private int index = 0;

        internal CEnumExplorerCommand(IEnumerable<ExplorerCommand> commands)
        {
            this.commands = commands.ToArray();
        }

        public int Clone(out IEnumExplorerCommand? ppenum)
        {
            ppenum = new CEnumExplorerCommand(commands);
            return HResults.S_OK;
        }

        public unsafe int Next(uint celt, void** pUICommand, uint* pceltFetched)
        {
            if (pceltFetched != null)
            {
                *pceltFetched = 0;
            }

            if (commands == null || commands.Length == 0) return HResults.E_NOTIMPL;

            var start = index;
            for (int i = 0; i < celt && start + i < commands.Length; i++)
            {
                pUICommand[i] = (void*)ComHelpers.ComWrappers.GetOrCreateComInterfaceForObject(new CExplorerCommand(commands[index]), CreateComInterfaceFlags.None);
                index++;
            }
            if (pceltFetched != null)
            {
                *pceltFetched = (uint)(index - start);
            }

            return (index - start == celt) ? HResults.S_OK : HResults.S_FALSE;
        }

        public int Reset()
        {
            index = 0;
            return HResults.S_OK;
        }

        public int Skip(uint celt)
        {
            return HResults.E_NOTIMPL;
        }
    }
}
