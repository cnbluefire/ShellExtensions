using ShellExtensions.ComInterfaces;
using ShellExtensions.Helpers;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com;
using Windows.Win32.System.Com.StructuredStorage;

namespace ShellExtensions.ComObjects
{
    [GeneratedComClass]
    internal unsafe partial class CExplorerCommand : IExplorerCommand, IExplorerCommandState, IInitializeCommand, IObjectWithSite
    {
        private readonly ExplorerCommand impl;
        private readonly IDirectoryBackgroundAccessor directoryBackgroundAccessor;
        private readonly IContextMenuTypeAccessor? contextMenuTypeAccessor;

        internal unsafe CExplorerCommand(ExplorerCommand impl)
        {
            objectWithSiteHelper = new ObjectWithSiteHelper();
            this.impl = impl;

            directoryBackgroundAccessor = new DirectoryBackgroundAccessor(this, s =>
            {
                if (s is CExplorerCommand strongThis)
                {
                    var helper = strongThis.objectWithSiteHelper;
                    if (helper != null)
                    {
                        Windows.Win32.UI.Shell.IShellItem* folder = null;
                        fixed (Guid* riid = &Windows.Win32.UI.Shell.IShellItem.IID_Guid)
                        {
                            var hr = helper.GetFolderFromSite(riid, (void**)&folder);
                            if (hr.Succeeded) return new ShellFolder(folder);
                        }
                    }
                }
                return null;
            });
            if (impl.ServiceProvider is ServiceFactory serviceFactory)
            {
                contextMenuTypeAccessor = new ContextMenuTypeAccessor();
                serviceFactory.UpdateFactory<IContextMenuTypeAccessor>(contextMenuTypeAccessor);
            }
        }

        public int EnumSubCommands(out IEnumExplorerCommand? ppEnum)
        {
            LogHelper.Instance.Debug(nameof(EnumSubCommands));

            ppEnum = null;

            if (impl.Children != null && impl.Children.Count > 0)
            {
                ppEnum = new CEnumExplorerCommand(impl.Children);
                return HResults.S_OK;
            }

            return HResults.E_NOTIMPL;
        }

        public unsafe int GetCanonicalName(Guid* pguidCommandName)
        {
            LogHelper.Instance.Debug(nameof(GetCanonicalName));

            try
            {
                UpdateImplServices(true);

                var guid = impl.CanonicalName;
                if (!guid.HasValue) return HResults.E_NOTIMPL;

                *pguidCommandName = guid.Value;
                return HResults.S_OK;
            }
            catch (Exception ex)
            {
                return ex.HResult;
            }
            finally
            {
                UpdateImplServices(false);
            }
        }

        public unsafe int GetFlags(uint* pFlags)
        {
            LogHelper.Instance.Debug(nameof(GetFlags));

            try
            {
                UpdateImplServices(true);

                *pFlags = (uint)impl.Flags;
                return HResults.S_OK;
            }
            catch (Exception ex)
            {
                return ex.HResult;
            }
            finally
            {
                UpdateImplServices(false);
            }
        }

        public unsafe int GetState(Windows.Win32.UI.Shell.IShellItemArray* psiItemArray, [MarshalAs(UnmanagedType.Bool)] bool fOkToBeSlow, uint* pCmdState)
        {
            LogHelper.Instance.Debug($"{nameof(GetState)} {nameof(fOkToBeSlow)} = {fOkToBeSlow}");

            try
            {
                if (this.contextMenuTypeAccessor is ContextMenuTypeAccessor contextMenuTypeAccessor)
                {
                    var ts = System.Diagnostics.Stopwatch.GetTimestamp();

                    if (objectWithSiteHelper.GetContextMenuType(out var contextMenuType).Succeeded)
                        contextMenuTypeAccessor.ContextMenuType = contextMenuType;
                    // fallback to ClassicContextMenu
                    else contextMenuTypeAccessor.ContextMenuType = ContextMenuType.ClassicContextMenu;

                    var time = System.Diagnostics.Stopwatch.GetElapsedTime(ts);
                    LogHelper.Instance.Debug($"Get ContextMenuType {time}");

                    LogHelper.Instance.Debug($"ContextMenuType: {contextMenuTypeAccessor.ContextMenuType}");
                }
                UpdateImplServices(true);

                *pCmdState = (uint)impl.GetState(new ShellItemArray(psiItemArray), fOkToBeSlow, out var pending);

                if (pending)
                {
                    return HResults.E_PENDING;
                }
                else
                {
                    return HResults.S_OK;
                }
            }
            catch (Exception ex)
            {
                return ex.HResult;
            }
            finally
            {
                UpdateImplServices(false);
            }
        }

