using ShellExtensions.ComInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32.System.Com;
using Windows.Win32.Foundation;
using System.Diagnostics.Metrics;
using ShellExtensions.Helpers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ShellExtensions.ComObjects
{
    internal class ObjectWithSiteHelper : IObjectWithSite
    {
        private nint _punkSite;

        public unsafe int GetSite(Guid* riid, void** ppvSite)
        {
            *ppvSite = null;

            if (_punkSite != 0)
            {
                return ((IUnknown*)_punkSite)->QueryInterface(riid, ppvSite);
            }
            return HResults.E_FAIL;
        }

        public unsafe int SetSite(IUnknown* pUnkSite)
        {
            if (_punkSite != 0)
            {
                ((IUnknown*)_punkSite)->Release();
                _punkSite = 0;
            }

            if (pUnkSite != null)
            {
                fixed (Guid* riid = &IUnknown.IID_Guid)
                fixed (nint* ppv = &_punkSite)
                {
                    return pUnkSite->QueryInterface(riid, (void**)ppv);
                }
            }

            return HResults.E_NOINTERFACE;
        }

        public unsafe HRESULT GetWindow(HWND* hWnd)
        {
            *hWnd = default;

            var site = _punkSite;
            if (site == 0) return (HRESULT)HResults.E_NOTIMPL;

            ((IUnknown*)site)->AddRef();
            try
            {
                return Windows.Win32.PInvoke.IUnknown_GetWindow((IUnknown*)site, hWnd);
            }
            finally
            {
                ((IUnknown*)site)->Release();
            }
        }

        public unsafe HRESULT GetFolderFromSite(Guid* riid, void** folder)
        {
            var site = _punkSite;
            if (site == 0) return (HRESULT)HResults.E_NOTIMPL;

            void* pFolderView = null;

            ((IUnknown*)site)->AddRef();
            try
            {
                var IID_FolderView = new Guid(0xCDE725B0, 0xCCC9, 0x4519, 0x91, 0x7E, 0x32, 0x5D, 0x72, 0xFA, 0xB4, 0xCE);

                var hr = Windows.Win32.PInvoke.IUnknown_QueryService(
                    (IUnknown*)site,
                    &IID_FolderView,
                    &IID_FolderView,
                    &pFolderView);

                if (hr.Failed) return hr;

                return ((delegate* unmanaged[Stdcall]<void*, Guid*, void**, HRESULT>)(*(void***)pFolderView)[5])(pFolderView, riid, folder);

            }
            finally
            {
                if (pFolderView != null) ((IUnknown*)pFolderView)->Release();

                ((IUnknown*)site)->Release();
            }
        }

        public unsafe HRESULT GetContextMenuType(out ContextMenuType contextMenuType)
        {
            const string NamespaceTreeControlClassName = "NamespaceTreeControl";
            const string ExplorerTopLevelWindowClassName = "CabinetWClass";
            const string ModernContextMenuWindowClassName = "Microsoft.UI.Content.PopupWindowSiteBridge";
            const string ClassicContextMenuWindowClassName = "#32768";

            contextMenuType = ContextMenuType.Unknown;

            var apartment = Thread.CurrentThread.GetApartmentState();
            HWND hWnd = default;
            var hr = (HRESULT)HResults.E_FAIL;

            if (apartment == ApartmentState.STA)
            {
                var site = this._punkSite;
                if (site != 0)
                {
                    ((IUnknown*)site)->AddRef();

                    try { hr = Windows.Win32.PInvoke.IUnknown_GetWindow((IUnknown*)site, &hWnd); }
                    finally { ((IUnknown*)site)->Release(); }
                }

                if (hr.Succeeded)
                {
                    contextMenuType = ContextMenuType.ClassicContextMenu;

                    var className = GetClassName(hWnd, NamespaceTreeControlClassName.Length + 1);

                    LogHelper.Instance.Debug($"Get ContextMenuType From IUnknown_GetWindow");
                    LogHelper.Instance.Debug($"className: {className}");

                    if (className == NamespaceTreeControlClassName)
                    {
                        contextMenuType = ContextMenuType.ModernContextMenuInTreeView;
                        return (HRESULT)HResults.S_OK;
                    }
                    else
                    {
                        contextMenuType = ContextMenuType.ClassicContextMenu;
                        return (HRESULT)HResults.S_OK;
                    }
                }
                else if (hr.Value == HResults.E_NOTIMPL)
                {
                    contextMenuType = ContextMenuType.ClassicContextMenu;

                    // UIAutomation 会导致 dllhost.exe 进程无法退出
                    //if (UIAutomationHelper.IsTreeItemFocused()) contextMenuType = ContextMenuType.ClassicContextMenuInTreeView;

                    return (HRESULT)HResults.S_OK;
                }
                else if (hr.Value == HResults.E_NOINTERFACE)
                {
                    contextMenuType = ContextMenuType.ModernContextMenu;
                    return (HRESULT)HResults.S_OK;
                }
            }

            HWND parentWindow;
            if (hr.Succeeded) parentWindow = Windows.Win32.PInvoke.GetParent(hWnd);
            else parentWindow = Windows.Win32.PInvoke.GetForegroundWindow();

            if (!parentWindow.IsNull)
            {
                if (ExplorerTopLevelWindowClassName == GetClassName(parentWindow, ExplorerTopLevelWindowClassName.Length + 1))
                {
                    var threadId = Windows.Win32.PInvoke.GetWindowThreadProcessId(parentWindow, null);
                    if (threadId != 0)
                    {
                        LogHelper.Instance.Debug($"Get ContextMenuType From Window ClassName");

                        HWND modernContextMenuHwnd = default;
                        HWND classicContextMenuHwnd = default;

                        EnumThreadWindows(threadId, _hWnd =>
                        {
                            var _className = GetClassName(_hWnd, ModernContextMenuWindowClassName.Length + 1);

                            if (_className == ClassicContextMenuWindowClassName)
                            {
                                classicContextMenuHwnd = _hWnd;
                                return false;
                            }
                            else if (IsWindows11OrGreater()
                                && _className == ModernContextMenuWindowClassName
                                && Windows.Win32.PInvoke.GetWindow(_hWnd, Windows.Win32.UI.WindowsAndMessaging.GET_WINDOW_CMD.GW_OWNER) == parentWindow
                                && IsVisiblePopup(_hWnd))
                            {
                                LogHelper.Instance.Debug($"className: {_className}, isVisible: {IsVisiblePopup(_hWnd)}");

                                modernContextMenuHwnd = _hWnd;
                                return false;
                            }

                            return true;
                        });

                        //if (!modernContextMenuHwnd.IsNull)
                        //{
                        //    const int RPC_E_CANTCALLOUT_ININPUTSYNCCALL = unchecked((int)0x8001010D);
                        //    if (hr == RPC_E_CANTCALLOUT_ININPUTSYNCCALL)
                        //    {
                        //        // maybe not modern context menu
                        //        modernContextMenuHwnd = default;
                        //    }
                        //}

                        if (!classicContextMenuHwnd.IsNull || modernContextMenuHwnd.IsNull)
                        {
                            contextMenuType = ContextMenuType.ClassicContextMenu;
                            return (HRESULT)HResults.S_OK;
                        }
                        else
                        {
                            contextMenuType = ContextMenuType.ModernContextMenu;
                            return (HRESULT)HResults.S_OK;
                        }
                    }
                }
            }

            return hr;

            static unsafe string GetClassName(HWND hWnd, int maxCount = 256)
            {
                if (maxCount > 256)
                {
                    var buffer = new char[maxCount];
                    fixed (char* ptr = buffer)
                    {
                        var count = Windows.Win32.PInvoke.GetClassName(hWnd, ptr, maxCount);
                        if (count > 0)
                        {
                            return new string(buffer, 0, count);
                        }
                    }
                }
                else if (maxCount > 1)
                {
                    char* buffer = stackalloc char[maxCount];
                    var count = Windows.Win32.PInvoke.GetClassName(hWnd, buffer, maxCount);
                    if (count > 0)
                    {
                        return new string(buffer, 0, count);
                    }
                }
                return string.Empty;
            }

            static unsafe bool EnumThreadWindows(uint threadId, Func<HWND, bool>? predicate = null)
            {
                if (predicate == null) return false;

                var data = new object[1] { predicate };

                var gcHandle = GCHandle.Alloc(data, GCHandleType.Normal);
                var res = Windows.Win32.PInvoke.EnumThreadWindows(threadId, &EnumWindowsProc, GCHandle.ToIntPtr(gcHandle));
                gcHandle.Free();

                return res;

                [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
                static unsafe BOOL EnumWindowsProc(HWND _hWnd, LPARAM _lParam)
                {
                    var _gcHandle = GCHandle.FromIntPtr(_lParam.Value);
                    if (_gcHandle.IsAllocated && _gcHandle.Target is object[] _data)
                    {
                        var _predicate = (Func<HWND, bool>?)_data[0];
                        return _predicate?.Invoke(_hWnd) ?? true;
                    }

                    return false;
                }
            }

            static unsafe bool IsVisiblePopup(HWND _hWnd)
            {
                var style = Windows.Win32.PInvoke.GetWindowLong(_hWnd, Windows.Win32.UI.WindowsAndMessaging.WINDOW_LONG_PTR_INDEX.GWL_STYLE);
                return (style & 0x80000000) == 0x80000000
                    && (style & 0x10000000) == 0x10000000;
            }
        }


        private static bool? isWindows11OrGreater;

        private static bool IsWindows11OrGreater()
        {
            return isWindows11OrGreater ??= Environment.OSVersion.Version >= new Version(10, 0, 22000, 0);
        }

    }
}
