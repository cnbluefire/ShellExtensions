using ExplorerContextMenu.ComInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using IServiceProvider = ExplorerContextMenu.ComInterfaces.IServiceProvider;
using HResults = ShellExtensions.ComInterfaces.HResults;
using ShellExtensions.Helpers;

namespace ExplorerContextMenu.ComObjects
{
    [GeneratedComClass]
    internal partial class CCreatingProcess : IServiceProvider, ICreatingProcess
    {
        private static readonly Guid SID_ExecuteCreatingProcess = new Guid(ComInterfaceIds.IID_ICreatingProcess);
        private static readonly Guid IID_ICreatingProcess = new Guid(ComInterfaceIds.IID_ICreatingProcess);

        public unsafe int OnCreating(void* pcpi)
        {
            if (pcpi == null) return HResults.E_INVALIDARG;

            return OnProcessCreating((Windows.Win32.UI.Shell.ICreateProcessInputs*)pcpi);
        }

        public unsafe int QueryService(Guid* guidService, Guid* riid, void** ppvObject)
        {
            if (ppvObject == null) return HResults.E_INVALIDARG;

            *ppvObject = null;

            if (guidService->Equals(SID_ExecuteCreatingProcess) && riid->Equals(IID_ICreatingProcess))
            {
                var hr = StrategyBasedComWrappers.DefaultIUnknownStrategy.QueryInterface((void*)ComHelpers.ComWrappers.GetOrCreateComInterfaceForObject(this, System.Runtime.InteropServices.CreateComInterfaceFlags.None), IID_ICreatingProcess, out var ppObj);
                if (hr == 0)
                {
                    *ppvObject = ppObj;
                }
                return HResults.S_OK;
            }

            return HResults.E_NOTIMPL;
        }

        private unsafe int OnProcessCreating(Windows.Win32.UI.Shell.ICreateProcessInputs* pcpi)
        {
            var handler = ProcessCreating;
            if (handler != null)
            {
                var args = new ProcessCreatingEventArgs(pcpi);
                handler.Invoke(this, args);

                if (args.Canceled) return HResults.E_FAIL;
            }
            return HResults.S_OK;
        }

        public event ProcessCreatingEventHandler? ProcessCreating;

        public unsafe class ProcessCreatingEventArgs(Windows.Win32.UI.Shell.ICreateProcessInputs* createProcessInputs)
        {
            public Windows.Win32.UI.Shell.ICreateProcessInputs* CreateProcessInputs => createProcessInputs;

            public bool Canceled { get; set; }
        }

        public delegate void ProcessCreatingEventHandler(object? sender, ProcessCreatingEventArgs args);

        public static nint CreateSite(out CCreatingProcess creatingProcess)
        {
            creatingProcess = new CCreatingProcess();
            return ComHelpers.ComWrappers.GetOrCreateComInterfaceForObject(creatingProcess, System.Runtime.InteropServices.CreateComInterfaceFlags.None);
        }
    }
}