        public unsafe int GetIcon(Windows.Win32.UI.Shell.IShellItemArray* psiItemArray, PWSTR* ppszIcon)
        {
            LogHelper.Instance.Debug(nameof(GetIcon));

            try
            {
                UpdateImplServices(true);

                var str = impl.GetIcon(new ShellItemArray(psiItemArray));
                if (str == null) return HResults.E_NOTIMPL;

                return DuplicateString(str, ppszIcon);
            }
            catch (Exception ex)
            {
                return ex.HResult;
            }
            finally
            {
                UpdateImplServices(false);
            }
        }

        public unsafe int GetTitle(Windows.Win32.UI.Shell.IShellItemArray* psiItemArray, PWSTR* ppszName)
        {
            LogHelper.Instance.Debug(nameof(GetTitle));

            try
            {
                UpdateImplServices(true);

                var str = impl.GetTitle(new ShellItemArray(psiItemArray));
                if (str == null) return HResults.E_NOTIMPL;

                return DuplicateString(str, ppszName);
            }
            catch (Exception ex)
            {
                return ex.HResult;
            }
            finally
            {
                UpdateImplServices(false);
            }
        }

        public unsafe int GetToolTip(Windows.Win32.UI.Shell.IShellItemArray* psiItemArray, PWSTR* ppszInfotip)
        {
            LogHelper.Instance.Debug(nameof(GetToolTip));

            try
            {
                UpdateImplServices(true);

                var str = impl.GetToolTip(new ShellItemArray(psiItemArray));
                if (str == null) return HResults.E_NOTIMPL;

                return DuplicateString(str, ppszInfotip);
            }
            catch (Exception ex)
            {
                return ex.HResult;
            }
            finally
            {
                UpdateImplServices(false);
            }
        }

        public unsafe int Invoke(Windows.Win32.UI.Shell.IShellItemArray* psiItemArray, IBindCtx* pbc)
        {
            LogHelper.Instance.Debug(nameof(Invoke));

            var context = CreateInvokeContext(this, psiItemArray);

            var thread = new Thread(static state =>
            {
                ((InvokeContext)state!).Unwrap(
                    out var command,
                    out var shellItemArray,
                    out var folderItem);

                if (command != null)
                {
                    try
                    {
                        if (command.impl.ServiceProvider is ServiceFactory serviceFactory)
                        {
                            var shellFolder = folderItem != null ? new ShellFolder(folderItem) : null;
                            if (shellFolder != null)
                            {
                                serviceFactory.UpdateFactory<IDirectoryBackgroundAccessor>(new DirectoryBackgroundAccessor(command, c =>
                                {
                                    return shellFolder;
                                }));
                            }
                            else
                            {
                                serviceFactory.UpdateFactory<IDirectoryBackgroundAccessor>(null);
                            }
                        }

                        command?.impl.Invoke(new ExplorerCommandInvokeEventArgs(
                            new ShellItemArray(shellItemArray),
                            folderItem != null ? new ShellFolder(folderItem) : null));
                    }
                    finally
                    {
                        lock (state)
                        {
                            Monitor.Pulse(state);
                        }
                        command.UpdateImplServices(false);
                        if (shellItemArray != null) shellItemArray->Release();
                        if (folderItem != null) folderItem->Release();
                    }
                }
            })
            {
                IsBackground = true,
                Name = "CExplorerCommand::Invoke",
            };
            thread.SetApartmentState(ApartmentState.MTA);
            thread.Start(context);

            lock (context)
            {
                Monitor.Wait(context);
            }

            return HResults.S_OK;
        }

        #region IInitializeCommand

        public int Initialize([MarshalAs(UnmanagedType.LPWStr)] string pszCommandName, IPropertyBag* ppb)
        {
            LogHelper.Instance.Debug($"{nameof(Invoke)} {nameof(Initialize)} = {pszCommandName}");

            impl.CommandName = pszCommandName;
            return HResults.S_OK;
        }

        #endregion IInitializeCommand


        #region IObjectWithSite

        private ObjectWithSiteHelper objectWithSiteHelper;

        public unsafe int GetSite(Guid* riid, void** ppvSite)
        {
            LogHelper.Instance.Debug(nameof(GetSite));

            return objectWithSiteHelper.GetSite(riid, ppvSite);
        }

        public unsafe int SetSite(IUnknown* pUnkSite)
        {
            LogHelper.Instance.Debug(nameof(SetSite));

            return objectWithSiteHelper.SetSite(pUnkSite);
        }

        #endregion IObjectWithSite

        private void UpdateImplServices(bool enable)
        {
            var impl = this.impl;
            if (impl?.ServiceProvider is ServiceFactory serviceFactory)
            {
                if (enable)
                {
                    serviceFactory.UpdateFactory<IDirectoryBackgroundAccessor>(directoryBackgroundAccessor);
                }
                else
                {
                    serviceFactory.UpdateFactory<IDirectoryBackgroundAccessor>(null);
                }
            }
        }

        private static InvokeContext CreateInvokeContext(CExplorerCommand command, Windows.Win32.UI.Shell.IShellItemArray* shellItemArray)
        {
            Windows.Win32.UI.Shell.IShellItem* folder = null;

            fixed (Guid* riid_IShellItem = &Windows.Win32.UI.Shell.IShellItem.IID_Guid)
            {
                try
                {
                    var hr = command.objectWithSiteHelper.GetFolderFromSite(riid_IShellItem, (void**)&folder);
                    if (hr.Failed) folder = null;

                    return new InvokeContext(command, shellItemArray, folder);
                }
                finally
                {
                    if (folder != null) folder->Release();
                }
            }
        }


        private static int DuplicateString(string str, PWSTR* ppsz)
        {
            if (string.IsNullOrEmpty(str))
            {
                return HResults.E_NOTIMPL;
            }
            else
            {
                if (str[^1] == '\0')
                {
                    fixed (char* pstr = str)
                    {
                        return Windows.Win32.PInvoke.SHStrDup(pstr, ppsz);
                    }
                }

                var ptr = (char*)Windows.Win32.PInvoke.CoTaskMemAlloc((nuint)((str.Length + 1) * sizeof(char)));
                var span = new Span<char>(ptr, str.Length + 1);
                str.AsSpan().CopyTo(span);
                span[span.Length - 1] = '\0';

                *ppsz = ptr;
                return HResults.S_OK;
            }
        }


        private class InvokeContext
        {
            private MarshalToStream<Windows.Win32.UI.Shell.IShellItemArray>? shellItemArrayStream;
            private MarshalToStream<Windows.Win32.UI.Shell.IShellItem>? folderStream;
            private CExplorerCommand? command;

            public InvokeContext(
                CExplorerCommand command,
                Windows.Win32.UI.Shell.IShellItemArray* shellItemArray,
                Windows.Win32.UI.Shell.IShellItem* folder)
            {
                this.command = command;

                if (shellItemArray != null)
                {
                    shellItemArrayStream = new MarshalToStream<Windows.Win32.UI.Shell.IShellItemArray>(shellItemArray);
                }

                if (folder != null)
                {
                    folderStream = new MarshalToStream<Windows.Win32.UI.Shell.IShellItem>(folder);
                }
            }

            public void Unwrap(
                out CExplorerCommand? command,
                out Windows.Win32.UI.Shell.IShellItemArray* shellItemArray,
                out Windows.Win32.UI.Shell.IShellItem* folder)
            {
                command = this.command;

                shellItemArray = null;
                folder = null;

                this.command = null;

                if (shellItemArrayStream != null)
                {
                    fixed (Windows.Win32.UI.Shell.IShellItemArray** ppShellItemArray = &shellItemArray)
                    {
                        shellItemArrayStream.Unmarshal(ppShellItemArray);
                        shellItemArrayStream = null;
                    }
                }

                if (folderStream != null)
                {
                    fixed (Windows.Win32.UI.Shell.IShellItem** ppFolder = &folder)
                    {
                        folderStream.Unmarshal(ppFolder);
                        folderStream = null;
                    }
                }
            }


            private class MarshalToStream<T> where T : unmanaged, Windows.Win32.IComIID
            {
                private IStream* stream;

                public MarshalToStream(T* pObj)
                {
                    if (pObj != null)
                    {
                        fixed (Guid* riid = &T.Guid)
                        fixed (IStream** ppStream = &stream)
                        {
                            var hr = Windows.Win32.PInvoke.CoMarshalInterThreadInterfaceInStream(riid, (IUnknown*)pObj, ppStream);
                            if (hr.Failed) stream = null;
                        }
                    }
                }

                public HRESULT Unmarshal(T** ppv)
                {
                    lock (this)
                    {
                        if (stream != null)
                        {
                            fixed (Guid* riid = &T.Guid)
                            {
                                return Windows.Win32.PInvoke.CoGetInterfaceAndReleaseStream(stream, riid, (void**)ppv);
                            }
                        }
                    }
                    return (HRESULT)HResults.E_INVALIDARG;
                }
            }
        }

    }
}
